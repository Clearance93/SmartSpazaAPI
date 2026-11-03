namespace SmartConfiguration
{
    public class BusinessRuleException : BaseException
    {
        public BusinessRuleException(string message, int statusCode = 400) : base(message, statusCode)
        {
        }
    }
}
