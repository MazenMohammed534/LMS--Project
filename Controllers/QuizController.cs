using LMSTT.Models;
using LMSTT.Services;
using LMSTT.ViewModels.Quiz;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using LMSTT.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LMSTT.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly IQuizService _quizService;
        private readonly ICourseService _courseService;
        private readonly ApplicationDbContext _context;

        public QuizController(IQuizService quizService, ICourseService courseService, ApplicationDbContext context)
        {
            _quizService = quizService;
            _courseService = courseService;
            _context = context;
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherQuizzesView()
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var courses = await _courseService.GetCoursesByTeacherAsync(teacherId);
            var allQuizzes = new List<TeacherQuizViewModel>();

            foreach (var course in courses)
            {
                var quizzes = await _quizService.GetQuizzesByCourseAsync(course.Id);
                allQuizzes.AddRange(quizzes.Select(q => new TeacherQuizViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    CourseName = course.Title,
                    DueDate = q.DueDate,
                    QuestionsNumber = q.QuestionsNumbers,
                    Status = q.Status,
                    TimeLimit = q.TimeLimit
                }));
            }

            ViewBag.TeacherName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Teacher";
            return View(allQuizzes);
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var courses = await _courseService.GetCoursesByTeacherAsync(teacherId);
                
                var viewModel = new CreateQuizViewModel
                {
                    CoursesList = new SelectList(courses, "Id", "Title"),
                    DueDate = DateTime.Now.AddDays(7),
                    TimeLimit = TimeSpan.FromHours(1),
                    QuestionsNumber = 1
                };
                return View(viewModel);
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(TeacherQuizzesView));
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateQuizViewModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var quizId = await _quizService.CreateQuizAsync(model, userId);
                
                if (quizId > 0)
                {
                    return RedirectToAction("AddQuestion", new { 
                        quizId = quizId, 
                        questionNumber = 1, 
                        totalQuestions = model.QuestionsNumber 
                    });
                }
                
                // If we get here, something went wrong with quiz creation
                var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var courses = await _courseService.GetCoursesByTeacherAsync(teacherId);
                model.CoursesList = new SelectList(courses, "Id", "Title");
                ModelState.AddModelError("", "Failed to create quiz. Please try again.");
                return View(model);
            }
            catch (Exception ex)
            {
                var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var courses = await _courseService.GetCoursesByTeacherAsync(teacherId);
                model.CoursesList = new SelectList(courses, "Id", "Title");
                ModelState.AddModelError("", "An error occurred while creating the quiz. Please try again.");
                return View(model);
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> AddQuestion(int questionNumber, int totalQuestions, int quizId)
        {
            try
            {
                // Verify the quiz exists
                var quiz = await _quizService.GetQuizByIdAsync(quizId);
                if (quiz == null)
                {
                    return RedirectToAction(nameof(TeacherQuizzesView));
                }

                var viewModel = new AddQuestionViewModel
                {
                    QuizId = quizId,
                    CurrentQuestionNumber = questionNumber,
                    TotalQuestions = totalQuestions
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(TeacherQuizzesView));
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(AddQuestionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _quizService.AddQuestionAsync(model.QuizId, model);

                if (model.CurrentQuestionNumber < model.TotalQuestions)
                {
                    return RedirectToAction(nameof(AddQuestion), new 
                    { 
                        quizId = model.QuizId, 
                        questionNumber = model.CurrentQuestionNumber + 1,
                        totalQuestions = model.TotalQuestions
                    });
                }

                return RedirectToAction(nameof(TeacherQuizzesView));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while saving the question. Please try again.");
                return View(model);
            }
        }

        [Authorize(Roles = "Teacher")]
        public async Task<bool> IsTeacherAuthorizedForQuiz(int quizId, int teacherId)
        {
            return await _quizService.IsTeacherAuthorizedForQuiz(quizId, teacherId);
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (!await IsTeacherAuthorizedForQuiz(id, teacherId))
                {
                    return RedirectToAction(nameof(TeacherQuizzesView));
                }

                await _quizService.DeleteQuizAsync(id);
                return RedirectToAction(nameof(TeacherQuizzesView));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(TeacherQuizzesView));
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (!await IsTeacherAuthorizedForQuiz(id, teacherId))
                {
                    return RedirectToAction(nameof(TeacherQuizzesView));
                }

                var quiz = await _quizService.GetQuizByIdAsync(id);
                if (quiz == null)
                {
                    return RedirectToAction(nameof(TeacherQuizzesView));
                }

                var viewModel = new EditQuizViewModel
                {
                    Id = quiz.Id,
                    Title = quiz.Title,
                    DueDate = quiz.DueDate,
                    TimeLimit = TimeSpan.FromMinutes(quiz.TimeLimit),
                    Status = quiz.Status
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(TeacherQuizzesView));
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditQuizViewModel model)
        {
            try
            {
                var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (!await IsTeacherAuthorizedForQuiz(id, teacherId))
                {
                    return RedirectToAction(nameof(TeacherQuizzesView));
                }

                await _quizService.UpdateQuizAsync(id, model);
                return RedirectToAction(nameof(TeacherQuizzesView));
            }
            catch (Exception)
            {
                return View(model);
            }
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> QuizResults(int id)
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Get the quiz with course info
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == id && q.Course.TeacherId == teacherId);

            if (quiz == null)
            {
                return NotFound();
            }

            // Get all enrolled students and their latest quiz submissions in one query
            var studentResults = await (
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.Id
                join e in _context.Enrollments on u.Id equals e.StudentId
                where r.Name == "Student" && e.CourseId == quiz.CourseId
                join qs in _context.QuizSubmissions 
                    on new { StudentId = u.Id, QuizId = id } 
                    equals new { StudentId = qs.StudentId, QuizId = qs.QuizId } 
                    into submissions
                from sub in submissions.DefaultIfEmpty()
                group new { u, sub } by new { u.Id, u.FullName } into g
                select new StudentQuizResultViewModel
                {
                    StudentId = g.Key.Id,
                    StudentName = g.Key.FullName,
                    Status = g.Any(x => x.sub != null && x.sub.CompletionStatus == "Completed") 
                        ? "Completed" 
                        : "In Progress",
                    Score = g.Where(x => x.sub != null && x.sub.CompletionStatus == "Completed")
                        .OrderByDescending(x => x.sub.SubmittedAt)
                        .Select(x => x.sub.Score)
                        .FirstOrDefault(),
                    SubmittedAt = g.Where(x => x.sub != null)
                        .OrderByDescending(x => x.sub.SubmittedAt)
                        .Select(x => x.sub.SubmittedAt)
                        .FirstOrDefault()
                }).ToListAsync();

            var viewModel = new QuizResultsViewModel
            {
                CourseTitle = quiz.Course.Title,
                QuizTitle = quiz.Title,
                DueDate = quiz.DueDate,
                TimeLimit = quiz.TimeLimit,
                QuestionsNumber = quiz.QuestionsNumbers,
                StudentResults = studentResults
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentQuizzes(int? id = null)
        {
            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (id.HasValue)
            {
                // Get quizzes for a specific course
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == id.Value);

                if (course == null)
                {
                    return NotFound();
                }

                var quizzes = await _context.Quizzes
                    .Where(q => q.CourseId == id.Value)
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
                            .GroupBy(qs => qs.SubmittedAt)
                            .OrderByDescending(g => g.Key)
                            .Select(g => g.Sum(s => s.Score))
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                ViewBag.CourseTitle = course.Title;
                return View("~/Views/Quiz/StudentQuizzes.cshtml", quizzes);
            }
            else
            {
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
                            .GroupBy(qs => qs.SubmittedAt)
                            .OrderByDescending(g => g.Key)
                            .Select(g => g.Sum(s => s.Score))
                            .FirstOrDefault()
                    })
                    .OrderByDescending(q => q.DueDate)
                    .ToListAsync();

                ViewBag.CourseTitle = "All Quizzes";
                return View("~/Views/Quiz/StudentQuizzes.cshtml", quizzes);
            }
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> SaveQuizSubmission([FromBody] QuizEndViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Title == model.QuizTitle && q.CourseId == model.CourseId);

            if (quiz == null)
            {
                return NotFound();
            }

            // Get the first question ID from the quiz
            var firstQuestionId = quiz.Questions.FirstOrDefault()?.Id ?? 0;
            if (firstQuestionId == 0)
            {
                return BadRequest("No questions found for this quiz");
            }

            var submission = new QuizSubmissions
            {
                QuizId = quiz.Id,
                StudentId = studentId,
                Score = model.Score,
                StartedAt = DateTime.UtcNow,
                SubmittedAt = DateTime.UtcNow,
                CompletionStatus = "Completed",
                Answer = "Completed",
                QuestionId = firstQuestionId  // Use the first question's ID
            };

            _context.QuizSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            
            return Ok();
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ViewSubmission(int quizId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Get the quiz with questions and options
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            // Get the student's submissions for this quiz
            var submissions = await _context.QuizSubmissions
                .Where(s => s.QuizId == quizId && s.StudentId == studentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            var questionList = new List<QuestionSubmissionViewModel>();
            var questionNumber = 1;

            foreach (var question in quiz.Questions.OrderBy(q => q.Id))
            {
                var studentAnswer = submissions
                    .FirstOrDefault(s => s.QuestionId == question.Id)?.Answer;

                var questionOptions = JsonSerializer.Deserialize<QuestionOptions>(question.QuestionOptions);

                questionList.Add(new QuestionSubmissionViewModel
                {
                    QuestionNumber = questionNumber++,
                    QuestionText = questionOptions.QuestionText,
                    Options = questionOptions.Options,
                    CorrectAnswer = question.CorrectAnswer,
                    StudentAnswer = studentAnswer ?? "Not Answered",
                    IsCorrect = studentAnswer == question.CorrectAnswer
                });
            }

            var viewModel = new QuizSubmissionDetailsViewModel
            {
                QuizTitle = quiz.Title,
                TimeLimit = quiz.TimeLimit,
                TotalQuestions = quiz.QuestionsNumbers,
                Score = submissions.FirstOrDefault()?.Score ?? 0,
                CourseId = quiz.CourseId,
                Questions = questionList
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> TakeQuiz(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var questions = new List<QuizQuestionItem>();
            var questionNumber = 1;

            foreach (var question in quiz.Questions.OrderBy(q => q.Id))
            {
                var options = JsonSerializer.Deserialize<QuestionOptions>(question.QuestionOptions);
                questions.Add(new QuizQuestionItem
                {
                    Id = question.Id,
                    QuestionText = options.QuestionText,
                    Options = options.Options,
                    QuestionNumber = questionNumber++
                });
            }

            var viewModel = new QuizQuestionsViewModel
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                TimeLimit = quiz.TimeLimit,
                TotalQuestions = quiz.QuestionsNumbers,
                RemainingTime = quiz.TimeLimit * 60,
                Questions = questions
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(Dictionary<int, string> answers, int remainingTime, int quizId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var studentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var totalScore = 0;

            // Get the quiz with questions
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return RedirectToAction("StudentQuizzes");
            }

            // Process each answer
            foreach (var answer in answers)
            {
                var questionNumber = answer.Key; // This is the question number (1-based)
                var selectedAnswer = answer.Value;

                // Get the question based on its position in the ordered list
                var question = quiz.Questions
                    .OrderBy(q => q.Id)
                    .Skip(questionNumber - 1)
                    .FirstOrDefault();

                if (question != null)
                {
                    // Compare the selected answer with the correct answer
                    var isCorrect = selectedAnswer.Trim().Equals(question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
                    var score = isCorrect ? question.Points : 0;
                    totalScore += score;

                    var submission = new QuizSubmissions
                    {
                        QuizId = quizId,
                        StudentId = studentId,
                        QuestionId = question.Id,
                        Answer = selectedAnswer,
                        Score = score,
                        StartedAt = DateTime.UtcNow,
                        SubmittedAt = DateTime.UtcNow,
                        CompletionStatus = "Completed"
                    };

                    _context.QuizSubmissions.Add(submission);
                }
            }

            await _context.SaveChangesAsync();

            var resultModel = new QuizEndViewModel
            {
                QuizTitle = quiz.Title,
                Score = totalScore,
                TotalQuestions = quiz.QuestionsNumbers,
                StudentName = User.Identity.Name,
                CourseId = quiz.CourseId
            };

            return View("QuizResult", resultModel);
        }
    }
} 