using Microsoft.EntityFrameworkCore;
using ClassService.Models;

namespace ClassService.Data;

public class ClassDbContext : DbContext
{
    public ClassDbContext(DbContextOptions<ClassDbContext> options)
        : base(options)
    {
    }

    public DbSet<MemberClass> MemberClasses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MemberClass>()
            .Property(m => m.Name)
            .IsRequired();
    }
} 