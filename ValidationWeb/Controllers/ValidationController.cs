﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Engine.Models;
using ValidationWeb.Filters;
using ValidationWeb.Services;
using ValidationWeb.Services.Interfaces;

namespace ValidationWeb
{
    using System.IO;
    using System.Web.Hosting;

    using CsvHelper;

    using DataTables.AspNet.Core;
    using DataTables.AspNet.Mvc5;
    [PortalAuthorize(Roles = "DistrictUser,HelpDesk")]
    public class ValidationController : Controller
    {
        protected readonly IAppUserService _appUserService;
        protected readonly IEdOrgService _edOrgService;
        protected readonly ILoggingService _loggingService;
        protected readonly IRulesEngineService _rulesEngineService;
        protected readonly ISchoolYearService _schoolYearService;
        private readonly ISubmissionCycleService _submissionCycleService;
        protected readonly IValidationResultsService _validationResultsService;
        protected readonly IEmailService _emailService;
        protected readonly Model _engineObjectModel;

        public ValidationController(
            IAppUserService appUserService,
            IEdOrgService edOrgService,
            ILoggingService loggingService,
            IValidationResultsService validationResultsService,
            IRulesEngineService rulesEngineService,
            ISchoolYearService schoolYearService,
            ISubmissionCycleService submissionCycleService,
            IEmailService emailService,
            Model engineObjectModel)
        {
            _appUserService = appUserService;
            _edOrgService = edOrgService;
            _engineObjectModel = engineObjectModel;
            _loggingService = loggingService;
            _rulesEngineService = rulesEngineService;
            _schoolYearService = schoolYearService;
            _submissionCycleService = submissionCycleService;
            _validationResultsService = validationResultsService;
            _emailService = emailService;
        }

        // GET: Validation/Reports
        public ActionResult Reports()
        {
            var edOrg = _edOrgService.GetEdOrgById(_appUserService.GetSession().FocusedEdOrgId, _appUserService.GetSession().FocusedSchoolYearId);
            var rulesCollections = _engineObjectModel.Collections.OrderBy(x => x.CollectionId).ToList();
            var theUser = _appUserService.GetUser();
            var districtName = (edOrg == null) ? "Invalid Education Organization Selected" : edOrg.OrganizationName;


            var model = new ValidationReportsViewModel
            {
                DistrictName = districtName,
                TheUser = theUser,
                RulesCollections = rulesCollections,
                SchoolYears = _schoolYearService.GetSubmittableSchoolYears().ToList(),
                SubmissionCycles = _submissionCycleService.GetSubmissionCyclesOpenToday(),
                FocusedEdOrgId = _appUserService.GetSession().FocusedEdOrgId,
                FocusedSchoolYearId = _appUserService.GetSession().FocusedSchoolYearId,
            };
            return View(model);
        }

        public FileStreamResult DownloadReportSummariesCsv(int edOrgId)
        {
            var edOrg = _edOrgService.GetSingleEdOrg(edOrgId, _appUserService.GetSession().FocusedSchoolYearId);

            var results = _validationResultsService.GetValidationReportSummaries(edOrgId)
                .OrderByDescending(rs => rs.CompletedWhen)
                .ToList();

            var csvArray = WriteCsvToMemory(results.Select(
                x => new
                {
                    x.RequestedWhen,
                    Collection = $"{x.SchoolYear.StartYear}-{x.SchoolYear.EndYear} / {x.Collection}",
                    x.InitiatedBy,
                    x.Status,
                    x.CompletedWhen,
                    x.ErrorCount,
                    x.WarningCount
                }));

            var memoryStream = new MemoryStream(csvArray);

            return new FileStreamResult(memoryStream, "text/csv")
            {
                FileDownloadName = $"ValidationSummary_{edOrg.OrganizationName.Replace(' ', '-')}.csv"
            };
        }

