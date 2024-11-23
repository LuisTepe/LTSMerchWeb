using LTSMerchWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LTSMerchWebApp.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly LtsMerchStoreContext _context;

        public NotificationsController(LtsMerchStoreContext context)
        {
            _context = context;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            int? currentUserId = HttpContext.Session.GetInt32("UserId");

            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // Recupera el usuario actual
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);

            if (user == null)
            {
                return NotFound();
            }

            // Pasa el usuario al ViewBag
            ViewBag.User = user;

            // Recupera las notificaciones para el usuario actual
            var notifications = await _context.Notifications
                .Where(n => n.UserId == currentUserId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }



        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .AsNoTracking() // Mejora el rendimiento
                .FirstOrDefaultAsync(m => m.NotificationId == id);

            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/MarkAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            _context.Update(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // DELETE: Notifications/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.NotificationId == id);
        }
    }
}
