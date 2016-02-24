using System;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SampleUploadViewFactoryTests
{
    [TestFixture]
    internal class SampleUploadViewFactoryNUnitTests
    {
        private SampleUploadViewFactory CreateSampleUploadViewFactory(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires = null, QuestionnaireExportStructure questionnaireExportStructure=null)
        {
            return new SampleUploadViewFactory(questionnaires ?? new TestInMemoryPlainStorageAccessor<QuestionnaireBrowseItem>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireExportStructure>>(_=>_.GetById(Moq.It.IsAny<string>())== questionnaireExportStructure));
        }

        private SampleUploadViewInputModel CreateSampleUploadViewInputModel(Guid? questionnaireId=null)
        {
            return new SampleUploadViewInputModel(questionnaireId??Guid.NewGuid(), 1);
        }

        [Test]
        public void Load_When_questionnaire_is_missing_then_null_should_be_returned
            ()
        {
            var sampleUploadViewFactory =
                this.CreateSampleUploadViewFactory();

            var result = sampleUploadViewFactory.Load(CreateSampleUploadViewInputModel());

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Load_When_questionnaire_export_structure_is_missing_then_null_should_be_returned
            ()
        {
            var questionnaireId = Guid.NewGuid();

            var questionnaireStorage = new TestInMemoryPlainStorageAccessor<QuestionnaireBrowseItem>();
            questionnaireStorage.AsVersioned().Store(Create.QuestionnaireBrowseItem(questionnaireId: questionnaireId), questionnaireId.FormatGuid(), 1);

            var sampleUploadViewFactory =
                this.CreateSampleUploadViewFactory(questionnaires: questionnaireStorage);

            var result = sampleUploadViewFactory.Load(CreateSampleUploadViewInputModel(questionnaireId: questionnaireId));

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Load_When_questionnaire_export_structure_has_no_top_level_then_null_should_be_returned
            ()
        {
            var questionnaireId = Guid.NewGuid();

            var questionnaireStorage = new TestInMemoryPlainStorageAccessor<QuestionnaireBrowseItem>();
            var questionnaireExportStructureStorage = new TestInMemoryPlainStorageAccessor<QuestionnaireExportStructure>();

            questionnaireStorage.AsVersioned().Store(Create.QuestionnaireBrowseItem(questionnaireId: questionnaireId), questionnaireId.FormatGuid(), 1);
            (questionnaireExportStructureStorage).AsVersioned().Store(Create.QuestionnaireExportStructure(), questionnaireId.FormatGuid(), 1);

            var sampleUploadViewFactory =
                this.CreateSampleUploadViewFactory(questionnaires: questionnaireStorage);

            var result = sampleUploadViewFactory.Load(CreateSampleUploadViewInputModel(questionnaireId: questionnaireId));

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Load_When_questionnaire_has_1_featured_question_then_sample_preloading_structure_should_be_returned
            ()
        {
            var questionnaireId = Guid.NewGuid();
            var prefiledQuestionVarName = "pref";

            var questionnaireStorage = new TestInMemoryPlainStorageAccessor<QuestionnaireBrowseItem>();

            var prefilledTxtQuestion = Create.TextQuestion(preFilled: true, variable: prefiledQuestionVarName);
            (questionnaireStorage).AsVersioned()
                .Store(Create.QuestionnaireBrowseItem(
                    Create.QuestionnaireDocument(children:
                    new IComposite[] { prefilledTxtQuestion, Create.TextQuestion(preFilled: true) })),
                    questionnaireId.FormatGuid(), 1);

            var exportStructure = Create.QuestionnaireExportStructure();
            var headerStructure = Create.HeaderStructureForLevel();
            exportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(), headerStructure);
            headerStructure.HeaderItems.Add(prefilledTxtQuestion.PublicKey,
                Create.ExportedHeaderItem(questionId: prefilledTxtQuestion.PublicKey,
                    variableName: prefiledQuestionVarName));

            var sampleUploadViewFactory =
                this.CreateSampleUploadViewFactory(questionnaires: questionnaireStorage, questionnaireExportStructure: exportStructure);

            var result = sampleUploadViewFactory.Load(CreateSampleUploadViewInputModel(questionnaireId: questionnaireId));

            Assert.That(result, Is.Not.Null);

            Assert.That(result.ColumnListToPreload[0].Caption, Is.EqualTo(prefiledQuestionVarName));
            Assert.That(result.ColumnListToPreload[0].Id, Is.EqualTo(prefilledTxtQuestion.PublicKey));
        }
    }
}