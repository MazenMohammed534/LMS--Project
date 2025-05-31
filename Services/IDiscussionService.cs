using LMSTT.Models;
using LMSTT.ViewModels.Discussion;

namespace LMSTT.Services
{
    public interface IDiscussionService
    {
        Task<Discussion> GetOrCreateCourseDiscussionAsync(int courseId);
        Task<DiscussionViewModel> GetDiscussionAsync(int discussionId, int currentUserId);
        Task<List<DiscussionViewModel>> GetCourseDiscussionsAsync(int courseId);
        Task<List<DiscussionMessage>> GetDiscussionMessagesAsync(int discussionId);
        Task<DiscussionMessage> AddMessageAsync(int discussionId, string userName, string messageText);
        Task<bool> DeleteMessageAsync(int messageId, int userId);
    }
} 