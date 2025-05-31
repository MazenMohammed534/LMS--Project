using LMSTT.Models;
using LMSTT.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using LMSTT.ViewModels.Account;

namespace LMSTT.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private static readonly Dictionary<string, HashSet<string>> _userSessions = new();
        private static readonly object _lock = new();

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetByEmailAsync(model.Email);
            if (user == null || !await _userService.ValidateUserAsync(model.Email, model.Password))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            // Generate unique session ID
            var sessionId = Guid.NewGuid().ToString();

            // Track the session
            lock (_lock)
            {
                if (!_userSessions.ContainsKey(user.Id.ToString()))
                {
                    _userSessions[user.Id.ToString()] = new HashSet<string>();
                }
                _userSessions[user.Id.ToString()].Add(sessionId);
            }

            // Get user roles
            var roles = (await _userService.GetUserRolesAsync(user.Id)).ToList();

            // Set up claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("SessionId", sessionId)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24),
                AllowRefresh = true,
                // Use unique cookie name for this session
                Parameters = { { "CookieName", $".LMSTT.Auth.{sessionId}" } }
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Store session-specific data
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("SessionId", sessionId);

            // Redirect based on role
            if (roles.Any(r => r.Name == "Admin"))
                return RedirectToAction("AdminDashboard", "Admin");
            if (roles.Any(r => r.Name == "Teacher"))
                return RedirectToAction("TeacherDashboard", "Teacher");
            if (roles.Any(r => r.Name == "Student"))
                return RedirectToAction("Dashboard", "Student");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _userService.GetByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already in use.");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userService.AddAsync(user);
            await _userService.AssignStudentRoleAsync(user.Id);

            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sessionId = User.FindFirst("SessionId")?.Value;

            if (userId != null && sessionId != null)
            {
                lock (_lock)
                {
                    if (_userSessions.ContainsKey(userId))
                    {
                        _userSessions[userId].Remove(sessionId);
                        if (_userSessions[userId].Count == 0)
                        {
                            _userSessions.Remove(userId);
                        }
                    }
                }
            }

            // Clear the specific session cookie
            Response.Cookies.Delete($".LMSTT.Auth.{sessionId}");
            Response.Cookies.Delete(".LMSTT.Session");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
