namespace SmartInterfaces.IService
{
    public interface IConfirmationEmailSender
    {
        void ValidateConfiguration();

        Task SendConfirmationAsync(string email, string userId, string encodedToken);

        Task SendPasswordResetAsync(string email, string encodedToken);
    }
}
