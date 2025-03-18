using Microsoft.EntityFrameworkCore;
using MemberService.Models;

namespace MemberService.Data;

public class MemberDbContext : DbContext
{
    public MemberDbContext(DbContextOptions<MemberDbContext> options)
        : base(options)
    {
    }

    public DbSet<Member> Members { get; set; }
} 