namespace Domain.Entities
{
    public class ErrorLog
    {
        public int Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string ErrorType { get; set; }
    }
}
