#nullable enable
using System.Reflection;
using Android.Content;
using MvvmCross.Platforms.Android.Presenters;
using MvvmCross.ViewModels;

namespace WB.UI.Interviewer;

public class MvxAndroidViewPresenterWithExtra : MvxAndroidViewPresenter
{
    
    private readonly HashSet<Type> registeredViewModelsForClearBackStack = new HashSet<Type>();
    public MvxAndroidViewPresenterWithExtra(IEnumerable<Assembly> androidViewAssemblies) : base(androidViewAssemblies)
    {
    }
    
    public void RegisterViewModelForClearBackStack<TViewModel>() where TViewModel : IMvxViewModel
    {
        registeredViewModelsForClearBackStack.Add(typeof(TViewModel));
    }

    protected override Intent? CreateIntentForRequest(MvxViewModelRequest? request)
    {
        var intent =  base.CreateIntentForRequest(request);
        
        if (intent != null 
            && request?.ViewModelType != null 
            && registeredViewModelsForClearBackStack.Any(x => x.IsAssignableFrom(request.ViewModelType))) {
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        }

        return intent;
    }
}
