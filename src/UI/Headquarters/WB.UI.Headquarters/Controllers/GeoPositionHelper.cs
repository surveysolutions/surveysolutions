using System;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;

namespace WB.UI.Headquarters.Controllers
{
    public static class GeoPositionHelper
    {
        public static bool IsLatitude(this string exportColumnName)
        {
            return exportColumnName.IndexOf(ExpressionExtensions.GetName((GeoPosition geo) => geo.Latitude),
                StringComparison.OrdinalIgnoreCase) > -1;
        }

        public static bool IsLongtitude(this string exportColumnName)
        {
            return exportColumnName.IndexOf(ExpressionExtensions.GetName((GeoPosition geo) => geo.Longitude),
                StringComparison.OrdinalIgnoreCase) > -1;
        }

        public static bool IsRequiredGeoPositionColumn(this string exportColumnName)
        {
            return exportColumnName.IsLatitude() || exportColumnName.IsLongtitude();
        }

        public static string GetLatitideColumnName(this string prefilledQuestionVariable)
        {
            return $"{prefilledQuestionVariable}{QuestionDataParser.COLUMNDELIMITER}{ExpressionExtensions.GetName((GeoPosition geo) => geo.Latitude)}"
                .ToLower();
        }

        public static string GetLongitugeColumnName(this string prefilledQuestionVariable)
        {
            return $"{prefilledQuestionVariable}{QuestionDataParser.COLUMNDELIMITER}{ExpressionExtensions.GetName((GeoPosition geo) => geo.Longitude)}"
                .ToLower();
        }
    }
}