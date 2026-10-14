using Microsoft.AspNetCore.Mvc;

namespace MVC_Tiraz.Areas.Admin
{
    public class DashBoardController : Controller
    {
        public IActionResult DashBoard()
        {
            return View();
        }
    }
}
