using LMSTT.Models;
using LMSTT.ViewModels.Quiz;

namespace LMSTT.Services
{
    public interface IQuizService
    {
        // Teacher Quiz Methods
        Task<IEnumerable<Quizzes>> GetAllQuizzesAsync();
        Task<Quizzes> GetQuizByIdAsync(int id);
        Task<int> CreateQuizAsync(CreateQuizViewModel model, int teacherId);
        Task<Questions> AddQuestionAsync(int quizId, AddQuestionViewModel model);
        Task<IEnumerable<Quizzes>> GetQuizzesByCourseAsync(int courseId);
        Task<bool> IsTeacherAuthorizedForQuiz(int quizId, int teacherId);
        Task<IEnumerable<Questions>> GetQuizQuestionsAsync(int quizId);
        Task DeleteQuizAsync(int id);
        Task UpdateQuizAsync(int id, EditQuizViewModel model);

        // Student Quiz Methods
        Task<StudentQuizViewModel> GetQuizForStudent(int quizId, string userId);
        Task<QuizQuestionsViewModel> GetQuizQuestionsForStudent(int quizId);
        Task<QuizEndViewModel> GetQuizResult(int quizId, string userId);
        Task<IEnumerable<StudentQuizViewModel>> GetQuizzesForStudent(int courseId, string userId);
        Task<(bool IsSuccess, int Score)> SubmitQuiz(int quizId, string userId, Dictionary<int, string> answers);
    }
} 