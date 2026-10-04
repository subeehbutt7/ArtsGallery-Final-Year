using Microsoft.AspNetCore.Mvc;

namespace ArtGalleryFinal.Controllers
{
    public class HomeController : Controller
    {
        // Main Home Page
        public IActionResult Index()
        {
            return View();
        }

        // About Us Page
        public IActionResult AboutUs()
        {
            return View();
        }
    }
}
