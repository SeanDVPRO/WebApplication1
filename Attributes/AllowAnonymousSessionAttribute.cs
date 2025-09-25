using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.Attributes
{
    public class AllowAnonymousSessionAttribute : Attribute, IFilterMetadata
    {
    }
}