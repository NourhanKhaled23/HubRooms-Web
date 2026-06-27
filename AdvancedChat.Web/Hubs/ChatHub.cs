using AdvancedChat.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace AdvancedChat.Web.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ChatRoomService _roomService;
    private readonly UserManager<IdentityUser> _userManager;

    public ChatHub(ChatRoomService roomService, UserManager<IdentityUser> userManager)
    {
        _roomService = roomService;
        _userManager = userManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userName = Context.User?.Identity?.Name ?? "A user";
        await Clients.Caller.SendAsync("SystemMessage", $"{userName} has opened a connection.");
        await base.OnConnectedAsync();
    }

    public async Task JoinRoom(int roomId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrWhiteSpace(userId) || !await _roomService.UserCanAccessRoomAsync(roomId, userId))
        {
            throw new HubException("You are not a member of this room.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(roomId));
    }

    public async Task SendMessage(int roomId, string message)
    {
        var userId = Context.UserIdentifier;
        var text = message.Trim();
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (!await _roomService.UserCanAccessRoomAsync(roomId, userId))
        {
            throw new HubException("You are not a member of this room.");
        }

        var savedMessage = await _roomService.SaveMessageAsync(roomId, userId, text);

        var userName = Context.User?.Identity?.Name ?? "Unknown";
        await Clients.Group(RoomGroup(roomId)).SendAsync("ReceiveMessage", new
        {
            roomId,
            userName,
            text,
            sentAt = savedMessage.SentAt
        });
    }

    public async Task SendPrivateMessage(string targetUserId, string message)
    {
        var senderId = Context.UserIdentifier;
        var text = message.Trim();
        if (string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(targetUserId) || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var targetUser = await _userManager.FindByIdAsync(targetUserId);
        if (targetUser is null)
        {
            throw new HubException("Selected user was not found.");
        }

        var senderName = Context.User?.Identity?.Name ?? "Unknown";
        var payload = new
        {
            fromUserId = senderId,
            toUserId = targetUserId,
            userName = senderName,
            text,
            sentAt = DateTime.UtcNow
        };

        await Clients.User(targetUserId).SendAsync("ReceivePrivateMessage", payload);
        await Clients.Caller.SendAsync("ReceivePrivateMessage", payload);
    }

    public async Task<IReadOnlyList<object>> GetRecentMessages(int roomId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrWhiteSpace(userId) || !await _roomService.UserCanAccessRoomAsync(roomId, userId))
        {
            throw new HubException("You are not a member of this room.");
        }

        return await _roomService.GetRecentMessagesAsync(roomId);
    }

    private static string RoomGroup(int roomId) => $"room:{roomId}";
}