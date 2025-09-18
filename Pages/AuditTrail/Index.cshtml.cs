using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Pages.AuditTrail
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public List<WebApplication1.Models.AuditTrail> AuditLogs { get; set; } = new();
        public List<string> Users { get; set; } = new();
        public List<string> Types { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedUser { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync(int page = 1, int pageSize = 10)
        {
            CurrentPage = page;
            PageSize = pageSize;

            Users = await _context.AuditTrails
                .Select(a => a.UserId)
                .Distinct()
                .OrderBy(u => u)
                .ToListAsync();

            Types = await _context.AuditTrails
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            var query = _context.AuditTrails.AsQueryable();

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
