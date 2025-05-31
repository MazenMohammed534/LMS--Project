using LMSTT.Models;
using LMSTT.Services;
using LMSTT.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using LMSTT.Data;
using System.IO;
using System.Text.Json;
using LMSTT.ViewModels.Quiz;
using LMSTT.ViewModels;

namespace LMSTT.Controllers
{
    [Authorize(Policy = "RequireStudentRole")]
    public class StudentController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly IQuizService _quizService;
        //private readonly IMaterialService _materialService;

        public StudentController(
            ICourseService courseService,
            IUserService userService,
            ApplicationDbContext context,
            IQuizService quizService)
        {
            _courseService = courseService;
            _userService = userService;
            _context = context;
            _quizService = quizService;
        }
        public async Task<IActionResult> Dashboard()
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var enrolledCourses = await _courseService.GetEnrolledCoursesAsync(studentId);

            // Get total quizzes from all enrolled courses that are published
            var totalQuizzes = await _context.Quizzes
                .Where(q => q.Course.Enrollments.Any(e => e.StudentId == studentId) && q.Status == "Published")
                .CountAsync();

            var viewModel = new StudentDashboardViewModel
            {
                TotalCourses = enrolledCourses.Count(),
                TotalTasks = await _context.Assignments
                    .Where(a => a.Course.Enrollments.Any(e => e.StudentId == studentId))
                    .CountAsync(),
                TotalQuizzes = totalQuizzes
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Courses()
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var enrolledCourses = await _courseService.GetEnrolledCoursesAsync(studentId);
            
            // Make sure we're loading all related data
            var currentCourses = enrolledCourses
                .Where(c => !c.IsArchived)
                .Select(c => new CourseCardViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Code = c.Code,
                    TeacherName = c.Teacher?.FullName ?? "N/A",
                    Cover = string.IsNullOrEmpty(c.cover) ? "/assests/images/courses/default-course-cover.jpg" : c.cover,
                    CreatedAt = c.Created_at,
                    EnrollmentsCount = c.Enrollments?.Count ?? 0,
                    DepartmentName = c.Department?.Name ?? "N/A",
                    AcademicYearName = c.AcademicYear?.Name ?? "N/A"
                });

            var viewModel = new CourseListViewModel
            {
                Courses = currentCourses,
                IsArchived = false,
                PageTitle = "Current Courses"
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ArchivedCourses()
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var enrolledCourses = await _courseService.GetEnrolledCoursesAsync(studentId);
            
            // Make sure we're loading all related data
            var archivedCourses = enrolledCourses
                .Where(c => c.IsArchived)
                .Select(c => new CourseCardViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Code = c.Code,
                    TeacherName = c.Teacher?.FullName ?? "N/A",
                    Cover = string.IsNullOrEmpty(c.cover) ? "/assests/images/courses/default-course-cover.jpg" : c.cover,
                    CreatedAt = c.Created_at,
                    EnrollmentsCount = c.Enrollments?.Count ?? 0,
                    DepartmentName = c.Department?.Name ?? "N/A",
                    AcademicYearName = c.AcademicYear?.Name ?? "N/A"
                });

            var viewModel = new CourseListViewModel
            {
                Courses = archivedCourses,
                IsArchived = true,
                PageTitle = "Archived Courses"
            };

