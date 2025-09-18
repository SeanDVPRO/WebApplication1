using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public BooksController(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Books.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    action: "Create Book",
                    newValue: JsonConvert.SerializeObject(book),
                    description: $"Book '{book.Title}' created."
                );

                TempData["BookCreated"] = true;
                return RedirectToAction("Index");
            }
            return View(book);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Book book)
        {
            if (ModelState.IsValid)
            {
                var oldBook = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == book.Id);

                _context.Books.Update(book);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    action: "Edit Book",
                    oldValue: JsonConvert.SerializeObject(oldBook),
                    newValue: JsonConvert.SerializeObject(book),
                    description: $"Book '{book.Title}' updated."
                );

                TempData["BookUpdated"] = true;
                return RedirectToAction("Index");
            }
            return View(book);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "Delete Book",
                oldValue: JsonConvert.SerializeObject(book),
                description: $"Book '{book.Title}' deleted."
            );

            TempData["BookDeleted"] = true;
            return RedirectToAction("Index");
        }
    }
}
