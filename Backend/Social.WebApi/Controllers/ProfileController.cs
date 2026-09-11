using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Common.Constants;
using Social.Data.Model.File;
using Social.Data.Model.Request.User;
using Social.Data.Model.Response.User;
using Social.Data.Model.User;
using Social.Repository.Social.User.Interface;
using Social.Service.Social.File.Interface;
using Social.WebApi.Infrastructure.Services;

namespace Social.WebApi.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class ProfileController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileService _fileService;

        public ProfileController(
            IUserRepository userRepository,
            IFileService fileService,
            ICurrentUserService currentUserService) : base(currentUserService)
        {
            _userRepository = userRepository;
            _fileService = fileService;
        }

        [HttpPut("update-profile")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileRequest request)
        {
            try
            {
                if (!IsCurrentUserAuthenticated)
                {
                    return ApiUnauthorized();
                }

                var email = CurrentUserEmail;

                if (string.IsNullOrWhiteSpace(email))
                {
                    return ApiUnauthorized();
                }

                var user = await _userRepository.GetByEmailAsync(email);
                if (user is null)
                {
                    return ApiNotFound("Không tìm thấy tài khoản.", ErrorCode.USER_NOT_FOUND);
                }

                var profile = await _userRepository.GetProfileByUserIdAsync(user.Id);
                if (profile is null)
                {
                    profile = new UserProfiles
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        FullName = user.UserName,
                        CreatedByUserId = user.Id,
                    };
                    await _userRepository.AddProfileAsync(profile);
                }

                if (!string.IsNullOrWhiteSpace(request.FullName))
                {
                    profile.FullName = request.FullName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(request.FirstName))
                {
                    profile.FirstName = request.FirstName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(request.LastName))
                {
                    profile.LastName = request.LastName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(request.Address))
                {
                    profile.Address = request.Address.Trim();
                }

                if (request.DateOfBirth.HasValue)
                {
                    profile.DateOfBirth = request.DateOfBirth.Value;
                }

                if (request.Gender.HasValue)
                {
                    profile.Gender = request.Gender.Value;
                }

                await _userRepository.UpdateProfileAsync(profile);

                string? avatarUrl;
                if (request.AvatarFile is not null && request.AvatarFile.Length > 0)
                {
                    avatarUrl = await _fileService.UploadFileAsync(request.AvatarFile, "useravatars");
                    await _userRepository.UnsetPrimaryAvatarAsync(user.Id);

                    var fileMeta = new Files
                    {
                        Id = Guid.NewGuid(),
                        FileName = Path.GetFileName(avatarUrl),
                        FileNameOrigin = request.AvatarFile.FileName,
                        FileExtension = Path.GetExtension(request.AvatarFile.FileName),
                        FileType = 1,
                        FileSize = request.AvatarFile.Length,
                        StoragePath = avatarUrl,
                        StorageType = 1,
                        IsPublic = true,
                        FileVersion = false,
                        CreatedByUserId = CurrentUserId ?? Guid.Empty
                    };

                    await _userRepository.AddFileAsync(fileMeta);
                    await _userRepository.AddUserFileAsync(new UserFiles
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        FileId = fileMeta.Id,
                        FileType = UserFiles.UserFileType.Avatar,
                        IsPrimary = true,
                        CreatedByUserId = CurrentUserId ?? Guid.Empty
                    });
                }
                else
                {
                    var primaryAvatar = await _userRepository.GetPrimaryAvatarAsync(user.Id);
                    avatarUrl = primaryAvatar?.Files?.StoragePath;
                }

                return ApiOk(new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = profile.FullName,
                    DateOfBirth = profile.DateOfBirth,
                    CreatedDate = user.CreatedDate,
                    AvatarUrl = avatarUrl
                }, "Cập nhật hồ sơ thành công.");
            }

            catch (RequestFailedException ex)
            {
                Console.WriteLine($"[Azure Error] Status: {ex.Status}, Code: {ex.ErrorCode}, Message: {ex.Message}");
                throw;
            }
        }
    }
}
