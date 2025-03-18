using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassService.Data;
using ClassService.Services;
using ClassService.Models;

namespace ClassService.Controllers;

[ApiController]
[Route("[controller]")]
public class ClassController : ControllerBase
{
    private readonly ClassDbContext _context;
    private readonly IRabbitMQService _rabbitMQService;

    public ClassController(ClassDbContext context, IRabbitMQService rabbitMQService)
    {
        _context = context;
        _rabbitMQService = rabbitMQService;
    }

    [HttpGet("member/{id}")]
    public async Task<IActionResult> GetMemberClass(int id)
    {
        var memberClass = await _context.MemberClasses
            .FirstOrDefaultAsync(m => m.Id == id);

        if (memberClass == null)
            return NotFound();

        return Ok(memberClass);
    }

    [HttpPost("member")]
    public async Task<IActionResult> CreateMemberClass([FromBody] MemberClass memberClass)
    {
        memberClass.CreatedAt = DateTime.UtcNow;
        _context.MemberClasses.Add(memberClass);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMemberClass), new { id = memberClass.Id }, memberClass);
    }
} 