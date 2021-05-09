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
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Tests.CsvExport.Implementation.DoFiles
{
    [UseApprovalSubdirectory("../../CategoricalQuestionExportDoFiles-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [TestOf(typeof(InterviewsDoFilesExporter))]
    internal class CategoricalQuestionExportDoFiles : QuestionsExportDoFilesContext
    {
        [Test]
        public void when_one_single_question_use_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.SingleOptionQuestion(variable: "singleQuestion1", questionText: "questionText #1", categoryId: Id.g1),
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
                Create.SingleOptionQuestion(variable: "singleQuestion1", categoryId: Id.g1, questionText: "questionText #1"),
                Create.SingleOptionQuestion(variable: "singleQuestion2", categoryId: Id.g1, questionText: "questionText #2", variableLabel: "label #2"),
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
                Create.MultyOptionsQuestion(variable: "multiQuestion1", categoryId: Id.g1, questionText: "questionText #1"),
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
                Create.MultyOptionsQuestion(variable: "multiQuestion1", categoryId: Id.g1, questionText: "questionText #1"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", categoryId: Id.g1, questionText: "questionText #2", variableLabel: "label #2"),
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
                Create.SingleOptionQuestion(variable: "singleQuestion1", options: answers, questionText: "questionText #1"),
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
                Create.SingleOptionQuestion(variable: "singleQuestion1", options: answers, questionText: "questionText #1"),
                Create.SingleOptionQuestion(variable: "singleQuestion2", options: answers, questionText: "questionText #2", variableLabel: "label #2"),
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
                Create.MultyOptionsQuestion(variable: "multiQuestion1", options: answers, questionText: "questionText #1"),
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
                Create.MultyOptionsQuestion(variable: "multiQuestion1", options: answers, questionText: "questionText #1"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", options: answers, questionText: "questionText #2", variableLabel: "label #2"),
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
                Create.MultyOptionsQuestion(variable: "multiQuestion1", categoryId: Id.g1, questionText: "questionText #1"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", options: answers, questionText: "questionText #2", variableLabel:"label #2"),
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
                Create.MultyOptionsQuestion(variable: "multiQuestion1", isFilteredCombobox:true, options: answers, questionText: "questionText #1"),
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
                Create.MultyOptionsQuestion(variable: "multiQuestion1", isFilteredCombobox:true, options: answers, questionText: "questionText #1"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", isFilteredCombobox:true, options: answers, questionText: "questionText #2", variableLabel: "label #2"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_two_multi_filtered_questions_with_different_source_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.MultyOptionsQuestion(variable: "multiQuestion1", isFilteredCombobox:true, categoryId: Id.g1, questionText: "questionText #1"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", isFilteredCombobox:true, options: answers, questionText: "questionText #2"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }


        [Test]
        public void when_two_multi_filtered_questions_with_max_answers_count_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.MultyOptionsQuestion(variable: "multiQuestion1", isFilteredCombobox:true, categoryId: Id.g1, questionText: "questionText #1", maxAnswersCount: 3),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", isFilteredCombobox:true, options: answers, questionText: "questionText #2", maxAnswersCount: 2),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_two_multi_filtered_questions_With_reusable_categories_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(questionnaireCategories, new IQuestionnaireEntity[]
            {
                Create.MultyOptionsQuestion(variable: "multiQuestion1", isFilteredCombobox:true, categoryId: Id.g1, questionText: "questionText #1"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", isFilteredCombobox:true, categoryId: Id.g1, questionText: "questionText #2"),
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
                Create.SingleOptionQuestion(variable: "singleQuestion1", categoryId: Id.g1, questionText: "questionText #1"),
                Create.SingleOptionQuestion(variable: "singleQuestion2", options: answers, questionText: "questionText #2"),
                Create.MultyOptionsQuestion(variable: "filteredQuestion1", categoryId: Id.g1, questionText: "questionText #3"),
                Create.MultyOptionsQuestion(variable: "filteredQuestion2", options: answers, questionText: "questionText #4"),
                Create.MultyOptionsQuestion(variable: "multiQuestion1", isFilteredCombobox: true, categoryId: Id.g1, questionText: "questionText #5"),
                Create.MultyOptionsQuestion(variable: "multiQuestion2", isFilteredCombobox: true, options: answers, questionText: "questionText #6"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        private static Answer[] answers = new Answer[]
        {
            Create.Answer("title #111", 101),
            Create.Answer("title #112", 102),
            Create.Answer("title #113", 103),
            Create.Answer("title #114", 104),
            Create.Answer("title #115", 105),
        };

        private static readonly List<Categories> questionnaireCategories = new List<Categories>
        {
            Create.Entity.Categories(Id.g1, name: "reusableCategory1", values: new CategoryItem[]
            {
                Create.Entity.CategoryItem(201, "title #211"),
                Create.Entity.CategoryItem(202, "title #212"),
                Create.Entity.CategoryItem(203, "title #213"),
                Create.Entity.CategoryItem(204, "title #214"),
                Create.Entity.CategoryItem(205, "title #215"),
            }),
        };
    }
}
