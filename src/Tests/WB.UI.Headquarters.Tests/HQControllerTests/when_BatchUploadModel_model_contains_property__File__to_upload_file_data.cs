using System.Linq;
using System.Reflection;
using Machine.Specifications;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Tests.HQControllerTests
{
    public class when_BatchUploadModel_model_contains_property__File__to_upload_file_data
    {
        Establish context = () => 
            propertyInfo = typeof(BatchUploadModel).GetProperty("File");

        Because of = () =>
            attribute = propertyInfo.GetCustomAttributes(typeof(ValidateFileAttribute))
                .Cast<ValidateFileAttribute>()
                .FirstOrDefault();

        It should_have_ValidateFile_attribute_for_that_property_to_check_file_validity = () => 
            attribute.ShouldNotBeNull();

        private static PropertyInfo propertyInfo;
        private static ValidateFileAttribute attribute;
    }
}