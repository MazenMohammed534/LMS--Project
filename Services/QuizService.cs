using LMSTT.Data;
using LMSTT.Models;
using LMSTT.ViewModels.Quiz;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Text.Json;

namespace LMSTT.Services
{
    public class QuestionOptions
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
    }

    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;

        public QuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Quizzes>> GetAllQuizzesAsync()
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .ToListAsync();
        }

        public async Task<Quizzes> GetQuizByIdAsync(int id)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<int> CreateQuizAsync(CreateQuizViewModel model, int teacherId)
        {
            var quiz = new Quizzes
            {
                Title = model.Title,
                CourseId = model.CourseId,
                CreatedBy = teacherId,
                QuestionsNumbers = model.QuestionsNumber,
                DueDate = model.DueDate,
                TimeLimit = (int)model.TimeLimit.TotalMinutes,
                Status = model.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return quiz.Id;
        }

        public async Task<Questions> AddQuestionAsync(int quizId, AddQuestionViewModel model)
        {
            var questionOptions = new
            {
                QuestionText = model.QuestionText,
                Options = new[]
                {
                    model.Option1,
                    model.Option2,
                    model.Option3,
                    model.Option4
                }
            };

            var question = new Questions
            {
                QuizId = quizId,
                QuestionOptions = System.Text.Json.JsonSerializer.Serialize(questionOptions),
                CorrectAnswer = model.Answer,
                Points = model.Points
            };

            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<IEnumerable<Quizzes>> GetQuizzesByCourseAsync(int courseId)
        {
            return await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .Include(q => q.Questions)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsTeacherAuthorizedForQuiz(int quizId, int teacherId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            return quiz?.Course.TeacherId == teacherId;
        }

        public async Task<IEnumerable<Questions>> GetQuizQuestionsAsync(int quizId)
        {
            return await _context.Questions
                .Where(q => q.QuizId == quizId)
                .ToListAsync();
        }

        public async Task DeleteQuizAsync(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateQuizAsync(int id, EditQuizViewModel model)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                quiz.Title = model.Title;
                quiz.DueDate = model.DueDate;
                quiz.TimeLimit = (int)model.TimeLimit.TotalMinutes;
                quiz.Status = model.Status;

                await _context.SaveChangesAsync();
            }
        }

        // Student Quiz Methods
        public async Task<StudentQuizViewModel> GetQuizForStudent(int quizId, string userId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId && q.Status == "Published");

            if (quiz == null) return null;

            var studentId = int.Parse(userId);
            var submission = await _context.QuizSubmissions
                .Where(s => s.QuizId == quizId && s.StudentId == studentId)
                .FirstOrDefaultAsync();

            return new StudentQuizViewModel
            {
                Id = quiz.Id,
                Title = quiz.Title,
                DueDate = quiz.DueDate,
                QuestionsNumber = quiz.QuestionsNumbers,
                TimeLimit = quiz.TimeLimit,
                IsCompleted = submission?.CompletionStatus == "Completed",
                Score = submission?.Score ?? 0
            };
        }

        public async Task<QuizQuestionsViewModel> GetQuizQuestionsForStudent(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return null;

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

            return new QuizQuestionsViewModel
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                TimeLimit = quiz.TimeLimit,
                TotalQuestions = quiz.QuestionsNumbers,
                RemainingTime = quiz.TimeLimit * 60,
                Questions = questions
            };
        }

        public async Task<IEnumerable<StudentQuizViewModel>> GetQuizzesForStudent(int courseId, string userId)
        {
            var studentId = int.Parse(userId);
            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .Where(q => q.Status.Equals("Published"))
                .Include(q => q.Questions)
                .OrderBy(q => q.DueDate)
                .ToListAsync();

            // Get the latest submission for each quiz
            var submissions = await _context.QuizSubmissions
                .Where(s => s.StudentId == studentId)
                .GroupBy(s => s.QuizId)
                .Select(g => new 
                {
                    QuizId = g.Key,
                    CompletionStatus = g.Any(s => s.CompletionStatus.Equals("Completed")),
                    Score = g.Where(s => s.CompletionStatus == "Completed")
                           .GroupBy(s => s.SubmittedAt)
                           .OrderByDescending(g => g.Key)
                           .Select(g => g.Sum(s => s.Score))
                           .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.QuizId);

            return quizzes.Select(q => new StudentQuizViewModel
            {
                Id = q.Id,
                Title = q.Title,
                DueDate = q.DueDate,
                QuestionsNumber = q.QuestionsNumbers,
                TimeLimit = q.TimeLimit,
                IsCompleted = submissions.ContainsKey(q.Id) && submissions[q.Id].CompletionStatus,
                Score = submissions.ContainsKey(q.Id) ? submissions[q.Id].Score : 0
            }).ToList();
        }

        public async Task<(bool IsSuccess, int Score)> SubmitQuiz(int quizId, string userId, Dictionary<int, string> answers)
        {
            var studentId = int.Parse(userId);
            var totalScore = 0;

            // Get all questions for this quiz in order
            var questions = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.Id)
                .ToListAsync();

            // Process each answer
            foreach (var answer in answers)
            {
                var questionNumber = answer.Key; // This is the 1-based question number from the form
                if (questionNumber <= questions.Count)
                {
                    var question = questions[questionNumber - 1];
                    var selectedAnswer = answer.Value;
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
            return (true, totalScore);
        }

        public async Task<QuizEndViewModel> GetQuizResult(int quizId, string userId)
        {
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return null;

            var studentId = int.Parse(userId);
            var submissions = await _context.QuizSubmissions
                .Where(s => s.QuizId == quizId && s.StudentId == studentId)
                .ToListAsync();

            if (!submissions.Any()) return null;

            return new QuizEndViewModel
            {
                QuizTitle = quiz.Title,
                Score = submissions.Sum(s => s.Score),
                TotalQuestions = quiz.QuestionsNumbers,
                CourseId = quiz.CourseId
            };
        }
    }
} 