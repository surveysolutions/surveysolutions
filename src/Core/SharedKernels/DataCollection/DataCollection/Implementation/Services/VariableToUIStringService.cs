﻿using System;
using System.Globalization;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class VariableToUIStringService : IVariableToUIStringService
    {
        public string VariableToUIString(object variable)
        {
            if (variable == null) return string.Empty;

            string variableAsString = null;
            switch (variable)
            {
                case int i:
                    variableAsString = i.ToString(CultureInfo.CurrentCulture);
                    break;
                case double d:
                    variableAsString = d.ToString(CultureInfo.CurrentCulture);
                    break;
                case bool b:
                    variableAsString = b.ToString();
                    break;
                case DateTime dt:
                    variableAsString = dt.ToString(DateTimeFormat.DateFormat);
                    break;
                case DateTimeOffset dto:
                    variableAsString = dto.ToString(DateTimeFormat.DateFormat);
                    break;
            }

            return variableAsString ?? variable.ToString();
        }
    }
}
