using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Data
{
    public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
    {
        public void AddGroup(Group group)
        {
            context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            context.Messages.Add( message );
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove( message );
        }

        public async Task<Connection?> GetConnection(string connectionId)
        {
            return await context.Connections.FindAsync(connectionId);
        }

        public async Task<Group?> GetGroupForConnection(string connectionId)
        {
            return await context.Groups
                .Include(x => x.Connections)
                .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public async Task<Message?> GetMessage(int id)
        {
            return await context.Messages.FindAsync(id);
        }

        public async Task<Group?> GetMessageGroup(string groupName)
        {
            return await context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context.Messages
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();
            
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.Username && !x.RecipientDeleted),
                "Outbox" => query.Where(x => x.Sender.UserName == messageParams.Username && !x.SenderDeleted),
                _ => query.Where(x => x.Recipient.UserName == messageParams.Username && x.DateRead == null && !x.RecipientDeleted)
            };

            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUserName)
        {
            var messages = await context.Messages
                .Where(x => 
                    x.RecipientUsername == currentUsername && !x.RecipientDeleted && x.SenderUsername == recipientUserName || 
                    x.SenderUsername == currentUsername && !x.SenderDeleted && x.RecipientUsername == recipientUserName)
                .OrderBy(x => x.MessageSent)
                .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
                .ToListAsync(); 
            
            var unreadedMessages = messages.Where(x => x.DateRead == null && x.RecipientUsername == currentUsername).ToList();

            if(unreadedMessages.Count != 0)
            {
                unreadedMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
                await context.SaveChangesAsync();
            }

            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}