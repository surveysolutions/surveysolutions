using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;

namespace Main.Core.Tests.Entities.SubEntities.GeoPositionTests
{
    [Subject(typeof(GeoPosition))]
    internal class when_parsing_string_i_order_to_create_GeoPosition_object
    {
        Because of =
            () =>
            {
                foreach (var possibleGeoPositionString in PossibleGeoPositionStrings)
                {
                    Results.Add(possibleGeoPositionString,GeoPosition.Parse(possibleGeoPositionString));
                }
            };

        It should_result_be_null_for_empty_string = () =>
            Results[EmptyString].ShouldBeNull();

        It should_result_be_null_for_string_only_literals = () =>
           Results[UnparsedAtAll].ShouldBeNull();

        It should_result_be_null_for_missing_coodinats_value = () =>
            Results[CoordinatesAreMissing].ShouldBeNull();

        It should_result_be_null_for_only_one_coordinate = () =>
            Results[OnlyOneCoordinate].ShouldBeNull();

        It should_result_be_null_for_invalid_latitude = () =>
            Results[InvalidLatitude].ShouldBeNull();

        It should_result_be_null_for_invalid_longitude = () =>
            Results[InvalidLongitude].ShouldBeNull();

        It should_result_be_null_for_invalid_accuracy = () =>
            Results[UnparsedAccuracy].ShouldBeNull();

        It should_return_not_null_result_for_valid_format = () =>
            Results[ValidFormat].ShouldNotBeNull();

        It should_parsed_result_accuracy_be_equal_3 = () =>
           Results[ValidFormat].Accuracy.ShouldEqual(3);

        It should_parsed_result_latitude_be_equal_1 = () =>
           Results[ValidFormat].Latitude.ShouldEqual(1);

        It should_parsed_result_longitude_be_equal_2 = () =>
          Results[ValidFormat].Longitude.ShouldEqual(2);

        It should_parsed_result_altitude_be_equal44 = () =>
          Results[ValidFormatWithAltitude].Altitude.ShouldEqual(44);
        
        It should_result_be_null_for_invalid_altitude = () =>
            Results[FormatWithInvalidAltitude].ShouldBeNull();

        It should_result_be_null_for_invalid_count = () =>
            Results[InValidFormatWithAltitude].ShouldBeNull();


        protected static string[] PossibleGeoPositionStrings = new[]
        {
            EmptyString, UnparsedAtAll, CoordinatesAreMissing, OnlyOneCoordinate, InvalidLatitude, InvalidLongitude,
            UnparsedAccuracy, ValidFormat, ValidFormatWithAltitude, FormatWithInvalidAltitude, InValidFormatWithAltitude
        };

        protected static Dictionary<string, GeoPosition> Results = new Dictionary<string, GeoPosition>();
        private const string EmptyString = "";
        private const string UnparsedAtAll = "unparsed at all";
        private const string CoordinatesAreMissing = "[coordinates are missing]";
        private const string OnlyOneCoordinate = "34[only one coordinate]";
        private const string InvalidLatitude = "a,4[invalid latitude]";
        private const string InvalidLongitude = "4,b[invalid longitude]";
        private const string UnparsedAccuracy = "1,2[unparsed accuracy]";
        private const string ValidFormat = "1,2[3]";

        private const string InValidFormatWithAltitude = "2,1,2[3]4";
        private const string ValidFormatWithAltitude = "1,2[3]44";
        private const string FormatWithInvalidAltitude = "1,2[3]4q";
    }
}
