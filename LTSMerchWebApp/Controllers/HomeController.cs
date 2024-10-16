using LTSMerchWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace LTSMerchWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LtsMerchStoreContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public HomeController(ILogger<HomeController> logger, LtsMerchStoreContext context)
        {
            _logger = logger;
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null)
                {
                    // Verifica el hash de la contraseña
                    var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                
                    if (result == PasswordVerificationResult.Success)
                    {
                        // Lógica de autenticación
                        HttpContext.Session.SetString("UserEmail", user.Email);
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            }

            return View(model);
        }
        public IActionResult Thanks()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
