using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMSTT.Services;
using LMSTT.ViewModels.Discussion;
using Microsoft.EntityFrameworkCore;
using LMSTT.Data;
using Microsoft.Extensions.Logging;

namespace LMSTT.Controllers
{
    [Authorize]
    public class DiscussionController : Controller
    {
        private readonly IDiscussionService _discussionService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiscussionController> _logger;

        public DiscussionController(
            IDiscussionService discussionService, 
            ApplicationDbContext context,
            ILogger<DiscussionController> logger)
        {
            _discussionService = discussionService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Discussion(int id)
        {
            try
            {
                // Get current user ID
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Check if user is enrolled in the course or is the teacher
                var course = await _context.Courses
                    .Include(c => c.Enrollments)
                    .FirstOrDefaultAsync(c => c.Id == id);
                    
                if (course == null)
                {
                    return RedirectToAction("Error", "Home", new { message = "Course not found" });
                }
                
                if (course.TeacherId != userId && !course.Enrollments.Any(e => e.StudentId == userId))
                {
                    return RedirectToAction("Error", "Home", new { message = "You are not enrolled in this course" });
                }

                var discussion = await _discussionService.GetOrCreateCourseDiscussionAsync(id);
                var messages = await _discussionService.GetDiscussionMessagesAsync(discussion.Id);
                
                var viewModel = new DiscussionViewModel
                {
                    Id = discussion.Id,
                    CourseId = id,
                    Title = discussion.Title,
                    CreatedByName = discussion.CreatedBy?.FullName ?? "System",
                    CreatedAt = discussion.CreatedAt,
                    UpdatedAt = discussion.UpdatedAt,
                    Messages = messages.Select(m => new DiscussionMessageViewModel
                    {
                        Id = m.Id,
                        MessageText = m.MessageText,
                        UserName = m.User.FullName,
                        UserPhotoUrl = "/css/Images/profilephoto.jpg",
                        IsCurrentUser = m.User.FullName == User.Identity?.Name,
                        SentAt = m.SentAt
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (InvalidOperationException ex)
            {
                return RedirectToAction("Error", "Home", new { message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state when sending message");
                    return BadRequest(new { message = "Invalid message data" });
                }

                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    _logger.LogWarning("User not authenticated when trying to send message");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                _logger.LogInformation(
                    "Attempting to send message. DiscussionId: {DiscussionId}, User: {UserName}", 
                    model.DiscussionId, 
                    userName);

                var message = await _discussionService.AddMessageAsync(
                    model.DiscussionId,
                    userName,
                    model.MessageText);

                _logger.LogInformation(
                    "Message sent successfully. MessageId: {MessageId}", 
                    message.Id);

                return Ok(new { 
                    id = message.Id,
                    sentAt = message.SentAt
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error sending message: {ErrorMessage}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending message");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
} 