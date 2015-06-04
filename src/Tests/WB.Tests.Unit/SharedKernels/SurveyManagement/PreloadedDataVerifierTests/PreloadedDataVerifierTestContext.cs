using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    [Subject(typeof(PreloadedDataVerifier))]
    internal class PreloadedDataVerifierTestContext
    {
        protected static PreloadedDataVerifier CreatePreloadedDataVerifier(QuestionnaireDocument questionnaireDocument = null,
            IQuestionDataParser questionDataParser = null, IPreloadedDataService preloadedDataService = null, IUserViewFactory userViewFactory=null)
        {
            var questionnaireExportStructure = (questionnaireDocument == null
                ? null
                : new ExportViewFactory(new ReferenceInfoForLinkedQuestionsFactory(),
                    new QuestionnaireRosterStructureFactory(), Mock.Of<IFileSystemAccessor>()).CreateQuestionnaireExportStructure(questionnaireDocument, 1));
            var questionnaireRosterStructure = (questionnaireDocument == null
                ? null
                : new QuestionnaireRosterStructureFactory().CreateQuestionnaireRosterStructure(questionnaireDocument, 1));
            return
                new PreloadedDataVerifier(
                    Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(
                        _ =>
                            _.GetById(Moq.It.IsAny<string>()) == (questionnaireDocument != null
                                ? new QuestionnaireDocumentVersioned() { Questionnaire = questionnaireDocument }
                                : null)),
                    Mock.Of<IReadSideKeyValueStorage<QuestionnaireExportStructure>>(
                        _ =>
                            _.GetById(Moq.It.IsAny<string>()) == questionnaireExportStructure),
                    Mock.Of<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>(
                        _ => _.GetById(Moq.It.IsAny<string>()) == questionnaireRosterStructure),
                    Mock.Of<IPreloadedDataServiceFactory>(
                        _ =>
                            _.CreatePreloadedDataService(Moq.It.IsAny<QuestionnaireExportStructure>(), Moq.It.IsAny<QuestionnaireRosterStructure>(), Moq.It.IsAny<QuestionnaireDocument>()) ==
                                (preloadedDataService ?? Mock.Of<IPreloadedDataService>())),
                    userViewFactory ?? Mock.Of<IUserViewFactory>());
        }

        protected static PreloadedDataByFile CreatePreloadedDataByFile(string[] header=null, string[][] content=null, string fileName=null)
        {
            return new PreloadedDataByFile(Guid.NewGuid().FormatGuid(), fileName ?? "some file", header ?? new string[] { "Id", "ParentId" },
                content ?? new string[0][]);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Title = "some title",
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                        IsRoster = false
                    }
                }
            };
        }
    }
}
