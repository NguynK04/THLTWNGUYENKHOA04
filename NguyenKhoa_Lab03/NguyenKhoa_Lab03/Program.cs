using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NguyenKhoa_Lab03.Models;       // Hoặc thay bằng WebsiteBanHang.DataAccess nếu bạn đổi namespace
using NguyenKhoa_Lab03.Repositories;  // Hoặc thay bằng WebsiteBanHang.Repositories nếu bạn đổi namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



// Cấu hình Identity, chú ý type ApplicationUser hoặc IdentityUser tùy bạn dùng User class custom hay không
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()  // hoặc IdentityUser nếu không dùng custom user
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";          // ✔ đúng định dạng
    options.LogoutPath = "/Identity/Account/Logout";        // ✔ chỉ để 1 lần
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // ✔ đúng key
});


builder.Services.AddRazorPages();

builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thêm UseAuthentication trước UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=ProductManagerController}/{action=Index}/{id?}"
    );
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "Admin", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
