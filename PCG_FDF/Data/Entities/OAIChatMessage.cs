using Azure.AI.OpenAI;

namespace PCG_FDF.Data.Entities
{
    public sealed class OAIChatMessage
    {
        public string Message { get; set; }
        public ChatRole Role { get; set; }

#pragma warning disable CS8618  // JSON Constructor
        public OAIChatMessage() { }
#pragma warning restore CS8618  // JSON Constructor

        public OAIChatMessage(ChatRequestUserMessage User_Message)
        {
            Message = User_Message.Content;
            Role = User_Message.Role;
        }

        public OAIChatMessage(ChatRequestAssistantMessage User_Message)
        {
            Message = User_Message.Content;
            Role = User_Message.Role;
        }
    }
}
