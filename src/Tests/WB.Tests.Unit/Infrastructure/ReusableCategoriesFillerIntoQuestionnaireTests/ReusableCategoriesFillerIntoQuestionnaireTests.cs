using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Questionnaire;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.ReusableCategoriesFillerIntoQuestionnaireTests
{
    [TestFixture]
    public class ReusableCategoriesFillerIntoQuestionnaireTests
    {
        [Test]
        public void when_call_FillCategoriesIntoQuestionnaireDocument_should_fill_Answers_collection_from_categories()
        {
            List<CategoriesItem> items1 = new List<CategoriesItem>()
            {
                Create.Entity.CategoriesItem("1", 11, 111),
                Create.Entity.CategoriesItem("2", 22, 111),
                Create.Entity.CategoriesItem("3", 33, 222),
            };

            List<CategoriesItem> items2 = new List<CategoriesItem>()
            {
                Create.Entity.CategoriesItem("4", 44, 44),
                Create.Entity.CategoriesItem("5", 55, 44),
            };

            var document = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.SingleOptionQuestion(Id.g1, categoryId: Id.g1, answers: new List<Answer>()),
                Create.Entity.SingleOptionQuestion(Id.g2, categoryId: Id.g2, answers: new List<Answer>()),
                Create.Entity.MultyOptionsQuestion(Id.g3, categoryId: Id.g1, options: new List<Answer>()),
            });
            document.Categories = new List<Categories>()
            {
                Create.Entity.Categories(Id.g1),
                Create.Entity.Categories(Id.g2),
            };

            var reusableCategoriesStorage = Mock.Of<IReusableCategoriesStorage>(s => 
                s.GetOptions(Id.QuestionnaireIdentity1, Id.g1) == items1 &&
                s.GetOptions(Id.QuestionnaireIdentity1, Id.g2) == items2
                );
            var fillerIntoQuestionnaire = Create.Service.ReusableCategoriesFillerIntoQuestionnaire(reusableCategoriesStorage);

            var result = fillerIntoQuestionnaire.FillCategoriesIntoQuestionnaireDocument(Id.QuestionnaireIdentity1, document);

            var single1Answers = result.Find<ICategoricalQuestion>(Id.g1).Answers;
            CollectionAssert.AreEqual(single1Answers.Select(a => a.ParentValue), items1.Select(a => a.ParentId.ToString()));
            CollectionAssert.AreEqual(single1Answers.Select(a => a.ParentCode), items1.Select(a => a.ParentId));
            CollectionAssert.AreEqual(single1Answers.Select(a => a.AnswerCode), items1.Select(a => a.Id));
            CollectionAssert.AreEqual(single1Answers.Select(a => a.AnswerText), items1.Select(a => a.Text));
            CollectionAssert.AreEqual(single1Answers.Select(a => a.AnswerValue), items1.Select(a => a.Id.ToString()));

            var single2Answers = result.Find<ICategoricalQuestion>(Id.g2).Answers;
            CollectionAssert.AreEqual(single2Answers.Select(a => a.ParentValue), items2.Select(a => a.ParentId.ToString()));
            CollectionAssert.AreEqual(single2Answers.Select(a => a.ParentCode), items2.Select(a => a.ParentId));
            CollectionAssert.AreEqual(single2Answers.Select(a => a.AnswerCode), items2.Select(a => a.Id));
            CollectionAssert.AreEqual(single2Answers.Select(a => a.AnswerText), items2.Select(a => a.Text));
            CollectionAssert.AreEqual(single2Answers.Select(a => a.AnswerValue), items2.Select(a => a.Id.ToString()));

            var multiAnswers = result.Find<ICategoricalQuestion>(Id.g3).Answers;
            CollectionAssert.AreEqual(multiAnswers.Select(a => a.ParentValue), items1.Select(a => a.ParentId.ToString()));
            CollectionAssert.AreEqual(multiAnswers.Select(a => a.ParentCode), items1.Select(a => a.ParentId));
            CollectionAssert.AreEqual(multiAnswers.Select(a => a.AnswerCode), items1.Select(a => a.Id));
            CollectionAssert.AreEqual(multiAnswers.Select(a => a.AnswerText), items1.Select(a => a.Text));
            CollectionAssert.AreEqual(multiAnswers.Select(a => a.AnswerValue), items1.Select(a => a.Id.ToString()));
        }
    }
}
