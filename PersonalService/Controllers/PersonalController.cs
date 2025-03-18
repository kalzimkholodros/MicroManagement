using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalService.Data;
using PersonalService.Models;
using PersonalService.Services;

namespace PersonalService.Controllers;

[ApiController]
[Route("[controller]")]
public class PersonalController : ControllerBase
{
    private readonly PersonalDbContext _context;
    private readonly RabbitMQService _rabbitMQService;

    public PersonalController(PersonalDbContext context, RabbitMQService rabbitMQService)
    {
        _context = context;
        _rabbitMQService = rabbitMQService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Personal>>> GetPersonals()
    {
        return await _context.Personals.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Personal>> GetPersonal(int id)
    {
        var personal = await _context.Personals.FindAsync(id);
        if (personal == null)
        {
            return NotFound();
        }
        return personal;
    }

    [HttpPost]
    public async Task<ActionResult<Personal>> CreatePersonal(Personal personal)
    {
        personal.CreatedAt = DateTime.UtcNow;
        _context.Personals.Add(personal);
        await _context.SaveChangesAsync();

        // RabbitMQ üzerinden yeni personel oluşturulduğunu bildir
        _rabbitMQService.PublishMessage("personal.created", personal);

        return CreatedAtAction(nameof(GetPersonal), new { id = personal.Id }, personal);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePersonal(int id, Personal personal)
    {
        if (id != personal.Id)
        {
            return BadRequest();
        }

        _context.Entry(personal).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // RabbitMQ üzerinden personel güncellendiğini bildir
        _rabbitMQService.PublishMessage("personal.updated", personal);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePersonal(int id)
    {
        var personal = await _context.Personals.FindAsync(id);
        if (personal == null)
        {
            return NotFound();
        }

        _context.Personals.Remove(personal);
        await _context.SaveChangesAsync();

        // RabbitMQ üzerinden personel silindiğini bildir
        _rabbitMQService.PublishMessage("personal.deleted", personal);

        return NoContent();
    }
} 