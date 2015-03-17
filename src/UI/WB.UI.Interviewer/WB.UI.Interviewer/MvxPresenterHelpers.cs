using System;
using System.Linq;
using System.Reflection;
using Cirrious.CrossCore;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.ViewModels;
using Xamarin.Forms;

namespace WB.UI.Interviewer
{
    public static class MvxPresenterHelpers
    {
        public static IMvxViewModel LoadViewModel(MvxViewModelRequest request)
        {
            var viewModelLoader = Mvx.Resolve<IMvxViewModelLoader>();
            var viewModel = viewModelLoader.LoadViewModel(request, null);
            return viewModel;
        }

        public static ContentPage CreatePage(MvxViewModelRequest request)
        {
            var viewModelName = request.ViewModelType.Name;
            var pageName = viewModelName.Replace("ViewModel", "Page");
            var pageType = typeof(MvxPresenterHelpers).GetTypeInfo().Assembly.CreatableTypes()
                                  .FirstOrDefault(t => t.Name == pageName);
            if (pageType == null)
            {
                Mvx.Trace("Page not found for {0}", pageName);
                return null;
            }

            var page = Activator.CreateInstance(pageType) as ContentPage;
            if (page == null)
            {
                Mvx.Error("Failed to create ContentPage {0}", pageName);
            }
            return page;
        }
    }
}