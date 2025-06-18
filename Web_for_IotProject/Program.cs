using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web_for_IotProject.Data;
using Microsoft.Extensions.DependencyInjection;
using Web_for_IotProject.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AplicationDbContext") ?? throw new InvalidOperationException("Connection string 'AplicationDbContext' not found.")));

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

//builder.Services.AddDefaultIdentity<Web_for_IotProjectUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
// Đăng ký UserRepository trong DI
builder.Services.AddSingleton<UserRepository>();

// Đăng ký UserRepository với Dependency Injection

//builder.Services.AddScoped<UserRepository>();    need further information to determine remove  or not

// Đăng ký cấu hình database
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Startup.cs hoặc Program.cs — bật session
builder.Services.AddSession();

builder.Services.AddSingleton<SshService>();

builder.Services.AddHttpClient();

// sign up the device service
builder.Services.AddSingleton<DeviceRepository>();

/* websocket edition*/
builder.Services.AddSingleton<StreamRelay>();
builder.Services.AddSingleton<ControlRelay>();
/* websocket edition*/

/* udp edition*/

builder.Services.AddHostedService<UdpFrameReceiver>();

/* udp edition*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseWebSockets();

app.UseRouting();

app.MapControllers();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.UseSession();

app.Run();
