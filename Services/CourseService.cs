using LMSTT.Data;
using LMSTT.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMSTT.Services
{
    public class CourseService : BaseService<Course>, ICourseService
    {
        public CourseService(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Teacher)
                .Include(c => c.Department)
                .Include(c => c.AcademicYear)
                .Include(c => c.Enrollments)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByTeacherAsync(int teacherId)
        {
            var query = _dbSet.AsQueryable();
            return await query
                .Where(c => c.TeacherId == teacherId && !c.IsArchived)
                .Include(c => c.Enrollments)
                .Include(c => c.Department)
                .Include(c => c.AcademicYear)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetEnrolledCoursesAsync(int studentId)
        {
            return await _dbSet
                .Include(c => c.Teacher)
                .Include(c => c.Department)
                .Include(c => c.AcademicYear)
                .Include(c => c.Enrollments)
                .Where(c => c.Enrollments.Any(e => e.StudentId == studentId))
                .ToListAsync();
        }

        public async Task<bool> EnrollStudentAsync(int courseId, int studentId)
        {
            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                EnrolledAt = DateTime.UtcNow
            };

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveStudentAsync(int courseId, int studentId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            if (enrollment == null) return false;

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateCourseCodeAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;

            do
            {
                code = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            } while (await _dbSet.AnyAsync(c => c.Code == code));

            return code;
        }

        public async Task<Course> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task<IEnumerable<Course>> GetArchivedCoursesAsync(int userId, string role)
        {
            var query = _dbSet.AsQueryable();
            query = query.Where(c => c.IsArchived);
            
            if (role == "Teacher")
            {
                query = query.Where(c => c.TeacherId == userId);
            }
            else if (role == "Student")
            {
                query = query.Where(c => c.Enrollments.Any(e => e.StudentId == userId));
            }

            return await query
                .Include(c => c.Enrollments)
                .Include(c => c.Department)
                .Include(c => c.AcademicYear)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetEnrolledStudentsAsync(int courseId)
        {
            return await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.Student)
                .Select(e => e.Student)
                .ToListAsync();
        }

        public IEnumerable<SelectListItem> GetAcademicYearSelectList()
        {
            return _context.AcademicYear
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .OrderBy(c => c.Text)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<SelectListItem> GetDepartmentSelectList()
        {
            return _context.Department
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .OrderBy(c => c.Text)
                .AsNoTracking()
                .ToList();
        }

        public async Task<bool> IsStudentEnrolledAsync(int courseId, int studentId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId);
        }
    }
}
