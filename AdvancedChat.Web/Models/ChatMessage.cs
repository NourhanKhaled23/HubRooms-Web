using Microsoft.AspNetCore.Identity;

namespace AdvancedChat.Web.Models;

public class ChatMessage
{
    public int Id { get; set; }

    public int ChatRoomId { get; set; }

    public ChatRoom ChatRoom { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;

    public IdentityUser User { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
