﻿using System.ComponentModel.DataAnnotations;
using Microsoft.Practices.ServiceLocation;
using WB.UI.Shared.Web.MembershipProvider.Accounts;

namespace WB.UI.Shared.Web.DataAnnotations
{
    public class PasswordRegularExpressionAttribute : RegularExpressionAttribute
    {
        public PasswordRegularExpressionAttribute()
            : base(ServiceLocator.Current.GetInstance<IPasswordPolicy>().PasswordStrengthRegularExpression)
        {
        }

        public override bool IsValid(object value)
        {
            var hasPattern = !string.IsNullOrEmpty(this.Pattern);
            return !hasPattern || base.IsValid(value);
        }
    }
}