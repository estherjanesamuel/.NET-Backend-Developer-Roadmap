using Microsoft.EntityFrameworkCore;
using RazorMvcBasic.Data;

var builder = WebApplication.CreateBuilder(args);

// container DI
builder.Services.AddDbContext<GuestBookContext>(opt => {
    opt.UseInMemoryDatabase("GuestbookDataase");
});
builder.Services.AddRazorPages();
// builder.Services.AddControllersWithViews();

var app = builder.Build();
// app.MapDefaultControllerRoute();
app.MapRazorPages();

app.Run();
