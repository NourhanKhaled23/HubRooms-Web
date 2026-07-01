using System.ComponentModel.DataAnnotations;

namespace AdvancedChat.Web.Models;

public class ChatRoom
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(240)]
    public string? Description { get; set; }

    public string OwnerId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
