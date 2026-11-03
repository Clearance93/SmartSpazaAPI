using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using SmartConfiguration;
using SmartInterfaces.IService;

namespace SmartServices
{
    public class SmtpConfirmationEmailSender : IConfirmationEmailSender
    {
        private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(15);

        private readonly IConfiguration _Configuration;

        public SmtpConfirmationEmailSender(IConfiguration configuration)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void ValidateConfiguration()
        {
            var settings = ReadSettings();
            var host = settings.Host;
            var hostNameType = Uri.CheckHostName(host ?? string.Empty);

            if (string.IsNullOrWhiteSpace(host) ||
                host.Length > 255 ||
                (hostNameType != UriHostNameType.Dns &&
                 hostNameType != UriHostNameType.IPv4 &&
                 hostNameType != UriHostNameType.IPv6))
            {
                throw new BusinessRuleException("Email delivery is unavailable because the mail server is not configured.", 503);
            }

            if (settings.Port < 1 || settings.Port > 65535)
            {
                throw new BusinessRuleException("Email delivery is unavailable because the mail server port is not configured.", 503);
            }

            if (string.IsNullOrWhiteSpace(settings.FromAddress) ||
                !MailAddress.TryCreate(settings.FromAddress, out var fromAddress) ||
                !string.Equals(fromAddress.Address, settings.FromAddress.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessRuleException("Email delivery is unavailable because the sender address is not configured.", 503);
            }

            if (!string.IsNullOrWhiteSpace(settings.FromName) && settings.FromName.Length > 256)
            {
                throw new BusinessRuleException("Email delivery is unavailable because the sender name is not configured.", 503);
            }

            if (!IsValidLink(settings.ConfirmationUrl))
            {
                throw new BusinessRuleException("Email delivery is unavailable because the confirmation link is not configured.", 503);
            }

            if (!IsValidLink(settings.PasswordResetUrl))
            {
                throw new BusinessRuleException("Email delivery is unavailable because the password reset link is not configured.", 503);
            }

            var hasUsername = !string.IsNullOrWhiteSpace(settings.Username);
            var hasPassword = !string.IsNullOrWhiteSpace(settings.Password);

            if (hasUsername != hasPassword)
            {
                throw new BusinessRuleException("Email delivery is unavailable because the mail credentials are incomplete.", 503);
            }

            if (!settings.EnableSsl && !settings.AllowInsecure)
            {
                throw new BusinessRuleException("Email delivery requires SMTP TLS. Set Smtp:EnableSsl=true (or Smtp:AllowInsecure for local testing only).", 503);
            }

            if (!settings.EnableSsl && !IsLocalMailHost(settings.Host))
            {
                throw new BusinessRuleException("Plaintext SMTP is only allowed for local mail catchers such as Mailpit.", 503);
            }
        }

        public async Task SendConfirmationAsync(string email, string userId, string encodedToken)
        {
            ValidateConfiguration();

            var recipient = ValidateRecipient(email);

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new BusinessRuleException("A user reference is required.", 400);
            }

            if (string.IsNullOrWhiteSpace(encodedToken))
            {
                throw new BusinessRuleException("A confirmation token is required.", 400);
            }

            var settings = ReadSettings();

            var callbackUrl = QueryHelpers.AddQueryString(settings.ConfirmationUrl!, new Dictionary<string, string?>
            {
                ["userId"] = userId,
                ["token"] = encodedToken
            });

            var displayName = string.IsNullOrWhiteSpace(recipient.DisplayName) ? email.Trim() : recipient.DisplayName;
            var body = BuildEmailBody(
                heading: "Confirm your email",
                displayName: displayName,
                instruction: "Please confirm your email address to activate your SmartSpaza account.",
                buttonLabel: "Confirm email",
                callbackUrl: callbackUrl,
                footer: "If you did not create an account, you can ignore this email.");

            await SendEmailAsync(settings, email, displayName, "Confirm your SmartSpaza email address", body);
        }

        public async Task SendPasswordResetAsync(string email, string encodedToken)
        {
            ValidateConfiguration();

            var recipient = ValidateRecipient(email);

            if (string.IsNullOrWhiteSpace(encodedToken))
            {
                throw new BusinessRuleException("A reset token is required.", 400);
            }

            var settings = ReadSettings();

            var callbackUrl = QueryHelpers.AddQueryString(settings.PasswordResetUrl!, new Dictionary<string, string?>
            {
                ["email"] = email.Trim(),
                ["token"] = encodedToken
            });

            var displayName = string.IsNullOrWhiteSpace(recipient.DisplayName) ? email.Trim() : recipient.DisplayName;
            var body = BuildEmailBody(
                heading: "Reset your password",
                displayName: displayName,
                instruction: "We received a request to reset your SmartSpaza password. Use the button below to choose a new password.",
                buttonLabel: "Reset password",
                callbackUrl: callbackUrl,
                footer: "If you did not request a password reset, you can safely ignore this email.");

            await SendEmailAsync(settings, email, displayName, "Reset your SmartSpaza password", body);
        }

