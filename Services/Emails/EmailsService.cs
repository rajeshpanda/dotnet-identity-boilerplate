using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Clipp.Server.Services.Emails
{
    public class EmailsService : IEmailsService
    {
        private readonly IConfiguration _configuration;
        public EmailsService(IConfiguration configuration)  
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(List<string> to, string from, string subject, string body,
            bool isHTML = true, List<string> cc = null, string attachmentAddress = null)
        {
            MailMessage mail = new MailMessage();
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress(from);
            foreach (var address in to)
            {
                mail.To.Add(address);
            }

            if (cc != null && cc.Count > 0)
            {
                foreach (var address in cc)
                {
                    mail.CC.Add(address);
                }
            }
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            //if (!string.IsNullOrEmpty(attachmentAddress))
            //{
            //    // read file from address
            //    //var item = new Attachment();
            //    //mail.Attachments.Add(item);
            //}

            smtpClient.Port = 587;
            smtpClient.Credentials = new System.Net.NetworkCredential(_configuration["Email:Username"], _configuration["Email:Password"]);
            smtpClient.EnableSsl = true;
            await smtpClient.SendMailAsync(mail);
        }

        public string ReplaceEmail(string message, Dictionary<string, string> variables)
        {
            foreach(var item in variables)
            {
                message = message.Replace(item.Key, item.Value);
            }

            return message;
        }
    }
}
