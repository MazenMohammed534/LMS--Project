using LMSTT.Data;
using LMSTT.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace LMSTT.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        public UserService(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            Console.WriteLine("\n=== Login Attempt ===");
            Console.WriteLine($"Email: {email}");
            
            var user = await GetByEmailAsync(email);
            if (user == null)
            {
                Console.WriteLine("User not found for email: " + email);
                return false;
            }

            Console.WriteLine($"Found user: {user.FullName} (ID: {user.Id})");
            
            // Check user roles
            var roles = await GetUserRolesAsync(user.Id);
            Console.WriteLine("User roles:");
            foreach (var role in roles)
            {
                Console.WriteLine($"- {role.Name}");
            }

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            var hashedInput = HashPassword(password);
            Console.WriteLine("\nPassword Validation:");
            Console.WriteLine($"Input password: {password}");
            Console.WriteLine($"Password bytes: {BitConverter.ToString(passwordBytes)}");
            Console.WriteLine($"Hashed input: {hashedInput}");
            Console.WriteLine($"Stored hash: {user.Password}");
            Console.WriteLine($"Match: {user.Password == hashedInput}");
            Console.WriteLine("===================\n");

            return user.Password == hashedInput;
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToListAsync();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                var hashedBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
                .ToListAsync();
        }

        public async Task AssignStudentRoleAsync(int userId)
        {
            try
            {
                Console.WriteLine($"AssignStudentRoleAsync called for userId: {userId}");
                var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
                if (studentRole == null)
                {
                    Console.WriteLine("Student role not found in the database.");
                    throw new InvalidOperationException("Student role not found in the database.");
                }

                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = studentRole.Id
                };

                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
                Console.WriteLine($"UserRole added: UserId={userId}, RoleId={studentRole.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignStudentRoleAsync: {ex.Message}");
                throw;
            }
        }

        public async Task AssignTeacherRoleAsync(int userId)
        {
            var teacherRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Teacher");
            if (teacherRole == null)
            {
                throw new InvalidOperationException("Teacher role not found in the database.");
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = teacherRole.Id
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task AssignAdminRoleAsync(int userId)
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                throw new InvalidOperationException("Admin role not found in the database.");
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = adminRole.Id
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }

        public override async Task<User> AddAsync(User entity)
        {
            return await base.AddAsync(entity);
        }
    }
}
