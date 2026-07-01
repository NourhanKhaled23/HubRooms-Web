using AdvancedChat.Web.Data;
using AdvancedChat.Web.Models;
using AdvancedChat.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvancedChat.Web.Controllers;

[Authorize]
public class RoomsController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ChatRoomService _roomService;
    private readonly UserManager<IdentityUser> _userManager;

    public RoomsController(
        ApplicationDbContext dbContext,
        ChatRoomService roomService,
        UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _roomService = roomService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var rooms = await _roomService.GetRoomsForUserAsync(userId);
        var users = await _userManager.Users
            .AsNoTracking()
            .Where(user => user.Id != userId)
            .OrderBy(user => user.Email)
            .Select(user => new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "Unknown"
            })
            .ToListAsync();

        return View(new AdvancedChatViewModel
        {
            Rooms = rooms,
            Users = users,
            CurrentUserEmail = User.Identity?.Name
        });
    }

    public IActionResult Create()
    {
        return View(new CreateRoomViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoomViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var room = await _roomService.CreateRoomAsync(model, userId);
        return RedirectToAction(nameof(Details), new { id = room.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreate(string name)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["RoomMessage"] = "Enter a room name.";
            return RedirectToAction(nameof(Index));
        }

        await _roomService.CreateRoomAsync(new CreateRoomViewModel { Name = name }, userId);
        TempData["RoomMessage"] = "Room created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int roomId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var result = await _roomService.DeleteRoomAsync(roomId, userId);
        TempData["RoomMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        if (!await _roomService.UserCanAccessRoomAsync(id, userId))
        {
            return Forbid();
        }

        var room = await _dbContext.ChatRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(chatRoom => chatRoom.Id == id);

        if (room is null)
        {
            return NotFound();
        }

        var members = await _dbContext.ChatRoomMembers
            .AsNoTracking()
            .Where(member => member.ChatRoomId == id)
            .Select(member => member.User.Email ?? member.User.UserName ?? "Unknown")
            .OrderBy(email => email)
            .ToListAsync();

        var messages = await _dbContext.ChatMessages
            .AsNoTracking()
            .Where(message => message.ChatRoomId == id)
            .OrderByDescending(message => message.SentAt)
            .Take(50)
            .Select(message => new ChatMessageViewModel
            {
                UserName = message.User.UserName ?? message.User.Email ?? "Unknown",
                Text = message.Text,
                SentAt = message.SentAt
            })
            .ToListAsync();

        var model = new RoomDetailsViewModel
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            Members = members,
            Messages = messages
                .OrderBy(message => message.SentAt)
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUser(AddUserToRoomViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        if (!await _roomService.UserCanAccessRoomAsync(model.RoomId, userId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            TempData["RoomMessage"] = "Enter a valid email address.";
            return RedirectToAction(nameof(Details), new { id = model.RoomId });
        }

        var result = await _roomService.AddUserToRoomAsync(model.RoomId, model.Email);
        TempData["RoomMessage"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = model.RoomId });
    }
}
