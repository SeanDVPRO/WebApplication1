using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ContactController : Controller
    {

        private readonly string email = "contact@beststore.com";
        private readonly string address = "New York, USA";
        public IActionResult Index()
        {
            ViewData["EmailAddress"] = email;
            ViewBag.Address = address;

            return View();
        }


        [HttpPost]
        public IActionResult Index(ContactDto model)
        {
            ViewData["EmailAddress"] = email;
            ViewBag.Address = address;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //store the contact data in the database
            ViewBag.SuccessMessage = "Your Message is received successfuly!";
            ModelState.Clear();
            return View();
        }
    }
}
