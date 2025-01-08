using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PatientTrackingList.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("secrets.json", optional: false)
    .Build();
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddDbContext<ClinicalContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/UserLogin";
    });
builder.Services.AddRazorPages();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("secrets.json");

var directoryInfo = new DirectoryInfo(@"C:\Websites\Authentication");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(directoryInfo)
    .SetApplicationName("GeneticsWebAppHome");

builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.Name = ".AspNet.SharedCookie";
    options.Cookie.Path = "/";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{id?}");

app.MapRazorPages();

app.Run();
