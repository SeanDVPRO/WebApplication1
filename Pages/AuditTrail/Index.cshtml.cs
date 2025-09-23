using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Pages.AuditTrail
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public List<AuditLogViewModel> AuditLogs { get; set; } = new();

        public List<AuditUserOption> Users { get; set; } = new();

        public List<string> Types { get; set; } = new();

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

        public bool IsGuest => !User.Identity.IsAuthenticated;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync(int page = 1, int pageSize = 10)
        {
            CurrentPage = page;
            PageSize = pageSize;

            Users = await (
                from a in _context.AuditTrails
                join u in _context.Users on a.UserId equals u.Id
                select new AuditUserOption
                {
                    UserId = u.Id,
                    FullName = u.FullName
                }
            )
            .Distinct()
            .OrderBy(u => u.FullName)
            .ToListAsync();

            Types = await _context.AuditTrails
                .Select(a => a.Action)
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
                            UserId = a.UserId,
                            FullName = u.FullName,
                            Action = a.Action,
                            Description = a.Description,
                            OldValue = a.OldValue,
                            NewValue = a.NewValue,
                            Timestamp = a.Timestamp
                        };

            if (StartDate.HasValue)
                query = query.Where(a => a.Timestamp >= StartDate.Value);

            if (EndDate.HasValue)
                query = query.Where(a => a.Timestamp <= EndDate.Value);

            if (!string.IsNullOrEmpty(SelectedUser))
                query = query.Where(a => a.UserId == SelectedUser);

            if (!string.IsNullOrEmpty(SelectedType))
                query = query.Where(a => a.Action == SelectedType);

            if (!string.IsNullOrEmpty(SearchTerm))
                query = query.Where(a => a.Description.Contains(SearchTerm));

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            AuditLogs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
