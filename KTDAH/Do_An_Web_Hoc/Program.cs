using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// 💡 **Thêm dịch vụ Localization**
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// 💡 **Thêm MVC và hỗ trợ ViewLocalizer**
builder.Services.AddControllersWithViews()
    .AddViewLocalization()  // 🌟 Bổ sung hỗ trợ IViewLocalizer
    .AddDataAnnotationsLocalization();  // Nếu bạn muốn hỗ trợ localization trong Validation Messages

// 💡 **Cấu hình đa ngôn ngữ**
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("vi-VN"),  // Tiếng Việt (mặc định)
        new CultureInfo("en-US"),  // Tiếng Anh
        new CultureInfo("fr-FR")   // Tiếng Pháp (nếu cần)
    };

    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");




app.UseStaticFiles();
app.Run();
