using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.Tests.Views.Questonnaire
{
    [Subject(typeof(QuestionnaireView), "GetChaptersCount")]
    public class when_has_2_chapters
    {
        static QuestionnaireView view;
        static QuestionnaireDocument document;

        Establish context = () => {
            document = new QuestionnaireDocument();
            document.PublicKey = Guid.NewGuid();
            document.Add(new Group("test1"), document.PublicKey, null);
            document.Add(new Group("test2"), document.PublicKey, null);
        };

        Because of = () => view = new QuestionnaireView(document);

        It should_return_2 = () => view.GetChaptersCount().ShouldEqual(2);
    }
}