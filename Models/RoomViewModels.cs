using System.ComponentModel.DataAnnotations;

namespace AdvancedChat.Web.Models;

public class RoomListItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int MemberCount { get; set; }
}

public class UserListItemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

public class AdvancedChatViewModel
{
    public IReadOnlyList<RoomListItemViewModel> Rooms { get; set; } = [];

    public IReadOnlyList<UserListItemViewModel> Users { get; set; } = [];

    public string? CurrentUserEmail { get; set; }
}

public class RoomDetailsViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public IReadOnlyList<string> Members { get; set; } = [];

    public IReadOnlyList<ChatMessageViewModel> Messages { get; set; } = [];
}

public class ChatMessageViewModel
{
    public string UserName { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }
}

public class CreateRoomViewModel
{
    [Required]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(240)]
    public string? Description { get; set; }
}

public class AddUserToRoomViewModel
{
    public int RoomId { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
