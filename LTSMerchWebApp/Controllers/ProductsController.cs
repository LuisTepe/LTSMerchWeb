using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTSMerchWebApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace LTSMerchWebApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly LtsMerchStoreContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(LtsMerchStoreContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
            var products = await _context.Products
    .Include(p => p.ProductOptions)
        .ThenInclude(po => po.State)
    .Where(p => p.ProductOptions.Any(po => po.State != null && po.State.IsActive == true))
    .ToListAsync();


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
            // Validar que las contraseñas no estén vacías
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                return Json(new { success = false, message = "Ambos campos de contraseña son obligatorios." });
            }

            // Validar que las contraseñas coincidan
            if (password != confirmPassword)
            {
                return Json(new { success = false, message = "Las contraseñas no coinciden." });
            }

            // Buscar el producto por ID
            var product = _context.Products
                .Include(p => p.ProductOptions)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return Json(new { success = false, message = "Producto no encontrado." });
            }

            try
            {
                foreach (var option in product.ProductOptions)
                {
                    option.StateId = 2; // Cambiar a inactivo (Estado ID 2)
                }

                _context.SaveChanges();
                return Json(new { success = true, message = "Producto marcado como inactivo correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cambiar el estado del producto: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int size, int color, int quantity)
        {
            // Find the product option based on the selected product, size, and color
            var productOption = _context.ProductOptions
                .FirstOrDefault(po => po.ProductId == productId && po.SizeId == size && po.ColorId == color);

            if (productOption == null)
            {
                // Handle the case where the size and color combination is not available for this product
                return NotFound("Esta combinación de talla y color no está disponible.");
            }

            // Get or create the user's cart
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId.Value);

            if (cart == null)
            {
                cart = new Cart { UserId = userId.Value };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            // Check if the item is already in the cart
            var cartItem = _context.CartItems
                .FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductOptionId == productOption.ProductOptionId);

            if (cartItem != null)
            {
                // Update the quantity
                cartItem.Quantity += quantity;
            }
            else
            {
                // Add new item to cart
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductOptionId = productOption.ProductOptionId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            _context.SaveChanges();

            return RedirectToAction("ShoppingCart");
        }

        public IActionResult ShoppingCart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                ViewBag.Message = "Debes iniciar sesión para ver tu carrito de compras.";
                return View(new Cart());
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductOption)
                        .ThenInclude(po => po.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductOption)
                        .ThenInclude(po => po.Size)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductOption)
                        .ThenInclude(po => po.Color)
                .FirstOrDefault(c => c.UserId == userId);

            return View(cart ?? new Cart());
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

        [HttpGet]
        public IActionResult CheckOutPayment()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Orders
                .Include(o => o.User)
                .FirstOrDefault(o => o.UserId == userId && o.StatusTypeId == 1);

            if (order == null)
            {
                return RedirectToAction("ShoppingCart");
            }

            var viewModel = new PaymentViewModel
            {
                User = order.User,
                Order = order
            };

            return View(viewModel);
        }


        [HttpGet]
        public IActionResult CheckOutShipping()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductOption)
                        .ThenInclude(po => po.Product)
                .FirstOrDefault(c => c.UserId == userId);

            var order = new Order
            {
                UserId = userId.Value,
                ShippingAddress = user?.Address,
                User = user
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
        public IActionResult CheckOutShipping(string StreetAddress, string Neighborhood, string City, string State, string PostalCode)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user != null)
            {
                user.StreetAddress = StreetAddress;
                user.Neighborhood = Neighborhood;
                user.City = City;
                user.State = State;
                user.PostalCode = PostalCode;
                _context.Users.Update(user);
                _context.SaveChanges();
            }

            // Create a new order
            var order = new Order
            {
                UserId = userId.Value,
                ShippingAddress = $"{StreetAddress}, {Neighborhood}, {City}, {State}, {PostalCode}",
                CreatedAt = DateTime.Now,
                Total = CalculateOrderTotal(userId.Value),
                StatusTypeId = 1 // Assuming 1 = Pending Payment
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            return RedirectToAction("CheckOutPayment");
        }


        [HttpPost]
        public IActionResult UpdateShippingAddress(string StreetAddress, string Neighborhood, string City, string State, string PostalCode)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user != null)
            {
                user.StreetAddress = StreetAddress;
                user.Neighborhood = Neighborhood;
                user.City = City;
                user.State = State;
                user.PostalCode = PostalCode;
                _context.Users.Update(user);
                _context.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult CompleteOrder(IFormFile comprobante, int orderId)
        {
            // Validate the uploaded file
            if (comprobante == null || comprobante.Length == 0)
            {
                ModelState.AddModelError("Comprobante", "Por favor sube un comprobante de pago.");
                return View("CheckOutPayment");
            }

            // Save the payment voucher file
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "img/comprobantes");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = "Orden_" + orderId + Path.GetExtension(comprobante.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                comprobante.CopyTo(fileStream);
            }

            // Retrieve the order by ID
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Update order status
            order.StatusTypeId = 2; // Assuming 2 = Paid

            // Add payment information
            var payment = new Payment
            {
                Amount = order.Total,
                CreatedAt = DateTime.Now,
                VoucherPath = $"/img/comprobantes/{fileName}",
                OrderId = orderId,
                UserId = order.UserId
            };
            _context.Payments.Add(payment);

            // **Add order details from cart items**
            var userId = HttpContext.Session.GetInt32("UserId");
            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductOption)
                        .ThenInclude(po => po.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                ModelState.AddModelError("", "El carrito está vacío.");
                return View("CheckOutPayment");
            }

            foreach (var cartItem in cart.CartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = orderId,
                    ProductOptionId = cartItem.ProductOptionId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.ProductOption.Product.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }

            _context.SaveChanges();

            // Clear the cart after saving
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.SaveChanges();

            return RedirectToAction("Thanks", "Home");
        }



        // Método auxiliar para calcular el total del pedido con envío
        private decimal CalculateOrderTotal(int userId)
        {
            var cartItems = _context.CartItems
                .Include(ci => ci.ProductOption)
                    .ThenInclude(po => po.Product)
                .Where(ci => ci.Cart.UserId == userId)
                .ToList();

            var subtotal = cartItems.Sum(ci => ci.Quantity * ci.ProductOption.Product.Price);

            // Add shipping cost if applicable
            decimal shippingCost = 100; // Adjust based on your shipping logic

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
