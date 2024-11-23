using LTSMerchWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;

namespace LTSMerchWebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly LtsMerchStoreContext _context;

        public AdminController(LtsMerchStoreContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Dashboard()
        {
            var totalPedidos = _context.Orders.Count();
            var totalClientes = _context.Users.Count();
            var totalCategorias = _context.ProductCategories.Count();
            var totalProductos = _context.Products.Count();

            var pedidos = _context.Orders
                .Include(o => o.User)
                .Include(o => o.StatusType)
                .Select(o => new
                {
                    o.OrderId,
                    ClientName = o.User != null ? o.User.Name : "N/A",
                    ContactNumber = o.User != null ? o.User.PhoneNumber : "N/A",
                    o.CreatedAt,
                    Status = o.StatusType != null ? o.StatusType.StatusName : "Desconocido"
                })
                .ToList();

            ViewBag.TotalPedidos = totalPedidos;
            ViewBag.TotalClientes = totalClientes;
            ViewBag.TotalCategorias = totalCategorias;
            ViewBag.TotalProductos = totalProductos;
            ViewBag.Pedidos = pedidos;
            return PartialView("Dashboard");
        }
    }
}
