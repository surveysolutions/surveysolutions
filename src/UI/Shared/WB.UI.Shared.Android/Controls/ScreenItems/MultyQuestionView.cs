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
    public class MultyQuestionView : AbstractMultyQuestionView<AnswerViewModel>
    {
        public MultyQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source,
            Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership) { }

        protected SelectebleQuestionViewModel TypedMode
        {
            get { return this.Model as SelectebleQuestionViewModel; }
        }

        protected override IEnumerable<AnswerViewModel> Answers
        {
            get { return this.TypedMode.Answers; }
        }

        protected override int? MaxAllowedAnswers
        {
            get { return this.TypedMode.MaxAllowedAnswers; }
        }

        protected override bool? AreAnswersOrdered
        {
            get { return this.TypedMode.AreAnswersOrdered; }
        }
        
        protected override string GetAnswerId(AnswerViewModel answer)
        {
            return answer.PublicKey.ToString();
        }

        protected override string GetAnswerTitle(AnswerViewModel answer)
        {
            return answer.Title;
        }

        protected override AnswerViewModel FindAnswerInModelByCheckBoxTag(string tag)
        {
            var answerGuid = Guid.Parse(tag);
            return this.Answers.FirstOrDefault(
                a => a.PublicKey == answerGuid);
        }

        protected override AnswerQuestionCommand CreateSaveAnswerCommand(AnswerViewModel[] selectedAnswers)
        {
            var answered = selectedAnswers.Where(w => w.AnswerOrder>0).OrderBy(o => o.AnswerOrder).Select(a => a.Value)
                .Union(selectedAnswers.Where(w => w.AnswerOrder == 0).Select(s=>s.Value)).ToList();

            return new AnswerMultipleOptionsQuestionCommand(this.QuestionnairePublicKey,
                this.Membership.CurrentUser.Id,
                this.Model.PublicKey.Id, this.Model.PublicKey.PropagationVector, DateTime.UtcNow, answered.ToArray());
        }

        protected override bool IsAnswerSelected(AnswerViewModel answer)
        {
            return answer.Selected;
        }

        protected override int GetAnswerOrder(AnswerViewModel answer)
        {
            return answer.AnswerOrder;
        }
    }
}