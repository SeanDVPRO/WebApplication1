using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Attributes;

namespace WebApplication1.Controllers
{
    [SessionAuthorization]
    public class ContactController : Controller
    {
        private readonly string email = "contact@beststore.com";
        private readonly string address = "New York, Usa";

        public IActionResult Index()
        {
            ViewData["Email Address"] = email;
            ViewBag.Address = address;
            return View(new ContactDto());
        }

        [HttpPost]
        public IActionResult Index(ContactDto model)
        {
            ViewData["Email Address"] = email;
            ViewBag.Address = address;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["ContactSuccess"] = "Thank you for your message! We will get back to you soon.";
            return View(new ContactDto());
        }
    }
}
