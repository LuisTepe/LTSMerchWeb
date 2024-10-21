using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LTSMerchWebApp.Models;

namespace LTSMerchWebApp.Controllers
{
    public class OrderStatusTypesController : Controller
    {
        private readonly LtsMerchStoreContext _context;

        public OrderStatusTypesController(LtsMerchStoreContext context)
        {
            _context = context;
        }

        // GET: OrderStatusTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.OrderStatusTypes.ToListAsync());
        }

        // GET: OrderStatusTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderStatusType = await _context.OrderStatusTypes
                .FirstOrDefaultAsync(m => m.StatusTypeId == id);
            if (orderStatusType == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", orderStatusType);
        }

        // GET: OrderStatusTypes/Create
        public IActionResult Create()
        {
            return PartialView("_CreatePartial");
        }

        // POST: OrderStatusTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StatusTypeId,StatusName")] OrderStatusType orderStatusType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderStatusType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return PartialView("_CreatePartial", orderStatusType);
        }

        // GET: OrderStatusTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderStatusType = await _context.OrderStatusTypes.FindAsync(id);
            if (orderStatusType == null)
            {
                return NotFound();
            }
            return PartialView("_EditPartial", orderStatusType);
        }

        // POST: OrderStatusTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StatusTypeId,StatusName")] OrderStatusType orderStatusType)
        {
            if (id != orderStatusType.StatusTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderStatusType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderStatusTypeExists(orderStatusType.StatusTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return PartialView("_EditPartial", orderStatusType);
        }

        // GET: OrderStatusTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderStatusType = await _context.OrderStatusTypes
                .FirstOrDefaultAsync(m => m.StatusTypeId == id);
            if (orderStatusType == null)
            {
                return NotFound();
            }

            return PartialView("_DeletePartial", orderStatusType);
        }

        // POST: OrderStatusTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderStatusType = await _context.OrderStatusTypes.FindAsync(id);
            if (orderStatusType != null)
            {
                _context.OrderStatusTypes.Remove(orderStatusType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderStatusTypeExists(int id)
        {
            return _context.OrderStatusTypes.Any(e => e.StatusTypeId == id);
        }
    }
}
