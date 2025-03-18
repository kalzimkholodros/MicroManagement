using Microsoft.EntityFrameworkCore;
using PersonalService.Models;

namespace PersonalService.Data;

public class PersonalDbContext : DbContext
{
    public PersonalDbContext(DbContextOptions<PersonalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Personal> Personals { get; set; }
} 