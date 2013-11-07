using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Widget;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class MultyQuestionView : AbstractMultyQuestionView<AnswerViewModel>
    {
        public MultyQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source,
            Guid questionnairePublicKey, IAnswerOnQuestionCommandService commandService)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService) {}

        protected SelectebleQuestionViewModel TypedMode
        {
            get { return Model as SelectebleQuestionViewModel; }
        }

        protected override IEnumerable<AnswerViewModel> Answers
        {
            get { return TypedMode.Answers; }
        }

        protected override int? MaxAllowedAnswers
        {
            get { return TypedMode.MaxAllowedAnswers; }
        }

        protected override bool? AreAnswersOrdered
        {
            get { return TypedMode.AreAnswersOrdered; }
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
            return Answers.FirstOrDefault(
                a => a.PublicKey == answerGuid);
        }

        protected override AnswerQuestionCommand CreateSaveAnswerCommand(AnswerViewModel[] selectedAnswers)
        {
            var answered = selectedAnswers.Where(w => w.AnswerOrder>0).OrderBy(o => o.AnswerOrder).Select(a => a.Value)
                .Union(selectedAnswers.Where(w => w.AnswerOrder == 0).Select(s=>s.Value)).ToList();

            return new AnswerMultipleOptionsQuestionCommand(this.QuestionnairePublicKey,
                CapiApplication.Membership.CurrentUser.Id,
                Model.PublicKey.Id, this.Model.PublicKey.PropagationVector, DateTime.UtcNow, answered.ToArray());
        }

        protected override bool IsAnswerSelected(AnswerViewModel answer)
        {
            return answer.Selected;
        }

        protected override int GetAnswerOrder(AnswerViewModel answer)
        {
            return answer.AnswerOrder;
        }

        protected override void AddAdditionalAttributes(CheckBox checkBox, AnswerViewModel answer)
        {
            base.AddAdditionalAttributes(checkBox, answer);
            checkBox.AttachImage(answer);
        }
    }
}