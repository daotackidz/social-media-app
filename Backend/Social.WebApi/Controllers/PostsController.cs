using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Common.Constants;
using Social.Data.Model.File;
using Social.Data.Model.Request.Post;
using Social.Data.Model.Response.Profile;
using Social.Repository.Social.Post.Interface;
using Social.Repository.Social.User.Interface;
using Social.Service.Social.File.Interface;
using Social.WebApi.Infrastructure.Services;
using PostEntity = Social.Data.Model.Post.Posts;
using PostFileEntity = Social.Data.Model.Post.PostFiles;

namespace Social.WebApi.Controllers
{
    /// <summary>Creating a post (upload media + caption) — the write side of what ProfileViewController's posts endpoint reads back.</summary>
    [Route("api/posts")]
    [ApiController]
    [Authorize]
    public class PostsController : BaseApiController
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFileService _fileService;

        public PostsController(
            IPostRepository postRepository,
            IUserRepository userRepository,
            IFileService fileService,
            ICurrentUserService currentUserService) : base(currentUserService)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
            _fileService = fileService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreatePostRequest request)
        {
            if (!IsCurrentUserAuthenticated || CurrentUserId is null)
            {
                return ApiUnauthorized();
            }

            if (request.Files is null || request.Files.Count == 0)
            {
                return ApiBadRequest("Cần chọn ít nhất một ảnh hoặc video.", ErrorCode.FILE_REQUIRED);
            }

            foreach (var file in request.Files)
            {
                var contentType = file.ContentType ?? string.Empty;
                if (!contentType.StartsWith("image/") && !contentType.StartsWith("video/"))
                {
                    return ApiBadRequest("Chỉ hỗ trợ tệp ảnh hoặc video.", ErrorCode.VALIDATION_ERROR);
                }
            }

            var userId = CurrentUserId.Value;

            var post = new PostEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Caption = request.Caption?.Trim() ?? string.Empty,
                Content = request.Caption?.Trim() ?? string.Empty,
                Privacy = PostEntity.PostPrivacy.Public,
                IsAiGenerated = request.IsAiGenerated,
                CreatedByUserId = userId
            };

            var postFiles = new List<PostFileEntity>();
            string? coverUrl = null;

            for (var i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];
                var isVideo = (file.ContentType ?? string.Empty).StartsWith("video/");

                var url = await _fileService.UploadFileAsync(file, "postfiles");
                if (i == 0) coverUrl = url;

                var fileMeta = new Files
                {
                    Id = Guid.NewGuid(),
                    FileName = Path.GetFileName(url),
                    FileNameOrigin = file.FileName,
                    FileExtension = Path.GetExtension(file.FileName),
                    FileType = isVideo ? (ushort)2 : (ushort)1,
                    FileSize = file.Length,
                    StoragePath = url,
                    StorageType = 1,
                    IsPublic = true,
                    FileVersion = false,
                    CreatedByUserId = userId
                };
                await _userRepository.AddFileAsync(fileMeta);

                var altText = i < request.AltTexts.Count ? request.AltTexts[i]?.Trim() : null;

                postFiles.Add(new PostFileEntity
                {
                    Id = Guid.NewGuid(),
                    PostId = post.Id,
                    FileId = fileMeta.Id,
                    FileType = isVideo ? PostFileEntity.PostFileType.Video : PostFileEntity.PostFileType.Image,
                    IsPrimary = i == 0,
                    AltText = string.IsNullOrWhiteSpace(altText) ? null : altText,
                    CreatedByUserId = userId
                });
            }

            await _postRepository.CreateAsync(post, postFiles);

            var type = postFiles.Count > 1
                ? "carousel"
                : postFiles[0].FileType == PostFileEntity.PostFileType.Video ? "video" : "image";

            return ApiCreated(new ProfilePostResponse
            {
                Id = post.Id,
                Type = type,
                CoverUrl = coverUrl,
                LikeCount = 0,
                CommentCount = 0,
                IsAiGenerated = post.IsAiGenerated
            }, "Đăng bài viết thành công.");
        }
    }
}
