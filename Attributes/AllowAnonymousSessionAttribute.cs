using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.Attributes
{
    /// <summary>
    /// Attribute to allow anonymous access and bypass session authorization
    /// </summary>
    public class AllowAnonymousSessionAttribute : Attribute, IFilterMetadata
    {
    }
}