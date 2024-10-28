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
            ViewBag.Categories = new SelectList(_context.ProductCategories.ToList(), "category_id", "description");
            ViewBag.States = new SelectList(_context.ProductStates.ToList(), "state_id", "is_active");

            // Cargar productos y sus opciones
            var products = _context.Products
                .Include(p => p.ProductOptions)
                .ThenInclude(po => po.Color)
                .Include(p => p.ProductOptions)
                .ThenInclude(po => po.Size)
                .Include(p => p.ProductOptions)
                .ThenInclude(po => po.Category)
                .Include(p => p.ProductOptions)
                .ThenInclude(po => po.State)
                .ToList();

            return View(products);
        }

        public async Task<IActionResult> ProductsList()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
            .Include(p => p.ProductOptions)
                .ThenInclude(po => po.Size)
            .Include(p => p.ProductOptions)
                .ThenInclude(po => po.Color)
            .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.IsUserLoggedIn = HttpContext.Session.GetInt32("UserId") != null;

            return View(product);
        }

        [HttpPost]
        public IActionResult AddToCart(int size, int color, int quantity)
        {
            // Buscar la opción de producto según la talla y el color seleccionados
            var productOption = _context.ProductOptions
                .FirstOrDefault(po => po.SizeId == size && po.ColorId == color);

            if (productOption == null)
            {
                // Manejar el caso en que la combinación de talla y color no esté disponible
                return NotFound("Esta combinación de talla y color no está disponible.");
            }

            // Obtener o crear el carrito del usuario
            var userId = HttpContext.Session.GetInt32("UserId");
            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            // Agregar el item al carrito
            var cartItem = new CartItem
            {
                CartId = cart.CartId,
                ProductOptionId = productOption.ProductOptionId,
                Quantity = quantity
            };
            _context.CartItems.Add(cartItem);
            _context.SaveChanges();

            return RedirectToAction("ShoppingCart", "Products");
        }

        public IActionResult ShoppingCart()
        {
            // Obtener el ID del usuario de la sesión
            var userId = HttpContext.Session.GetInt32("UserId");

            // Verificar si el usuario no está autenticado
            if (!userId.HasValue)
            {
                // Retornar una vista que indique que el carrito está vacío o redirigir a la página de inicio
                ViewBag.Message = "Debes iniciar sesión para ver tu carrito de compras.";
                return View(new Cart()); // Retorna un carrito vacío
            }

            // Obtener el carrito del usuario
            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductOption)
                        .ThenInclude(po => po.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductOption)
                        .ThenInclude(po => po.Size) // Include Size
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductOption)
                        .ThenInclude(po => po.Color) // Include Color
                .FirstOrDefault(c => c.UserId == userId);

            // Pasar el carrito a la vista
            return View(cart ?? new Cart()); // Si no se encuentra el carrito, retorna uno vacío
        }

        [HttpPost]
        public IActionResult UpdateCartItem(int cartItemId, int quantity)
        {
            var cartItem = _context.CartItems.Find(cartItemId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                _context.SaveChanges();
            }
            return RedirectToAction("ShoppingCart");
        }

        [HttpPost]
        public IActionResult RemoveCartItem(int cartItemId)
        {
            var cartItem = _context.CartItems.Find(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }
            return RedirectToAction("ShoppingCart");
        }

        public IActionResult CheckOutPayment()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CheckOutShipping()
        {
            // Obtén el UserId de la sesión
            var userId = HttpContext.Session.GetInt32("UserId");

            // Obtén el carrito correspondiente al usuario
            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductOption)
                        .ThenInclude(po => po.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductOption)
                        .ThenInclude(po => po.Size) // Include Size
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductOption)
                        .ThenInclude(po => po.Color) // Include Color
                .FirstOrDefault(c => c.UserId == userId);


            // Crea el modelo de envío
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = "Hmo, xx, xx, xx",
            };

            var viewModel = new ShippingViewModel
            {
                Order = order,
                Cart = cart
            };

            return View(viewModel);
        }

        // Acción para procesar el formulario de CheckOutShipping
        [HttpPost]
        public IActionResult CheckOutShipping(string ShippingAddress, int ShippingMethod)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            // Obtén el pedido existente
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = ShippingAddress, // Asigna la nueva dirección
                CreatedAt = DateTime.Now,
                Total = CalculateOrderTotal(userId, ShippingMethod),
                StatusTypeId = 1 // Asumiendo que "1" es el estatus inicial del pedido
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            return RedirectToAction("CheckOutPayment", "Products");
        }


        // Método auxiliar para calcular el total del pedido con envío
        private decimal CalculateOrderTotal(int? userId, int shippingMethod)
        {
            var cartItems = _context.CartItems.Where(c => c.Cart.UserId == userId);
            var subtotal = cartItems.Sum(item => item.Quantity * item.ProductOption.Product.Price);

            decimal shippingCost = shippingMethod switch
            {
                1 => 50.00m,
                2 => 70.00m,
                3 => 100.00m,
                _ => 0m
            };

            return subtotal + shippingCost;
        }
        public IActionResult RenderShoppingCart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var cartItems = _context.CartItems.Where(c => c.Cart.UserId == userId).ToList();
            return PartialView("_ShoppingCartPartial", cartItems);
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
                        imageUrl = product.ImageUrl ?? existingProduct.ImageUrl,
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

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(IFormFile comprobante)
        {
            if (comprobante != null && comprobante.Length > 0)
            {
                // Crea el directorio si no existe
                var uploadDir = Path.Combine("wwwroot", "uploads");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                // Genera un nombre de archivo único para evitar sobrescritura
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(comprobante.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);

                // Guarda el archivo en el servidor
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await comprobante.CopyToAsync(stream);
                }

                // Obtén el UserId de la sesión
                var userId = HttpContext.Session.GetInt32("UserId");

                var order = _context.Orders.FirstOrDefault(x => x.UserId == userId && x.StatusTypeId == 1);

                // Valida que userId no sea nulo
                if (userId == null)
                {
                    ViewBag.Error = "No se pudo obtener el ID de usuario de la sesión.";
                    return View("productPayment");
                }

                // Crear el registro de pago
                var paymentRecord = new Payment
                {
                    UserId = userId.Value,
                    OrderId = order.OrderId,
                    VoucherPath = filePath,
                    PaymentMethodId = 1,
                    Amount = order.Total,
                    PaymentStatusTypeId = 2,
                };

                // Guarda el registro en la base de datos
                _context.Payments.Add(paymentRecord);
                await _context.SaveChangesAsync();

                // Redirige o muestra mensaje de confirmación
                return RedirectToAction("Thanks", "Home");
            }

            // Manejo de error si no se sube un archivo
            ViewBag.Error = "Por favor sube un comprobante de pago.";
            return View("CheckOutPayment");
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