            return View("Courses", viewModel);
        }

        [HttpGet]
        public IActionResult JoinCourse()
        {
            return View(new JoinCourseViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinCourse(JoinCourseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var course = await _courseService.GetByCodeAsync(model.CourseCode);
            if (course == null)
            {
                ModelState.AddModelError("CourseCode", "Invalid course code.");
                return View(model);
            }

            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            var enrolledCourses = await _courseService.GetEnrolledCoursesAsync(studentId);
            if (enrolledCourses.Any(c => c.Id == course.Id))
            {
                ModelState.AddModelError("CourseCode", "You are already enrolled in this course.");
                return View(model);
            }

            await _courseService.EnrollStudentAsync(course.Id, studentId);
            TempData["Success"] = "Successfully joined the course!";
            return RedirectToAction(nameof(Courses));
        }

        public async Task<IActionResult> SubjectCourse(int id)
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Include(c => c.Materials)
                .FirstOrDefaultAsync(c => c.Id == id && c.Enrollments.Any(e => e.StudentId == studentId));

            if (course == null)
                return NotFound();

            // Verify that the student is enrolled in this course
            var enrolledCourses = await _courseService.GetEnrolledCoursesAsync(studentId);
            if (!enrolledCourses.Any(c => c.Id == id))
                return Forbid();

            // Get count of published quizzes
            var publishedQuizzesCount = await _context.Quizzes
                .Where(q => q.CourseId == id && q.Status == "Published")
                .CountAsync();

            // Get task count for this course
            var taskCount = await _context.Assignments
                .Where(a => a.CourseId == id)
                .CountAsync();

            var viewModel = new StudentSubjectCourseViewModel
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                MaterialCount = course.Materials?.Count ?? 0,
                QuizCount = publishedQuizzesCount,
                TaskCount = taskCount,
                //DiscussionCount = course.Discussions?.Count ?? 0,

                // Set up navigation URLs
                CourseMaterialsUrl = Url.Action("StudentCourseMaterials", "Student", new { id = course.Id }),
                CourseQuizzesUrl = Url.Action("StudentQuizzes", "Quiz", new { id = course.Id }),
                CourseTasksUrl = Url.Action("CourseAssignments", "Assignment", new { id = course.Id }),
                CourseDiscussionsUrl = Url.Action("Discussion", "Discussion", new { id = course.Id })
            };

            return View(viewModel); 
        }

        [HttpGet]
        public async Task<IActionResult> StudentCourseMaterials(int id)
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Verify student is enrolled in the course
            var enrollment = await _context.Enrollments
                .AnyAsync(e => e.CourseId == id && e.StudentId == studentId);

            if (!enrollment)
                return Forbid();

            var materials = await _context.CourseMaterials
                .Where(m => m.CourseId == id)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();

            ViewBag.CourseId = id;
            return View(materials);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadMaterial(string fileName, int courseId)
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Verify student is enrolled in the course
            var enrollment = await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            if (!enrollment)
                return Forbid();

            var material = await _context.CourseMaterials
                .FirstOrDefaultAsync(m => m.StoredFileName == fileName && m.CourseId == courseId);

            if (material == null)
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Materials", material.StoredFileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, material.ContentType, material.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> CourseQuizzes(int id)  // id is courseId
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Get the course
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // Get all quizzes for this course and check student's submissions
            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == id)
                .Select(q => new StudentQuizViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    DueDate = q.DueDate,
                    QuestionsNumber = q.QuestionsNumbers,
                    Status = q.Status,
                    TimeLimit = q.TimeLimit,
                    IsCompleted = _context.QuizSubmissions
                        .Any(qs => qs.QuizId == q.Id && 
                                 qs.StudentId == studentId && 
                                 qs.CompletionStatus == "Completed"),
                    Score = _context.QuizSubmissions
                        .Where(qs => qs.QuizId == q.Id && 
                                   qs.StudentId == studentId && 
                                   qs.CompletionStatus == "Completed")
                        .Sum(qs => qs.Score)
                })
                .ToListAsync();

            ViewBag.CourseTitle = course.Title;
            return View("~/Views/Quiz/StudentQuizzes.cshtml", quizzes);
        }

        // Add this new action to handle /Quiz/StudentQuizzes
        [HttpGet]
        public async Task<IActionResult> StudentQuizzes()
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Get all quizzes for all courses the student is enrolled in
            var quizzes = await _context.Quizzes
                .Where(q => q.Course.Enrollments.Any(e => e.StudentId == studentId))
                .Select(q => new StudentQuizViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    DueDate = q.DueDate,
                    QuestionsNumber = q.QuestionsNumbers,
                    Status = q.Status,
                    TimeLimit = q.TimeLimit,
                    IsCompleted = _context.QuizSubmissions
                        .Any(qs => qs.QuizId == q.Id && 
                                 qs.StudentId == studentId && 
                                 qs.CompletionStatus == "Completed"),
                    Score = _context.QuizSubmissions
                        .Where(qs => qs.QuizId == q.Id && 
                                   qs.StudentId == studentId && 
                                   qs.CompletionStatus == "Completed")
                        .Sum(qs => qs.Score)
                })
                .OrderByDescending(q => q.DueDate)
                .ToListAsync();

            ViewBag.CourseTitle = "All Quizzes";
            return View("~/Views/Quiz/StudentQuizzes.cshtml", quizzes);
        }

        // GET: Student/TakeQuiz/5
        public async Task<IActionResult> TakeQuiz(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var quiz = await _quizService.GetQuizForStudent(id, userId);
            
            if (quiz == null)
                return NotFound();

            if (quiz.IsCompleted)
                return RedirectToAction(nameof(QuizResult), new { id });

            var questions = await _quizService.GetQuizQuestionsForStudent(id);
            return View("~/Views/Quiz/TakeQuiz.cshtml", questions);
        }

        // POST: Student/SubmitAnswer
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(Dictionary<int, string> answers, int remainingTime, int quizId)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _quizService.SubmitQuiz(quizId, userId, answers);

            return RedirectToAction(nameof(QuizResult), new { id = quizId });
        }

        // GET: Student/QuizResult/5
        public async Task<IActionResult> QuizResult(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _quizService.GetQuizResult(id, userId);

            if (result == null)
                return NotFound();

            var viewModel = new QuizEndViewModel
            {
                QuizTitle = result.QuizTitle,
                Score = result.Score,
                TotalQuestions = result.TotalQuestions,
                StudentName = User.FindFirst(ClaimTypes.Name)?.Value,
                CourseId = result.CourseId
            };

            return View("~/Views/Quiz/QuizResult.cshtml", viewModel);
        }

        // GET: Student/ViewQuestions/5
        public async Task<IActionResult> ViewQuestions(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var questions = await _quizService.GetQuizQuestionsForStudent(id);
            
            if (questions == null)
                return NotFound();

            return View("~/Views/Quiz/TakeQuiz.cshtml", questions);
        }
    }
}
