using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using WebApplication1.Services;

namespace UsersApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly AuditService _auditService;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, AuditService auditService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _auditService = auditService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    await _auditService.LogAsync(
                        action: "User Login",
                        description: $"User '{model.Email}' logged in."
                    );
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["LoginError"] = "Email or password is incorrect.";
                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Users users = new Users
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                };

                var result = await userManager.CreateAsync(users, model.Password);

                if (result.Succeeded)
                {
                    TempData["RegisterSuccess"] = true;
                    return View();
                }
                else
                {
                    TempData["RegisterError"] = string.Join(" ", result.Errors.Select(e => e.Description));
                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Something is wrong!");
                    return View(model);
                }
                else
                {
                    return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
                }
            }
            return View(model);
        }

        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }
            return View(new ChangePasswordViewModel { Email = username });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    var result = await userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddPasswordAsync(user, model.NewPassword);
                        TempData["PasswordChanged"] = true;
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        TempData["PasswordError"] = string.Join(" ", result.Errors.Select(e => e.Description));
                        return View(model);
                    }
                }
                else
                {
                    TempData["PasswordError"] = "Email not found!";
                    return View(model);
                }
            }

            TempData["PasswordError"] = "Something went wrong. Try again.";
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            var userId = User?.Identity?.Name ?? "Unknown";
            await _auditService.LogAsync(
                action: "User Logout",
                description: $"User '{userId}' logged out."
            );
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
