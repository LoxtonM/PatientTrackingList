using Microsoft.EntityFrameworkCore;
using PatientTrackingList.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(config.GetConnectionString("ConString")));


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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
