using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static System.Int32;

namespace ValidationWeb.Services
{
    using System.Data.Entity.Infrastructure;

    public class SchoolYearService : ISchoolYearService
    {

        public SchoolYearService(IDbContextFactory<ValidationPortalDbContext> validationPortalDataContextFactory)
        {
            ValidationPortalDataContextFactory = validationPortalDataContextFactory;
        }

        protected IDbContextFactory<ValidationPortalDbContext> ValidationPortalDataContextFactory { get; set; }

        public IList<SchoolYear> GetSubmittableSchoolYears()
        {
            using (var validationPortalDataContext = ValidationPortalDataContextFactory.Create()) 
            {
                return validationPortalDataContext.SchoolYears.ToList();
            }
        }

        public Dictionary<int, string> GetSubmittableSchoolYearsDictionary()
        {
            using (var validationPortalDataContext = ValidationPortalDataContextFactory.Create())
            {
                return validationPortalDataContext.SchoolYears.ToDictionary(x => x.Id, x => x.ToString());
            }
        }

        public void SetSubmittableSchoolYears(IEnumerable<SchoolYear> years)
        {
            using (var validationPortalDataContext = ValidationPortalDataContextFactory.Create())
            {
                validationPortalDataContext.SchoolYears.RemoveRange(validationPortalDataContext.SchoolYears.ToArray());
                validationPortalDataContext.SaveChanges();

                validationPortalDataContext.SchoolYears.AddRange(years);
                validationPortalDataContext.SaveChanges();
            }
        }

        public bool UpdateErrorThresholdValue(int id, decimal thresholdValue)
        {
            using (var validationPortalDataContext = ValidationPortalDataContextFactory.Create())
            {
                var schoolYearRecord = validationPortalDataContext.SchoolYears.FirstOrDefault(schoolYear => schoolYear.Id == id);

                if (schoolYearRecord == null)
                {
                    return false;
                }

                schoolYearRecord.ErrorThreshold = thresholdValue;
                validationPortalDataContext.SaveChanges();

                return true;
            }
        }

        public bool AddNewSchoolYear(string startDate, string endDate)
        {
            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return false;
            }

            if (!ValidateYears(startDate, endDate))
            {
                return false;
            }

            var newSchoolYear = new SchoolYear(startDate, endDate);

            using (var validationPortalDataContext = ValidationPortalDataContextFactory.Create())
            {
                validationPortalDataContext.SchoolYears.Add(newSchoolYear);
                validationPortalDataContext.SaveChanges();
            }

            return true;
        }

        // AddNewSchoolYear Validator to make sure the dates are just one year apart
        public bool ValidateYears(string startDate, string endDate)
        {
            int startDateCheck;
            var didParse = TryParse(startDate, out startDateCheck);

            if (!didParse)
            {
                return false;
            }

            startDateCheck = startDateCheck + 1; 
            return startDateCheck.ToString() == endDate;
        }

        public bool RemoveSchoolYear(int id)
        {
            // TODO - Don't delete school year if it is the last one.
            using (var validationPortalDataContext = ValidationPortalDataContextFactory.Create())
            {
                var schoolYears = validationPortalDataContext.SchoolYears.ToList();
                var schoolYearRecord = schoolYears.FirstOrDefault(schoolYear => schoolYear.Id == id);

                // If no record is found and it is the last record in the school year table return false,
                // because we have to have at minimum 1 school year.
                if (schoolYearRecord == null || schoolYears.Count == 1)
                {
                    return false;
                }

                validationPortalDataContext.SchoolYears.Remove(schoolYearRecord);
                validationPortalDataContext.SaveChanges();
                return true;
            }
        }

        public SchoolYear GetSchoolYearById(int id)
        {
            using (var validationPortalDataContext = ValidationPortalDataContextFactory.Create())
            {
                var schoolYear = validationPortalDataContext.SchoolYears.FirstOrDefault(sy => sy.Id == id);
                return schoolYear ?? validationPortalDataContext.SchoolYears.First();
            }
        }
    }
}
