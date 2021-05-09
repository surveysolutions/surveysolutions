using System.Collections.Generic;
using System.Globalization;
using WB.Services.Export.Interview.Entities;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.CsvExport.Exporters
{
    internal static class ExportHelper
    {
        public static int Interviewer => 1;
        public static int Supervisor => 2;
        public static int Headquarter => 3;
        public static int Administrator => 4;
        public static int ApiUser => 5;
        public static int UnknownRole => 0;

        public static string NoRole => string.Empty;

        public static Dictionary<int, string> RolesMap= new Dictionary<int, string>()
        {
            {UnknownRole,"<UNKNOWN ROLE>"},
            {Interviewer, "Interviewer"},
            {Supervisor, "Supervisor" },
            {Headquarter,"Headquarter"},
            {Administrator, "Administrator"},
            {ApiUser, "API User" }
        };

        private static int GetUserRoleNumericValue(UserRoles userRole)
        {
            switch (userRole)
            {
                case UserRoles.Interviewer:
                    return Interviewer;
                case UserRoles.Supervisor:
                    return Supervisor;
                case UserRoles.Headquarter:
                    return Headquarter;
                case UserRoles.Administrator:
                    return Administrator;
                case UserRoles.ApiUser:
                    return ApiUser;

            }
            return UnknownRole;
        }

        public static string GetUserRoleDisplayValue(UserRoles userRole)
        {
            return GetUserRoleNumericValue(userRole).ToString(CultureInfo.InvariantCulture);
        }

        public static int GetParadataRole(string recordOriginatorRole)
        {
            if (string.IsNullOrWhiteSpace(recordOriginatorRole))
                return 0;

            switch (recordOriginatorRole.ToLower())
            {
                case "interviewer":
                    return 1;
                case "supervisor":
                    return 2;
                case "headquarter":
                    return 3;
                case "administrator":
                    return 4;
                case "apiuser":
                    return 5;
                default:
                    return 0;
            }
        }
    }
}
