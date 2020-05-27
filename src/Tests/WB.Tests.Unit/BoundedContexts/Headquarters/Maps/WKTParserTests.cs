using NUnit.Framework;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Maps
{
    public class WKTParserTests
    {
        [Test]
        [Ignore("Maybe when there will be a good parser for .net we will be able to extract data from it")]
        public void should_be_able_to_parse_known_text()
        {
            string text =
@"PROJCS[""WGS_1984_UTM_Zone_35S"",
    GEOGCS[""WGS 84"",
        DATUM[""WGS_1984"",
            SPHEROID[""WGS 84"",6378137,298.257223563,
                AUTHORITY[""EPSG"",""7030""]],
            AUTHORITY[""EPSG"",""6326""]],
        PRIMEM[""Greenwich"",0],
        UNIT[""degree"",0.0174532925199433],
        AUTHORITY[""EPSG"",""4326""]],
    PROJECTION[""Transverse_Mercator""],
    PARAMETER[""latitude_of_origin"",0],
    PARAMETER[""central_meridian"",27],
    PARAMETER[""scale_factor"",0.9996],
    PARAMETER[""false_easting"",500000],
    PARAMETER[""false_northing"",10000000],
    UNIT[""metre"",1,
        AUTHORITY[""EPSG"",""9001""]],
    AUTHORITY[""EPSG"",""32735""]]";
            //
            
            // var reader = new WktReader();
            // IGeometry geometry = reader.Read(text);
        }
        
    }
}
