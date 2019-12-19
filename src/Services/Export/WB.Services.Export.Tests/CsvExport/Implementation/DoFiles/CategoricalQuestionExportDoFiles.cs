using System;
using System.Collections.Generic;
using System.Threading;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using Moq;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;

namespace WB.Services.Export.Tests.CsvExport.Implementation.DoFiles
{
    [UseApprovalSubdirectory("CategoricalQuestionExportDoFiles-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [TestOf(typeof(InterviewsDoFilesExporter))]
    internal class CategoricalQuestionExportDoFiles : StataEnvironmentContentGeneratorTestContext
    {
        [Test]
        public void when_two_text_question_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.TextQuestion(variable: "textQuestion1", questionText: "is it ?"),
                Create.TextQuestion(variable: "textQuestion2", questionText: "to be or not to be", variableLabel: "it is variable label"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_one_single_question_use_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.SingleOptionQuestion(variable: "singleQuestion1", questionText: "to be or not to be", categoryId: Id.g1),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_two_single_questions_use_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.SingleOptionQuestion(variable: "singleQuestion1", categoryId: Id.g1, questionText: "to be or not to be"),
                Create.SingleOptionQuestion(variable: "singleQuestion2", categoryId: Id.g1, questionText: "To Be?", variableLabel: "label"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_one_multi_question_use_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.MultyOptionsQuestion(variable: "multiQuestion1", categoryId: Id.g1, questionText: "to be or not to be"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_two_multi_questions_use_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.MultyOptionsQuestion(variable: "multiQuestion1", categoryId: Id.g1, questionText: "to be or not to be"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", categoryId: Id.g1, questionText: "To Be?", variableLabel: "label"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_one_single_question_dont_use_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.SingleOptionQuestion(variable: "singleQuestion1", options: answers, questionText: "to be or not to be"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }


        [Test]
        public void when_two_single_questions_dont_use_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.SingleOptionQuestion(variable: "singleQuestion1", options: answers, questionText: "to be or not to be"),
                Create.SingleOptionQuestion(variable: "singleQuestion2", options: answers, questionText: "To Be?", variableLabel: "label"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_one_multi_question_dont_use_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.MultyOptionsQuestion(variable: "multiQuestion1", options: answers, questionText: "to be or not to be"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_two_multi_questions_dont_use_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.MultyOptionsQuestion(variable: "multiQuestion1", options: answers, questionText: "to be or not to be"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", options: answers, questionText: "To Be?", variableLabel: "label"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_two_multi_questions_With_and_without_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.MultyOptionsQuestion(variable: "multiQuestion1", categoryId: Id.g1, questionText: "to be or not to be"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", options: answers, questionText: "to be?", variableLabel:"label"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_one_multi_filtered_question_dont_use_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.MultyOptionsQuestion(variable: "multiQuestion1", isFilteredCombobox:true, options: answers, questionText: "to be or not to be"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_two_multi_filtered_questions_dont_use_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.MultyOptionsQuestion(variable: "multiQuestion1", isFilteredCombobox:true, options: answers, questionText: "to be or not to be"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", isFilteredCombobox:true, options: answers, questionText: "To Be?", variableLabel: "label"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_two_multi_filtered_questions_With_and_without_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.MultyOptionsQuestion(variable: "multiQuestion1", isFilteredCombobox:true, categoryId: Id.g1, questionText: "to be or not to be"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", isFilteredCombobox:true, options: answers, questionText: "to be?"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }


        [Test]
        public void when_many_categorical_questions_with_different_option_source_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.SingleOptionQuestion(variable: "singleQuestion1", categoryId: Id.g1, questionText: "to be single?"),
                Create.SingleOptionQuestion(variable: "singleQuestion2", options: answers, questionText: "or not be single?"),
                Create.MultyOptionsQuestion(variable: "filteredQuestion1", categoryId: Id.g1, questionText: "to be multi?"),
                Create.MultyOptionsQuestion(variable: "filteredQuestion2", options: answers, questionText: "or not to be?"),
                Create.MultyOptionsQuestion(variable: "multiQuestion1", isFilteredCombobox: true, categoryId: Id.g1, questionText: "to be multi filtered?"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", isFilteredCombobox: true, options: answers, questionText: "or not to be filtered?"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }


        private IInterviewsDoFilesExporter CreateInterviewsDoFilesExporter(Action<string> returnContentAction)
        {
            var fileSystemAccessor = CreateFileSystemAccessor(returnContentAction);
            var exporter = Create.InterviewsDoFilesExporter(fileSystemAccessor);
            return exporter;
        }

        private QuestionnaireExportStructure CreateQuestionnaireExportStructure(List<Categories> reusableCategories, IQuestionnaireEntity[] children)
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: children);
            questionnaireDocument.Categories = reusableCategories;

            var exportStructureFactory = new QuestionnaireExportStructureFactory(Mock.Of<IQuestionnaireStorage>());
            var exportStructure = exportStructureFactory.CreateQuestionnaireExportStructure(questionnaireDocument);
            return exportStructure;
        }

        private static Answer[] answers = new Answer[]
        {
            Create.Answer("title #1", 1),
            Create.Answer("title #2", 2),
            Create.Answer("title #3", 3),
            Create.Answer("title #4", 4),
            Create.Answer("title #5", 5),
        };

        private static readonly List<Categories> questionnaireCategories = new List<Categories>
        {
            Create.Entity.Categories(Id.g1, name: "reusableCategory1", values: new CategoryItem[]
            {
                Create.Entity.CategoryItem(1, "title #1"),
                Create.Entity.CategoryItem(2, "title #2"),
                Create.Entity.CategoryItem(3, "title #3"),
                Create.Entity.CategoryItem(4, "title #4"),
                Create.Entity.CategoryItem(5, "title #5"),
            }),
        };
    }
}
