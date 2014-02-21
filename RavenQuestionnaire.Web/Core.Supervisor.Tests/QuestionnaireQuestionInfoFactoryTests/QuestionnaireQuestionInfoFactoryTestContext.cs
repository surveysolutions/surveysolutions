using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Reposts.Factories;
using Core.Supervisor.Views.Reposts.InputModels;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Tests.QuestionnaireQuestionInfoFactoryTests
{
    internal class QuestionnaireQuestionInfoFactoryTestContext
    {
        protected static IQuestion CreateQuestion(QuestionType questionType, Guid? questionId = null)
        {
            return Mock.Of<IQuestion>(x
                => x.QuestionType == questionType
                   && x.StataExportCaption == questionType.ToString()
                   && x.PublicKey == (questionId.HasValue? questionId.Value : Guid.NewGuid())
                );
        }
        protected static QuestionnaireQuestionInfoFactory CreateQuestionnaireQuestionInfoFactory(IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireStore = null)
        {
            return new QuestionnaireQuestionInfoFactory(questionnaireStore ?? Mock.Of<IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned>>());
        }

        protected static QuestionnaireQuestionInfoInputModel CreateQuestionnaireQuestionInfoInputModel(Guid questionnaireId, long version = 1, QuestionType? questionType = null)
        {
            return new QuestionnaireQuestionInfoInputModel
            {
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = version,
                QuestionType = questionType
            };
        }
    }
}
