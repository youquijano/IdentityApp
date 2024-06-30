namespace Api.DTOs.Account
{
    public class EmailSendDto
    {
        public EmailSendDto(string subject, string to, string body)
        {
            Subject = subject;
            To = to;
            Body = body;
        }

        public string Subject { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
    }
}
