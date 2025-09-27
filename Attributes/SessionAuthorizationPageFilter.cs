using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.Attributes
{
    public class SessionAuthorizationPageFilter : IPageFilter
    {
        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var user = context.HttpContext.User;
            var session = context.HttpContext.Session;
            
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                session.Clear();
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
            
            var sessionId = session.Id;
            if (string.IsNullOrEmpty(sessionId))
            {
                context.Result = new RedirectToActionResult("Logout", "Account", null);
                return;
            }
            
            var lastActivityString = session.GetString("LastActivity");
            if (!string.IsNullOrEmpty(lastActivityString))
            {
                if (DateTime.TryParse(lastActivityString, out DateTime lastActivity))
                {
                    var timeoutMinutes = 30;
                    if (DateTime.UtcNow.Subtract(lastActivity).TotalMinutes > timeoutMinutes)
                    {
                        session.Clear();
                        context.Result = new RedirectToActionResult("Logout", "Account", null);
                        return;
                    }
                }
            }
            
            var userId = user.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                session.Clear();
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
            
            var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                session.Clear();
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
            
            session.SetString("LastActivity", DateTime.UtcNow.ToString());
            session.SetString("UserId", userId);
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }
    }
}