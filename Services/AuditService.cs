using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;
using Newtonsoft.Json;

namespace WebApplication1.Services
{
    public class AuditService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string? oldValue = null, string? newValue = null, string? description = null)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
            var audit = new AuditTrail
            {
                UserId = userId,
                Action = action,
                OldValue = oldValue,
                NewValue = newValue,
                Description = description,
                Timestamp = DateTime.UtcNow
            };
            _context.AuditTrails.Add(audit);
            await _context.SaveChangesAsync();
        }
        public async Task CreateBookAsync(Book book)
        {
            await LogAsync(
                action: "Create Book",
                newValue: JsonConvert.SerializeObject(book),
                description: $"Book '{book.Title}' created."
            );
        }
    }
}