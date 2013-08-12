using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class SelectebleQuestionViewModel:QuestionViewModel
    {
        public SelectebleQuestionViewModel(
            ItemPublicKey publicKey, 
            string text,
            QuestionType questionType, 
            IEnumerable<AnswerViewModel> answers, 
            bool enabled, 
            string instructions, 
            string comments, 
            bool valid, 
            bool mandatory, 
            bool capital,
            object answerObject, 
            string validationMessage)
            : base(publicKey, text, questionType, enabled, instructions, comments, valid, mandatory, capital, answerObject, validationMessage)
        {
            Answers = answers;
        }

        [JsonConstructor]
        public SelectebleQuestionViewModel(
            ItemPublicKey publicKey,
            string text,
            QuestionType questionType,
            IEnumerable<AnswerViewModel> answers,
            QuestionStatus status,
            string instructions,
            string comments,
            bool mandatory,
            bool capital,
            object answerObject,
            string validationMessage)
            : base(publicKey, text, questionType, status, instructions, comments, mandatory, capital, answerObject, validationMessage)
        {
            Answers = answers;
        }
        public IEnumerable<AnswerViewModel> Answers { get; private set; }
        public override IQuestionnaireItemViewModel Clone(Guid propagationKey)
        {
            IList<AnswerViewModel> newAnswers = new List<AnswerViewModel>();
            foreach (AnswerViewModel answerViewModel in Answers)
            {
                newAnswers.Add(answerViewModel.Clone() as AnswerViewModel);
            }
            return new SelectebleQuestionViewModel(new ItemPublicKey(this.PublicKey.PublicKey, propagationKey),
                                                   this.Text, this.QuestionType, newAnswers,
                                                   this.Status.HasFlag(QuestionStatus.Enabled), this.Instructions,
                                                   this.Comments, this.Status.HasFlag(QuestionStatus.Valid),
                                                   this.Mandatory, this.Capital, this.AnswerString, this.ValidationMessage);
        }

        public override void SetAnswer(object answer)
        {
            if (answer == null)
            {
                return;
            }
            var typedAnswers = answer as decimal[];
            if(typedAnswers==null)
                return;
            foreach (var item in this.Answers)
            {
                item.Selected = typedAnswers.Contains(item.Value);
            }
            base.SetAnswer(answer);
            if (QuestionType==QuestionType.MultyOption && Status.HasFlag(QuestionStatus.Answered) && !Answers.Any(a=>a.Selected))
            {
                Status &= ~QuestionStatus.Answered;
                RaisePropertyChanged("Status");
            }
        }
    }
}