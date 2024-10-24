using Microsoft.AspNetCore.Mvc;

namespace LTSMerchWebApp.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
