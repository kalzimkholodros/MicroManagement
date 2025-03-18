using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly PaymentDbContext _context;
    private readonly RedisService _redisService;
    private readonly RabbitMQService _rabbitMQService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        PaymentDbContext context,
        RedisService redisService,
        RabbitMQService rabbitMQService,
        ILogger<PaymentController> logger)
    {
        _context = context;
        _redisService = redisService;
        _rabbitMQService = rabbitMQService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Payment>> GetPayment(int id)
    {
        // Önce Redis'ten kontrol et
        var cacheKey = $"payment:{id}";
        var cachedPayment = await _redisService.GetAsync<Payment>(cacheKey);

        if (cachedPayment != null)
        {
            return cachedPayment;
        }

        // Redis'te yoksa PostgreSQL'den al
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        // PostgreSQL'den aldığımız veriyi Redis'e kaydet (5 dakika)
        await _redisService.SetAsync(cacheKey, payment, TimeSpan.FromMinutes(5));

        return payment;
    }

    [HttpPost]
    public async Task<ActionResult<Payment>> CreatePayment(Payment payment)
    {
        payment.CreatedAt = DateTime.UtcNow;
        payment.Status = "Pending";

        // PostgreSQL'e kaydet
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Redis'e kaydet (5 dakika)
        var cacheKey = $"payment:{payment.Id}";
        await _redisService.SetAsync(cacheKey, payment, TimeSpan.FromMinutes(5));

        // RabbitMQ'ya bildir
        _rabbitMQService.PublishMessage("payment.created", payment);

        return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(int id, Payment payment)
    {
        if (id != payment.Id)
        {
            return BadRequest();
        }

        // PostgreSQL'den mevcut ödemeyi al
        var existingPayment = await _context.Payments.FindAsync(id);
        if (existingPayment == null)
        {
            return NotFound();
        }

        payment.CreatedAt = existingPayment.CreatedAt;
        if (payment.Status == "Completed" && existingPayment.Status != "Completed")
        {
            payment.PaidAt = DateTime.UtcNow;
        }

        // PostgreSQL'i güncelle
        _context.Entry(existingPayment).CurrentValues.SetValues(payment);
        await _context.SaveChangesAsync();

        // Redis'i güncelle (5 dakika)
        var cacheKey = $"payment:{id}";
        await _redisService.SetAsync(cacheKey, payment, TimeSpan.FromMinutes(5));

        // RabbitMQ'ya bildir
        _rabbitMQService.PublishMessage("payment.updated", payment);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        // PostgreSQL'den sil
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();

        // Redis'ten sil
        var cacheKey = $"payment:{id}";
        await _redisService.RemoveAsync(cacheKey);

        // RabbitMQ'ya bildir
        _rabbitMQService.PublishMessage("payment.deleted", payment);

        return NoContent();
    }
} 