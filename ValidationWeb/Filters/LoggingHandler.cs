﻿using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using ValidationWeb.Services;

namespace ValidationWeb
{
    /// <summary>
    /// Initiates logging capability by attaching an ID that will by added to all the log messages for this Request - as well as
    /// a start time to use to determine the age of the request at the time of each log message - used for performance monitoring.
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        protected const string RequestStartTimeKeyName = "RequestStartTime";
        protected const string CorrelationIdKeyName = "CorrelationId";
        protected const string LogCorrelationIdKeyName = "LogCorrelationId";

        protected readonly ILoggingService Logger;

        public LoggingHandler(ILoggingService logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Attach an ID that will by added to all the log messages for this Request. Starts the clock on performance monitoring
        /// included in all log requests. 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            #region Add Logging Context to the Request
            var newGuid = Guid.NewGuid();
            // "B" means "Braces" - {00000000-0000-0000-0000-000000000000}
            var logCorrelationId = $"[API Request {newGuid.ToString("N")}] ";
            request.Properties.Add(CorrelationIdKeyName, newGuid);
            request.Properties.Add(LogCorrelationIdKeyName, logCorrelationId);
            Logger.LogInfoMessage("BEGINNING OF REQUEST ------------------");
            #endregion Add Logging Context to the Request

            #region Add Performance Measuring Context to the Request
            request.Properties.Add(RequestStartTimeKeyName, DateTime.UtcNow);
            #endregion Add Performance Measuring Context to the Request

            #region Dispatch the request.
            var response = await base.SendAsync(request, cancellationToken);
            return response;
            #endregion Dispatch the request.
        }
    }
}