namespace WebApplication1.Models
{
    public class AuditLogViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
