using System.ComponentModel.DataAnnotations;

namespace ClassService.Models;

public class MemberClass
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; }
} 