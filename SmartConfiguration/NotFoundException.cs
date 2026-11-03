namespace SmartConfiguration
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message, int statusCode = 404) : base(message, statusCode)
        {
        }
    }
}
