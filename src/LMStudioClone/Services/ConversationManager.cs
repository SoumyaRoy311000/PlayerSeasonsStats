using System.Collections.ObjectModel;
using Serilog;

namespace LMStudioClone.Services
{
    public class ConversationManager
    {
        private readonly ILogger _logger = Log.ForContext<ConversationManager>();
        
        public ObservableCollection<ChatMessage> Messages { get; } = new();
        
        public void AddMessage(string sender, string content, bool isUser = false)
        {
            var message = new ChatMessage
            {
                Sender = sender,
                Content = content,
                IsUser = isUser,
                Timestamp = DateTime.Now
            };
            
            Messages.Add(message);
            _logger.Information("Added message from {Sender}: {Content}", sender, content.Substring(0, Math.Min(50, content.Length)));
        }
        
        public void ClearMessages()
        {
            Messages.Clear();
            _logger.Information("Conversation cleared");
        }
        
        public void SaveConversation(string filePath)
        {
            // TODO: Implement conversation saving
            _logger.Information("Saving conversation to {FilePath}", filePath);
        }
        
        public void LoadConversation(string filePath)
        {
            // TODO: Implement conversation loading
            _logger.Information("Loading conversation from {FilePath}", filePath);
        }
    }
    
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

