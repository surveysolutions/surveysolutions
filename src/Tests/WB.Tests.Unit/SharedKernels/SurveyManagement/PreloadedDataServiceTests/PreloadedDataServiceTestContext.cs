using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    [Subject(typeof(PreloadedDataService))]
    internal class PreloadedDataServiceTestContext
    {
        protected static PreloadedDataService CreatePreloadedDataService(QuestionnaireDocument questionnaireDocument = null)
        {
            var questionnaireExportStructure = (questionnaireDocument == null
                ? null
                : new ExportViewFactory(
                    new QuestionnaireRosterStructureFactory(), 
                    Mock.Of<IFileSystemAccessor>(_ => _.MakeStataCompatibleFileName(questionnaireDocument.Title) == questionnaireDocument.Title),
                    new ExportQuestionService()).CreateQuestionnaireExportStructure(questionnaireDocument, 1));

            var questionnaireRosterStructure = (questionnaireDocument == null
                ? null
                : new QuestionnaireRosterStructureFactory().CreateQuestionnaireRosterStructure(questionnaireDocument, 1));

            var userViewFactory = new Mock<IUserViewFactory>();
            return new PreloadedDataService(questionnaireExportStructure, questionnaireRosterStructure, questionnaireDocument,
                new QuestionDataParser(), userViewFactory.Object);
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

        protected static PreloadedDataByFile CreatePreloadedDataByFile(string[] header = null, string[][] content = null, string fileName = null)
        {
            return new PreloadedDataByFile(Guid.NewGuid().FormatGuid(), fileName ?? "some file", header ?? new string[] { ServiceColumns.Id, ServiceColumns.ParentId },
                content ?? new string[0][]);
        }
    }
}
