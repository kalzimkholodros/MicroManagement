using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberService.Data;
using MemberService.Models;
using MemberService.Services;

namespace MemberService.Controllers;

[ApiController]
[Route("[controller]")]
public class MemberController : ControllerBase
{
    private readonly MemberDbContext _context;
    private readonly RabbitMQService _rabbitMQService;

    public MemberController(MemberDbContext context, RabbitMQService rabbitMQService)
    {
        _context = context;
        _rabbitMQService = rabbitMQService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
    {
        return await _context.Members.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
        {
            return NotFound();
        }
        return member;
    }

    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember(Member member)
    {
        member.CreatedAt = DateTime.UtcNow;
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        // RabbitMQ üzerinden yeni üye oluşturulduğunu bildir
        _rabbitMQService.PublishMessage("member.created", member);

        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, member);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(int id, Member member)
    {
        if (id != member.Id)
        {
            return BadRequest();
        }

        _context.Entry(member).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // RabbitMQ üzerinden üye güncellendiğini bildir
        _rabbitMQService.PublishMessage("member.updated", member);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
        {
            return NotFound();
        }

        _context.Members.Remove(member);
        await _context.SaveChangesAsync();

        // RabbitMQ üzerinden üye silindiğini bildir
        _rabbitMQService.PublishMessage("member.deleted", member);

        return NoContent();
    }
} 