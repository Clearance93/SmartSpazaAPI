using Microsoft.Extensions.Configuration;
using SmartConfiguration;
using SmartServices;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class SmtpConfigurationTests
    {
        [Fact]
        public void ValidTlsConfiguration_Passes()
        {
            var sender = CreateSender(new Dictionary<string, string?>
            {
                ["Smtp:Host"] = "smtp.example.com",
                ["Smtp:Port"] = "587",
                ["Smtp:FromAddress"] = "no-reply@example.com",
                ["AppSettings:EmailConfirmationUrl"] = "https://app.example.com/confirm-email",
                ["AppSettings:PasswordResetUrl"] = "https://app.example.com/reset-password"
            });

            sender.ValidateConfiguration();
        }

        [Fact]
        public void MissingMailServer_Throws()
        {
            var sender = CreateSender(new Dictionary<string, string?>
            {
                ["Smtp:Host"] = "",
                ["Smtp:FromAddress"] = "no-reply@example.com",
                ["AppSettings:EmailConfirmationUrl"] = "https://app.example.com/confirm-email",
                ["AppSettings:PasswordResetUrl"] = "https://app.example.com/reset-password"
            });

            Assert.Throws<BusinessRuleException>(() => sender.ValidateConfiguration());
        }

        [Fact]
        public void PlainSmtpWithoutExplicitOptIn_Throws()
        {
            var sender = CreateSender(new Dictionary<string, string?>
            {
                ["Smtp:Host"] = "smtp.example.com",
                ["Smtp:Port"] = "25",
                ["Smtp:EnableSsl"] = "false",
                ["Smtp:FromAddress"] = "no-reply@example.com",
                ["AppSettings:EmailConfirmationUrl"] = "https://app.example.com/confirm-email",
                ["AppSettings:PasswordResetUrl"] = "https://app.example.com/reset-password"
            });

            Assert.Throws<BusinessRuleException>(() => sender.ValidateConfiguration());
        }

        [Fact]
        public void PlainSmtpWithExplicitOptIn_PassesForLocalMailCatcher()
        {
            var sender = CreateSender(new Dictionary<string, string?>
            {
                ["Smtp:Host"] = "mailpit",
                ["Smtp:Port"] = "1025",
                ["Smtp:EnableSsl"] = "false",
                ["Smtp:AllowInsecure"] = "true",
                ["Smtp:FromAddress"] = "no-reply@smartspaza.local",
                ["AppSettings:EmailConfirmationUrl"] = "http://localhost:3000/confirm-email",
                ["AppSettings:PasswordResetUrl"] = "http://localhost:3000/reset-password"
            });

            sender.ValidateConfiguration();
        }

        [Fact]
        public void PlainSmtpToRemoteHost_IsRejectedEvenWhenInsecureOptInIsSet()
        {
            var sender = CreateSender(new Dictionary<string, string?>
            {
                ["Smtp:Host"] = "smtp.example.com",
                ["Smtp:Port"] = "25",
                ["Smtp:EnableSsl"] = "false",
                ["Smtp:AllowInsecure"] = "true",
                ["Smtp:FromAddress"] = "no-reply@example.com",
                ["AppSettings:EmailConfirmationUrl"] = "https://app.example.com/confirm-email",
                ["AppSettings:PasswordResetUrl"] = "https://app.example.com/reset-password"
            });

            Assert.Throws<BusinessRuleException>(() => sender.ValidateConfiguration());
        }

        private static SmtpConfirmationEmailSender CreateSender(Dictionary<string, string?> settings)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            return new SmtpConfirmationEmailSender(configuration);
        }
    }
}
