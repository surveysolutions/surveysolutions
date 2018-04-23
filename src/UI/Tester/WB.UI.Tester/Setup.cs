using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Content;
using MvvmCross;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Converters;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using MvvmCross.Views;
using WB.Core.BoundedContexts.Tester;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Tester.Activities;
using WB.UI.Tester.Converters;
using WB.UI.Tester.ServiceLocation;
using Xamarin;

namespace WB.UI.Tester
{
    public class Setup : EnumeratorSetup
    {
        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override IMvxIoCProvider CreateIocProvider()
        {
            return IoCAdapterSetup.CreateIocProvider();
        }

        protected override void InitializeViewLookup()
        {
            base.InitializeViewLookup();

            var viewModelViewLookup = new Dictionary<Type, Type>()
            {
                {typeof (LoginViewModel), typeof (LoginActivity)},
                {typeof (InterviewViewModel), typeof (InterviewActivity)},
                {typeof (DashboardViewModel), typeof (DashboardActivity)},
                {typeof (CompleteInterviewViewModel), typeof (CompleteInterviewFragment)},
                {typeof (PrefilledQuestionsViewModel), typeof (PrefilledQuestionsActivity)},
#if !EXCLUDEEXTENSIONS
                { typeof (WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel), typeof (WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorActivity)}
#endif
            };

            var container = Mvx.Resolve<IMvxViewsContainer>();
            container.AddAll(viewModelViewLookup);
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);
            
            registry.AddOrOverwrite("PublicBackground", new QuestionnairePublicityBackgroundConverter());
        }

        public override IEnumerable<Assembly> GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies().Union(new[]
            {
                typeof(TesterBoundedContextModule).Assembly,

#if !EXCLUDEEXTENSIONS
                typeof(WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel).Assembly
#endif

            });
        }
    }
}
