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
    public class ProductOptionsController : Controller
    {
        private readonly LtsMerchStoreContext _context;

        public ProductOptionsController(LtsMerchStoreContext context)
        {
            _context = context;
        }

        // GET: ProductOptions
        public async Task<IActionResult> Index()
        {
            var ltsMerchStoreContext = _context.ProductOptions.Include(p => p.Color).Include(p => p.Product).Include(p => p.Size);
            return View(await ltsMerchStoreContext.ToListAsync());
        }

        // GET: ProductOptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productOption = await _context.ProductOptions
                .Include(p => p.Color)
                .Include(p => p.Product)
                .Include(p => p.Size)
                .FirstOrDefaultAsync(m => m.ProductOptionId == id);
            if (productOption == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", productOption);
        }

        // GET: ProductOptions/Create
        public IActionResult Create()
        {
            ViewData["ColorId"] = new SelectList(_context.Colors, "ColorId", "ColorId");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId");
            ViewData["SizeId"] = new SelectList(_context.Sizes, "SizeId", "SizeId");
            return PartialView("_CreatePartial");
        }

        // POST: ProductOptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductOptionId,ProductId,SizeId,ColorId,Stock")] ProductOption productOption)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productOption);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ColorId"] = new SelectList(_context.Colors, "ColorId", "ColorId", productOption.ColorId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", productOption.ProductId);
            ViewData["SizeId"] = new SelectList(_context.Sizes, "SizeId", "SizeId", productOption.SizeId);
            return PartialView("_CreatePartial", productOption);
        }

        // GET: ProductOptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productOption = await _context.ProductOptions.FindAsync(id);
            if (productOption == null)
            {
                return NotFound();
            }
            ViewData["ColorId"] = new SelectList(_context.Colors, "ColorId", "ColorId", productOption.ColorId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", productOption.ProductId);
            ViewData["SizeId"] = new SelectList(_context.Sizes, "SizeId", "SizeId", productOption.SizeId);
            return PartialView("_EditPartial", productOption);
        }

        // POST: ProductOptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductOptionId,ProductId,SizeId,ColorId,Stock")] ProductOption productOption)
        {
            if (id != productOption.ProductOptionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productOption);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductOptionExists(productOption.ProductOptionId))
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
            ViewData["ColorId"] = new SelectList(_context.Colors, "ColorId", "ColorId", productOption.ColorId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", productOption.ProductId);
            ViewData["SizeId"] = new SelectList(_context.Sizes, "SizeId", "SizeId", productOption.SizeId);
            return PartialView("_EditPartial", productOption);
        }

        // GET: ProductOptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productOption = await _context.ProductOptions
                .Include(p => p.Color)
                .Include(p => p.Product)
                .Include(p => p.Size)
                .FirstOrDefaultAsync(m => m.ProductOptionId == id);
            if (productOption == null)
            {
                return NotFound();
            }

            return PartialView("_DeletePartial", productOption);
        }

        // POST: ProductOptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productOption = await _context.ProductOptions.FindAsync(id);
            if (productOption != null)
            {
                _context.ProductOptions.Remove(productOption);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductOptionExists(int id)
        {
            return _context.ProductOptions.Any(e => e.ProductOptionId == id);
        }
    }
}