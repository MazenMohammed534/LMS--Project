using LMSTT.Models;
using LMSTT.Services;
using LMSTT.ViewModels.Admin;
using LMSTT.ViewModels.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using LMSTT.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using LMSTT.Settings;
using LMSTT.ViewModels.CourseMaterial;
using System.Text.Json;
using LMSTT.ViewModels.Quiz;

namespace LMSTT.Controllers
{
    [Authorize(Policy = "RequireTeacherRole")]
    public class TeacherController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly IAssignmentService _assignmentService;

        public TeacherController(
            ICourseService courseService,
            IUserService userService,
            ApplicationDbContext context,
            IAssignmentService assignmentService)
        {
            _courseService = courseService;
            _userService = userService;
            _context = context;
            _assignmentService = assignmentService;
        }

        public async Task<IActionResult> TeacherDashboard()
        {
            var teacherIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(teacherIdStr, out var teacherId))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacher = await _userService.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                return NotFound();
            }

            // Get teacher's courses
            var teacherCourses = await _context.Courses
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();

            // Get students enrolled in teacher's courses
            var enrolledStudents = await _context.Enrollments
                .Where(e => teacherCourses.Select(c => c.Id).Contains(e.CourseId))
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            // Get total quizzes for the teacher's courses
            var courseIds = teacherCourses.Select(c => c.Id).ToList();
            var totalQuizzes = await _context.Quizzes
                .Where(q => courseIds.Contains(q.CourseId))
                .CountAsync();

            // Get total tasks (assignments) for the teacher's courses
            var totalTasks = await _context.Assignments
                .Where(a => courseIds.Contains(a.CourseId))
                .CountAsync();

            var viewModel = new TeacherDashboardViewModel
            {
                TeacherId = teacherId.ToString(),
                TeacherName = teacher.FullName,
                TotalCourses = teacherCourses.Count,
                TotalStudents = enrolledStudents,
                TotalTasks = totalTasks,
                TotalQuizzes = totalQuizzes
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Students()
        {
            var teacherIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(teacherIdStr, out var teacherId))
                return RedirectToAction("Login", "Account");

            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacherId)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .ToListAsync();

            var studentList = new List<TeacherStudentListItemViewModel>();
            foreach (var course in courses)
            {
                foreach (var enrollment in course.Enrollments)
                {
                    studentList.Add(new TeacherStudentListItemViewModel
                    {
                        StudentId = enrollment.Student.Id,
                        StudentName = enrollment.Student.FullName,
                        CourseId = course.Id,
                        CourseName = course.Title
                    });
                }
            }

            return View(studentList);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStudent(int studentId, int courseId)
        {
            await _courseService.RemoveStudentAsync(courseId, studentId);
            return RedirectToAction("Students");
        }

        public async Task<IActionResult> Courses()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var courses = await _courseService.GetCoursesByTeacherAsync(userId);

            var viewModel = new CourseListViewModel
            {
                Courses = courses.Select(c => new CourseCardViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Code = c.Code,
                    Cover = string.IsNullOrEmpty(c.cover) ? "/assests/images/courses/default-course-cover.jpg" : c.cover,
                    CreatedAt = c.Created_at,
                    EnrollmentsCount = c.Enrollments.Count,
                    DepartmentName = c.Department?.Name ?? "N/A",
                    AcademicYearName = c.AcademicYear?.Name ?? "N/A"
                }),
                IsArchived = false,
                PageTitle = "Current Courses"
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ArchivedCourses()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var courses = await _courseService.GetArchivedCoursesAsync(userId, "Teacher");

            var viewModel = new CourseListViewModel
            {
                Courses = courses.Select(c => new CourseCardViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Code = c.Code,
                    Cover = string.IsNullOrEmpty(c.cover) ? "/assests/images/courses/default-course-cover.jpg" : c.cover,
                    CreatedAt = c.Created_at,
                    EnrollmentsCount = c.Enrollments.Count,
                    DepartmentName = c.Department?.Name ?? "N/A",
                    AcademicYearName = c.AcademicYear?.Name ?? "N/A"
                }),
                IsArchived = true,
                PageTitle = "Archived Courses"
            };

            return View("Courses", viewModel);
        }

        [HttpGet]
        public IActionResult AddCourse()
        {
            AddCourseViewModel viewModel = new()
            {
                AcademicYears = _courseService.GetAcademicYearSelectList(),
                Departments = _courseService.GetDepartmentSelectList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(AddCourseViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Repopulate dropdown lists
                    model.AcademicYears = _courseService.GetAcademicYearSelectList();
                    model.Departments = _courseService.GetDepartmentSelectList();
                    return View(model);
                }

                var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var course = new Course
                {
                    Title = model.Title.Trim(),
                    Code = model.Code.Trim(),
                    TeacherId = teacherId,
                    Created_at = DateTime.UtcNow,
                    IsArchived = false,
                    AcademicYearId = model.AcademicYearId,
                    DepartmentId = model.DepartmentId,
                    cover = "/assests/images/courses/default-course-cover.jpg" // Update path to match assets folder
                };

                // Handle file upload only if a cover image was provided
                if (model.cover != null && model.cover.Length > 0)
                {
                    // Validate file if uploaded
                    if (!FileSettings.AllowedExtensions.Split(',').Contains(Path.GetExtension(model.cover.FileName).ToLower()))
                    {
                        ModelState.AddModelError("cover", "Invalid file type. Allowed types: " + FileSettings.AllowedExtensions);
                        model.AcademicYears = _courseService.GetAcademicYearSelectList();
                        model.Departments = _courseService.GetDepartmentSelectList();
                        return View(model);
                    }

                    if (model.cover.Length > FileSettings.MaxFileSizeInBytes)
                    {
                        ModelState.AddModelError("cover", $"File size cannot exceed {FileSettings.MaxFileSizeInMB} MB");
                        model.AcademicYears = _courseService.GetAcademicYearSelectList();
                        model.Departments = _courseService.GetDepartmentSelectList();
                        return View(model);
                    }

                    // Create a unique filename
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.cover.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assests", "images", "courses", fileName);
                    
                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.cover.CopyToAsync(stream);
                    }

                    // Update the cover path
                    course.cover = $"/assests/images/courses/{fileName}";
                }

                await _context.Courses.AddAsync(course);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Course added successfully!";
                return RedirectToAction(nameof(Courses));
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while adding the course. Please try again.");
                model.AcademicYears = _courseService.GetAcademicYearSelectList();
                model.Departments = _courseService.GetDepartmentSelectList();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateCode()
        {
            var code = await _courseService.GenerateCourseCodeAsync();
            return Json(code);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
                // Get the course
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacherId);

                if (course == null)
                {
                    TempData["Error"] = "Course not found or you don't have permission to delete it.";
                    return RedirectToAction(nameof(Courses));
                }

                // Delete the course cover image if it exists and is not the default
                if (!string.IsNullOrEmpty(course.cover) && 
                    !course.cover.EndsWith("default-course-cover.jpg"))
                {
                    var imagePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        course.cover.TrimStart('/')
                    );
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Remove the course
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Course deleted successfully!";
                return RedirectToAction(nameof(Courses));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the course.";
                return RedirectToAction(nameof(Courses));
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var model = new UpdateCourseViewModel
            {
                Id = course.Id,
                Title = course.Title,
                DepartmentId = course.DepartmentId,
                AcademicYearId = course.AcademicYearId,
                Departments = new SelectList(_courseService.GetDepartmentSelectList(), "Value", "Text"),
                AcademicYears = new SelectList(_courseService.GetAcademicYearSelectList(), "Value", "Text")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCourse(UpdateCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = new SelectList(_courseService.GetDepartmentSelectList(), "Value", "Text");
                model.AcademicYears = new SelectList(_courseService.GetAcademicYearSelectList(), "Value", "Text");
                return View(model);
            }

            var course = await _context.Courses.FindAsync(model.Id);
            if (course == null)
            {
                return NotFound();
            }

            course.Title = model.Title.Trim();
            course.DepartmentId = model.DepartmentId;
            course.AcademicYearId = model.AcademicYearId;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Courses));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while updating the course.");
                model.Departments = new SelectList(_courseService.GetDepartmentSelectList(), "Value", "Text");
                model.AcademicYears = new SelectList(_courseService.GetAcademicYearSelectList(), "Value", "Text");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SubjectCourse(int id)
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Get the course with related data
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Include(c => c.Materials)
                .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacherId);

            if (course == null)
            {
                return RedirectToAction(nameof(SubjectCourse));
            }

            // Get quiz count for this course
            var quizCount = await _context.Quizzes
                .Where(q => q.CourseId == course.Id)
                .CountAsync();

            // Get task count for this course
            var tasks = await _assignmentService.GetCourseAssignmentsAsync(id);
            var taskCount = tasks.Count;

            var viewModel = new SubjectCourseViewModel
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                CourseCode = course.Code,
                TeacherName = course.Teacher?.FullName ?? "N/A",
                CourseCover = string.IsNullOrEmpty(course.cover) ?
                    "/assests/images/courses/default-course-cover.jpg" : course.cover,

                // Statistics
                StudentCount = course.Enrollments.Count,
                MaterialCount = course.Materials.Count,
                QuizCount = quizCount,
                TaskCount = taskCount,

                // Navigation URLs
                CourseStudentsUrl = Url.Action("CourseStudent", "Teacher", new { id = course.Id }) ?? "#",
                CourseMaterialsUrl = Url.Action("TeacherCourseMaterial", "Teacher", new { id = course.Id }) ?? "#",
                CourseQuizzesUrl = Url.Action("CourseQuizzes", "Teacher", new { id = course.Id }) ?? "#",
                CourseTasksUrl = Url.Action("TeacherCourseAssignments", "Assignment", new { id = course.Id }) ?? "#"
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CourseStudent(int id)
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var course = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return RedirectToAction(nameof(SubjectCourse), new { id = id });
            }

            var students = course.Enrollments.Select(e => new CourseStudentViewModel
            {
                StudentId = e.Student.Id,
                StudentName = e.Student.FullName,
                CourseId = id,
                CourseName = course.Title
            }).ToList();

            ViewBag.CourseId = id;

            return View(students);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStudentFromCourse(int studentId, int courseId)
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacherId);

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Student removed from course successfully.";
            }

            return RedirectToAction(nameof(CourseStudent), new { id = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> TeacherCourseMaterial(int id)
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var course = await _context.Courses
                .Include(c => c.Materials)
                .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacherId);

            if (course == null)
                return NotFound();

            ViewBag.CourseId = id;
            return View(course.Materials);
        }

        [HttpGet]
        public IActionResult UploadMaterial(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View(new UploadFilesFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMaterial(UploadFilesFormViewModel model, int courseId)
        {
            if (!ModelState.IsValid)
                return View(model);

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return NotFound();

            // Create Materials directory if it doesn't exist
            var materialsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Materials");
            if (!Directory.Exists(materialsPath))
                Directory.CreateDirectory(materialsPath);

            List<CourseMaterial> uploadedFiles = new();
            foreach (var file in model.Files)
            {
                var fakeFileName = Path.GetRandomFileName();

                CourseMaterial uploadedFile = new()
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    StoredFileName = fakeFileName,
                    CourseId = courseId,
                    UploadedAt = DateTime.UtcNow
                };

                var path = Path.Combine(materialsPath, fakeFileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                uploadedFiles.Add(uploadedFile);
            }

            _context.AddRange(uploadedFiles);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(TeacherCourseMaterial), new { id = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMaterial(string fileName, int courseId)
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            var material = await _context.CourseMaterials
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.StoredFileName == fileName && 
                                        m.CourseId == courseId && 
                                        m.Course.TeacherId == teacherId);

            if (material == null)
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Materials", material.StoredFileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.CourseMaterials.Remove(material);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(TeacherCourseMaterial), new { id = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> CourseQuizzes(int id)
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Get the course with related quizzes
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacherId);

            if (course == null)
            {
                return RedirectToAction(nameof(Courses));
            }

            // Get quizzes for this course
            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == id)
                .Select(q => new TeacherQuizViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    DueDate = q.DueDate,
                    QuestionsNumber = q.QuestionsNumbers,
                    Status = q.Status,
                    TimeLimit = q.TimeLimit
                })
                .ToListAsync();

            ViewBag.CourseTitle = course.Title;
            ViewBag.CourseId = id;
            return View(quizzes);
        }

    }
}
