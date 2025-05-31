using LMSTT.Models;
using LMSTT.ViewModels.Assignment;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LMSTT.Services
{
    public interface IAssignmentService
    {
        // Assignment Management
        Task<List<AssignmentViewModel>> GetTeacherAssignmentsAsync(int teacherId);
        Task<List<AssignmentViewModel>> GetCourseAssignmentsAsync(int courseId);
        Task<AssignmentViewModel> GetAssignmentByIdAsync(int assignmentId);
        Task<bool> CreateAssignmentAsync(AssignmentViewModel model, int teacherId);
        Task<bool> UpdateAssignmentAsync(AssignmentViewModel model);
        Task<bool> DeleteAssignmentAsync(int assignmentId);
        
        // Submission Management
        Task<List<StudentSubmissionInfo>> GetAssignmentSubmissionsAsync(int assignmentId);
        Task<AssignmentSubmissionViewModel> GetSubmissionByIdAsync(int submissionId);
        Task<bool> UpdateSubmissionStatusAsync(int submissionId, ActionStatus status);

        // Student Methods
        Task<StudentTasksViewModel> GetStudentCourseTasksAsync(int courseId, int studentId);
        Task<bool> SubmitAssignmentAsync(int assignmentId, int studentId, string submittedText, IFormFile submissionFile);
        Task<AssignmentSubmissionViewModel> GetStudentSubmissionAsync(int assignmentId, int studentId);
    }
} 