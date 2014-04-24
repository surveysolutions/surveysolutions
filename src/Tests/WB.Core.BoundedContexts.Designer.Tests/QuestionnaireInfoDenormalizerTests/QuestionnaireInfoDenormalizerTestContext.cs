using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoDenormalizerTests
{
    internal class QuestionnaireInfoDenormalizerTestContext
    {
        protected static QuestionnsAndGroupsCollectionDenormalizer CreateQuestionnaireInfoDenormalizer(
            IReadSideRepositoryWriter<QuestionsAndGroupsCollectionView> readsideRepositoryWriter = null,
            IQuestionDetailsFactory questionDetailsFactory = null, 
            IQuestionFactory questionFactory = null)
        {
            return new QuestionnsAndGroupsCollectionDenormalizer(readsideRepositoryWriter ?? Mock.Of<IReadSideRepositoryWriter<QuestionsAndGroupsCollectionView>>(),
                questionDetailsFactory ?? Mock.Of<IQuestionDetailsFactory>(),
                questionFactory ?? Mock.Of<IQuestionFactory>());
        }
    }
}
