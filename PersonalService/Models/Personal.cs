using System.ComponentModel.DataAnnotations;

namespace PersonalService.Models;

public class Personal
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string Position { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int? MemberId { get; set; }
} 