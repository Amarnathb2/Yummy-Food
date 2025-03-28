using Microsoft.AspNetCore.Authentication.Cookies;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using YummyFood.Models;

var builder = WebApplication.CreateBuilder(args);
// Configure MailKit
builder.Services.AddMailKit(config =>
{
    config.UseMailKit(new MailKitOptions
    {
        Server = builder.Configuration["MailKitOptions:Server"],
        Port = int.Parse(builder.Configuration["MailKitOptions:Port"]),
        SenderEmail = builder.Configuration["MailKitOptions:SenderEmail"],
        SenderName = builder.Configuration["MailKitOptions:SenderName"],
        Account = builder.Configuration["MailKitOptions:SenderEmail"],
        Password = builder.Configuration["MailKitOptions:Password"],
        Security = true // Enables StartTLS
    });
});
// ✅ Register EmailService for Dependency Injection
builder.Services.AddScoped<EmailService>();

builder.Services.AddSession();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/AccessDenied";
    });
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization();
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseAuthentication(); // Ensure this is BEFORE app.UseAuthorization()
app.UseAuthorization();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
