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
            string answerString, 
            string validationExpression,
            string validationMessage)
            : base(publicKey, text, questionType, enabled, instructions, comments, valid, mandatory, capital, answerString, validationExpression, validationMessage)
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
            string answerString,
            string validationExpression,
            string validationMessage)
            : base(publicKey, text, questionType, status, instructions, comments,  mandatory, capital, answerString, validationExpression, validationMessage)
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
                                                   this.Mandatory, this.Capital, this.AnswerString, this.ValidationExpression,this.ValidationMessage);
        }

        public override string AnswerObject
        {
            get
            {
                var value = this.Answers.Where(a => a.Selected).Select(a => a.Value).FirstOrDefault();
                if (value == null)
                    return string.Empty;
                return value;
            }
        }

        public override void SetAnswer(List<Guid> answer, string answerString)
        {
            if (answer == null)
            {
                return;
            }
            foreach (var item in this.Answers)
            {
                item.Selected = answer.Contains(item.PublicKey);
            }
            base.SetAnswer(answer, answerString);
            if (QuestionType==QuestionType.MultyOption && Status.HasFlag(QuestionStatus.Answered) && !Answers.Any(a=>a.Selected))
            {
                Status &= ~QuestionStatus.Answered;
                RaisePropertyChanged("Status");
            }
        }
    }
}