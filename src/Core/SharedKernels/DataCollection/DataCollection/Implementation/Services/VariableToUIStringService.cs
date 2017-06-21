using System;
using System.Globalization;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class VariableToUIStringService : IVariableToUIStringService
    {
        public string VariableToUIString(object variable)
        {
            if (variable == null) return string.Empty;

            var variableAsString = variable.ToString();

            TypeSwitch.Do(
                variable,
                TypeSwitch.Case<int>(value => { variableAsString = value.ToString(CultureInfo.CurrentCulture); }),
                TypeSwitch.Case<double>(value => { variableAsString = value.ToString(CultureInfo.CurrentCulture); }),
                TypeSwitch.Case<DateTime>(value => { variableAsString = value.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern); }),
                TypeSwitch.Case<bool>(value => { variableAsString = value.ToString(); }));

            return variableAsString;
        }
    }
}