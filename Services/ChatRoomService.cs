using AdvancedChat.Web.Data;
using AdvancedChat.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AdvancedChat.Web.Services;

public class ChatRoomService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;

    public ChatRoomService(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<RoomListItemViewModel>> GetRoomsForUserAsync(string userId)
    {
        return await _dbContext.ChatRoomMembers
            .Where(member => member.UserId == userId)
            .Select(member => new RoomListItemViewModel
            {
                Id = member.ChatRoom.Id,
                Name = member.ChatRoom.Name,
                Description = member.ChatRoom.Description,
                MemberCount = member.ChatRoom.Members.Count
            })
            .OrderBy(room => room.Name)
            .ToListAsync();
    }

    public async Task<ChatRoom> CreateRoomAsync(CreateRoomViewModel model, string ownerId)
    {
        var room = new ChatRoom
        {
            Name = model.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            OwnerId = ownerId
        };

        room.Members.Add(new ChatRoomMember { UserId = ownerId });
        _dbContext.ChatRooms.Add(room);
        await _dbContext.SaveChangesAsync();

        return room;
    }

    public async Task<(bool Success, string Message)> DeleteRoomAsync(int roomId, string userId)
    {
        var room = await _dbContext.ChatRooms
            .FirstOrDefaultAsync(chatRoom => chatRoom.Id == roomId);

        if (room is null)
        {
            return (false, "Room was not found.");
        }

        if (room.OwnerId != userId)
        {
            return (false, "Only the room owner can delete this room.");
        }

        _dbContext.ChatRooms.Remove(room);
        await _dbContext.SaveChangesAsync();
        return (true, "Room deleted.");
    }

    public async Task<bool> UserCanAccessRoomAsync(int roomId, string userId)
    {
        return await _dbContext.ChatRoomMembers
            .AnyAsync(member => member.ChatRoomId == roomId && member.UserId == userId);
    }

    public async Task<(bool Success, string Message)> AddUserToRoomAsync(int roomId, string email)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            return (false, "No registered user was found with that email.");
        }

        var alreadyMember = await _dbContext.ChatRoomMembers
            .AnyAsync(member => member.ChatRoomId == roomId && member.UserId == user.Id);
        if (alreadyMember)
        {
            return (false, "That user is already in the room.");
        }

        _dbContext.ChatRoomMembers.Add(new ChatRoomMember
        {
            ChatRoomId = roomId,
            UserId = user.Id
        });

        await _dbContext.SaveChangesAsync();
        return (true, "User added to the room.");
    }

    public async Task<ChatMessage> SaveMessageAsync(int roomId, string userId, string text)
    {
        var message = new ChatMessage
        {
            ChatRoomId = roomId,
            UserId = userId,
            Text = text
        };
        _dbContext.ChatMessages.Add(message);
        await _dbContext.SaveChangesAsync();
        return message;
    }

    public async Task<IReadOnlyList<object>> GetRecentMessagesAsync(int roomId)
    {
        var items = await _dbContext.ChatMessages
            .AsNoTracking()
            .Where(m => m.ChatRoomId == roomId)
            .OrderByDescending(m => m.SentAt)
            .Take(50)
            .Select(m => new
            {
                roomId,
                userName = m.User.UserName ?? m.User.Email ?? "Unknown",
                text = m.Text,
                sentAt = m.SentAt
            })
            .ToListAsync();

        return items.OrderBy(i => i.sentAt).ToList();
    }
}
