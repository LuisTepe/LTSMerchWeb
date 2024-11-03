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
            ViewData["HideHeaderFooter"] = true;
            // Llenar el ViewBag con los datos de colores y tallas
            ViewBag.Colors = new SelectList(_context.Colors.ToList(), "ColorId", "ColorName");
            ViewBag.Sizes = new SelectList(_context.Sizes.ToList(), "SizeId", "SizeName");
            ViewBag.Categories = new SelectList(_context.ProductCategories.ToList(), "CategoryId", "Description");
            ViewBag.States = new SelectList(_context.ProductStates.ToList(), "StateId", "IsActive");

            // Cargar productos y sus opciones
            var products = await _context.Products
                .Include(p => p.ProductOptions)
                    .ThenInclude(po => po.Color)
                .Include(p => p.ProductOptions)
                    .ThenInclude(po => po.Size)
                .Include(p => p.ProductOptions)
                    .ThenInclude(po => po.Category)
                .Include(p => p.ProductOptions)
                    .ThenInclude(po => po.State)
                .ToListAsync();

            // Verificar si hay productos sin opciones para evitar referencias nulas
            foreach (var product in products)
            {
                foreach (var option in product.ProductOptions)
                {
                    option.Category ??= new ProductCategory { Description = "Sin categoría" };
                    option.State ??= new ProductState { IsActive = false };
                }
            }

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
        public IActionResult Deleted(int id, string password, string confirmPassword)
        {
            // Valida que las contraseñas coincidan
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Las contraseñas no coinciden.");
                return BadRequest("Las contraseñas no coinciden.");
            }

            // Busca el producto por ID, incluyendo sus opciones y relaciones necesarias
            var product = _context.Products
                                  .Include(p => p.ProductOptions)
                                  .ThenInclude(po => po.CartItems)
                                  .Include(p => p.ProductOptions)
                                  .ThenInclude(po => po.OrderDetails)
                                  .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound("Producto no encontrado.");
            }

            // Verificar si alguna ProductOption está en uso en CartItems o OrderDetails
            bool isOptionInUse = product.ProductOptions.Any(po => po.CartItems.Any() || po.OrderDetails.Any());

            if (isOptionInUse)
            {
                return Json(new { success = false, message = "No se puede eliminar el producto porque una o más opciones están en uso." });
            }

            // Si ninguna opción está en uso, procedemos a eliminar las opciones de producto
            try
            {
                _context.ProductOptions.RemoveRange(product.ProductOptions); // Eliminar todas las opciones del producto
                _context.Products.Remove(product); // Eliminar el producto
                _context.SaveChanges();

                // Retorna un mensaje de éxito
                return Json(new { success = true, message = "Producto y opciones eliminados correctamente." });
            }
            catch (Exception ex)
            {
                // Captura cualquier error inesperado durante la eliminación
                return Json(new { success = false, message = "Ocurrió un error al eliminar el producto: " + ex.Message });
            }
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
        public async Task<IActionResult> Create(Product product, List<int> CategoryIds, List<int> ColorIds, List<int> SizeIds, int StateId, IFormFile ImageUrl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Modelo inválido");
            }

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
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Crear combinaciones únicas de opciones
            var addedCombinations = new HashSet<(int categoryId, int colorId, int sizeId)>();

            for (int i = 0; i < CategoryIds.Count; i++)
            {
                int categoryId = CategoryIds[i];
                int colorId = ColorIds[i];
                int sizeId = SizeIds[i];

                // Verificar si la combinación ya existe
                if (!addedCombinations.Contains((categoryId, colorId, sizeId)))
                {
                    var productOption = new ProductOption
                    {
                        ProductId = product.ProductId,
                        CategoryId = categoryId,
                        ColorId = colorId,
                        SizeId = sizeId,
                        Stock = product.Stock,
                        StateId = StateId

                    };

                    _context.ProductOptions.Add(productOption);
                    addedCombinations.Add((categoryId, colorId, sizeId));
                }
            }

            await _context.SaveChangesAsync();

            // Obtener nombres de categorías, colores y tallas
            var categoryNames = _context.ProductCategories
                                .Where(c => CategoryIds.Contains(c.CategoryId))
                                .Select(c => c.Description).ToList();

            var colorNames = _context.Colors
                                .Where(c => ColorIds.Contains(c.ColorId))
                                .Select(c => c.ColorName).ToList();

            var sizeNames = _context.Sizes
                                .Where(s => SizeIds.Contains(s.SizeId))
                                .Select(s => s.SizeName).ToList();

            // Devolver los datos en formato JSON para actualizar la interfaz sin recargar la página
            return Json(new
            {
                success = true,
                productId = product.ProductId,
                name = product.Name,
                price = product.Price,
                stock = product.Stock,
                description = product.Description,
                imageUrl = product.ImageUrl,
                state = StateId == 1 ? "Activo" : "Inactivo",
                categories = categoryNames,
                colors = colorNames,
                sizes = sizeNames
            });
        }





        // GET: Products/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
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
            ViewBag.Categories = new SelectList(_context.ProductCategories.ToList(), "CategoryId", "Description");
            ViewBag.States = new SelectList(_context.ProductStates.ToList(), "StateId", "IsActive");
            var stateId = product.ProductOptions.FirstOrDefault()?.StateId;

            // Obtener el StateId del primer ProductOption si existe
            var productOption = product.ProductOptions.FirstOrDefault();
            

            // Preparar combinaciones de opciones para enviarlas a la vista
            var productOptions = product.ProductOptions.Select(po => new
            {
                colorId = po.ColorId,
                sizeId = po.SizeId,
                categoryId = po.CategoryId,
                stock = po.Stock,
                stateId = po.StateId
            }).ToList();

            return Json(new
            {
                productId = product.ProductId,
                name = product.Name,
                description = product.Description,
                price = product.Price,
                stock = product.Stock, // Stock de Product
                stateId = stateId,
                imageUrl = product.ImageUrl,
                productOptions // Todas las combinaciones de ProductOption
            });
        }






        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, List<int> CategoryIds, List<int> ColorIds, List<int> SizeIds, int StateId, IFormFile? ImageUrl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Modelo inválido");
            }

            var existingProduct = await _context.Products
                .Include(p => p.ProductOptions)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (existingProduct == null)
            {
                return NotFound("Producto no encontrado");
            }

            // Actualizar detalles del producto
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.Stock = product.Stock;

            // Procesar imagen
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

                existingProduct.ImageUrl = newFileName;
            }

            // Limpiar combinaciones previas y evitar duplicados en nuevas combinaciones
            _context.ProductOptions.RemoveRange(existingProduct.ProductOptions);

            var addedCombinations = new HashSet<(int categoryId, int colorId, int sizeId)>();
            for (int i = 0; i < CategoryIds.Count; i++)
            {
                int categoryId = CategoryIds[i];
                int colorId = ColorIds[i];
                int sizeId = SizeIds[i];

                if (!addedCombinations.Contains((categoryId, colorId, sizeId)))
                {
                    var productOption = new ProductOption
                    {
                        ProductId = existingProduct.ProductId,
                        CategoryId = categoryId,
                        ColorId = colorId,
                        SizeId = sizeId,
                        Stock = product.Stock,
                        StateId = StateId
                    };

                    _context.ProductOptions.Add(productOption);
                    addedCombinations.Add((categoryId, colorId, sizeId));
                }
            }

            await _context.SaveChangesAsync();

            // Obtener nombres de categorías, colores y tallas para la respuesta
            var categoryNames = _context.ProductCategories
                .Where(c => CategoryIds.Contains(c.CategoryId))
                .Select(c => c.Description).ToList();

            var colorNames = _context.Colors
                .Where(c => ColorIds.Contains(c.ColorId))
                .Select(c => c.ColorName).ToList();

            var sizeNames = _context.Sizes
                .Where(s => SizeIds.Contains(s.SizeId))
                .Select(s => s.SizeName).ToList();

            return Json(new
            {
                success = true,
                productId = existingProduct.ProductId,
                name = existingProduct.Name,
                price = existingProduct.Price,
                stock = existingProduct.Stock,
                description = existingProduct.Description,
                imageUrl = existingProduct.ImageUrl,
                state = StateId == 1 ? "Activo" : "Inactivo",
                categories = categoryNames,
                colors = colorNames,
                sizes = sizeNames
            });
        }






        //Aqui va lo de delete productos









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
        [HttpPost]
        public IActionResult Delete(int id, string password, string confirmPassword)
        {
            // Valida que las contraseñas coincidan
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Las contraseñas no coinciden.");
                return BadRequest("Las contraseñas no coinciden.");
            }

            // Busca el producto por ID, incluyendo sus opciones y relaciones necesarias
            var product = _context.Products
                                  .Include(p => p.ProductOptions)
                                  .ThenInclude(po => po.CartItems)
                                  .Include(p => p.ProductOptions)
                                  .ThenInclude(po => po.OrderDetails)
                                  .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound("Producto no encontrado.");
            }

            // Verificar si alguna ProductOption está en uso en CartItems o OrderDetails
            bool isOptionInUse = product.ProductOptions.Any(po => po.CartItems.Any() || po.OrderDetails.Any());

            if (isOptionInUse)
            {
                return Json(new { success = false, message = "No se puede eliminar el producto porque una o más opciones están en uso." });
            }

            // Si ninguna opción está en uso, procedemos a eliminar las opciones de producto
            try
            {
                _context.ProductOptions.RemoveRange(product.ProductOptions); // Eliminar todas las opciones del producto
                _context.Products.Remove(product); // Eliminar el producto
                _context.SaveChanges();

                // Retorna un mensaje de éxito
                return Json(new { success = true, message = "Producto y opciones eliminados correctamente." });
            }
            catch (Exception ex)
            {
                // Captura cualquier error inesperado durante la eliminación
                return Json(new { success = false, message = "Ocurrió un error al eliminar el producto: " + ex.Message });
            }
        }

        // POST: Products/Delete/5
        /*
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
        */
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
