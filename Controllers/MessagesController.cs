using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPortfolioApi.data;
using WebPortfolioApi.Dtos;
using WebPortfolioApi.Models;

namespace WebPortfolioApi.Controllers;

[ApiController]
[Route("/api/[controller]")]

public class MessagesController(WebfolioDbContext context) : ControllerBase
{
    private readonly WebfolioDbContext _context = context;

    [HttpGet]
    public async Task<IActionResult> getAllMessages()
    {
        List<Message> messages = await _context.Messages.ToListAsync();
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> addNewMessage([FromBody] MessageDto messageDto)
    {
        Message message = new()
        {
            Id = new Guid(),
            Name = messageDto.Name,
            EmailAddress = messageDto.EmailAddress,
            Content = messageDto.Content
        };

        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();

        return Ok(messageDto);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> deleteMessage([FromRoute] Guid id)
    {
        Message? message = await _context.Messages.FindAsync(id);

        if (message == null)
        {
            return NotFound();
        }

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return Ok();
    }
}