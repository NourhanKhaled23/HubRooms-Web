using Microsoft.AspNetCore.Identity;

namespace AdvancedChat.Web.Models;

public class ChatRoomMember
{
    public int Id { get; set; }

    public int ChatRoomId { get; set; }

    public ChatRoom ChatRoom { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;

    public IdentityUser User { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
