using LMSTT.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LMSTT.Services
{
    public interface ICourseService : IBaseService<Course>
    {
        Task<IEnumerable<Course>> GetCoursesByTeacherAsync(int teacherId);
        Task<IEnumerable<Course>> GetEnrolledCoursesAsync(int studentId);
        Task<bool> EnrollStudentAsync(int courseId, int studentId);
        Task<bool> RemoveStudentAsync(int courseId, int studentId);
        Task<string> GenerateCourseCodeAsync();
        Task<Course> GetByCodeAsync(string code);
        Task<IEnumerable<Course>> GetArchivedCoursesAsync(int userId, string role);
        Task<IEnumerable<User>> GetEnrolledStudentsAsync(int courseId);
        IEnumerable<SelectListItem> GetAcademicYearSelectList();
        IEnumerable<SelectListItem> GetDepartmentSelectList();
        Task<bool> IsStudentEnrolledAsync(int courseId, int studentId);
    }
}
