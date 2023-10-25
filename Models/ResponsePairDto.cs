namespace app.Models
{
    public class ResponsePairDto
    {
        public ResponsePairDto()
        {
        }

        public ResponsePairDto(bool status, string message)
        {
            Status = status;
            Message = message;
        }

        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
