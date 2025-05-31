using LMSTT.Data;
using LMSTT.Hubs;
using LMSTT.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Temporary code to generate hash - Remove after use
string adminPassword = "Admin@123";
string teacherPassword = "Teacher@123";

using (var sha256 = SHA256.Create())
{
    byte[] passwordBytes1 = Encoding.UTF8.GetBytes(adminPassword);
    var hashedBytes1 = sha256.ComputeHash(passwordBytes1);
    var hash1 = Convert.ToBase64String(hashedBytes1);
    Console.WriteLine("=== Password Hash Generation ===");
    Console.WriteLine($"Input password: {teacherPassword}");
    Console.WriteLine($"Password bytes: {BitConverter.ToString(passwordBytes1)}");
    Console.WriteLine($"Hashed bytes: {BitConverter.ToString(hashedBytes1)}");
    Console.WriteLine($"Final hash: {hash1}");
    Console.WriteLine("==============================");

    byte[] passwordBytes = Encoding.UTF8.GetBytes(teacherPassword);
    var hashedBytes = sha256.ComputeHash(passwordBytes);
    var hash = Convert.ToBase64String(hashedBytes);
    Console.WriteLine("=== Password Hash Generation ===");
    Console.WriteLine($"Input password: {teacherPassword}");
    Console.WriteLine($"Password bytes: {BitConverter.ToString(passwordBytes)}");
    Console.WriteLine($"Hashed bytes: {BitConverter.ToString(hashedBytes)}");
    Console.WriteLine($"Final hash: {hash}");
    Console.WriteLine("==============================");
}

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".LMSTT.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddScoped<LMSTT.Services.IUserService, LMSTT.Services.UserService>();
builder.Services.AddScoped<LMSTT.Services.ICourseService, LMSTT.Services.CourseService>();
builder.Services.AddScoped<LMSTT.Services.IQuizService, LMSTT.Services.QuizService>();
builder.Services.AddScoped<LMSTT.Services.IAssignmentService, LMSTT.Services.AssignmentService>();
// Add the discussion service
builder.Services.AddScoped<IDiscussionService, DiscussionService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = ".LMSTT.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.Cookie.Path = "/";
        
        // Allow multiple sessions
        options.Cookie.Name = $".LMSTT.Auth.{Guid.NewGuid()}";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireTeacherRole", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("RequireStudentRole", policy => policy.RequireRole("Student"));
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hub before other endpoints
app.MapHub<DiscussionHub>("/discussionHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
