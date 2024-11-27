using LTSMerchWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using LTSMerchWebApp.Services;

namespace LTSMerchWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LtsMerchStoreContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly EmailService _emailService;
        private readonly int _adminId = 1;
        public HomeController(ILogger<HomeController> logger, LtsMerchStoreContext context, EmailService emailService)
        {
            _logger = logger;
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _emailService = emailService;
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
        public async Task<IActionResult> LoginAsync(LoginViewModel model, string action)
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

                        if (user.RoleTypeId == _adminId)
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        return RedirectToAction("ProductsList", "Products");
                    }
                }
                TempData["ErrorMessage"] = "Correo o contrasena incorrectos.";
            }
            if (action == "Register")
            {
                if (ModelState.IsValid)
                {
                    if (_context.Users.Any(u => u.Email == model.Email))
                    {
                        TempData["ErrorMessage"] = "Ya existe una cuenta con ese correo electrónico.";
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

                        // Generar token de confirmación
                        var token = Guid.NewGuid().ToString();
                        var emailConfirmation = new EmailConfirmation
                        {
                            UserId = newUser.UserId,
                            Token = token,
                            IsConfirmed = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.EmailConfirmations.Add(emailConfirmation);
                        _context.SaveChanges();

                        // Enviar correo de confirmación
                        var confirmationLink = Url.Action("ConfirmEmail", "Home", new { token }, Request.Scheme);
                        var emailBody = $"<p>Gracias por registrarte en LTS Merch Store.</p><p>Haz clic en el siguiente enlace para confirmar tu cuenta:</p><a href='{confirmationLink}'>Confirmar cuenta</a>";

                        await _emailService.SendEmailAsync(newUser.Email, "Confirmación de cuenta", emailBody);

                        TempData["SuccessMessage"] = "Cuenta creada correctamente. Revisa tu correo para confirmar tu cuenta.";
                        return RedirectToAction("Login", "Home");
                    }
                }
            }
            return View(model);
        }

        public IActionResult ConfirmEmail(string token)
        {
            var emailConfirmation = _context.EmailConfirmations.FirstOrDefault(ec => ec.Token == token);

            if (emailConfirmation == null || emailConfirmation.IsConfirmed == true)
            {
                TempData["ErrorMessage"] = "El enlace de confirmación no es válido o ya fue usado.";
                return RedirectToAction("Login");
            }

            // Confirmar el correo
            emailConfirmation.IsConfirmed = true;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Correo confirmado correctamente. Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
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

        [HttpPost]
        public IActionResult ClearTempData()
        {
            TempData.Clear();
            return Ok();
        }
    }
}
