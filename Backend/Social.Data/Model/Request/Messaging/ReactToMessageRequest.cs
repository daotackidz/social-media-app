using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.Messaging
{
    public class ReactToMessageRequest
    {
        [Required]
        public string Emoji { get; set; } = string.Empty;
    }
}
