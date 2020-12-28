using System;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public class PreloadedDataValidator
    {
        public PreloadedDataValidator(
            Func<UserToImport, bool> validationFunction,
            string code,
            Expression<Func<UserToImport, string>> valueSelector)
        {
            this.ValidationFunction = validationFunction;
            this.Code = code;
            this.ValueSelector = valueSelector;
            this.ValueSelectorCompiled = new Lazy<Func<UserToImport, string>>(() => valueSelector.Compile());
        }

        public Func<UserToImport, bool> ValidationFunction { get; private set; }
        public string Code { get; private set; }
        public Expression<Func<UserToImport, string>> ValueSelector { get; private set; }
        public Lazy<Func<UserToImport, string>> ValueSelectorCompiled { get; }
    }
}
