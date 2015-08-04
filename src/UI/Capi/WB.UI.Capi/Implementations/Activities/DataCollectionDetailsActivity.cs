using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.Droid.Views;
using Cirrious.CrossCore.Exceptions;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using Java.Lang;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Infrastructure;
using Ninject.Modules;
using Ninject.Planning.Bindings;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Implementations.Adapters;
using WB.UI.Capi.SnapshotStore;
using WB.UI.Shared.Android.Activities;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Tester.Activities;
using WB.UI.Tester.CustomControls;
using Environment = System.Environment;
using Exception = System.Exception;
using InterviewViewModel = WB.Core.BoundedContexts.Capi.Views.InterviewDetails.InterviewViewModel;

namespace WB.UI.Capi.Implementations.Activities
{
    public class HybridEventBus : IEventBus
    {
        private readonly ILiteEventBus liteEventBus;
        private readonly IEventBus cqrsEventBus;

        public HybridEventBus(ILiteEventBus liteEventBus, IEventBus cqrsEventBus)
        {
            this.liteEventBus = liteEventBus;
            this.cqrsEventBus = cqrsEventBus;
        }

        public void CommitUncommittedEvents(IAggregateRoot aggregateRoot, string origin)
        {
            this.liteEventBus.CommitUncommittedEvents(aggregateRoot, origin);
        }

        public void PublishUncommittedEvents(IAggregateRoot aggregateRoot, bool isBulk = false)
        {
            ExecuteAllThrowOneAggregate(
                () => this.liteEventBus.PublishUncommittedEvents(aggregateRoot, isBulk),
                () => this.cqrsEventBus.PublishUncommittedEvents(aggregateRoot, isBulk));
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            this.cqrsEventBus.Publish(eventMessage);
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            this.cqrsEventBus.Publish(eventMessages);
        }

        private static void ExecuteAllThrowOneAggregate(params Action[] actions)
        {
            var exceptions = new List<Exception>();

            foreach (var action in actions)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }

    public static class NinjectKernelExtensions
    {
        public static void VerifyIfDebug(this IKernel kernel)
        {
#if DEBUG
            kernel.Verify();
#endif
        }

        private static void Verify(this IKernel kernel)
        {
            List<string> errors = new List<string>();

            foreach (var type in kernel.GetAllRegisteredTypes().Where(t => !t.ContainsGenericParameters))
            {
                try
                {
                    kernel.Get(type);
                }
                catch (Exception exception)
                {
                    var split = exception.Message.Split(new [] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                    errors.Add(string.Format("{0} - {1}", type.Name, string.Join(Environment.NewLine, split.Take(2))));
                }
            }

            if (errors.Count > 0)
                throw new Exception(string.Format(
                    "Failed to resolve {1} following non-generic types:{0}{2}",
                    Environment.NewLine,
                    errors.Count,
                    string.Join(Environment.NewLine, errors)));
        }

        private static Type[] GetAllRegisteredTypes(this IKernel kernel)
        {
            return ((Multimap<Type, IBinding>)typeof(KernelBase)
               .GetField("bindings", BindingFlags.Instance)
               .GetValue(kernel)).Select(x => x.Key).ToArray();
        }
    }

    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class DataCollectionDetailsActivity : DetailsActivity
    {
        private IAnswerProgressIndicator AnswerProgressIndicator
        {
            get { return ServiceLocator.Current.GetInstance<IAnswerProgressIndicator>(); }
        }

        protected override ContentFrameAdapter CreateFrameAdapter(InterviewItemId? screenId)
        {
            return new DataCollectionContentFrameAdapter(this.SupportFragmentManager, this.ViewModel as InterviewViewModel, screenId);
        }

        protected override InterviewViewModel GetInterviewViewModel(Guid interviewId)
        {
            return CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(new QuestionnaireScreenInput(interviewId));
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetupActionBar();
        }

        public override void Finish()
        {
            base.Finish();

            var snapshotStore = NcqrsEnvironment.Get<ISnapshotStore>() as FileBasedSnapshotStore;
            if (snapshotStore != null)
                snapshotStore.PersistShapshot(this.QuestionnaireId);
        }

        private void SetupActionBar()
        {
            this.ActionBar.SetDisplayShowHomeEnabled(false);
            this.ActionBar.SetDisplayShowTitleEnabled(false);
            this.ActionBar.SetDisplayShowCustomEnabled(true);
            this.ActionBar.SetDisplayUseLogoEnabled(true);
            this.ActionBar.SetCustomView(Resource.Layout.InterviewActionBar);
            
            var txtTitle = (TextView)this.ActionBar.CustomView.FindViewById(Resource.Id.txtTitle);
            txtTitle.Text = Title;

            var imgProgress = (ImageView)this.ActionBar.CustomView.FindViewById(Resource.Id.imgAnswerProgress);

            this.AnswerProgressIndicator.Setup(
                show: () => this.RunOnUiThread(() => imgProgress.Visibility = ViewStates.Visible),
                hide: () => this.RunOnUiThread(() => imgProgress.Visibility = ViewStates.Invisible));
        }
    }
}