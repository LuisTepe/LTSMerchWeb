using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTSMerchWebApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LTSMerchWebApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly LtsMerchStoreContext _context;

        public ProductsController(LtsMerchStoreContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            // Llenar el ViewBag con los datos de colores y tallas
            ViewBag.Colors = new SelectList(_context.Colors.ToList(), "ColorId", "ColorName");
            ViewBag.Sizes = new SelectList(_context.Sizes.ToList(), "SizeId", "SizeName");

            // Cargar productos y sus opciones
            var products = _context.Products
                .Include(p => p.ProductOptions)
                .ThenInclude(po => po.Color)
                .Include(p => p.ProductOptions)
                .ThenInclude(po => po.Size)
                .ToList();

            return View(products);
        }

        // GET: Products/Details/5
        public IActionResult Details()
        {
            /*
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            */
            return View();
        }
        public IActionResult ShoppingCart()
        {
            return View();
        }

        public IActionResult CheckOutPayment()
        {
            return View();
        }

        public IActionResult CheckOutShipping()
        {
            return View();
        }

        public IActionResult Shipping()
        {
            return View();
        }

        public IActionResult Create()
        {


            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Name,Description,Price,Stock,CreatedAt")] Product product, IFormFile ImageUrl, int ColorId, int SizeId)
        {
            if (ModelState.IsValid)
            {
                if (ImageUrl != null && ImageUrl.Length > 0)
                {
                    var fileName = Path.GetFileNameWithoutExtension(ImageUrl.FileName);
                    var extension = Path.GetExtension(ImageUrl.FileName);
                    var newFileName = $"{fileName}_{DateTime.Now.Ticks}{extension}";

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", newFileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await ImageUrl.CopyToAsync(stream);
                    }

                    product.ImageUrl = newFileName;
                }

                // Guardar el producto
                _context.Add(product);
                await _context.SaveChangesAsync();

                // Crear entrada en ProductOption
                var productOption = new ProductOption
                {
                    ProductId = product.ProductId,
                    ColorId = ColorId,
                    SizeId = SizeId,
                    Stock = product.Stock
                };

                _context.ProductOptions.Add(productOption);
                await _context.SaveChangesAsync();

                // Devuelve el producto creado como JSON
                return Json(new
                {
                    productId = product.ProductId,
                    name = product.Name,
                    price = product.Price,
                    description = product.Description,
                    imageUrl = product.ImageUrl,
                  
                    color = _context.Colors.FirstOrDefault(c => c.ColorId == ColorId)?.ColorName, // Ajusta según tu lógica de Color
                    size = _context.Sizes.FirstOrDefault(s => s.SizeId == SizeId)?.SizeName, // Ajusta según tu lógica de Talla
                   
                });
            }

            ViewBag.Colors = new SelectList(_context.Colors.ToList(), "ColorId", "ColorName");
            ViewBag.Sizes = new SelectList(_context.Sizes.ToList(), "SizeId", "SizeName");

            return View(product);
        }





        // GET: Products/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Cargar el producto a editar
            var product = await _context.Products
                .Include(p => p.ProductOptions)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Cargar los datos necesarios para las listas desplegables
            ViewBag.Colors = new SelectList(_context.Colors.ToList(), "ColorId", "ColorName");
            ViewBag.Sizes = new SelectList(_context.Sizes.ToList(), "SizeId", "SizeName");
            

            // Devuelve los datos del producto como JSON (para que lo reciba el frontend)
            return Json(new
            {
                productId = product.ProductId,
                name = product.Name,
                description = product.Description,
                price = product.Price,
                
                
                
                colorId = product.ProductOptions.FirstOrDefault()?.ColorId,
                sizeId = product.ProductOptions.FirstOrDefault()?.SizeId,
                
            });
        }


        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Price,Description")] Product product, int ColorId, int SizeId)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray() });
            }

            try
            {
                // Obtener el producto existente de la base de datos con ProductOptions
                var existingProduct = await _context.Products
                    .Include(p => p.ProductOptions)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (existingProduct == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                // Mantener el valor original de CreatedAt
                product.CreatedAt = existingProduct.CreatedAt;

                // Actualizar los campos del producto
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;

                // Actualizar los valores de ProductOptions (color y talla)
                var productOption = existingProduct.ProductOptions.FirstOrDefault();
                if (productOption != null)
                {
                    productOption.ColorId = ColorId;
                    productOption.SizeId = SizeId;
                }
                else
                {
                    // Si no existe, crear una nueva opción de producto
                    productOption = new ProductOption
                    {
                        ProductId = existingProduct.ProductId,
                        ColorId = ColorId,
                        SizeId = SizeId
                    };
                    _context.ProductOptions.Add(productOption);
                }

                // Guardar los cambios
                await _context.SaveChangesAsync();

                // Devolver el producto actualizado como JSON
                return Json(new
                {
                    success = true,
                    product = new
                    {
                        product.ProductId,
                        product.Name,
                        product.Price,
                        product.Description,
                        color = _context.Colors.FirstOrDefault(c => c.ColorId == ColorId)?.ColorName,  // Obtener el nombre del color
                        size = _context.Sizes.FirstOrDefault(s => s.SizeId == SizeId)?.SizeName        // Obtener el nombre de la talla
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                // Capturar la excepción interna
                var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = $"Error updating product: {innerException}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
