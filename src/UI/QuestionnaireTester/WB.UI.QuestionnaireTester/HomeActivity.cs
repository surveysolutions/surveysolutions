using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Cirrious.MvvmCross.Droid.Simple;
using Main.Core.Documents;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.UI.QuestionnaireTester.Implementations.Activities;

namespace WB.UI.QuestionnaireTester
{
    [Activity(/*MainLauncher = true,*/ ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class HomeActivity : MvxSimpleBindingActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Home);

            var buttonStart = FindViewById<Button>(Resource.Id.btnStart);

            buttonStart.Click += this.btnLogin_Click;
 
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Guid interviewId = Guid.NewGuid();
            Guid interviewUserId = Guid.NewGuid();
            Guid supervisorUserId = Guid.NewGuid();

            var templateContent = "{ \"$type\": \"Main.Core.Documents.QuestionnaireDocument, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Group, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Question.AutoPropagateQuestion, Main.Core\", \"AddNumericAttr\": null, \"IntAttr\": 0, \"Triggers\": [ \"c0749385-133d-4d7f-9c72-977966054e19\" ], \"MaxValue\": 5, \"AnswerOrder\": 2, \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"3d540a0d-27e0-4c75-be6e-d56614cfb355\", \"QuestionScope\": 0, \"QuestionText\": \"Propagation\", \"QuestionType\": 8, \"StataExportCaption\": \"Propagation1\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null }, { \"$type\": \"Main.Core.Entities.SubEntities.Group, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Question.TextQuestion, Main.Core\", \"AddTextAttr\": null, \"AnswerOrder\": 0, \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"8da9dd9c-61f6-4558-8085-913282fe4be6\", \"QuestionScope\": 0, \"QuestionText\": \"Propagation question\", \"QuestionType\": 7, \"StataExportCaption\": \"Tex1\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null } ], \"ConditionExpression\": \"\", \"Enabled\": true, \"Description\": \"\", \"Propagated\": 2, \"PublicKey\": \"c0749385-133d-4d7f-9c72-977966054e19\", \"Title\": \"Propagation Group\", \"Triggers\": [] } ], \"ConditionExpression\": \"\", \"Enabled\": true, \"Description\": \"\", \"Propagated\": 0, \"PublicKey\": \"3c47aa81-fc83-4960-a616-7185382db718\", \"Title\": \"Chapter 1 (propagation)\", \"Triggers\": [] }, { \"$type\": \"Main.Core.Entities.SubEntities.Group, Main.Core\", \"Children\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Question.MultyOptionsQuestion, Main.Core\", \"AreAnswersOrdered\": true, \"MaxAllowedAnswers\": 2, \"AnswerOrder\": 0, \"Answers\": [], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"e9cdd51f-7f6b-45c4-9d7c-fb352eb5b461\", \"QuestionScope\": 0, \"QuestionText\": \"Multi option linked\", \"QuestionType\": 3, \"StataExportCaption\": \"Q1\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": \"8da9dd9c-61f6-4558-8085-913282fe4be6\" }, { \"$type\": \"Main.Core.Entities.SubEntities.Question.MultyOptionsQuestion, Main.Core\", \"AreAnswersOrdered\": true, \"MaxAllowedAnswers\": 2, \"AnswerOrder\": 0, \"Answers\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"1\", \"AnswerType\": 0, \"AnswerValue\": \"1\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"453a5bd2-468a-4ea4-8c9e-67ce8537bc41\" }, { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"2\", \"AnswerType\": 0, \"AnswerValue\": \"2\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"7b22d1c1-a236-452b-a3fc-6640c91e6d26\" }, { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"3\", \"AnswerType\": 0, \"AnswerValue\": \"3\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"44a44ca2-3183-40ec-ac87-33555e7cfb84\" } ], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"8ed54541-3363-4933-a933-4c44528f370f\", \"QuestionScope\": 0, \"QuestionText\": \"Multi option question\", \"QuestionType\": 3, \"StataExportCaption\": \"Q2\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null }, { \"$type\": \"Main.Core.Entities.SubEntities.Question.MultyOptionsQuestion, Main.Core\", \"AreAnswersOrdered\": true, \"MaxAllowedAnswers\": 2, \"AnswerOrder\": 0, \"Answers\": [ { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"1\", \"AnswerType\": 0, \"AnswerValue\": \"1\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"70d18545-b9ac-407c-a649-42df396e34e9\" }, { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"2\", \"AnswerType\": 0, \"AnswerValue\": \"2\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"23eb81d8-d614-40de-af95-598ea8ab9541\" }, { \"$type\": \"Main.Core.Entities.SubEntities.Answer, Main.Core\", \"AnswerImage\": null, \"AnswerText\": \"3\", \"AnswerType\": 0, \"AnswerValue\": \"3\", \"Mandatory\": false, \"NameCollection\": null, \"PublicKey\": \"3ad76f3f-7712-4a4e-bf08-ab66e87b690d\" } ], \"Capital\": false, \"Cards\": [], \"Children\": [], \"Comments\": null, \"ConditionExpression\": \"\", \"ConditionalDependentGroups\": [], \"ConditionalDependentQuestions\": [], \"Featured\": false, \"Instructions\": \"\", \"Mandatory\": false, \"PublicKey\": \"61c5f54a-9252-4abe-be6e-585f3f29739a\", \"QuestionScope\": 1, \"QuestionText\": \"Supervisor multioption\", \"QuestionType\": 3, \"StataExportCaption\": \"sq1\", \"ValidationExpression\": \"\", \"ValidationMessage\": \"\", \"LinkedToQuestionId\": null } ], \"ConditionExpression\": \"\", \"Enabled\": true, \"Description\": \"\", \"Propagated\": 0, \"PublicKey\": \"a4b21ca2-3ed6-4de0-a3b8-80878438efae\", \"Title\": \"Chapter 2\", \"Triggers\": [] } ], \"CloseDate\": null, \"ConditionExpression\": \"\", \"CreationDate\": \"2013-11-06T14:04:35.9730568Z\", \"LastEntryDate\": \"2013-11-06T15:04:38.966378Z\", \"OpenDate\": null, \"IsDeleted\": false, \"CreatedBy\": \"b07c7946-c255-40d4-910c-9760e7ec6d93\", \"IsPublic\": false, \"Propagated\": 0, \"PublicKey\": \"177dd47a-1d3d-4b37-8501-7b06cf7befa0\", \"Title\": \"Demo Multianswer Ordered and Max Answer Limitation\", \"Description\": null, \"Triggers\": [], \"SharedPersons\": [], \"LastEventSequence\": 19}";

            var template = JsonUtils.GetObject<QuestionnaireDocument>(templateContent);
            NcqrsEnvironment.Get<ICommandService>().Execute(new ImportFromDesignerForTester(template));

            var st = NcqrsEnvironment.Get<IEventStore>();

            var it = st.ReadFrom(template.PublicKey, 0, 1000);
            
            try
            {
                NcqrsEnvironment.Get<ICommandService>().Execute(new CreateInterviewForTestingCommand(interviewId, interviewUserId,
                    template.PublicKey, new Dictionary<Guid, object>(), DateTime.UtcNow));
            }
            catch (Exception exc)
            {
                var r = exc.Message;
                //throw;
            }

            var t = CapiTesterApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                    new QuestionnaireScreenInput(interviewId));

            if (t == null)
            {
                var test = false;
            }

            var intent = new Intent(this, typeof(TesterDetailsActivity));
            intent.PutExtra("publicKey", interviewId.ToString());
            this.StartActivity(intent);
        }
    }
}