namespace SmartConfiguration
{
    public class ConflictException : BaseException
    {
        public ConflictException(string message, int statusCode = 409) : base(message, statusCode)
        {
        }
    }
}
