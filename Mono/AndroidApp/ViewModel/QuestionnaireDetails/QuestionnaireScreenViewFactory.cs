// -----------------------------------------------------------------------
// <copyright file="QuestionnaireScreenViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Entities.SubEntities;
using Main.Core.View;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireScreenViewFactory : IViewFactory<QuestionnaireScreenInput, QuestionnaireScreenViewModel>
    {
        #region Implementation of IViewFactory<QuestionnaireScreenInput,QuestionnaireScreenViewModel>

        public QuestionnaireScreenViewModel Load(QuestionnaireScreenInput input)
        {
            var screens =  new QuestionnaireNavigationPanelItem[]
                {
                    new QuestionnaireNavigationPanelItem(Guid.NewGuid(),"hello1",20,1),
                    new QuestionnaireNavigationPanelItem(Guid.NewGuid(),"hello2",30,14),
                };
            var answers = new AnswerView[]
                {
                    new AnswerView(Guid.NewGuid(),"a1",false), 
                    new AnswerView(Guid.NewGuid(),"a2",false), 
                    new AnswerView(Guid.NewGuid(),"a3",true), 
                    new AnswerView(Guid.NewGuid(),"a4",false), 
                    new AnswerView(Guid.NewGuid(),"a5",false), 
                };
            var questions = new QuestionView[]
                {
                    new ValueQuestionView(Guid.NewGuid(), "numeric", QuestionType.Numeric, "10", true, string.Empty,
                                          "comments on to question"),
                    new ValueQuestionView(Guid.NewGuid(), "text", QuestionType.Text, "answer", true, "hey punk",
                                          string.Empty),
                    new ValueQuestionView(Guid.NewGuid(), "current date", QuestionType.DateTime, DateTime.Now.ToString(),
                                          true, "hello world", string.Empty),
                    new SelectebleQuestionView(Guid.NewGuid(), "single choise", QuestionType.SingleOption, answers, true,
                                               string.Empty, string.Empty),
                    new SelectebleQuestionView(Guid.NewGuid(), "multy choise", QuestionType.MultyOption, answers, true,
                                               " public class QuestionnaireScreenViewFactory : IViewFactory<QuestionnaireScreenInput, QuestionnaireScreenViewModel>",
                                               string.Empty),
                    new SelectebleQuestionView(Guid.NewGuid(), "multy choise", QuestionType.MultyOption, answers, false,
                                               string.Empty, "comment on disabled question")
                };
            return new QuestionnaireScreenViewModel(input.QuestionnaireId, input.ScreenPublicKey ?? Guid.NewGuid(),
                                                    input.PropagationKey, questions,screens,
                                                    Enumerable.Empty<QuestionnaireNavigationPanelItem>(), screens);
        }

        #endregion
    }
}
