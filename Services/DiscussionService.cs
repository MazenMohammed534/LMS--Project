using LMSTT.Data;
using LMSTT.Models;
using LMSTT.ViewModels.Discussion;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using LMSTT.Hubs;

namespace LMSTT.Services
{
    public class DiscussionService : IDiscussionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<DiscussionHub> _hubContext;

        public DiscussionService(ApplicationDbContext context, IHubContext<DiscussionHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<DiscussionViewModel> GetDiscussionAsync(int discussionId, int currentUserId)
        {
            var discussion = await _context.Discussions
                .Include(d => d.CreatedBy)
                .Include(d => d.Messages)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(d => d.Id == discussionId);

            if (discussion == null)
                return null;

            return new DiscussionViewModel
            {
                Id = discussion.Id,
                Title = discussion.Title,
                CreatedByName = discussion.CreatedBy.FullName,
                CreatedAt = discussion.CreatedAt,
                Messages = discussion.Messages
                    .OrderBy(m => m.SentAt)
                    .Select(m => new DiscussionMessageViewModel
                    {
                        Id = m.Id,
                        UserName = m.User.FullName,
                        UserPhotoUrl = "/css/Images/profilephoto.jpg",
                        MessageText = m.MessageText,
                        SentAt = m.SentAt,
                        IsCurrentUser = m.UserId == currentUserId
                    }).ToList()
            };
        }

        public async Task<List<DiscussionViewModel>> GetCourseDiscussionsAsync(int courseId)
        {
            return await _context.Discussions
                .Where(d => d.CourseId == courseId)
                .Include(d => d.CreatedBy)
                .OrderByDescending(d => d.UpdatedAt)
                .Select(d => new DiscussionViewModel
                {
                    Id = d.Id,
                    Title = d.Title,
                    CreatedByName = d.CreatedBy.FullName,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteMessageAsync(int messageId, int userId)
        {
            var message = await _context.DiscussionMessages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.UserId == userId);

            if (message == null)
                return false;

            _context.DiscussionMessages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DiscussionMessage>> GetDiscussionMessagesAsync(int discussionId)
        {
            return await _context.DiscussionMessages
                .Include(m => m.User)
                .Where(m => m.DiscussionId == discussionId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<DiscussionMessage> AddMessageAsync(int discussionId, string userName, string messageText)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FullName == userName);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var discussion = await _context.Discussions.FindAsync(discussionId);
            if (discussion == null)
            {
                throw new InvalidOperationException("Discussion not found");
            }

            var message = new DiscussionMessage
            {
                DiscussionId = discussionId,
                UserId = user.Id,
                MessageText = messageText,
                SentAt = DateTime.UtcNow
            };

            _context.DiscussionMessages.Add(message);
            discussion.UpdatedAt = DateTime.UtcNow;
            _context.Discussions.Update(discussion);

            await _context.SaveChangesAsync();

            // Send the message to all clients in the discussion group
            await _hubContext.Clients.Group(discussionId.ToString())
                .SendAsync("ReceiveMessage", userName, messageText);

            return message;
        }

        public async Task<Discussion> GetOrCreateCourseDiscussionAsync(int courseId)
        {
            var discussion = await _context.Discussions
                .Include(d => d.CreatedBy)
                .FirstOrDefaultAsync(d => d.CourseId == courseId);

            if (discussion == null)
            {
                var course = await _context.Courses
                    .Include(c => c.Teacher)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    throw new InvalidOperationException("Course not found");
                }

                if (course.TeacherId == null)
                {
                    throw new InvalidOperationException("Course teacher not assigned");
                }

                discussion = new Discussion
                {
                    CourseId = courseId,
                    Title = "Course Chat",
                    CreatedById = course.TeacherId.Value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Discussions.Add(discussion);
                await _context.SaveChangesAsync();

                discussion = await _context.Discussions
                    .Include(d => d.CreatedBy)
                    .FirstAsync(d => d.Id == discussion.Id);
            }

            return discussion;
        }
    }
} 