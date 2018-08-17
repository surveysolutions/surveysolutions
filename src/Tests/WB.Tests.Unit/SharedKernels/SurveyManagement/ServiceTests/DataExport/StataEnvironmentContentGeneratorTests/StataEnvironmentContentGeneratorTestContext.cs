using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    [NUnit.Framework.TestOf(typeof(StataEnvironmentContentService))]
    internal class StataEnvironmentContentGeneratorTestContext
    {
        protected static StataEnvironmentContentService CreateStataEnvironmentContentGenerator(
            IFileSystemAccessor fileSystemAccessor)
        {
            return new StataEnvironmentContentService(fileSystemAccessor, 
                new QuestionnaireLabelFactory(),
                new InterviewActionsExporter(Mock.Of<InterviewDataExportSettings>(), 
                    Mock.Of<IFileSystemAccessor>(), 
                    Mock.Of<ICsvWriter>(),
                    Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                    Mock.Of<ILogger>(),
                    Mock.Of<IUnitOfWork>()),
                new CommentsExporter(Mock.Of<InterviewDataExportSettings>(),
                    Mock.Of<IFileSystemAccessor>(),
                    Mock.Of<ICsvWriter>(),
                    Mock.Of<IQueryableReadSideRepositoryReader<InterviewCommentaries>>(),
                    Mock.Of<ILogger>()),
                new InterviewErrorsExporter(Mock.Of<ICsvWriter>(), Mock.Of<IQuestionnaireStorage>(), Mock.Of<IFileSystemAccessor>()),
                new DiagnosticsExporter(Mock.Of<InterviewDataExportSettings>(), 
                    Mock.Of<IFileSystemAccessor>(),
                    Mock.Of<ICsvWriter>(),
                    Mock.Of<ILogger>(),
                    Mock.Of<IInterviewDiagnosticsFactory>()));
        }

        protected static IFileSystemAccessor CreateFileSystemAccessor(Action<string> returnContentAction)
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.MakeStataCompatibleFileName(Moq.It.IsAny<string>()))
                .Returns<string>(s => s);
            fileSystemAccessorMock.Setup(x => x.WriteAllText(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Callback<string, string>((path, content) => returnContentAction(content));

            return fileSystemAccessorMock.Object;
        }

        protected static HeaderStructureForLevel CreateHeaderStructureForLevel(string levelName = null,
            params ExportedQuestionHeaderItem[] exportedQuestionHeaderItems)
        {
            var result = new HeaderStructureForLevel();
            result.LevelScopeVector = new ValueVector<Guid>();
            result.LevelName = levelName;
            result.LevelIdColumnName = "Id";
            foreach (var exportedHeaderItem in exportedQuestionHeaderItems)
            {
                result.HeaderItems.Add(exportedHeaderItem.PublicKey, exportedHeaderItem);
            }

            return result;
        }

        protected static ExportedQuestionHeaderItem CreateExportedHeaderItem(string variableName = "item",
            string title = "some item",
            params LabelItem[] labels)
        {
            return new ExportedQuestionHeaderItem()
            {
                PublicKey = Guid.NewGuid(),
                VariableName = variableName,
                QuestionType = QuestionType.Numeric,
                ColumnHeaders = GetHeaderColumns(variableName, title),
                Labels = (labels ?? new LabelItem[0]).ToList()
            };
        }

        protected static LabelItem CreateLabelItem(string caption = "caption", string title = "title")
        {
            return new LabelItem {Caption = caption, Title = title};
        }

        protected static List<HeaderColumn> GetHeaderColumns(string variableName, string title)
        {
            return new List<HeaderColumn> {new HeaderColumn {Name = variableName, Title = title}};
        }
    }
}