        private static string BuildEmailBody(string heading,
                                             string displayName,
                                             string instruction,
                                             string buttonLabel,
                                             string callbackUrl,
                                             string footer)
        {
            var encodedUrl = WebUtility.HtmlEncode(callbackUrl);
            var encodedName = WebUtility.HtmlEncode(displayName);

            return $@"<!DOCTYPE html>
<html>
  <body style=""margin:0;padding:0;background-color:#f4f4f7;font-family:Arial,Helvetica,sans-serif;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f4f7;padding:24px 12px;"">
      <tr>
        <td align=""center"">
          <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:560px;background-color:#ffffff;border-radius:8px;padding:32px;"">
            <tr>
              <td style=""font-size:20px;font-weight:bold;color:#1f2937;padding-bottom:16px;"">{WebUtility.HtmlEncode(heading)}</td>
            </tr>
            <tr>
              <td style=""font-size:14px;line-height:20px;color:#374151;padding-bottom:24px;"">Hello {encodedName},</td>
            </tr>
            <tr>
              <td style=""font-size:14px;line-height:20px;color:#374151;padding-bottom:24px;"">{WebUtility.HtmlEncode(instruction)}</td>
            </tr>
            <tr>
              <td style=""padding-bottom:24px;"">
                <a href=""{encodedUrl}"" style=""display:inline-block;background-color:#16a34a;color:#ffffff;text-decoration:none;font-size:14px;font-weight:bold;padding:12px 24px;border-radius:6px;"">{WebUtility.HtmlEncode(buttonLabel)}</a>
              </td>
            </tr>
            <tr>
              <td style=""font-size:12px;line-height:18px;color:#6b7280;padding-bottom:8px;"">If the button does not work, copy and paste this link into your browser:</td>
            </tr>
            <tr>
              <td style=""font-size:12px;line-height:18px;color:#6b7280;word-break:break-all;"">
                <span>{encodedUrl}</span>
              </td>
            </tr>
            <tr>
              <td style=""font-size:12px;line-height:18px;color:#6b7280;padding-top:24px;"">{WebUtility.HtmlEncode(footer)}</td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
        }

        private static async Task SendEmailAsync(SmtpSettings settings, string email, string displayName, string subject, string body)
        {
            try
            {
                using var smtp = new SmtpClient(settings.Host, settings.Port)
                {
                    EnableSsl = settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var hasUsername = !string.IsNullOrWhiteSpace(settings.Username);
                var hasPassword = !string.IsNullOrWhiteSpace(settings.Password);

                if (hasUsername && hasPassword)
                {
                    smtp.Credentials = new NetworkCredential(settings.Username, settings.Password);
                }

                using var message = new MailMessage
                {
                    From = new MailAddress(settings.FromAddress!, settings.FromName, Encoding.UTF8),
                    Subject = subject,
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true,
                    Body = body
                };

                message.To.Add(new MailAddress(email, displayName));

                // SmtpClient.Timeout only applies to the synchronous Send call, so enforce a
                // timeout explicitly for the async path.
                await smtp.SendMailAsync(message).WaitAsync(SendTimeout);
            }
            catch (SmtpException)
            {
                throw new BusinessRuleException("The email could not be sent at this time. Please try again later.", 503);
            }
            catch (InvalidOperationException)
            {
                throw new BusinessRuleException("The email could not be sent at this time. Please try again later.", 503);
            }
            catch (TimeoutException)
            {
                throw new BusinessRuleException("The email could not be sent at this time. Please try again later.", 503);
            }
        }

        private static MailAddress ValidateRecipient(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email, out var recipient))
            {
                throw new BusinessRuleException("A valid email address is required.", 400);
            }

            return recipient;
        }

        private SmtpSettings ReadSettings()
        {
            return new SmtpSettings
            {
                Host = _Configuration["Smtp:Host"],
                Port = _Configuration.GetValue("Smtp:Port", 587),
                EnableSsl = _Configuration.GetValue("Smtp:EnableSsl", true),
                AllowInsecure = _Configuration.GetValue("Smtp:AllowInsecure", false),
                Username = _Configuration["Smtp:Username"],
                Password = _Configuration["Smtp:Password"],
                FromAddress = _Configuration["Smtp:FromAddress"],
                FromName = _Configuration["Smtp:FromName"],
                ConfirmationUrl = _Configuration["AppSettings:EmailConfirmationUrl"],
                PasswordResetUrl = _Configuration["AppSettings:PasswordResetUrl"]
            };
        }

        private static bool IsValidLink(string? url)
        {
            return !string.IsNullOrWhiteSpace(url) &&
                   Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttps || IsLoopbackHttpAllowed(uri)) &&
                   uri.Fragment.Length == 0 &&
                   uri.Query.Length == 0;
        }

        private static bool IsLocalMailHost(string? host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return false;
            }

            return string.Equals(host, "mailpit", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                   (IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address));
        }

        private static bool IsLoopbackHttpAllowed(Uri uri)
        {
            return uri.Scheme == Uri.UriSchemeHttp &&
                   (uri.IsLoopback ||
                    string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(uri.Host, "::1", StringComparison.OrdinalIgnoreCase));
        }

        private sealed class SmtpSettings
        {
            public string? Host { get; init; }

            public int Port { get; init; }

            public bool EnableSsl { get; init; }

            public bool AllowInsecure { get; init; }

            public string? Username { get; init; }

            public string? Password { get; init; }

            public string? FromAddress { get; init; }

            public string? FromName { get; init; }

            public string? ConfirmationUrl { get; init; }

            public string? PasswordResetUrl { get; init; }
        }
    }
}
