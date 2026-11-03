namespace SmartConfiguration
{
    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string message, int statusCode = 403) : base(message, statusCode)
        {
        }
    }
}