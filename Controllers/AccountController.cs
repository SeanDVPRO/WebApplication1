using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using WebApplication1.Services;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using WebApplication1.Attributes;
using System.Text.Json;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> _signInManager;
        private readonly UserManager<Users> _userManager;
        private readonly AuditService _auditService;
        private readonly IEmailService _emailService;

        private static readonly TimeSpan _resetInterval = TimeSpan.FromMinutes(5);
        private static readonly int _maxRequestsPerHour = 1;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, AuditService auditService, IEmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditService = auditService;
            _emailService = emailService;
        }

        [AllowAnonymousSession]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymousSession]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email ?? "");

                if (user == null)
                {
                    TempData["LoginError"] = "This email is not registered. Please create an account first.";
                    TempData["UnregisteredEmail"] = model.Email;
                    return View(model);
                }

                if (!user.IsEmailVerified)
                {
                    TempData["LoginError"] = "Please verify your email address before logging in.";
                    TempData["UnverifiedEmail"] = user.Email;
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email ?? "", model.Password ?? "", model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    await _auditService.LogAsync(
                        action: "User Login",
                        description: $"User '{user.Email}' logged in successfully."
                    );

                    TempData["LoginSuccess"] = $"Welcome back, {user.FullName}! You have successfully logged in.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    await _auditService.LogAsync(
                        action: "Failed Login Attempt",
                        description: $"Failed login attempt for user '{user.Email}' - incorrect password."
                    );

                    TempData["LoginError"] = "Incorrect password. Please try again.";
                    return View(model);
                }
            }
            return View(model);
        }

        [AllowAnonymousSession]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymousSession]
        public IActionResult EmailVerificationPending()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymousSession]
        public async Task<IActionResult> VerifyEmail(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["VerificationError"] = "Invalid verification link.";
                return RedirectToAction("EmailVerificationError");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["VerificationError"] = "User not found.";
                return RedirectToAction("EmailVerificationError");
            }

            if (user.IsEmailVerified)
            {
                TempData["VerificationMessage"] = "Email is already verified.";
                return RedirectToAction("EmailVerificationSuccess");
            }

            if (user.EmailVerificationToken != token)
            {
                TempData["VerificationError"] = "Invalid verification token.";
                return RedirectToAction("EmailVerificationError");
            }

            if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            {
                TempData["VerificationError"] = "Verification token has expired. Please request a new verification email.";
                return RedirectToAction("EmailVerificationError");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _auditService.LogAsync(
                    action: "Email Verification",
                    description: $"User '{user.Email}' verified their email address."
                );

                TempData["VerificationSuccess"] = true;
                return RedirectToAction("EmailVerificationSuccess");
            }
            else
            {
                TempData["VerificationError"] = "Failed to verify email. Please try again.";
                return RedirectToAction("EmailVerificationError");
            }
        }

        [AllowAnonymousSession]
        public IActionResult EmailVerificationSuccess()
        {
            return View();
        }

        [AllowAnonymousSession]
        public IActionResult EmailVerificationError()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymousSession]
        public async Task<IActionResult> ResendVerificationEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ResendError"] = "Email address is required.";
                return RedirectToAction("EmailVerificationPending");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["ResendError"] = "User not found.";
                return RedirectToAction("EmailVerificationPending");
            }

            if (user.IsEmailVerified)
            {
                TempData["ResendMessage"] = "Email is already verified.";
                return RedirectToAction("EmailVerificationPending");
            }

            user.EmailVerificationToken = Guid.NewGuid().ToString();
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var verificationLink = Url.Action("VerifyEmail", "Account",
                    new { email = user.Email, token = user.EmailVerificationToken },
                    Request.Scheme);

                await _emailService.SendEmailVerificationAsync(user.Email!, verificationLink!, user.FullName);

                TempData["ResendSuccess"] = "Verification email has been resent.";
            }
            else
            {
                TempData["ResendError"] = "Failed to resend verification email. Please try again.";
            }

            return RedirectToAction("EmailVerificationPending");
        }

        [HttpPost]
        [AllowAnonymousSession]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var users = new Users
                {
                    FullName = model.Name ?? "",
                    Email = model.Email ?? "",
                    UserName = model.Email ?? "",
                    IsEmailVerified = false,
                    EmailVerificationToken = Guid.NewGuid().ToString(),
                    EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
                };

                var result = await _userManager.CreateAsync(users, model.Password ?? "");

                if (result.Succeeded)
                {
                    var verificationLink = Url.Action("VerifyEmail", "Account",
                        new { email = users.Email, token = users.EmailVerificationToken },
                        Request.Scheme);

                    await _emailService.SendEmailVerificationAsync(users.Email!, verificationLink!, users.FullName);

                    await _auditService.LogAsync(
                        action: "User Registration",
                        description: $"User '{users.Email}' registered and verification email sent."
                    );

                    TempData["RegisterSuccess"] = true;
                    TempData["VerificationEmailSent"] = true;
                    TempData["UserEmail"] = users.Email;
                    return View(model);
                }
                else
                {
                    TempData["RegisterError"] = string.Join("||", result.Errors.Select(e => e.Description));
                    return View(model);
                }
            }
            return View(model);
        }

        [AllowAnonymousSession]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymousSession]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var rateLimitResult = await CheckRateLimitAsync(model.Email!);
                if (!rateLimitResult.Allowed)
                {
                    TempData["EmailError"] = rateLimitResult.Message;
                    return View(model);
                }

                var user = await _userManager.FindByEmailAsync(model.Email ?? "");

                if (user == null)
                {
                    await TrackRateLimitAttemptAsync(model.Email!);

                    TempData["EmailError"] = "This email address is not registered. Please check your email or create an account first.";
                    return View(model);
                }

                try
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                    var resetLink = Url.Action("ChangePassword", "Account",
                        new { email = user.Email, token = encodedToken },
                        Request.Scheme);

                    await _emailService.SendPasswordResetEmailAsync(user.Email!, resetLink!, user.FullName ?? user.UserName!);

                    await TrackRateLimitAttemptAsync(model.Email!);

                    await _auditService.LogAsync(
                        action: "Password Reset Request",
                        description: $"Password reset email sent to '{user.Email}'"
                    );

                    TempData["EmailSent"] = true;
                    return View(model);
                }
                catch (Exception ex)
                {
                    await _auditService.LogAsync(
                        action: "Password Reset Email Failed",
                        description: $"Failed to send password reset email to '{model.Email}'. Error: {ex.Message}"
                    );

                    TempData["EmailSent"] = true;
                    return View(model);
                }
            }
            return View(model);
        }

        [AllowAnonymousSession]
        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymousSession]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email ?? "");

                if (user == null)
                {
                    TempData["VerificationError"] = "User not found.";
                    return RedirectToAction("EmailVerificationError");
                }

                if (user.IsEmailVerified)
                {
                    TempData["VerificationMessage"] = "Email is already verified.";
                    return RedirectToAction("EmailVerificationSuccess");
                }

                user.EmailVerificationToken = Guid.NewGuid().ToString();
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var verificationLink = Url.Action("VerifyEmail", "Account",
                        new { email = user.Email, token = user.EmailVerificationToken },
                        Request.Scheme);

                    await _emailService.SendEmailVerificationAsync(user.Email!, verificationLink!, user.FullName);

                    await _auditService.LogAsync(
                        action: "Email Verification Resent",
                        description: $"Email verification resent to '{user.Email}'"
                    );

                    TempData["VerificationEmailSent"] = true;
                    return RedirectToAction("EmailVerificationPending");
                }
                else
                {
                    TempData["VerificationError"] = "Failed to resend verification email. Please try again.";
                    return RedirectToAction("EmailVerificationError");
                }
            }
            return View(model);
        }

        [AllowAnonymousSession]
        public IActionResult ChangePassword(string? email, string? token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("ForgotPassword", "Account");
            }

            return View(new ChangePasswordViewModel
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost]
        [AllowAnonymousSession]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email ?? "");
                if (user == null)
                {
                    TempData["PasswordError"] = "Invalid request. Please try again.";
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.Token))
                {
                    TempData["PasswordError"] = "Invalid reset token. Please request a new password reset.";
                    return View(model);
                }

                try
                {
                    var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

                    var passwordHasher = new PasswordHasher<Users>();
                    var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, model.NewPassword ?? "");

                    if (verificationResult == PasswordVerificationResult.Success)
                    {
                        TempData["PasswordError"] = "New password cannot be the same as your current password. Please choose a different password.";
                        return View(model);
                    }

                    var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword ?? "");

                    if (result.Succeeded)
                    {
                        await _auditService.LogAsync(
                            action: "Password Reset Completed",
                            description: $"Password successfully reset for user '{user.Email}'"
                        );

                        TempData["PasswordChanged"] = true;
                        return View(model);
                    }
                    else
                    {
                        TempData["PasswordError"] = string.Join("||", result.Errors.Select(e => e.Description));
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    await _auditService.LogAsync(
                        action: "Password Reset Failed",
                        description: $"Password reset failed for user '{model.Email}'. Error: {ex.Message}"
                    );

                    TempData["PasswordError"] = "Invalid or expired reset token. Please request a new password reset.";
                    return View(model);
                }
            }

            TempData["PasswordError"] = "Please correct the errors and try again.";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = User?.Identity?.Name ?? "Unknown";

            await _auditService.LogAsync(
                action: "User Logout",
                description: $"User '{userId}' logged out."
            );

            HttpContext.Session.Clear();
            
            if (HttpContext.Session.IsAvailable)
            {
                await HttpContext.Session.CommitAsync();
            }

            await _signInManager.SignOutAsync();

            foreach (var cookie in Request.Cookies.Keys)
            {
                if (cookie.StartsWith(".AspNetCore") || 
                    cookie.Contains("Identity") || 
                    cookie.Contains("Auth") ||
                    cookie.Contains("Session") ||
                    cookie.Contains("__RequestVerificationToken"))
                {
                    Response.Cookies.Delete(cookie, new CookieOptions
                    {
                        Path = "/",
                        Domain = Request.Host.Host,
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Lax
                    });
                }
            }

            HttpContext.User = new System.Security.Claims.ClaimsPrincipal();

            TempData["LogoutSuccess"] = true;
            
            return RedirectToAction("LoggedOut");
        }

        [AllowAnonymousSession]
        public IActionResult LoggedOut()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymousSession]
        public async Task<IActionResult> CheckEmailExists([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { exists = false, message = "Email is required" });
            }

            var user = await _userManager.FindByEmailAsync(email);
            bool exists = user != null;

            return Json(new { exists = exists, message = exists ? "This email is already registered" : "Email is available" });
        }

        private async Task<(bool Allowed, string Message)> CheckRateLimitAsync(string email)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "rate_limits.json");
            var directory = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            if (!System.IO.File.Exists(filePath))
                return (true, string.Empty);

            try
            {
                var json = await System.IO.File.ReadAllTextAsync(filePath);
                var rateLimits = JsonSerializer.Deserialize<Dictionary<string, List<DateTime>>>(json) ?? new();

                var cleanEmail = email.ToLowerInvariant();
                var now = DateTime.UtcNow;

                if (rateLimits.ContainsKey(cleanEmail))
                {
                    var attempts = rateLimits[cleanEmail];

                    var recentAttempts = attempts.Where(a => (now - a) < TimeSpan.FromHours(1)).ToList();

                    if (recentAttempts.Count >= _maxRequestsPerHour)
                    {
                        var oldestAttempt = recentAttempts.Min();
                        var waitTime = TimeSpan.FromHours(1) - (now - oldestAttempt);
                        return (false, $"Too many password reset attempts. Please try again in {waitTime.Minutes} minutes.");
                    }

                    if (recentAttempts.Any())
                    {
                        var lastAttempt = recentAttempts.Max();
                        if ((now - lastAttempt) < _resetInterval)
                        {
                            var waitTime = _resetInterval - (now - lastAttempt);
                            return (false, $"Please wait {waitTime.TotalSeconds:0} seconds before requesting another password reset.");
                        }
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception)
            {
                return (true, string.Empty);
            }
        }

        private async Task TrackRateLimitAttemptAsync(string email)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "rate_limits.json");
            var directory = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            Dictionary<string, List<DateTime>> rateLimits = new();

            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    var json = await System.IO.File.ReadAllTextAsync(filePath);
                    rateLimits = JsonSerializer.Deserialize<Dictionary<string, List<DateTime>>>(json) ?? new();
                }
                catch (Exception)
                {
                    rateLimits = new();
                }
            }

            var cleanEmail = email.ToLowerInvariant();
            var now = DateTime.UtcNow;

            if (!rateLimits.ContainsKey(cleanEmail))
                rateLimits[cleanEmail] = new List<DateTime>();

            rateLimits[cleanEmail].Add(now);

            rateLimits[cleanEmail] = rateLimits[cleanEmail]
                .Where(a => (now - a) < TimeSpan.FromHours(2))
                .ToList();

            try
            {
                var jsonOutput = JsonSerializer.Serialize(rateLimits, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(filePath, jsonOutput);
            }
            catch (Exception)
            {
            }
        }
    }
}