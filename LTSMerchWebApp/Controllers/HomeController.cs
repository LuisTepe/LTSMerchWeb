using LTSMerchWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace LTSMerchWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LtsMerchStoreContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly int _adminId = 1;
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
        public IActionResult Login(LoginViewModel model, string action)
        {
            if (action == "Login")
            {
                if (ModelState.IsValid)
                {
                    var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                    if (user != null &&
                        _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Success)
                    {
                        HttpContext.Session.SetString("UserEmail", user.Email);
                        HttpContext.Session.SetInt32("UserId", user.UserId);
                        HttpContext.Session.SetInt32("RoleTypeId", (int)user.RoleTypeId);

                        TempData["SuccessMessage"] = "Inicio de sesion exitoso.";
                        if (user.RoleTypeId == _adminId)
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        return RedirectToAction("ProductsList", "Products");
                    }
                }
                TempData["ErrorMessage"] = "Correo o contrasena incorrectos.";
            }
            else if (action == "Register")
            {
                if (ModelState.IsValid)
                {
                    if (_context.Users.Any(u => u.Email == model.Email))
                    {
                        TempData["ErrorMessage"] = "Ya existe una cuenta con ese correo electronico.";
                    }
                    else
                    {
                        var newUser = new User
                        {
                            Name = model.FirstName + " " + model.LastName,
                            Email = model.Email,
                            PasswordHash = _passwordHasher.HashPassword(null, model.Password),
                            RoleTypeId = 2
                        };
                        _context.Users.Add(newUser);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "Cuenta creada correctamente.";
                        return RedirectToAction("Login", "Home");
                    }
                }
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("ProductsList", "Products");
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
