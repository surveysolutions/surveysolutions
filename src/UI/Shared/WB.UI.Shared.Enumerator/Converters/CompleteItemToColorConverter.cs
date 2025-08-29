using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Converters;

public class CompleteItemToColorConverter : MvxValueConverter<EntityWithErrorsViewModel, int>
{
    protected override int Convert(EntityWithErrorsViewModel item, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(item.Comment)) 
            return Resource.Drawable.complete_item_status_commented; 
        if (item.IsError) 
            return Resource.Drawable.complete_item_status_error; 
        return Resource.Drawable.complete_item_status_unanswered;
    }
}
