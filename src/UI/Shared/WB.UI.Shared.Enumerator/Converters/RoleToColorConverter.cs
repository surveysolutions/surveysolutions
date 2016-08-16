using System;
using System.Globalization;
using Main.Core.Entities.SubEntities;
using MvvmCross.Platform.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class RoleToColorConverter : MvxValueConverter<UserRoles, int>
    {
        protected override int Convert(UserRoles value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case UserRoles.Administrator:
                case UserRoles.Supervisor:
                case UserRoles.Headquarter:
                case UserRoles.ApiUser:
                    return Resource.Color.comment_from_authority;
                default:
                    return Resource.Color.comment_from_interviewer;
            }
        }
    }
}