using System;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class PreloadedDataValidator
    {
        public PreloadedDataValidator(
            Func<UserPreloadingDataRecord, bool> validationFunction,
            string code,
            Expression<Func<UserPreloadingDataRecord, string>> valueSelector)
        {
            this.ValidationFunction = validationFunction;
            this.Code = code;
            this.ValueSelector = valueSelector;
        }

        public Func<UserPreloadingDataRecord, bool> ValidationFunction { get; private set; }
        public string Code { get; private set; }
        public Expression<Func<UserPreloadingDataRecord, string>> ValueSelector { get; private set; }
    }
}