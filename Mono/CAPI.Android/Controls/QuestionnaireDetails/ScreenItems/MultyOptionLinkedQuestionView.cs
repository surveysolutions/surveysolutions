using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class MultyOptionLinkedQuestionView: AbstractMultyQuestionView<LinkedAnswerViewModel>
    {
        private const char Separator = ',';

        public MultyOptionLinkedQuestionView(
            Context context,
            IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel model,
            Guid questionnairePublicKey,
            IAnswerOnQuestionCommandService commandService
            )
            : base(context, bindingActivity, model, questionnairePublicKey, commandService)
        {
            this.Model.PropertyChanged += ModelOnPropertyChanged;
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            bool answerOptionsChanged = propertyChangedEventArgs.PropertyName == "AnswerOptions";
            if (!answerOptionsChanged)
                return;

            this.CreateCheckBoxesByOptions();
        }

        protected LinkedQuestionViewModel TypedMode {
            get { return Model as LinkedQuestionViewModel; }
        }

        protected override IEnumerable<LinkedAnswerViewModel> Answers
        {
            get { return TypedMode.AnswerOptions; }
        }

        protected override string GetAnswerId(LinkedAnswerViewModel answer)
        {
            return string.Join(Separator.ToString(), answer.PropagationVector);
        }

        protected override string GetAnswerTitle(LinkedAnswerViewModel answer)
        {
            return answer.Title;
        }

        protected override LinkedAnswerViewModel FindAnswerInModelByCheckBoxTag(string tag)
        {
            var vector = tag.Split(Separator).Select(int.Parse).ToArray();
            return Answers.FirstOrDefault(
              a => LinkedQuestionViewModel.IsVectorsEqual(a.PropagationVector, vector));
        }

        protected override AnswerQuestionCommand CreateSaveAnswerCommand(LinkedAnswerViewModel selectedAnswer, bool isChecked)
        {
            var answered = TypedMode.SelectedAnswers.ToList();

            if (isChecked)
                answered.Add(selectedAnswer.PropagationVector);
            else
            {
                answered.RemoveAll(answer => LinkedQuestionViewModel.IsVectorsEqual(selectedAnswer.PropagationVector, answer));
            }
            return new AnswerMultipleOptionsLinkedQuestionCommand(this.QuestionnairePublicKey,
                CapiApplication.Membership.CurrentUser.Id,
                Model.PublicKey.Id, this.Model.PublicKey.PropagationVector, DateTime.UtcNow,
                answered.ToArray());
        }

        protected override bool IsAnswerSelected(LinkedAnswerViewModel answer)
        {
            return TypedMode.SelectedAnswers.Any(a => LinkedQuestionViewModel.IsVectorsEqual(a, answer.PropagationVector));
        }
    }
}