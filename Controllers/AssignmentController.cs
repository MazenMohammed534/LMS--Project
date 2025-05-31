using LMSTT.Models;
using LMSTT.Services;
using LMSTT.ViewModels.Assignment;
using LMSTT.ViewModels.Teacher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LMSTT.Controllers
{
    [Authorize]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ICourseService _courseService;
        private readonly ILogger<AssignmentController> _logger;

        public AssignmentController(IAssignmentService assignmentService, ICourseService courseService, ILogger<AssignmentController> logger)
        {
            _assignmentService = assignmentService;
            _courseService = courseService;
            _logger = logger;
        }

        // Teacher Actions
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherAssignments()
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var assignments = await _assignmentService.GetTeacherAssignmentsAsync(teacherId);
            return View(assignments);
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create()
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var courses = await _courseService.GetCoursesByTeacherAsync(teacherId);
            ViewBag.Courses = new SelectList(courses, "Id", "Title");
            return View(new AssignmentViewModel
            {
                DueDate = DateTime.Now.AddDays(7)
            });
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(AssignmentViewModel model)
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _assignmentService.CreateAssignmentAsync(model, teacherId);

            if (result)
            {
                return RedirectToAction(nameof(TeacherAssignments));
            }

            var courses = await _courseService.GetCoursesByTeacherAsync(teacherId);
            ViewBag.Courses = new SelectList(courses, "Id", "Title");
            return View(model);
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
                return NotFound();

            return View(assignment);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var result = await _assignmentService.UpdateAssignmentAsync(model);

                if (result)
                {
                    return RedirectToAction(nameof(TeacherAssignments));
                }

                ModelState.AddModelError("", "Failed to update assignment");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating the assignment: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            await _assignmentService.DeleteAssignmentAsync(id);
            return RedirectToAction(nameof(TeacherAssignments));
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherCourseAssignments(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null)
                return NotFound();

            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var tasks = await _assignmentService.GetCourseAssignmentsAsync(id);

            var viewModel = new TeacherCourseAssignments
            {
                CourseId = id,
                CourseTitle = course.Title,
                Tasks = tasks
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherViewSubmission(int id)
        {
            var submissions = await _assignmentService.GetAssignmentSubmissionsAsync(id);
            if (submissions == null)
                return NotFound();

            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
                return NotFound();

            var viewModel = new TeacherViewSubmissionViewModel
            {
                AssignmentId = id,
                CourseTitle = assignment.CourseName,
                AssignmentTitle = assignment.Title,
                StudentSubmissions = submissions
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ApproveSubmission(int id)
        {
            var result = await _assignmentService.UpdateSubmissionStatusAsync(id, ActionStatus.Approved);
            if (!result)
                return NotFound();

            var submission = await _assignmentService.GetSubmissionByIdAsync(id);
            return RedirectToAction(nameof(TeacherViewSubmission), new { id = submission.AssignmentId });
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> RejectSubmission(int id)
        {
            var result = await _assignmentService.UpdateSubmissionStatusAsync(id, ActionStatus.Rejected);
            if (!result)
                return NotFound();

            var submission = await _assignmentService.GetSubmissionByIdAsync(id);
            return RedirectToAction(nameof(TeacherViewSubmission), new { id = submission.AssignmentId });
        }

        // Student Actions
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentTasks(int courseId)
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var viewModel = await _assignmentService.GetStudentCourseTasksAsync(courseId, studentId);
            
            if (viewModel == null)
                return NotFound();

            return View("StudentTasks", viewModel);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(int id)
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            
            if (assignment == null)
                return NotFound();

            var submission = await _assignmentService.GetStudentSubmissionAsync(id, studentId);
            if (submission != null && submission.CompletionStatus == SubmissionStatus.Completed && 
                submission.ActionStatus != ActionStatus.Rejected)
            {
                return RedirectToAction(nameof(StudentViewSubmission), new { id });
            }

            var model = new AssignmentSubmissionViewModel
            {
                AssignmentId = id,
                StudentId = studentId,
                Assignment = new Assignments 
                { 
                    Title = assignment.Title,
                    Description = assignment.Description,
                    DueDate = assignment.DueDate
                }
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(AssignmentSubmissionViewModel model)
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var assignment = await _assignmentService.GetAssignmentByIdAsync(model.AssignmentId);
            
            if (assignment == null)
                return NotFound();

            var result = await _assignmentService.SubmitAssignmentAsync(model.AssignmentId, studentId, model.SubmittedText, model.SubmissionFile);

            if (result)
                return RedirectToAction(nameof(StudentTasks), new { courseId = assignment.CourseId });

            return View(model);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentViewSubmission(int id)
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var submission = await _assignmentService.GetStudentSubmissionAsync(id, studentId);

            if (submission == null)
                return NotFound();

            return View(submission);
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherViewSubmissionStudent(int id)
        {
            var submission = await _assignmentService.GetSubmissionByIdAsync(id);
            
            if (submission == null)
                return NotFound();

            var assignmentViewModel = await _assignmentService.GetAssignmentByIdAsync(submission.AssignmentId);
            if (assignmentViewModel == null)
                return NotFound();

            // Map the assignment data to the Assignments model
            submission.Assignment = new Assignments
            {
                Id = assignmentViewModel.Id,
                Title = assignmentViewModel.Title,
                Description = assignmentViewModel.Description,
                DueDate = assignmentViewModel.DueDate,
                CourseId = assignmentViewModel.CourseId
            };

            return View(submission);
        }

        [Authorize(Roles = "Student,Teacher")]
        public async Task<IActionResult> DownloadSubmission(int id)
        {
            var submission = await _assignmentService.GetSubmissionByIdAsync(id);
            if (submission == null || string.IsNullOrEmpty(submission.StoredFileName))
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Tasks", submission.StoredFileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/octet-stream";
            var fileName = submission.SubmittedFileName;

            return PhysicalFile(filePath, contentType, fileName);
        }
    }
}