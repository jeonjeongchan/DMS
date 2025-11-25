using DMS.Data;
using DMS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(option => option.UseOracle(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(300);
});


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(300);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // CamelCase 강제하지 않고 C# 속성 그대로 사용
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.DictionaryKeyPolicy = null;

    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;

});

builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<DocumentClassService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<ApprovalService>();

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var path = context.Request.Path;

            // AJAX 요청이면 Error 페이지로 Redirect 하지 않고 JSON 반환
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Response.StatusCode = 500; // 기본 서버 오류
                context.Response.ContentType = "application/json";

                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    message = "서버 처리 중 오류가 발생했습니다."
                });
                await context.Response.WriteAsync(result);
            }
            else
            {
                // 뷰 요청일 경우 기존 Error 페이지로 이동
                context.Response.Redirect("/Home/Error");
            }
        });
    });

    app.UseHsts();
}



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
   context.Database.EnsureCreated();
    // DbInitializer.Initialize(context);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Drawing}/{action=DrawingMain}/{OID?}");
    pattern: "{controller=Main}/{action=DashBoard}/{OID?}");

app.Run();

