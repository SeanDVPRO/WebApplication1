using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
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

            ViewBag.SuccessMessage = "Your message has been sent successfully!";
            ModelState.Clear();
            return View(new ContactDto());
        }
    }
}
