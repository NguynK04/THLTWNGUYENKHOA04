﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NguyenKhoa_2280601517.Extensions;
using NguyenKhoa_2280601517.Models;
using NguyenKhoa_2280601517.Repositories;

namespace NguyenKhoa_2280601517.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ShoppingCartController(ApplicationDbContext context,
    UserManager<ApplicationUser> userManager, IProductRepository
    productRepository)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Checkout()
        {
            return View(new Order());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart =
            HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                // Xử lý giỏ hàng trống... 
                return RedirectToAction("Index");
            }
            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");
            return View("OrderCompleted", order.Id); // Trang xác nhận hoàn  thành đơn hàng
} 
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId 
            var product = await GetProductFromDatabase(productId);

            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity
            };
            var cart =
        HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new
        ShoppingCart();
            cart.AddItem(cartItem);

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }
        public IActionResult Index()
        {
            var cart =
        HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new
        ShoppingCart();
            return View(cart);
        }
        // Các actions khác... 
        private async Task<Product> GetProductFromDatabase(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm 
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

            if (cart is not null)
            {
                cart.RemoveItem(productId);
                // Save the updated cart back to the session
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            // Redirect to the Index action regardless of whether the cart was null or not
            return RedirectToAction("Index");
        }

       }

    }

