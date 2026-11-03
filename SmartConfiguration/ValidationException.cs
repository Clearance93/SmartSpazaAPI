namespace SmartConfiguration
{
    public class ValidationException : BaseException
    {
        public ValidationException(string message, int statusCode = 400) : base(message, statusCode)
        {
        }
    }
}
