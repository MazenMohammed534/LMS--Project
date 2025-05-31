using Microsoft.AspNetCore.SignalR;
using LMSTT.ViewModels.Discussion;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;

namespace LMSTT.Hubs
{
    [Authorize]
    public class DiscussionHub : Hub
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _userConnections = new();
        private static readonly ConcurrentDictionary<string, HashSet<string>> _discussionGroups = new();

        public async Task JoinDiscussion(string discussionId)
        {
            var userId = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;
            var sessionId = Context.GetHttpContext()?.Session.GetString("SessionId") ?? Guid.NewGuid().ToString();

            // Add to user connections
            _userConnections.AddOrUpdate(userId,
                new ConcurrentDictionary<string, string> { [connectionId] = sessionId },
                (_, connections) =>
                {
                    connections[connectionId] = sessionId;
                    return connections;
                });

            // Add to discussion group
            _discussionGroups.AddOrUpdate(discussionId,
                new HashSet<string> { connectionId },
                (_, connections) =>
                {
                    lock (connections)
                    {
                        connections.Add(connectionId);
                        return connections;
                    }
                });

            await Groups.AddToGroupAsync(connectionId, discussionId);
            await Clients.Caller.SendAsync("JoinedDiscussion", discussionId);
        }

        public async Task LeaveDiscussion(string discussionId)
        {
            var userId = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            await Groups.RemoveFromGroupAsync(connectionId, discussionId);

            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.TryRemove(connectionId, out _);
                if (connections.IsEmpty)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }

            if (_discussionGroups.TryGetValue(discussionId, out var groupConnections))
            {
                lock (groupConnections)
                {
                    groupConnections.Remove(connectionId);
                    if (groupConnections.Count == 0)
                    {
                        _discussionGroups.TryRemove(discussionId, out _);
                    }
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.TryRemove(connectionId, out _);
                if (connections.IsEmpty)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }

            foreach (var group in _discussionGroups)
            {
                lock (group.Value)
                {
                    if (group.Value.Contains(connectionId))
                    {
                        group.Value.Remove(connectionId);
                        if (group.Value.Count == 0)
                        {
                            _discussionGroups.TryRemove(group.Key, out _);
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;

            _userConnections.AddOrUpdate(
                userId,
                new ConcurrentDictionary<string, string>() { [connectionId] = connectionId },
                (_, connections) =>
                {
                    connections[connectionId] = connectionId;
                    return connections;
                });

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string discussionId, string message)
        {
            var user = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;
            var sessionId = Context.GetHttpContext()?.Session.GetString("SessionId");

            if (_discussionGroups.TryGetValue(discussionId, out var groupConnections))
            {
                // Send to all connections in the discussion group
                await Clients.Group(discussionId).SendAsync("ReceiveMessage", user, message);
            }
        }
    }
} 