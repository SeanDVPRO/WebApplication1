using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ContactDto
    {
        [Required(ErrorMessage = "The First Name is required")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "The Last Name is required")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "The Email is required")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "The Message is required")]
        public string Message { get; set; } = "";

    }
}
