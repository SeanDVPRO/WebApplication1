using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class UrlShorteningService : IUrlShorteningService
    {
        private readonly AppDbContext _context;
        private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public UrlShorteningService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateShortenedUrlAsync(string originalUrl, int expirationMinutes = 60)
        {
            var shortCode = GenerateShortCode();
            
            while (await _context.ShortenedUrls.AnyAsync(u => u.ShortCode == shortCode))
            {
                shortCode = GenerateShortCode();
            }

            var shortenedUrl = new ShortenedUrl
            {
                ShortCode = shortCode,
                OriginalUrl = originalUrl,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Purpose = "password_reset"
            };

            _context.ShortenedUrls.Add(shortenedUrl);
            await _context.SaveChangesAsync();

            return shortCode;
        }

        public async Task<string?> GetOriginalUrlAsync(string shortCode)
        {
            var shortenedUrl = await _context.ShortenedUrls
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            if (shortenedUrl == null || !shortenedUrl.IsValid)
            {
                return null;
            }

            return shortenedUrl.OriginalUrl;
        }

        public async Task MarkAsUsedAsync(string shortCode)
        {
            var shortenedUrl = await _context.ShortenedUrls
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            if (shortenedUrl != null)
            {
                shortenedUrl.IsUsed = true;
                shortenedUrl.UsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task CleanupExpiredUrlsAsync()
        {
            var expiredUrls = await _context.ShortenedUrls
                .Where(u => u.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            _context.ShortenedUrls.RemoveRange(expiredUrls);
            await _context.SaveChangesAsync();
        }

        private string GenerateShortCode()
        {
            const int length = 8;
            var result = new StringBuilder(length);
            
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes);
                
                for (int i = 0; i < length; i++)
                {
                    result.Append(Characters[bytes[i] % Characters.Length]);
                }
            }
            
            return result.ToString();
        }
    }
}