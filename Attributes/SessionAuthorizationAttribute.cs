using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.Attributes
{
    public class SessionAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(m => m is AllowAnonymousSessionAttribute);
            
            if (allowAnonymous)
            {
                base.OnActionExecuting(context);
                return;
            }
            
            var user = context.HttpContext.User;
            var session = context.HttpContext.Session;
            
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
            
            var sessionId = session.Id;
            if (string.IsNullOrEmpty(sessionId))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
            
            session.SetString("LastActivity", DateTime.UtcNow.ToString());
            
            base.OnActionExecuting(context);
        }
    }
}