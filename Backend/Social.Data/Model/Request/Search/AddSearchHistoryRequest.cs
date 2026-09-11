using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.Search
{
    public record AddSearchHistoryRequest(
        [Required] Guid TargetUserId
    );
}
