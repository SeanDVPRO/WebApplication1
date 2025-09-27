using System;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface IUrlShorteningService
    {
        Task<string> CreateShortenedUrlAsync(string originalUrl, int expirationMinutes = 60);

        Task<string?> GetOriginalUrlAsync(string shortCode);

        Task MarkAsUsedAsync(string shortCode);

        Task CleanupExpiredUrlsAsync();
    }
}