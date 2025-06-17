using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NguyenKhoa_2280601517.Models;
using NguyenKhoa_2280601517.Repositories;

namespace NguyenKhoa_2280601517.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
   
        public class CategoriesManagerController : Controller
        {
            private readonly IProductRepository _productRepository;
            private readonly ICategoryRepository _categoryRepository;

            public CategoriesManagerController(IProductRepository productRepository, ICategoryRepository categoryRepository)
            {
                _productRepository = productRepository;
                _categoryRepository = categoryRepository;
            }

            public async Task<IActionResult> Index()
            {
                var categories = await _categoryRepository.GetAllAsync();
                return View(categories);
            }


            public async Task<IActionResult> Display(int id)
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                return View(category);
            }

            public IActionResult Add()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken] // Recommended for security
            public async Task<IActionResult> Add(Category category)
            {
                if (ModelState.IsValid)
                {
                    await _categoryRepository.AddAsync(category);
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }

            public async Task<IActionResult> Update(int id)
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }

            [HttpPost]
            [ValidateAntiForgeryToken] // Recommended for security
            public async Task<IActionResult> Update(int id, Category category)
            {
                if (id != category.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    await _categoryRepository.UpdateAsync(category);
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }

            public async Task<IActionResult> Delete(int id)
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }

            [HttpPost, ActionName("DeleteConfirmed")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null) return NotFound();

                var products = await _productRepository.GetAllAsync();
                var affectedProducts = products.Where(p => p.CategoryId == id);

                foreach (var product in affectedProducts)
                {
                    product.CategoryId = null;
                    await _productRepository.UpdateAsync(product);
                }

                await _categoryRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }

        }
    }

