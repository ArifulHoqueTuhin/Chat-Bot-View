namespace ChatBotView.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
