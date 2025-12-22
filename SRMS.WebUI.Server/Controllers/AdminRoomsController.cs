using Microsoft.AspNetCore.Mvc;
using SRMS.Application.Rooms.Interfaces;
using SRMS.Application.Rooms.DTOs;
using SRMS.Application.SystemSettings.Interfaces;
using SRMS.Application.SystemSettings.DTOs;
using SRMS.Domain.Rooms.Enums;
using Microsoft.AspNetCore.Authorization;


namespace SRMS.WebUI.Server.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")] // Assuming there's an Admin role
public class AdminRoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IFeesConfigurationService _feesConfigurationService;

    public AdminRoomsController(IRoomService roomService, IFeesConfigurationService feesConfigurationService)
    {
        _roomService = roomService;
        _feesConfigurationService = feesConfigurationService;
    }

    // Room Management Endpoints
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetAllRooms()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomDto>> GetRoomById(Guid id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return Ok(room);
    }

    [HttpPost]
    public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomDto dto)
    {
        try
        {
            var room = await _roomService.CreateRoomAsync(dto);
            return CreatedAtAction(nameof(GetRoomById), new { id = room.Id }, room);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoomDto>> UpdateRoom(Guid id, [FromBody] UpdateRoomDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest("Room ID mismatch.");
        }
        try
        {
            var room = await _roomService.UpdateRoomAsync(dto);
            return Ok(room);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRoom(Guid id)
    {
        try
        {
            var result = await _roomService.DeleteRoomAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    // Fees Configuration Endpoints
    [HttpGet("fees-configurations")]
    public async Task<ActionResult<IEnumerable<FeesConfigurationDto>>> GetAllFeesConfigurations()
    {
        var fees = await _feesConfigurationService.GetAllFeesConfigurationsAsync();
        return Ok(fees);
    }

    [HttpGet("fees-configurations/{id}")]
    public async Task<ActionResult<FeesConfigurationDto>> GetFeesConfigurationById(Guid id)
    {
        var fees = await _feesConfigurationService.GetFeesConfigurationByIdAsync(id);
        if (fees == null)
        {
            return NotFound();
        }
        return Ok(fees);
    }

    [HttpPost("fees-configurations")]
    public async Task<ActionResult<FeesConfigurationDto>> CreateFeesConfiguration([FromBody] CreateFeesConfigurationDto dto)
    {
        try
        {
            var fees = await _feesConfigurationService.CreateFeesConfigurationAsync(dto);
            return CreatedAtAction(nameof(GetFeesConfigurationById), new { id = fees.Id }, fees);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("fees-configurations/{id}")]
    public async Task<ActionResult<FeesConfigurationDto>> UpdateFeesConfiguration(Guid id, [FromBody] UpdateFeesConfigurationDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest("Fees Configuration ID mismatch.");
        }
        try
        {
            var fees = await _feesConfigurationService.UpdateFeesConfigurationAsync(dto);
            return Ok(fees);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("fees-configurations/{id}")]
    public async Task<ActionResult> DeleteFeesConfiguration(Guid id)
    {
        try
        {
            var result = await _feesConfigurationService.DeleteFeesConfigurationAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}