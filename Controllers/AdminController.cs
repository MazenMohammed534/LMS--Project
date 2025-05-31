using LMSTT.Data;
using LMSTT.Models;
using LMSTT.Services;
using LMSTT.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace LMSTT.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICourseService  _courseService;
        private readonly ApplicationDbContext _context;

        public AdminController(IUserService userService, ICourseService courseService , ApplicationDbContext context)
        {
            _userService = userService;
            _courseService = courseService;
            _context = context;
        }

        public async Task<IActionResult> AdminDashboard()
        {
            var viewModel = new AdminDashboardViewModel
            { 
                TotalTeachers = (await _userService.GetUsersByRoleAsync("Teacher")).Count(),
                TotalStudents = (await _userService.GetUsersByRoleAsync("Student")).Count(),
                TotalCourses = (await _courseService.GetAllAsync()).Count()
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userService.DeleteAsync(id);
            TempData["Success"] = "Admin deleted successfully!";
            return RedirectToAction("AdminView");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeacher(string email, string password, string fullName)
        {
            var existingUser = await _userService.GetByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["Error"] = "Email already in use.";
                return RedirectToAction(nameof(Users));
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                Password = HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userService.AddAsync(user);
            await _userService.AssignTeacherRoleAsync(user.Id);

            TempData["Success"] = "Teacher account created successfully.";
            return RedirectToAction(nameof(Users));
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<IActionResult> AdminView()
        {
            var admins = await _userService.GetUsersByRoleAsync("Admin");
            var viewModel = admins.Select(a => new AdminListItemViewModel
            {
                Id = a.Id,
                FullName = a.FullName,
                Email = a.Email
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AddAdmin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin(AddAdminViewModel model)
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
            await _userService.AssignAdminRoleAsync(user.Id);

            TempData["Success"] = "Admin added successfully!";
            return RedirectToAction("AdminView");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var viewModel = new EditAdminViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditAdminViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.Password = HashPassword(model.Password);
            }

            await _userService.UpdateAsync(user);

            TempData["Success"] = "Admin updated successfully!";
            return RedirectToAction("AdminView");
        }

        public async Task<IActionResult> TeacherView()
        {
            var teachers = await _userService.GetUsersByRoleAsync("Teacher");
            var viewModel = teachers.Select(t => new TeacherListItemViewModel
            {
                Id = t.Id,
                FullName = t.FullName,
                Email = t.Email
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AddTeacher()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTeacher(AddTeacherViewModel model)
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
            await _userService.AssignTeacherRoleAsync(user.Id);

            TempData["Success"] = "Teacher added successfully!";
            return RedirectToAction("TeacherView");
        }

        [HttpGet]
        public async Task<IActionResult> EditTeacher(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var viewModel = new EditTeacherViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditTeacher(EditTeacherViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.Password = HashPassword(model.Password);
            }

            await _userService.UpdateAsync(user);

            TempData["Success"] = "Teacher updated successfully!";
            return RedirectToAction("TeacherView");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            await _userService.DeleteAsync(id);
            TempData["Success"] = "Teacher deleted successfully!";
            return RedirectToAction("TeacherView");
        }

        public async Task<IActionResult> ViewStudent()
        {
            var students = await _userService.GetUsersByRoleAsync("Student");
            var viewModel = students.Select(s => new StudentListItemViewModel
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email
            }).ToList();
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(AddStudentViewModel model)
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

            TempData["Success"] = "Student added successfully!";
            return RedirectToAction("ViewStudent");
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var viewModel = new EditStudentViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(EditStudentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.Password = HashPassword(model.Password);
            }

            await _userService.UpdateAsync(user);
            TempData["Success"] = "Student updated successfully!";
            return RedirectToAction("ViewStudent");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            await _userService.DeleteAsync(id);
            TempData["Success"] = "Student deleted successfully!";
            return RedirectToAction("ViewStudent");
        }

        public async Task<IActionResult> ViewCourse()
        {
            var courses = await _courseService.GetAllAsync();
            return View(courses);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            await _courseService.DeleteAsync(id);
            TempData["Success"] = "Course deleted successfully!";
            return RedirectToAction(nameof(ViewCourse));
        }

        [HttpGet]
        public IActionResult AddDepartment()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddDepartment(AddDepartmentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var department = new Department { Name = model.Name };
            _context.Department.Add(department);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Department added successfully!";
            return RedirectToAction(nameof(ViewDepartments));
        }

        public async Task<IActionResult> ViewDepartments()
        {
            var departments = await _context.Department.ToListAsync();
            return View(departments);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Department.FindAsync(id);
            if (department != null)
            {
                _context.Department.Remove(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Department deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Department not found!";
            }
            return RedirectToAction(nameof(ViewDepartments));
        }
    }
}
