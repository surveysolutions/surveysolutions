using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireChangeHistoryFactoryTests
{
    [Subject(typeof(QuestionnaireChangeHistoryFactory))]
    internal class QuestionnaireChangeHistoryFactoryTestContext
    {
        protected static QuestionnaireChangeHistoryFactory CreateQuestionnaireChangeHistoryFactory(
            IQueryableReadSideRepositoryReader<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage = null,
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage = null)
        {
            return
                new QuestionnaireChangeHistoryFactory(
                    questionnaireChangeHistoryStorage ??
                    Mock.Of<IQueryableReadSideRepositoryReader<QuestionnaireChangeRecord>>(),
                    questionnaireDocumentStorage ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>());
        }
    }
}
