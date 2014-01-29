using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class TextListQuestionView : AbstractTextListQuestionView<TextListAnswerViewModel>
    {
        public TextListQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, 
                                    QuestionViewModel source, Guid questionnairePublicKey, 
                                    ICommandService commandService, IAnswerOnQuestionCommandService answerCommandService,
                                    IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }

        protected override IEnumerable<TextListAnswerViewModel> ListAnswers
        {
            get { return this.TypedModel.ListAnswers; }
        }

        protected TextListQuestionViewModel TypedModel
        {
            get { return this.Model as TextListQuestionViewModel; }
        }

        protected override int? MaxAnswerCount
        {
            get { return TypedModel.MaxAnswerCount; }
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            return this.Model.AnswerString;
        }

        
        protected override string GetAnswerId(TextListAnswerViewModel answer)
        {
            return answer.Value.ToString();
        }

        protected override string GetAnswerTitle(TextListAnswerViewModel answer)
        {
            return answer.Answer;
        }

        /*protected override TextListAnswerViewModel FindAnswerInModelByCheckBoxTag(string tag)
        {

            throw new NotImplementedException();
        }*/

        protected override AnswerQuestionCommand CreateSaveAnswerCommand(TextListAnswerViewModel[] selectedAnswers)
        {
            var answers = selectedAnswers.Select(a => new Tuple<decimal, string>(a.Value, a.Answer)).ToList();

            return new AnswerTextListQuestionCommand(this.QuestionnairePublicKey,this.Membership.CurrentUser.Id,
                this.Model.PublicKey.Id, this.Model.PublicKey.InterviewItemPropagationVector, DateTime.UtcNow, answers.ToArray());
        }
        
    }
}