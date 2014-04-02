using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WB.UI.Headquarters.Code
{
    public class ValidateFileAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            var file = value as HttpPostedFileBase;

            return file != null;
        }
    }
}