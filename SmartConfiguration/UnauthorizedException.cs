namespace SmartConfiguration
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message, int statusCode = 401) : base(message, statusCode)
        {
        }
    }
}
