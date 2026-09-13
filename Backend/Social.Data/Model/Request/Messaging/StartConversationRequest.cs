using System.ComponentModel.DataAnnotations;

namespace Social.Data.Model.Request.Messaging
{
    public class StartConversationRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
    }
}
