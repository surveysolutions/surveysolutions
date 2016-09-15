using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    [Subject(typeof(PreloadedDataVerifier))]
    internal class PreloadedDataVerifierTestContext
    {
        protected static PreloadedDataVerifier CreatePreloadedDataVerifier(
            QuestionnaireDocument questionnaireDocument = null, 
            IPreloadedDataService preloadedDataService = null,
            IUserViewFactory userViewFactory=null)
        {
            var questionnaireExportStructure = (questionnaireDocument == null
                ? null
                : new ExportViewFactory(new QuestionnaireRosterStructureFactory(), Mock.Of<IFileSystemAccessor>(), new ExportQuestionService()).CreateQuestionnaireExportStructure(questionnaireDocument, 1));

            var questionnaireRosterStructure = (questionnaireDocument == null
                ? null
                : new QuestionnaireRosterStructureFactory().CreateQuestionnaireRosterStructure(questionnaireDocument, 1));

            var preloadedService = new PreloadedDataService(questionnaireExportStructure, questionnaireRosterStructure,
                questionnaireDocument, new QuestionDataParser(), Mock.Of<IUserViewFactory>());
            return
                new PreloadedDataVerifier(
                    Mock.Of<IPreloadedDataServiceFactory>(
                        _ =>
                            _.CreatePreloadedDataService(Moq.It.IsAny<QuestionnaireExportStructure>(),
                                Moq.It.IsAny<QuestionnaireRosterStructure>(), Moq.It.IsAny<QuestionnaireDocument>()) ==
                            (preloadedDataService ?? preloadedService)),
                    userViewFactory ?? Mock.Of<IUserViewFactory>(),
                    Mock.Of<IQuestionnaireStorage>(
                        _ =>
                            _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) ==
                            questionnaireDocument),
                    Mock.Of<IQuestionnaireRosterStructureStorage>( _ => _.GetQuestionnaireRosterStructure(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaireRosterStructure),
                    Mock.Of<IQuestionnaireExportStructureStorage>(_ => _.GetQuestionnaireExportStructure(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaireExportStructure));
        }

        protected static PreloadedDataByFile CreatePreloadedDataByFile(string[] header=null, string[][] content=null, string fileName=null)
        {
            return new PreloadedDataByFile(Guid.NewGuid().FormatGuid(), fileName ?? "some file", header ?? new string[] { ServiceColumns.Id, ServiceColumns.ParentId },
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
