﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ValidationWeb
{
    public class EdOrgQuery
    {
        public static string AllEdOrgQuery =
@"SELECT 
	Id = lea.LocalEducationAgencyId,
	OrganizationShortName = eo.ShortNameOfInstitution,
	OrganizationName = eo.NameOfInstitution,
	StateOrganizationId = eo.StateOrganizationId,
	ParentId = lea.ParentLocalEducationAgencyId,
	StateLevelOrganizationId = lea.StateEducationAgencyId,
	OrgTypeCodeValue = leact.CodeValue,
	OrgTypeShortDescription = leact.ShortDescription
FROM edfi.LocalEducationAgency lea
LEFT OUTER JOIN edfi.EducationOrganization eo ON eo.EducationOrganizationId = lea.LocalEducationAgencyId
LEFT OUTER JOIN edfi.LocalEducationAgencyCategoryType leact ON leact.LocalEducationAgencyCategoryTypeId = lea.LocalEducationAgencyCategoryTypeId;";

        public static string SingleEdOrgsQuery =
@"SELECT 
	Id = lea.LocalEducationAgencyId,
	OrganizationShortName = eo.ShortNameOfInstitution,
	OrganizationName = eo.NameOfInstitution,
	StateOrganizationId = eo.StateOrganizationId,
	ParentId = lea.ParentLocalEducationAgencyId,
	StateLevelOrganizationId = lea.StateEducationAgencyId,
	OrgTypeCodeValue = leact.CodeValue,
	OrgTypeShortDescription = leact.ShortDescription
FROM edfi.LocalEducationAgency lea
LEFT OUTER JOIN edfi.EducationOrganization eo ON eo.EducationOrganizationId = lea.LocalEducationAgencyId
LEFT OUTER JOIN edfi.LocalEducationAgencyCategoryType leact ON leact.LocalEducationAgencyCategoryTypeId = lea.LocalEducationAgencyCategoryTypeId
WHERE lea.LocalEducationAgencyId = @lea_id;";


        public const string IdColumnName = "Id";
        public const string OrganizationShortNameColumnName = "OrganizationShortName";
        public const string OrganizationNameColumnName = "OrganizationName";
        public const string StateOrganizationIdColumnName = "StateOrganizationId";
        public const string ParentIdColumnName = "ParentId";
        public const string StateLevelOrganizationIdColumnName = "StateLevelOrganizationId";
        public const string OrgTypeCodeValueColumnName = "OrgTypeCodeValue";
        public const string OrgTypeShortDescriptionColumnName = "OrgTypeShortDescription";

        public int Id { get; set; }
        public string ShortOrganizationName { get; set; }
        public string OrganizationName { get; set; }
        public string StateOrganizationId { get; set; }
        public string ParentId { get; set; }
        public string StateLevelOrganizationId { get; set; }
        public string OrgTypeCodeValue { get; set; }
        public string OrgTypeShortDescription { get; set; }
    }
}