        public JsonResult ReportSummaries(int edOrgId, IDataTablesRequest request)
        {
            var session = _appUserService.GetSession();
            // var focusedSchoolYear = _schoolYearService.GetSchoolYearById(session.FocusedSchoolYearId);

            var results = _validationResultsService.GetValidationReportSummaries(edOrgId).OrderByDescending(rs => rs.CompletedWhen).Where(rs => rs.SchoolYearId == session.FocusedSchoolYearId).ToList();

            IEnumerable<ValidationReportSummary> sortedResults = results;

            var sortColumn = request.Columns.FirstOrDefault(x => x.Sort != null);
            if (sortColumn != null)
            {
                Func<ValidationReportSummary, string> orderingFunctionString = null;
                Func<ValidationReportSummary, int?> orderingFunctionNullableInt = null;
                Func<ValidationReportSummary, DateTime> orderingFunctionDateTime = null;
                Func<ValidationReportSummary, DateTime?> orderingFunctionNullableDateTime = null;

                switch (sortColumn.Field)
                {
                    case "requestedWhen":
                        {
                            orderingFunctionDateTime = x => x.RequestedWhen;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionDateTime)
                                                : results.OrderByDescending(orderingFunctionDateTime);
                            break;
                        }
                    case "collection":
                        {
                            orderingFunctionString = x => $"{x.SchoolYear.StartYear}-{x.SchoolYear.EndYear} / {x.Collection}";
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionString)
                                                : results.OrderByDescending(orderingFunctionString);
                            break;
                        }
                    case "initiatedBy":
                        {
                            orderingFunctionString = x => x.InitiatedBy;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionString)
                                                : results.OrderByDescending(orderingFunctionString);
                            break;
                        }
                    case "status":
                        {
                            orderingFunctionString = x => x.Status;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionString)
                                                : results.OrderByDescending(orderingFunctionString);
                            break;
                        }
                    case "completedWhen":
                        {
                            orderingFunctionNullableDateTime = x => x.CompletedWhen;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionNullableDateTime)
                                                : results.OrderByDescending(orderingFunctionNullableDateTime);
                            break;
                        }
                    case "errorCount":
                        {
                            orderingFunctionNullableInt = x => x.ErrorCount;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionNullableInt)
                                                : results.OrderByDescending(orderingFunctionNullableInt);
                            break;
                        }
                    case "warningCount":
                        {
                            orderingFunctionNullableInt = x => x.WarningCount;
                            sortedResults = sortColumn.Sort.Direction == SortDirection.Ascending
                                                ? results.OrderBy(orderingFunctionNullableInt)
                                                : results.OrderByDescending(orderingFunctionNullableInt);
                            break;
                        }
                    default:
                        {
                            sortedResults = results;
                            break;
                        }
                }
            }

            var pagedResults = sortedResults.Skip(request.Start).Take(request.Length);
            var response = DataTablesResponse.Create(request, results.Count, results.Count, pagedResults);
            var jsonResult = new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult Report(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Reports");
            }

            var emails = new List<string>();
            emails.Add("Test@test.com");
            emails.Add("Test2@test.com");
            emails.Add("Test3@test.com");
            emails.Add("Test4@test.com");

            var model = new ValidationReportDetailsViewModel
            {
                Details = _validationResultsService.GetValidationReportDetails(id),
                Emails = emails
            };
            return View(model);
        }

        public FileStreamResult DownloadReportCsv(int id, int reportSummaryId)
        {
            var memoryStream = CreateCsvInMemoryStream(reportSummaryId);

            var reportSummary = _validationResultsService.GetValidationReportDetails(reportSummaryId);

            return new FileStreamResult(memoryStream, "text/csv")
            {
                FileDownloadName = $"ValidationErrors_{reportSummary.DistrictName.Replace(' ', '-')}_{reportSummary.CollectionName}_{reportSummary.CompletedWhen?.ToShortDateString()}.csv"
            };
        }

        // SendReportEmail
        public void EmailCsvReport(int id, int reportSummaryId, string emailRecipient)
        {
            var csvMemoryStream = CreateCsvInMemoryStream(reportSummaryId); ;

            var reportSummary = _validationResultsService.GetValidationReportDetails(reportSummaryId);

            var csvName = $"ValidationErrors_{reportSummary.DistrictName.Replace(' ', '-')}_{reportSummary.CollectionName}_{reportSummary.CompletedWhen?.ToShortDateString()}.csv";

            _emailService.SendReportEmail(emailRecipient, csvName,  "Email report for West Ada.", csvMemoryStream);
        }

        private MemoryStream CreateCsvInMemoryStream(int reportSummaryId)
        {
            var results = _validationResultsService.GetValidationErrors(reportSummaryId);

            var groupedByStudent = new List<dynamic>();

            foreach (var detailRow in results)
            {
                groupedByStudent.Add(
                    new
                    {
                        StudentId = detailRow.StudentUniqueId,
                        Student = detailRow.StudentFullName,
                        School = CombineDetailField(detailRow.ErrorEnrollmentDetails, x => x.School),
                        SchoolId = CombineDetailField(detailRow.ErrorEnrollmentDetails, x => x.SchoolId),
                        DateEnrolled = CombineDetailField(
                            detailRow.ErrorEnrollmentDetails,
                            x => x.DateEnrolled?.ToShortDateString()),
                        DateWithdrawn = CombineDetailField(
                            detailRow.ErrorEnrollmentDetails,
                            x => x.DateWithdrawn == null
                                ? "Present"
                                : x.DateWithdrawn.Value.ToShortDateString()),
                        Grade = CombineDetailField(detailRow.ErrorEnrollmentDetails, x => x.Grade),
                        detailRow.Severity.CodeValue,
                        detailRow.ErrorCode,
                        detailRow.ErrorText
                    });
            }

            var csvArray = WriteCsvToMemory(groupedByStudent);
            return new MemoryStream(csvArray);
        }

        [PortalAuthorize(Roles = "DistrictUser")]
        [HttpPost]
        public ActionResult RunEngine(int submissionCycleId)
        {
            SubmissionCycle submissionCycle = _submissionCycleService.GetSubmissionCycle(submissionCycleId);
            if (submissionCycle == null)
            {
                string strMessage = $"Collection cycle with id {submissionCycleId} not found.";
                _loggingService.LogErrorMessage(strMessage);
                throw new InvalidOperationException(strMessage);
            }

            // TODO: Validate the user's access to district, action, school year
            // todo: all security

            ValidationReportSummary summary = _rulesEngineService.SetupValidationRun(
                submissionCycle, 
                submissionCycle.CollectionId);

            HostingEnvironment.QueueBackgroundWorkItem(
                cancellationToken => _rulesEngineService.RunValidationAsync(
                    submissionCycle, 
                    summary.ValidationReportSummaryId));

            return Json(summary);
        }

        protected byte[] WriteCsvToMemory<T>(IEnumerable<T> records)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    using (var csvWriter = new CsvWriter(streamWriter))
                    {
                        csvWriter.WriteRecords(records);
                        streamWriter.Flush();
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        protected string CombineDetailField(
            ICollection<ValidationErrorEnrollmentDetail> details,
            Func<ValidationErrorEnrollmentDetail, string> func)
        {
            return string.Join(Environment.NewLine, details.Select(func));
        }
    }
}