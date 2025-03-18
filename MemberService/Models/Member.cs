using System.ComponentModel.DataAnnotations;

namespace MemberService.Models;

public class Member
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int? ClassId { get; set; }
} 