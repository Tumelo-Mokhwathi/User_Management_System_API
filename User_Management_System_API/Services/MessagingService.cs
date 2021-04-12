using User_Management_System_API.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace User_Management_System_API.Services
{
    public class MessagingService : IEmailSender, ISmsSender
    {
        private readonly ILogger<MessagingService> _logger;
        private readonly MailOptions _mailOptions;
        private readonly SMSoptions _smsOptions;
        private readonly Credentials _credentials;

        public MessagingService(
            IOptions<Credentials> credentials,
            IOptions<MailOptions> mailOptionsAccessor,
            IOptions<SMSoptions> smsOptionsAccessor,
            ILogger<MessagingService> logger)
        {
            _credentials = credentials.Value;
            _logger = logger;
            _mailOptions = mailOptionsAccessor.Value;
            _smsOptions = smsOptionsAccessor.Value;
        }

        public async Task SendEmailAsync(string email, string name, string subject, string message)
        {
            _logger.LogInformation("Executing SendEmailAsync");

            _logger.LogDebug("Mail message inputs: From address - {0}, To address - {1}, Subject - {2}, Message - {3}",
                _mailOptions.FromAddress, email, subject, message);

            var mimeMsg = new MimeMessage();
            mimeMsg.From.Add(new MailboxAddress(_mailOptions.FromName, _mailOptions.FromAddress));
            mimeMsg.To.Add(new MailboxAddress(name, email));
            mimeMsg.Subject = subject;
            mimeMsg.Body = new TextPart(TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_mailOptions.Server, _mailOptions.Port, _mailOptions.UseSsl);
                await client.AuthenticateAsync(_credentials.Username, _credentials.Password);
                await client.SendAsync(mimeMsg);
                await client.DisconnectAsync(true);
            }
            _logger.LogInformation("Executed SendEmailAsync");
        }

        public Task SendSmsAsync(string number, string message)
        {
            var accountSid = _smsOptions.SMSAccountIdentification;

            var authToken = _smsOptions.SMSAccountPassword;

            TwilioClient.Init(accountSid, authToken);

            return MessageResource.CreateAsync(
              to: new Twilio.Types.PhoneNumber(number),
              from: new Twilio.Types.PhoneNumber(_smsOptions.SMSAccountFrom),
              body: message);
        }
    }
}
