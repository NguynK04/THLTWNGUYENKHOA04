using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
// Thêm using cho namespace chứa các ViewModel mới của bạn
using NguyenKhoa_2280601517.Areas.Admin.Models;
using NguyenKhoa_2280601517.Models;

namespace NguyenKhoa_2280601517.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync(); // Dùng ToListAsync
            var userList = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = roles.FirstOrDefault()
                });
            }
            return View(userList);
        }

        public async Task<IActionResult> Create()
        {
            var model = new CreateUserViewModel
            {
                RoleList = await LoadRoles()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.RoleList = await LoadRoles();
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                SelectedRole = userRoles.FirstOrDefault(),
                RoleList = await LoadRoles()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email;

                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    var oldRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, oldRoles);
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);

                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.RoleList = await LoadRoles();
            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = roles.FirstOrDefault()
            };
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }

        // --- PRIVATE HELPER METHOD ---
        // Hàm private để lấy danh sách vai trò, tránh lặp code (DRY Principle)
        private async Task<List<SelectListItem>> LoadRoles()
        {
            return await _roleManager.Roles
                .Where(r => r.Name != SD.Role_Admin)
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToListAsync();
        }
    }
}