using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Attributes;

namespace WebApplication1.Pages.AuditTrail
{
    [SessionAuthorization]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public List<AuditLogViewModel> AuditLogs { get; set; } = new List<AuditLogViewModel>();

        public List<AuditUserOption> Users { get; set; } = new List<AuditUserOption>();

        public List<string> Types { get; set; } = new List<string>();

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedUser { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string SelectedType { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public bool IsGuest => !(User.Identity?.IsAuthenticated ?? false);

        public IndexModel(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task OnGetAsync(int page = 1, int pageSize = 10)
        {
            CurrentPage = Math.Max(1, page);
            PageSize = Math.Clamp(pageSize, 1, 100);

            Users = await (
                from a in _context.AuditTrails
                join u in _context.Users on a.UserId equals u.Id
                where u.FullName != null
                select new AuditUserOption
                {
                    UserId = u.Id,
                    FullName = u.FullName ?? "Unknown User"
                }
            )
            .Distinct()
            .OrderBy(u => u.FullName)
            .ToListAsync();

            Types = await _context.AuditTrails
                .Where(a => a.Action != null)
                .Select(a => a.Action ?? "Unknown Action")
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            if (IsGuest)
            {
                AuditLogs = new List<AuditLogViewModel>();
                TotalPages = 1;
                return;
            }

            var query = from a in _context.AuditTrails
                        join u in _context.Users on a.UserId equals u.Id into userGroup
                        from u in userGroup.DefaultIfEmpty()
                        select new AuditLogViewModel
                        {
                            Id = a.Id,
                            UserId = a.UserId ?? string.Empty,
                            FullName = u.FullName ?? "Unknown User",
                            Action = a.Action ?? "Unknown Action",
                            Description = a.Description ?? string.Empty,
                            OldValue = a.OldValue,
                            NewValue = a.NewValue,
                            Timestamp = a.Timestamp
                        };

            if (StartDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                var endOfDay = EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.Timestamp <= endOfDay);
            }

            if (!string.IsNullOrEmpty(SelectedUser))
            {
                query = query.Where(a => a.UserId == SelectedUser);
            }

            if (!string.IsNullOrEmpty(SelectedType))
            {
                query = query.Where(a => a.Action == SelectedType);
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(a =>
                    (a.Description != null && a.Description.Contains(SearchTerm)) ||
                    (a.Action != null && a.Action.Contains(SearchTerm)) ||
                    (a.FullName != null && a.FullName.Contains(SearchTerm))
                );
            }

            var totalCount = await query.CountAsync();
            TotalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)PageSize) : 1;

            AuditLogs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }

    public class AuditLogViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class AuditUserOption
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}