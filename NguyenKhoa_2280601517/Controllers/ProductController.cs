using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NguyenKhoa_2280601517.Models;
using NguyenKhoa_2280601517.Repositories;
using System.Globalization;
using System.Text;


namespace NguyenKhoa_2280601517.Controllers
{

    public class ProductController : Controller
    {

        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository,
ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // Hiển thị danh sách sản phẩm 
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, decimal? priceSearchValue, string sortOrder)
        {
            // Lưu trạng thái hiện tại của bộ lọc và sắp xếp vào ViewData để gửi về View
            ViewData["CurrentSearchString"] = searchString;
            ViewData["CurrentPriceSearchValue"] = priceSearchValue;
            ViewData["SortOrder"] = sortOrder; // Dùng để đánh dấu option đã chọn trong dropdown

            // Lấy tất cả sản phẩm dưới dạng IQueryable để có thể xây dựng truy vấn động
            IQueryable<Product> products = (await _productRepository.GetAllAsync()).AsQueryable();

            // 1. Lọc theo chuỗi tìm kiếm (tên, mô tả, danh mục - không dấu, không phân biệt hoa thường)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string normalizedSearchString = RemoveDiacritics(searchString).ToLower();
                var keywords = normalizedSearchString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                products = products.Where(p =>
                    p.Name != null && keywords.Any(keyword => RemoveDiacritics(p.Name).ToLower().Contains(keyword)) ||
                    p.Description != null && keywords.Any(keyword => RemoveDiacritics(p.Description).ToLower().Contains(keyword)) ||
                    p.Category != null && p.Category.Name != null && keywords.Any(keyword => RemoveDiacritics(p.Category.Name).ToLower().Contains(keyword))
                );
            }

            // 2. Lọc và sắp xếp theo giá tiệm cận
            if (priceSearchValue.HasValue)
            {
                decimal targetPrice = priceSearchValue.Value;
                decimal tolerance = 10.500m; // Ngưỡng 10% của giá mục tiêu. Bạn có thể thay đổi hoặc làm tham số
                // Hoặc ngưỡng cố định: decimal tolerance = 5000m;

                decimal minRange = targetPrice * (1 - tolerance);
                decimal maxRange = targetPrice;

                // Đảm bảo minRange không âm
                if (minRange < 0) minRange = 0;

                products = products.Where(p => p.Price >= minRange && p.Price <= maxRange);

                // Sắp xếp chính theo độ tiệm cận (gần giá mục tiêu nhất)
                // Sắp xếp phụ theo giá tăng dần (để những sản phẩm tiệm cận có thứ tự hợp lý)
                products = products.OrderBy(p => Math.Abs(p.Price - targetPrice))
                                   .ThenByDescending(p => p.Price); // Hoặc ThenByDescending(p => p.Price) nếu muốn ngược lại
            }

            // 3. Sắp xếp theo lựa chọn của người dùng (áp dụng sau các bộ lọc)
            // Nếu đã sắp xếp tiệm cận, các case 'price_asc'/'price_desc' ở đây sẽ ghi đè thứ tự tiệm cận.
            // Nếu muốn tiệm cận là ưu tiên hàng đầu, hãy điều chỉnh hoặc bỏ các case đó.
            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price_asc":
                    // Nếu đã tìm kiếm tiệm cận, đây sẽ ghi đè sắp xếp tiệm cận
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    // Nếu đã tìm kiếm tiệm cận, đây sẽ ghi đè sắp xếp tiệm cận
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    // Sắp xếp mặc định nếu không có bộ lọc giá tiệm cận hoặc sortOrder không xác định
                    if (!priceSearchValue.HasValue)
                    {
                        products = products.OrderBy(p => p.Name);
                    }
                    break;
            }

            // Cuối cùng, thực hiện truy vấn và chuyển đổi sang List
            return View(products.ToList());// Sử dụng ToListAsync() nếu repository hỗ trợ Async
        }

        // Hàm hỗ trợ để loại bỏ dấu (giúp tìm kiếm kiểu Telex)
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            string formD = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char ch in formD)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }



        // Hiển thị form thêm sản phẩm mới 
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        // Xử lý thêm sản phẩm mới 
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(Product product, IFormFile
imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage 
                    product.ImageUrl = await SaveImage(imageUrl);
                }


                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập 
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // Viết thêm hàm SaveImage (tham khảo bài 02) 

        private async Task<string> SaveImage(IFormFile image)
        {
            var fileName = Path.GetFileName(image.FileName);
            var filePath = Path.Combine("wwwroot/images", fileName);

            // Tạo thư mục nếu chưa tồn tại
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return "/images/" + fileName;
        }

        [Authorize]
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        // Hiển thị form cập nhật sản phẩm 
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name",
product.CategoryId);
            return View(product);
        }


        // Xử lý cập nhật sản phẩm 
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id, Product product,
IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl"); // Loại bỏ xác thực ModelState cho  ImageUrl
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                var existingProduct = await
                    _productRepository.GetByIdAsync(id); // Giả định có phương thức GetByIdAsync 

                // Giữ nguyên thông tin hình ảnh nếu không có hình mới được tải lên
                if (imageUrl == null)
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
                else
                {
                    // Lưu hình ảnh mới 
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                // Cập nhật các thông tin khác của sản phẩm 
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;
            
            await _productRepository.UpdateAsync(existingProduct);
            return RedirectToAction(nameof(Index));
        }
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name"); 
return View(product);
    }
        // Hiển thị form xác nhận xóa sản phẩm 
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        // Xử lý xóa sản phẩm 
        [HttpPost, ActionName("DeleteConfirmed")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
