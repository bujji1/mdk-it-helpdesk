using MdkHelpdesk.Application.DTOs;
using MdkHelpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MdkHelpdesk.Web.Controllers;

/// <summary>
/// REST API endpoints for managing IT helpdesk support tickets.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>
    /// Lists all support tickets, most recently updated first.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TicketResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TicketResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var tickets = await _ticketService.GetTicketsAsync(cancellationToken);
        return Ok(tickets);
    }

    /// <summary>
    /// Retrieves a single support ticket by its unique ID.
    /// </summary>
    /// <param name="id">Ticket ID.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetByIdAsync(id, cancellationToken);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    /// <summary>
    /// Creates a new support ticket in the Open state.
    /// </summary>
    /// <param name="request">Ticket creation payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketResponse>> Create(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _ticketService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing support ticket's title, description and status.
    /// </summary>
    /// <param name="id">Ticket ID.</param>
    /// <param name="request">Updated ticket payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> Update(
        int id,
        [FromBody] UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _ticketService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Assigns a support ticket to a helpdesk user, or unassigns it.
    /// </summary>
    /// <param name="id">Ticket ID.</param>
    /// <param name="request">Assignment payload (a null AssignedUserId unassigns the ticket).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPatch("{id:int}/assign")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> Assign(
        int id,
        [FromBody] AssignTicketRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _ticketService.AssignAsync(id, request.AssignedUserId, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}
