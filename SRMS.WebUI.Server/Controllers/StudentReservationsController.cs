using Microsoft.AspNetCore.Mvc;
using SRMS.Application.Reservations.Interfaces;
using SRMS.Application.Reservations.DTOs;
using Microsoft.AspNetCore.Authorization;


namespace SRMS.WebUI.Server.Controllers;

[ApiController]
[Route("api/student/[controller]")]
[Authorize(Roles = "Student")] // Assuming there's a Student role
public class StudentReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public StudentReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet("residences")]
    public async Task<ActionResult<IEnumerable<ResidenceDto>>> GetAvailableResidences()
    {
        var residences = await _reservationService.GetAvailableResidencesAsync();
        return Ok(residences);
    }

    [HttpGet("residences/{residenceId}/vacant-rooms")]
    public async Task<ActionResult<IEnumerable<RoomAvailabilityDto>>> GetVacantRoomsByResidence(Guid residenceId)
    {
        var rooms = await _reservationService.GetVacantRoomsByResidenceAsync(residenceId);
        return Ok(rooms);
    }

    [HttpPost("reserve-room")]
    public async Task<ActionResult<ReserveRoomResponse>> ReserveRoom([FromBody] ReserveRoomRequest request)
    {
        // For demonstration, we assume the StudentId is passed in the request body.
        // In a real application, you would get the student ID from the authenticated user context.
        if (request.StudentId == Guid.Empty)
        {
            // Placeholder: In a real app, retrieve student ID from ClaimsPrincipal
            // For now, if not provided, use a dummy or throw error.
            return BadRequest("StudentId is required for reservation.");
        }

        try
        {
            var response = await _reservationService.ReserveRoomAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
