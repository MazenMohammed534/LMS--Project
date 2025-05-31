using LMSTT.Data;
using LMSTT.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;
using LMSTT.ViewModels.Assignment;
using Microsoft.AspNetCore.Http;

namespace LMSTT.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(ApplicationDbContext context, ILogger<AssignmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AssignmentViewModel>> GetTeacherAssignmentsAsync(int teacherId)
        {
            return await _context.Assignments
                .Where(a => a.CreatedBy == teacherId)
                .Include(a => a.Course)
                .Include(a => a.Creator)
                .Select(a => new AssignmentViewModel
                {
                    Id = a.Id,
                    CourseId = a.CourseId,
                    Title = a.Title,
                    Description = a.Description,
                    DueDate = a.DueDate,
                    CourseName = a.Course.Title,
                    TeacherName = a.Creator.FullName,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<AssignmentViewModel>> GetCourseAssignmentsAsync(int courseId)
        {
            return await _context.Assignments
                .Where(a => a.CourseId == courseId)
                .Include(a => a.Course)
                .Include(a => a.Creator)
                .Select(a => new AssignmentViewModel
                {
                    Id = a.Id,
                    CourseId = a.CourseId,
                    Title = a.Title,
                    Description = a.Description,
                    DueDate = a.DueDate,
                    CourseName = a.Course.Title,
                    TeacherName = a.Creator.FullName,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<AssignmentViewModel> GetAssignmentByIdAsync(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Creator)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) return null;

            return new AssignmentViewModel
            {
                Id = assignment.Id,
                CourseId = assignment.CourseId,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate,
                CourseName = assignment.Course.Title,
                TeacherName = assignment.Creator.FullName,
                CreatedAt = assignment.CreatedAt
            };
        }

        public async Task<bool> CreateAssignmentAsync(AssignmentViewModel model, int teacherId)
        {
            var assignment = new Assignments
            {
                CourseId = model.CourseId,
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate,
                CreatedBy = teacherId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAssignmentAsync(AssignmentViewModel model)
        {
            try
            {
                var assignment = await _context.Assignments
                    .Include(a => a.Course)
                    .Include(a => a.Creator)
                    .FirstOrDefaultAsync(a => a.Id == model.Id);

                if (assignment == null) return false;

                assignment.Title = model.Title;
                assignment.Description = model.Description;
                assignment.DueDate = model.DueDate;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging configured
                Console.WriteLine($"Error updating assignment: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAssignmentAsync(int assignmentId)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return false;

            _context.Assignments.Remove(assignment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<StudentSubmissionInfo>> GetAssignmentSubmissionsAsync(int assignmentId)
        {
            try
            {
                // Get the assignment with its course
                var assignment = await _context.Assignments
                    .Include(a => a.Course)
                    .FirstOrDefaultAsync(a => a.Id == assignmentId);

                if (assignment == null) return null;

                // Get all enrolled students in the course with their details
                var enrolledStudents = await _context.Enrollments
                    .Where(e => e.CourseId == assignment.CourseId)
                    .Include(e => e.Student)
                    .Select(e => e.Student)
                    .ToListAsync();

                if (!enrolledStudents.Any())
                {
                    _logger.LogWarning($"No students enrolled in course {assignment.CourseId}");
                    return new List<StudentSubmissionInfo>();
                }

                // Get existing submissions for this assignment
                var submissions = await _context.AssignmentSubmissions
                    .Where(s => s.AssignmentId == assignmentId)
                    .ToDictionaryAsync(s => s.StudentId);

                // Create submission info for all enrolled students
                var result = enrolledStudents.Select(student => {
                    var hasSubmission = submissions.TryGetValue(student.Id, out var submission);
                    
                    return new StudentSubmissionInfo
                    {
                        SubmissionId = hasSubmission ? submission.Id : 0,
                        StudentId = student.Id,
                        StudentName = student.FullName,
                        DueDate = assignment.DueDate,
                        CompletionStatus = hasSubmission ? submission.CompletionStatus : SubmissionStatus.NotCompleted,
                        ActionStatus = hasSubmission ? submission.ActionStatus : ActionStatus.NoAction
                    };
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAssignmentSubmissionsAsync for assignmentId: {AssignmentId}", assignmentId);
                throw;
            }
        }

        public async Task<AssignmentSubmissionViewModel> GetSubmissionByIdAsync(int submissionId)
        {
            try
            {
                var submission = await _context.AssignmentSubmissions
                    .Include(s => s.Student)
                    .Include(s => s.Assignment)
                    .Where(s => s.Id == submissionId)
                    .Select(s => new AssignmentSubmissionViewModel
                    {
                        Id = s.Id,
                        AssignmentId = s.AssignmentId,
                        StudentId = s.StudentId,
                        SubmittedText = s.SubmittedText,
                        SubmittedFileName = s.SubmittedFile,
                        StoredFileName = s.StoredFileName,
                        SubmittedAt = s.SubmittedAt,
                        CompletionStatus = s.CompletionStatus,
                        ActionStatus = s.ActionStatus,
                        Student = s.Student,
                        Assignment = s.Assignment
                    })
                    .FirstOrDefaultAsync();

                return submission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSubmissionByIdAsync for submissionId: {SubmissionId}", submissionId);
                throw;
            }
        }

        public async Task<bool> UpdateSubmissionStatusAsync(int submissionId, ActionStatus status)
        {
            try
            {
                var submission = await _context.AssignmentSubmissions.FindAsync(submissionId);
                if (submission == null) return false;

                submission.ActionStatus = status;
                if (status == ActionStatus.Rejected)
                {
                    submission.CompletionStatus = SubmissionStatus.NotCompleted;
                }
                else if (status == ActionStatus.Approved)
                {
                    submission.CompletionStatus = SubmissionStatus.Completed;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateSubmissionStatusAsync for submissionId: {SubmissionId}", submissionId);
                return false;
            }
        }

        public async Task<StudentTasksViewModel> GetStudentCourseTasksAsync(int courseId, int studentId)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null) return null;

                var assignments = await _context.Assignments
                    .Where(a => a.CourseId == courseId)
                    .ToListAsync();

                var submissions = await _context.AssignmentSubmissions
                    .Where(s => s.Assignment.CourseId == courseId && s.StudentId == studentId)
                    .ToDictionaryAsync(s => s.AssignmentId);

                var tasks = assignments.Select(a =>
                {
                    var hasSubmission = submissions.TryGetValue(a.Id, out var submission);
                    
                    return new StudentTaskInfo
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Description = a.Description,
                        DueDate = a.DueDate,
                        CompletionStatus = hasSubmission ? submission.CompletionStatus : SubmissionStatus.NotCompleted,
                        ActionStatus = hasSubmission ? submission.ActionStatus : ActionStatus.NoAction
                    };
                }).ToList();

                return new StudentTasksViewModel
                {
                    CourseId = courseId,
                    CourseTitle = course.Title,
                    Tasks = tasks
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStudentCourseTasksAsync for courseId: {CourseId}, studentId: {StudentId}", courseId, studentId);
                throw;
            }
        }

        public async Task<bool> SubmitAssignmentAsync(int assignmentId, int studentId, string submittedText, IFormFile submissionFile)
        {
            var submission = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

            if (submission == null)
            {
                submission = new AssignmentSubmissions
                {
                    AssignmentId = assignmentId,
                    StudentId = studentId,
                    SubmittedText = submittedText,
                    SubmittedAt = DateTime.UtcNow,
                    CompletionStatus = SubmissionStatus.Completed,
                    ActionStatus = ActionStatus.NoAction
                };
                _context.AssignmentSubmissions.Add(submission);
            }
            else
            {
                submission.SubmittedText = submittedText;
                submission.SubmittedAt = DateTime.UtcNow;
                submission.CompletionStatus = SubmissionStatus.Completed;
                submission.ActionStatus = ActionStatus.NoAction;
            }

            if (submissionFile != null)
            {
                var fileName = Path.GetFileName(submissionFile.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Tasks");
                Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(submission.StoredFileName))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, submission.StoredFileName);
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await submissionFile.CopyToAsync(stream);
                }

                submission.SubmittedFile = fileName;
                submission.StoredFileName = uniqueFileName;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<AssignmentSubmissionViewModel> GetStudentSubmissionAsync(int assignmentId, int studentId)
        {
            try
            {
                var submission = await _context.AssignmentSubmissions
                    .Include(s => s.Assignment)
                    .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

                if (submission == null) return null;

                return new AssignmentSubmissionViewModel
                {
                    Id = submission.Id,
                    AssignmentId = submission.AssignmentId,
                    StudentId = submission.StudentId,
                    SubmittedText = submission.SubmittedText,
                    SubmittedFileName = submission.SubmittedFile,
                    StoredFileName = submission.StoredFileName,
                    SubmittedAt = submission.SubmittedAt,
                    CompletionStatus = submission.CompletionStatus,
                    ActionStatus = submission.ActionStatus,
                    Assignment = submission.Assignment
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStudentSubmissionAsync for assignmentId: {AssignmentId}, studentId: {StudentId}", assignmentId, studentId);
                return null;
            }
        }
    }
} 