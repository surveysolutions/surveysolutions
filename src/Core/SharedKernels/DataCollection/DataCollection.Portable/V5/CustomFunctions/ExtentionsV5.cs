using System.Text.RegularExpressions;

namespace WB.Core.SharedKernels.DataCollection.V5.CustomFunctions
{
    public static class ExtentionsV5
    {
        /// <summary>
        /// Verifies if a string represents a nominally valid (plausible) email address.
        /// </summary>
        /// <param name="email">email address being verified</param>
        /// <returns>True if the candidate string satisfies requirements for email addresses. False otherwise.</returns>
        /// 
        /// \warning The function cannot confirm whether the specified email account actually exists.
        /// \warning Individual mail servers may be further configured to disallow accounts, 
        /// which this function may classify as valid.
        public static bool IsValidEmail(this string email)
        {
            // http://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address
            Regex _regex = new Regex("^((([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+(\\.([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+)*)|((\\x22)((((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(([\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x7f]|\\x21|[\\x23-\\x5b]|[\\x5d-\\x7e]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(\\\\([\\x01-\\x09\\x0b\\x0c\\x0d-\\x7f]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF]))))*(((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(\\x22)))@((([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.)+(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-|\\.|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            return _regex.IsMatch(email);
        }

        public static bool IsYes(this bool? yesNoOrMissing)
        {
            return yesNoOrMissing == true;
        }

        public static bool IsNo(this bool? yesNoOrMissing)
        {
            return yesNoOrMissing == false;
        }

        public static bool IsMissing(this bool? yesNoOrMissing)
        {
            return !yesNoOrMissing.HasValue;
        }
    }
}
