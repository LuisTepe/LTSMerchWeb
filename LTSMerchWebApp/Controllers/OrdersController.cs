using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LTSMerchWebApp.Models;
using LTSMerchWebApp.Services;

namespace LTSMerchWebApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly LtsMerchStoreContext _context;

        public OrdersController(LtsMerchStoreContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            ViewData["HideHeaderFooter"] = true;
            var ltsMerchStoreContext = _context.Orders.Include(o => o.StatusType).Include(o => o.User);
            return View(await ltsMerchStoreContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["StatusTypeId"] = new SelectList(_context.OrderStatusTypes, "StatusTypeId", "StatusTypeId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return PartialView("_CreatePartial");
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,UserId,Total,ShippingAddress,StatusTypeId,CreatedAt")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StatusTypeId"] = new SelectList(_context.OrderStatusTypes, "StatusTypeId", "StatusTypeId", order.StatusTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return PartialView("_CreatePartial", order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["StatusTypeId"] = new SelectList(_context.OrderStatusTypes, "StatusTypeId", "StatusName", order.StatusTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", order.UserId);
            return PartialView("_EditPartial", order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,UserId,Total,ShippingAddress,StatusTypeId,CreatedAt")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalOrder = await _context.Orders
                        .Include(o => o.User) // Incluir información del usuario
                        .FirstOrDefaultAsync(o => o.OrderId == id);

                    if (originalOrder == null)
                    {
                        return NotFound();
                    }

                    // Guardar el valor original del estado antes de la actualización
                    var originalStatusTypeId = originalOrder.StatusTypeId;

                    Console.WriteLine($"Estado original: {originalStatusTypeId}, Nuevo estado: {order.StatusTypeId}");

                    // Actualizar el pedido
                    _context.Entry(originalOrder).CurrentValues.SetValues(order);
                    await _context.SaveChangesAsync();

                    // Comparar el estado original con el nuevo estado
                    if (originalStatusTypeId != order.StatusTypeId)
                    {
                        var newStatus = await _context.OrderStatusTypes
                            .FirstOrDefaultAsync(s => s.StatusTypeId == order.StatusTypeId);

                        if (newStatus != null && !string.IsNullOrEmpty(originalOrder.User?.Email))
                        {
                            var emailService = new EmailService();
                            string subject = "Actualización del estado de tu pedido";
                            string body = $@"
                    <h1>Hola {originalOrder.User.Name ?? "Cliente"}</h1>
                    <p>Tu pedido con el número <strong>{order.OrderId}</strong> ha cambiado de estado.</p>
                    <p>Estado actual: <strong>{newStatus.StatusName}</strong></p>
                    <p>Gracias por comprar en LTS Merch Store.</p>";

                            Console.WriteLine($"Enviando correo a: {originalOrder.User.Email}");
                            await emailService.SendEmailAsync(originalOrder.User.Email, subject, body);
                            Console.WriteLine("Correo enviado correctamente.");
                        }
                        else
                        {
                            Console.WriteLine("No se puede enviar el correo. Usuario o estado inválido.");
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["StatusTypeId"] = new SelectList(_context.OrderStatusTypes, "StatusTypeId", "Name", order.StatusTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Name", order.UserId);
            return PartialView("_EditPartial", order);
        }




        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.StatusType)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return PartialView("_DeletePartial", order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments) // Include Payments to ensure they are loaded
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order != null)
            {
                // Remove associated OrderDetails
                if (order.OrderDetails != null)
                {
                    _context.OrderDetails.RemoveRange(order.OrderDetails);
                }

                // Remove associated Payments
                if (order.Payments != null)
                {
                    _context.Payments.RemoveRange(order.Payments);
                }

                // Remove the Order
                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }

        // GET: Orders/UserOrders
        public async Task<IActionResult> UserOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var userOrders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.StatusType)
                .ToListAsync();

            if (!userOrders.Any())
            {
                ViewBag.Message = "No tienes pedidos por el momento.";
            }

            return View("UserOrders", userOrders);
        }
    }
}
