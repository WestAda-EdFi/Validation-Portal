using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web.Mvc;

namespace ValidationWeb.Services.Interfaces
{
    public class EmailService : IEmailService
    {
        private readonly string _senderEmail = ConfigurationManager.AppSettings["EmailAddress"];
        private readonly string _senderPassword = ConfigurationManager.AppSettings["EmailPassword"];
        protected readonly ILoggingService _logger;

        public EmailService(ILoggingService logger)
        {
            _logger = logger;
        }

        public void SendReportEmail(string emailRecipient, string csvName, string body, MemoryStream csvArray)
        {
            var smtpClient = GetSmtpClient();

            var mailMessage = GetMailMessage(_senderEmail, emailRecipient, body);

            var attachment = new Attachment(csvArray, "text/csv")
            {
                Name = csvName
            };

            mailMessage.Attachments.Add(attachment);

            smtpClient.Send(mailMessage);

            _logger.LogInfoMessage($"EMAILED report successfully sent to {emailRecipient}");
        }

        private SmtpClient GetSmtpClient()
        {
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);

            var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                UseDefaultCredentials = false
            };
            smtpClient.Host = smtpHost;
            smtpClient.Port = smtpPort;
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

            return smtpClient;
        }

        private MailMessage GetMailMessage(string from, string sendTo, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(from),
                Subject = $"Ods validation report {DateTime.Now:MM-dd-yyyy}",
                SubjectEncoding = Encoding.UTF8,
                Body = body,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            if (IsEmailValid(sendTo))
            {
                mailMessage.To.Add(sendTo);
            }

            return mailMessage;
        }

        private bool IsEmailValid(string emailAddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailAddress);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
