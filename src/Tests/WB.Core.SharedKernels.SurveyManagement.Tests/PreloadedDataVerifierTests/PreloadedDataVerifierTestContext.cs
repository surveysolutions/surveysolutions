﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadedDataVerifierTests
{
    [Subject(typeof(PreloadedDataVerifier))]
    internal class PreloadedDataVerifierTestContext
    {
        protected static PreloadedDataVerifier CreatePreloadedDataVerifier(QuestionnaireDocument questionnaireDocument = null, IQuestionDataParser questionDataParser=null)
        {
            var questionnaireExportStructure = (questionnaireDocument == null
                ? null
                : new ExportViewFactory(new ReferenceInfoForLinkedQuestionsFactory(),
                    new QuestionnaireRosterStructureFactory()).CreateQuestionnaireExportStructure(questionnaireDocument, 1));
            var questionnaireRosterStructure = (questionnaireDocument == null
                ? null
                : new QuestionnaireRosterStructureFactory().CreateQuestionnaireRosterStructure(questionnaireDocument, 1));
            var dataFileServiceMock = new Mock<IDataFileService>();
            dataFileServiceMock.Setup(x => x.CreateValidFileName(Moq.It.IsAny<string>())).Returns<string>((name) => name);
            return
                new PreloadedDataVerifier(
                    Mock.Of<IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned>>(
                        _ =>
                            _.GetById(Moq.It.IsAny<string>(), Moq.It.IsAny<long>()) == (questionnaireDocument != null
                                ? new QuestionnaireDocumentVersioned() { Questionnaire = questionnaireDocument }
                                : null)),
                    Mock.Of<IVersionedReadSideRepositoryReader<QuestionnaireExportStructure>>(
                        _ =>
                            _.GetById(Moq.It.IsAny<string>(), Moq.It.IsAny<long>()) == questionnaireExportStructure),
                    questionDataParser ?? Mock.Of<IQuestionDataParser>(),
                    new QuestionnaireFactory(),
                    Mock.Of<IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure>>(
                        _ => _.GetById(Moq.It.IsAny<string>(), Moq.It.IsAny<long>()) == questionnaireRosterStructure),
                    Mock.Of<IPreloadedDataServiceFactory>(
                        _ => _.CreatePreloadedDataService(questionnaireExportStructure, questionnaireRosterStructure, questionnaireDocument) ==
                            new PreloadedDataService(questionnaireExportStructure, questionnaireRosterStructure, questionnaireDocument, dataFileServiceMock.Object)));
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
