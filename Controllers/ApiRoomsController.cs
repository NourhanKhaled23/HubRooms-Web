using AdvancedChat.Web.Models;
using AdvancedChat.Web.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedChat.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/rooms")]
public class ApiRoomsController : ControllerBase
{
    private readonly ChatRoomService _roomService;
    private readonly UserManager<IdentityUser> _userManager;

    public ApiRoomsController(ChatRoomService roomService, UserManager<IdentityUser> userManager)
    {
        _roomService = roomService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var rooms = await _roomService.GetRoomsForUserAsync(userId);
        return Ok(rooms);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom(CreateRoomViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var room = await _roomService.CreateRoomAsync(model, userId);
        return Ok(new { room.Id, room.Name, room.Description });
    }

    [HttpPost("{roomId:int}/users")]
    public async Task<IActionResult> AddUser(int roomId, AddUserRequest request)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        if (!await _roomService.UserCanAccessRoomAsync(roomId, userId))
        {
            return Forbid();
        }

        var result = await _roomService.AddUserToRoomAsync(roomId, request.Email);
        return result.Success ? Ok(new { result.Message }) : BadRequest(new { result.Message });
    }
}

public class AddUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
