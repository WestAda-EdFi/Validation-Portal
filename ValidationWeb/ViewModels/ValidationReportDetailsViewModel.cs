using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ValidationWeb
{
    public class ValidationReportDetailsViewModel
    {
        public ValidationReportDetails Details { get; set; }

        public List<string> Emails { get; set; }
    }
}