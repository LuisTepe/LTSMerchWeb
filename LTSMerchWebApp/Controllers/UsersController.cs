using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LTSMerchWebApp.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace LTSMerchWebApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly LtsMerchStoreContext _context;

        public UsersController(LtsMerchStoreContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            ViewData["HideHeaderFooter"] = true;
            var ltsMerchStoreContext = _context.Users.Include(u => u.RoleType);
            return View(await ltsMerchStoreContext.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.RoleType)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            // If the request is an AJAX call, return the partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsPartial", user);
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["RoleTypeId"] = new SelectList(_context.RoleTypes, "RoleTypeId", "RoleTypeId");

            // If the request is an AJAX call, return the partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial");
            }

            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Name,Email,PasswordHash,PhoneNumber,Address,RoleTypeId,CreatedAt")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();

                // If it's an AJAX call, you can send a JSON response or just close the panel via JavaScript.
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleTypeId"] = new SelectList(_context.RoleTypes, "RoleTypeId", "RoleTypeId", user.RoleTypeId);

            // If it's an AJAX call, return the partial view with validation errors
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", user);
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewData["RoleTypeId"] = new SelectList(_context.RoleTypes, "RoleTypeId", "RoleTypeId", user.RoleTypeId);

            // If the request is an AJAX call, return the partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", user);
            }

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Name,Email,PasswordHash,PhoneNumber,Address,RoleTypeId,CreatedAt")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // If it's an AJAX call, you can send a JSON response or just close the panel via JavaScript.
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["RoleTypeId"] = new SelectList(_context.RoleTypes, "RoleTypeId", "RoleTypeId", user.RoleTypeId);

            // If it's an AJAX call, return the partial view with validation errors
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EditPartial", user);
            }

            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.RoleType)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            // If the request is an AJAX call, return the partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DeletePartial", user);
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Busca el usuario por ID
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Busca el EmailConfirmation relacionado
                var emailConfirmation = await _context.EmailConfirmations
                    .FirstOrDefaultAsync(ec => ec.UserId == user.UserId);

                // Si existe, elimina el EmailConfirmation
                if (emailConfirmation != null)
                {
                    _context.EmailConfirmations.Remove(emailConfirmation);
                }

                // Elimina el usuario
                _context.Users.Remove(user);

                // Guarda los cambios
                await _context.SaveChangesAsync();
            }

            // Si es una llamada AJAX, retorna una respuesta JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            // Redirige al índice
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/UserInfo
        public async Task<IActionResult> UserInfo(int? id)
        {
            // If no ID is provided, get it from the session
            if (id == null)
            {
                id = HttpContext.Session.GetInt32("UserId");

                // If still null, redirect to login
                if (id == null)
                {
                    return RedirectToAction("Login", "Home");
                }
            }

            var user = await _context.Users
                .Include(u => u.RoleType)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        public async Task<IActionResult> _PersonalDataPartial(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return PartialView("_PersonalDataPartial", user);
        }



        [HttpPost]
        public async Task<IActionResult> UpdatePersonalData(User updatedUser)
        {
            if (updatedUser == null || updatedUser.UserId == 0)
            {
                return BadRequest();
            }

            // Fetch the user from the database
            var user = await _context.Users.FindAsync(updatedUser.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            user.Name = updatedUser.Name;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.BirthDate = updatedUser.BirthDate;

            // Save changes to the database
            _context.Update(user);
            await _context.SaveChangesAsync();

            // Redirect to the UserInfo action to refresh the data
            return RedirectToAction("UserInfo", new { id = user.UserId });
        }



        public IActionResult _PasswordPartial()
        {
            return PartialView("_PasswordPartial");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Las contraseñas no coinciden.";
                return PartialView("_PasswordPartial");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado. Por favor, inicia sesión nuevamente.";
                return PartialView("_PasswordPartial");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado en la base de datos.";
                return PartialView("_PasswordPartial");
            }

            // Initialize the PasswordHasher
            var passwordHasher = new PasswordHasher<LTSMerchWebApp.Models.User>();

            // Verify the current password
            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (verificationResult != PasswordVerificationResult.Success)
            {
                TempData["ErrorMessage"] = "La contraseña actual no es correcta.";
                return PartialView("_PasswordPartial");
            }

            // Hash the new password
            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);

            // Save the updated user object in the database
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "¡Contraseña cambiada exitosamente!";
            return PartialView("_PasswordPartial");
        }



        private string HashPassword(string password, out string saltString)
        {
            // Generate a new salt
            byte[] salt = new byte[16]; // 128-bit salt
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Convert salt to Base64 for storage
            saltString = Convert.ToBase64String(salt);

            // Hash the password with PBKDF2
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32)); // 256-bit hash

            // Combine the salt and hash into a single string for storage
            return $"{saltString}.{hashed}";
        }


        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            // Split the stored hash into salt and hash components
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;

            // Extract the salt and hash
            var salt = Convert.FromBase64String(parts[0]);
            var storedHashedPassword = parts[1];

            // Hash the input password using the extracted salt
            string hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: inputPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32)); // 256-bit hash

            // Compare the hashes
            return storedHashedPassword == hashedInput;
        }




        public IActionResult _EmailPartial()
        {
            return PartialView("_EmailPartial");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeEmail(string currentPassword, string newEmail, string confirmEmail)
        {
            if (newEmail != confirmEmail)
            {
                TempData["ErrorMessage"] = "Los correos electrónicos no coinciden.";
                return RedirectToAction("UserInfo", new { id = HttpContext.Session.GetInt32("UserId") });
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado. Por favor, inicia sesión nuevamente.";
                return RedirectToAction("UserInfo", new { id = userId });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado en la base de datos.";
                return RedirectToAction("UserInfo", new { id = userId });
            }

            var passwordHasher = new PasswordHasher<LTSMerchWebApp.Models.User>();
            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);

            if (verificationResult != PasswordVerificationResult.Success)
            {
                TempData["ErrorMessage"] = "La contraseña actual no es correcta.";
                return RedirectToAction("UserInfo", new { id = userId });
            }

            user.Email = newEmail;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "¡Correo electrónico actualizado exitosamente!";
            return RedirectToAction("UserInfo", new { id = userId });
        }



        public IActionResult EditAddress(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            user.States = new List<string>
            {
                "Aguascalientes","Baja California","Baja California Sur","Campeche","Chiapas","Chihuahua","Ciudad de México","Coahuila","Colima","Durango","Estado de México","Guanajuato","Guerrero","Hidalgo","Jalisco","Michoacán","Morelos","Nayarit","Nuevo León","Oaxaca","Puebla","Querétaro","Quintana Roo","San Luis Potosí","Sinaloa","Sonora","Tabasco","Tamaulipas","Tlaxcala","Veracruz","Yucatán","Zacatecas"            
            };

            return PartialView("_AddressPartial", user);
        }


        public IActionResult _DeleteAccountPartial()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado. Por favor, inicia sesión nuevamente.";
                return RedirectToAction("Login", "Home");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado en la base de datos.";
                return RedirectToAction("UserInfo", new { id = userId });
            }

            return PartialView("_DeleteAccountPartial", user);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int? userId)
        {
            if (userId == null || userId <= 0)
            {
                TempData["ErrorMessage"] = "Usuario inválido.";
                return RedirectToAction("UserInfo", new { id = HttpContext.Session.GetInt32("UserId") });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado en la base de datos.";
                return RedirectToAction("UserInfo", new { id = userId });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cuenta eliminada exitosamente.";
            return RedirectToAction("Login", "Home");
        }



        [HttpPost]
        public async Task<IActionResult> UpdateAddress(User updatedUser)
        {
            if (updatedUser == null || updatedUser.UserId == 0)
            {
                return BadRequest();
            }

            // Fetch the user from the database
            var user = await _context.Users.FindAsync(updatedUser.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            user.StreetAddress = updatedUser.StreetAddress;
            user.Neighborhood = updatedUser.Neighborhood;
            user.City = updatedUser.City;
            user.State = updatedUser.State;
            user.PostalCode = updatedUser.PostalCode;

            // Save changes to the database
            _context.Update(user);
            await _context.SaveChangesAsync();

            // Redirect to the UserInfo action to refresh the data
            return RedirectToAction("UserInfo", new { id = user.UserId });
        }

    }
}
