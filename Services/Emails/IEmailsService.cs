using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clipp.Server.Services.Emails
{
    public interface IEmailsService
    {
        Task SendEmailAsync(List<string> to, string from, string subject, string body, 
            bool isHTML = false, List<string> cc = null, string attachmentAddress = null);

        string ReplaceEmail(string message, Dictionary<string, string> variables);
    }
}
