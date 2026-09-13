using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Social.Common.Constants;
using Social.Data.Model.Response.Base;
using Social.Repository.Social.User.Interface;
using Social.Service.Social.Email.Models;
using Social.WebApi.Infrastructure.Extensions;
using Social.WebApi.Installers;
using Social.WebApi.Middleware;
using Social.WebApi.Models;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Kestrel's default MaxRequestBodySize is ~28.6MB — plenty for a photo, not
// for a video, so a video post silently failed the upload before it even
// reached PostsController. Raised to 500MB here (matches FormOptions below,
// which ASP.NET Core's form-parsing middleware also enforces independently).
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 500 * 1024 * 1024;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500 * 1024 * 1024;
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums (ErrorCode included) serialize as their member name, e.g.
        // "EMAIL_EXISTS", instead of a raw number the frontend would have to
        // memorize positionally.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ExceptionMiddleware and the JwtBearerEvents handlers below write JSON via
// HttpResponse.WriteAsJsonAsync, which goes through this (separate from MVC's
// AddJsonOptions above) — register the same enum converter here too, or
// ErrorCode would serialize as a raw number on exactly the responses meant to
// demonstrate the standardized envelope.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Every model-validation failure ([Required]/[EmailAddress]/... on a request
// DTO) goes through this instead of ASP.NET's default ValidationProblemDetails,
// so 400s look exactly like every other ApiResponse<T> the API returns.
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = string.Join(
            " ",
            context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m)));

        return new Microsoft.AspNetCore.Mvc.ObjectResult(new ApiResponse<object>
        {
            Success = false,
            Message = string.IsNullOrWhiteSpace(message) ? "Dữ liệu gửi lên không hợp lệ." : message,
            StatusCode = 400,
            ErrorCode = ErrorCode.VALIDATION_ERROR
        })
        {
            StatusCode = 400
        };
    };
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Social API",
        Version = "v1"
    });

    options.MapType<Microsoft.AspNetCore.Http.IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    options.OperationFilter<Social.WebApi.Infrastructure.Filters.FormFileOperationFilter>();
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
            ValidAudience = builder.Configuration["JwtConfig:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["JwtConfig:Key"]!)),

            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        // [Authorize] rejects a missing/invalid/expired token before any
        // controller action runs, so without these handlers it writes an
        // empty 401/403 body instead of the ApiResponse<T> envelope every
        // other error on the API uses.
        options.Events = new JwtBearerEvents
        {
            // Enforces "one active session per client type": a JWT is only honored while its
            // "sid" claim still matches UserSessions' current token for (userId, clientType).
            // Logging in again on the same clientType overwrites that row (JwtService), which
            // makes every request from the older session fail here from that point on — this
            // is what actually signs the other session out, since a stateless JWT can't
            // otherwise be revoked before it expires on its own.
            OnTokenValidated = async context =>
            {
                var principal = context.Principal;
                var userIdValue = principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal?.FindFirstValue("userId");
                var sessionToken = principal?.FindFirstValue("sid");
                var clientTypeValue = principal?.FindFirstValue("clientType");

                if (!Guid.TryParse(userIdValue, out var userId)
                    || string.IsNullOrEmpty(sessionToken)
                    || !Enum.TryParse<ClientType>(clientTypeValue, out var clientType))
                {
                    // Tokens minted before this feature shipped carry none of these claims —
                    // fail closed rather than treat a missing session as still valid.
                    context.Fail("Phiên đăng nhập không hợp lệ.");
                    return;
                }

                var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var isActive = await userRepository.IsSessionActiveAsync(userId, clientType, sessionToken);
                if (!isActive)
                {
                    context.Fail("Tài khoản đã được đăng nhập ở một nơi khác.");
                }
            },
            OnChallenge = async context =>
            {
                // OnTokenValidated above sets this message specifically for a session
                // that's been superseded by a newer login — surfaced as its own
                // ErrorCode so the frontend can redirect to /login with a distinct
                // "signed in elsewhere" notice instead of the generic "please sign in".
                var revoked = context.AuthenticateFailure?.Message == "Tài khoản đã được đăng nhập ở một nơi khác.";

                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = revoked ? "Tài khoản đã được đăng nhập ở một nơi khác." : "Bạn cần đăng nhập để thực hiện thao tác này.",
                    StatusCode = 401,
                    ErrorCode = revoked ? ErrorCode.SESSION_REVOKED : ErrorCode.UNAUTHORIZED
                });
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Bạn không có quyền thực hiện thao tác này.",
                    StatusCode = 403,
                    ErrorCode = ErrorCode.UNAUTHORIZED
                });
            }
        };
    });

builder.Services.AddAppDbContext(builder.Configuration);
builder.Services.AddHttpContextAccessor();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Services.Configure<AzureStorageSettings>(
    builder.Configuration.GetSection(AzureStorageSettings.SectionName));

builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration["AZURE_STORAGE_CONNECTION_STRING"]
        ?? configuration[$"{AzureStorageSettings.SectionName}:ConnectionString"]
        ?? string.Empty;

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Azure Storage connection string is not configured. " +
            "Set AZURE_STORAGE_CONNECTION_STRING or use user secrets for AzureStorageSettings:ConnectionString.");
    }

    return new BlobServiceClient(connectionString);
});

builder.Services.InstallerServicesInAssemply(builder.Configuration);

// Settings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<OtpSettings>(
    builder.Configuration.GetSection("OtpSettings"));

var app = builder.Build();

// Registered first so it wraps every other middleware and can turn any
// unhandled exception (from auth, routing, controllers, ...) into the same
// ApiResponse<T> envelope the rest of the API returns.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Social WebAPI V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();