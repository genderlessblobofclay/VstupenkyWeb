using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using VstupenkyWeb.Extensions; // Add this line
using VstupenkyWeb.Logging;
using VstupenkyWeb.Models; // Add this line

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// **CORRECT LOCATION FOR SERVICE REGISTRATION**
Console.WriteLine($"Connection string: {builder.Configuration.GetConnectionString("VstupenkyDB")}");
Console.WriteLine($"SendGrid API Key: {builder.Configuration["SendGrid:ApiKey"]}");
builder.Services.AddTransient<VstupenkyWeb.Models.VstupenkyManager>();
builder.Services.AddTransient<LoginManager>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var passwordHasher = provider.GetRequiredService<IPasswordHasher<IdentityUser>>();
    var logger = provider.GetRequiredService<ILogger<LoginManager>>();
    return new LoginManager(configuration, passwordHasher, logger);
});
builder.Services.AddScoped<IPasswordHasher<IdentityUser>, PasswordHasher<IdentityUser>>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Index";
    });

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Enable Windows Event Log provider only on Windows
if (OperatingSystem.IsWindows())
{
    builder.Logging.AddEventLog();
}

// Add file logger
builder.Services.Configure<FileLoggerOptions>(builder.Configuration.GetSection("Logging:File"));
builder.Logging.AddFile(builder.Configuration.GetSection("Logging:File"));
builder.Services.Configure<ErrorFileLoggerOptions>(builder.Configuration.GetSection("Logging:ErrorFile"));
builder.Logging.AddErrorFile(builder.Configuration.GetSection("Logging:ErrorFile"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Place this before UseRouting
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseActionLogging();
app.UseGlobalExceptionHandling();

app.MapControllers();
app.MapRazorPages();

//app.UseHttpsRedirection();

//app.MapStaticAssets(); // Remove this line
//app.MapRazorPages() // Remove this line
//    .WithStaticAssets(); // Remove this line

app.Run();