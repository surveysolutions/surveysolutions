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
            var answers = new AnswerViewModel[]
                {
                    new AnswerViewModel(Guid.NewGuid(),"a1",false), 
                    new AnswerViewModel(Guid.NewGuid(),"a2",false), 
                    new AnswerViewModel(Guid.NewGuid(),"a3",true), 
                    new AnswerViewModel(Guid.NewGuid(),"a4",false), 
                    new AnswerViewModel(Guid.NewGuid(),"a5",false), 
                };
            var questions = new IQuestionnaireItemViewModel[]
                {
                    new ValueQuestionViewModel(Guid.NewGuid(), "numeric", QuestionType.Numeric, "10", true, string.Empty,
                                          "comments on to question", true, true),
                    new ValueQuestionViewModel(Guid.NewGuid(), "text", QuestionType.Text, "answer", true, "hey punk",
                                          string.Empty, false, false),
                    new ValueQuestionViewModel(Guid.NewGuid(), "un answered question", QuestionType.Text, null, true,
                                          string.Empty,
                                          string.Empty, true, false),
                    new GroupViewModel(Guid.NewGuid(), "middle group", true),
                    new ValueQuestionViewModel(Guid.NewGuid(), "current date", QuestionType.DateTime, DateTime.Now.ToString(),
                                          true, "hello world", string.Empty, true, false),
                    new SelectebleQuestionViewModel(Guid.NewGuid(), "single choise", QuestionType.SingleOption, answers, true,
                                               string.Empty, string.Empty, false, false),
                    new GroupViewModel(Guid.NewGuid(), "disabled group", false),
                    new SelectebleQuestionViewModel(Guid.NewGuid(), "multy choise", QuestionType.MultyOption, answers, true,
                                               " public class QuestionnaireScreenViewFactory : IViewFactory<QuestionnaireScreenInput, QuestionnaireScreenViewModel>",
                                               string.Empty, true, false),
                    new SelectebleQuestionViewModel(Guid.NewGuid(), "multy choise", QuestionType.MultyOption, answers, false,
                                               string.Empty, "comment on disabled question", true, false)
                };
            return new QuestionnaireScreenViewModel(input.QuestionnaireId, input.ScreenPublicKey ?? Guid.NewGuid(),
                                                    input.PropagationKey, questions,screens,
                                                    Enumerable.Empty<QuestionnaireNavigationPanelItem>(), screens);
        }

        #endregion
    }
}
