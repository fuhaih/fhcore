using Microsoft.AspNetCore.Mvc;

namespace FHCore.MVC.Controllers
{
    public class ResourceController:Controller
    {
        public IActionResult Images()
        {
            return View();
        }
    }
}