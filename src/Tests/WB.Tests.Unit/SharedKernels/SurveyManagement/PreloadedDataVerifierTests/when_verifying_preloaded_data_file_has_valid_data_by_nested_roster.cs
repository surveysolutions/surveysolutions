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
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_has_valid_data_by_nested_roster : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var rosterId = Guid.NewGuid();
            var nestedRosterId = Guid.NewGuid();

            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(
                    Create.Entity.FixedRoster(rosterId: rosterId,
                        obsoleteFixedTitles: new[] {"a"}, title: rosterTitle, children: new IComposite[]
                        {
                            Create.Entity.FixedRoster(rosterId: nestedRosterId,
                                obsoleteFixedTitles: new[] {"a"}, title: nestedRosterTitle)
                        }));

            questionnaire.Title = questionnaireTitle;

            preloadedDataByFileTopLevel = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId }, new string[][] { new string[] { "1" } },
                questionnaireTitle + ".csv");
            preloadedDataByFileRosterLevel = CreatePreloadedDataByFile(new[] { rosterTitle + "__id", "ParentId1" }, new string[][] { new string[] { "5", "1" } },
                rosterTitle + ".csv");
            preloadedDataByFileNestedRosterLevel = CreatePreloadedDataByFile(new[] { nestedRosterTitle + "__id", "ParentId1", "ParentId2" }, new string[][] { new string[] { "10", "5", "1" } },
                nestedRosterTitle + ".csv");

            files = new[] { preloadedDataByFileTopLevel, preloadedDataByFileRosterLevel, preloadedDataByFileNestedRosterLevel };
            preloadedDataServiceMock = new Mock<IPreloadedDataService>();
            preloadedDataServiceMock.Setup(x => x.GetIdColumnIndex(preloadedDataByFileRosterLevel)).Returns(0);
            preloadedDataServiceMock.Setup(x => x.GetParentIdColumnIndexes(preloadedDataByFileRosterLevel)).Returns(new[] { 1 });
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(Moq.It.IsAny<PreloadedDataByFile>(), Moq.It.IsAny<string>())).Returns(-1);
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(preloadedDataByFileTopLevel.FileName)).Returns(new HeaderStructureForLevel(){LevelIdColumnName = ServiceColumns.InterviewId });
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(preloadedDataByFileRosterLevel.FileName))
                .Returns(new HeaderStructureForLevel() { LevelScopeVector = new ValueVector<Guid>(new[] { rosterId }) });
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(preloadedDataByFileNestedRosterLevel.FileName))
                .Returns(new HeaderStructureForLevel() { LevelScopeVector = new ValueVector<Guid>(new[] { rosterId, nestedRosterId }) });

            preloadedDataServiceMock.Setup(x => x.GetParentDataFile(preloadedDataByFileRosterLevel.FileName, files))
                .Returns(preloadedDataByFileTopLevel);

            preloadedDataServiceMock.Setup(x => x.GetAvailableIdListForParent(preloadedDataByFileTopLevel, Moq.It.IsAny<ValueVector<Guid>>(), new[] { "1" }, files))
                .Returns(new [] { 5 });

            preloadedDataServiceMock.Setup(x => x.GetAvailableIdListForParent(preloadedDataByFileNestedRosterLevel, Moq.It.IsAny<ValueVector<Guid>>(), new[] { "5", "1" }, files))
                .Returns(new [] { 10 });

            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFileTopLevel, Moq.It.IsAny<string>())).Returns(-1);
            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);
        };

        Because of =
            () => importDataVerifier.VerifyPanelFiles(questionnaireId, 1, files, status);

        It should_result_has_0_errors = () =>
            status.VerificationState.Errors.Count().ShouldEqual(0);

        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFileTopLevel;
        private static PreloadedDataByFile preloadedDataByFileRosterLevel;
        private static PreloadedDataByFile preloadedDataByFileNestedRosterLevel;
        private static string questionnaireTitle = "questionnaire";
        private static string rosterTitle = "roster";
        private static string nestedRosterTitle = "nestedRoster";

        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
        private static PreloadedDataByFile[] files;
    }
}
