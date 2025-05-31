using LMSTT.Models;

namespace LMSTT.Services
{
    public interface IUserService : IBaseService<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> ValidateUserAsync(string email, string password);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
        Task<IEnumerable<Role>> GetUserRolesAsync(int id);
        Task AssignStudentRoleAsync(int userId);
        Task AssignTeacherRoleAsync(int userId);
        Task AssignAdminRoleAsync(int userId);
    }
}
