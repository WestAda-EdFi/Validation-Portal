using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ValidationWeb.Services.Interfaces
{
    public interface IEmailService
    {
        void SendReportEmail(string emailRecipient, string csvName, string body, MemoryStream csvArray);
    }
}