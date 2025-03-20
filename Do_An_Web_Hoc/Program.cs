using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using Do_An_Web_Hoc.Models;
using Microsoft.EntityFrameworkCore;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Do_An_Web_Hoc.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 💡 **Thêm cấu hình Database**
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 💡 **Thêm dịch vụ Authentication & Authorization**
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Index"; // Chuyển hướng khi chưa đăng nhập
        options.AccessDeniedPath = "/Home/AccessDenied"; // Chuyển hướng khi không có quyền
    });

builder.Services.AddAuthorization();
// 💡 **Thêm dịch vụ Localization**
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// 💡 **Thêm dịch vụ Session & Cache**
builder.Services.AddDistributedMemoryCache(); // Cần thiết để Session hoạt động
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session hết hạn sau 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// 💡 **Thêm Repository**
builder.Services.AddScoped<IUserAccountRepository, EFUserAccountRepository>();

// 💡 **Cấu hình MVC & View Localization**
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// 💡 **Cấu hình đa ngôn ngữ**
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("vi-VN"),  // Tiếng Việt (mặc định)
        new CultureInfo("en-US"),  // Tiếng Anh
        new CultureInfo("fr-FR")   // Tiếng Pháp
    };

    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

// 💡 **Áp dụng cài đặt ngôn ngữ**
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);

// 🔹 **Middleware xử lý lỗi**
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 💡 **Thêm Session vào pipeline**
app.UseRouting();
app.UseSession();  //Bắt buộc phải gọi trước `UseAuthorization`
app.UseAuthentication(); //  BẮT BUỘC: Xác thực người dùng
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
