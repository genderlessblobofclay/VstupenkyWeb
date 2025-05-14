using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// **SPRÁVNÉ UMÍSTĚNÍ REGISTRACE SLUŽBY**
builder.Services.AddTransient<VstupenkyWeb.Models.VstupenkyManager>();
builder.Services.AddTransient<VstupenkyWeb.Models.LoginManager>();

builder.Services.AddHttpContextAccessor();



builder.Services.AddScoped<IPasswordHasher<IdentityUser>, PasswordHasher<IdentityUser>>();

builder.Services.AddRazorPages();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Index";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
//app.UseHttpsRedirection();


app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();

// **TOTO JE ŠPATNĚ - PO app.Build() UŽ NEMŮŽEŠ PŘIDÁVAT SLUŽBY**
// builder.Services.AddTransient<VstupenkyWeb.Models.VstupenkyManager>();