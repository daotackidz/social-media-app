using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Social.WebApi.Infrastructure.Filters
{
    public class FormFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parameters = context.MethodInfo.GetParameters();
            if (!parameters.Any(p => p.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile)
                                     || p.ParameterType == typeof(IEnumerable<Microsoft.AspNetCore.Http.IFormFile>)
                                     || p.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile[])
                                     || p.ParameterType.GetProperties().Any(x => x.PropertyType == typeof(Microsoft.AspNetCore.Http.IFormFile))))
            {
                return;
            }

            operation.RequestBody ??= new OpenApiRequestBody();
            operation.RequestBody.Content.Clear();
            operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>(),
                    Required = new SortedSet<string>()
                }
            };

            foreach (var parameter in parameters)
            {
                if (parameter.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile))
                {
                    operation.RequestBody.Content["multipart/form-data"].Schema.Properties[parameter.Name!] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    };
                    operation.RequestBody.Content["multipart/form-data"].Schema.Required.Add(parameter.Name!);
                    continue;
                }

                foreach (var property in parameter.ParameterType.GetProperties())
                {
                    var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    if (propertyType == typeof(Microsoft.AspNetCore.Http.IFormFile))
                    {
                        operation.RequestBody.Content["multipart/form-data"].Schema.Properties[property.Name] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        };
                        operation.RequestBody.Content["multipart/form-data"].Schema.Required.Add(property.Name);
                        continue;
                    }

                    if (propertyType == typeof(string))
                    {
                        operation.RequestBody.Content["multipart/form-data"].Schema.Properties[property.Name] = new OpenApiSchema
                        {
                            Type = "string"
                        };
                        continue;
                    }

                    if (propertyType == typeof(bool))
                    {
                        operation.RequestBody.Content["multipart/form-data"].Schema.Properties[property.Name] = new OpenApiSchema
                        {
                            Type = "boolean"
                        };
                        continue;
                    }

                    if (propertyType == typeof(DateTime))
                    {
                        operation.RequestBody.Content["multipart/form-data"].Schema.Properties[property.Name] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "date"
                        };
                        continue;
                    }

                    if (propertyType == typeof(byte) || propertyType == typeof(sbyte) || propertyType == typeof(short)
                        || propertyType == typeof(ushort) || propertyType == typeof(int) || propertyType == typeof(uint)
                        || propertyType == typeof(long) || propertyType == typeof(ulong))
                    {
                        operation.RequestBody.Content["multipart/form-data"].Schema.Properties[property.Name] = new OpenApiSchema
                        {
                            Type = "integer"
                        };
                        continue;
                    }

                    if (propertyType == typeof(float) || propertyType == typeof(double) || propertyType == typeof(decimal))
                    {
                        operation.RequestBody.Content["multipart/form-data"].Schema.Properties[property.Name] = new OpenApiSchema
                        {
                            Type = "number"
                        };
                        continue;
                    }
                }
            }
        }
    }
}
