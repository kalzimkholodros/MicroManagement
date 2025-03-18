using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models;

public class Payment
{
    public int Id { get; set; }

    [Required]
    public int MemberId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }
} 