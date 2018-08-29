using System;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestFixture]
    public class SpecialValuesViewModelTests
    {
        string interviewId = Id.g1.FormatGuid();
        Identity entityIdentity = Identity.Create(Id.g2, RosterVector.Empty);

        [Test]
        public void Init_special_values_for_unanswered_integer_question()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(Id.g2, specialValues: Create.Entity.Options(1, 2)));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            var optionsModel = Create.ViewModel.FilteredOptionsViewModel(entityIdentity, questionnaire, interview);
            var model = Create.ViewModel.SpecialValues(optionsModel, interviewRepository: Setup.StatefulInterviewRepository(interview));

            // Act
            model.Init(interviewId, entityIdentity, Mock.Of<IQuestionStateViewModel>());


            Assert.That(model.SpecialValues.Count, Is.EqualTo(2));
            Assert.That(model.SpecialValues[0].Selected, Is.False);
            Assert.That(model.SpecialValues[0].Value, Is.EqualTo(1));
            Assert.That(model.SpecialValues[0].Title, Is.EqualTo("Option 1"));

            Assert.That(model.SpecialValues[1].Selected, Is.False);
            Assert.That(model.SpecialValues[1].Value, Is.EqualTo(2));
            Assert.That(model.SpecialValues[1].Title, Is.EqualTo("Option 2"));

            Assert.That(model.IsSpecialValue, Is.Null);
        }

        [Test]
        public void Init_special_values_for_unanswered_real_question()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(Id.g2, specialValues: Create.Entity.Options(1, 2)));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var optionsModel = Create.ViewModel.FilteredOptionsViewModel(entityIdentity, questionnaire, interview);
            var model = Create.ViewModel.SpecialValues(optionsModel, interviewRepository: Setup.StatefulInterviewRepository(interview));

            // Act
            model.Init(interviewId, entityIdentity, Mock.Of<IQuestionStateViewModel>());

            //Assert
            Assert.That(model.SpecialValues.Count, Is.EqualTo(2));
            Assert.That(model.SpecialValues[0].Selected, Is.False);
            Assert.That(model.SpecialValues[0].Value, Is.EqualTo(1));
            Assert.That(model.SpecialValues[0].Title, Is.EqualTo("Option 1"));

            Assert.That(model.SpecialValues[1].Selected, Is.False);
            Assert.That(model.SpecialValues[1].Value, Is.EqualTo(2));
            Assert.That(model.SpecialValues[1].Title, Is.EqualTo("Option 2"));

            Assert.That(model.IsSpecialValue, Is.Null);
        }

        [Test]
        public void Init_special_values_for_answered_with_special_value_real_question()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(Id.g2, specialValues: Create.Entity.Options(1, 2)));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerNumericIntegerQuestion(Id.gF, entityIdentity.Id, entityIdentity.RosterVector, DateTime.Now, 1);

            var optionsModel = Create.ViewModel.FilteredOptionsViewModel(entityIdentity, questionnaire, interview);
            var model = Create.ViewModel.SpecialValues(optionsModel, interviewRepository: Setup.StatefulInterviewRepository(interview));

            // Act
            model.Init(interviewId, entityIdentity, Mock.Of<IQuestionStateViewModel>());

            //Assert
            Assert.That(model.SpecialValues.Count, Is.EqualTo(2));
            Assert.That(model.SpecialValues[0].Selected, Is.True);
            Assert.That(model.SpecialValues[0].Value, Is.EqualTo(1));
            Assert.That(model.SpecialValues[0].Title, Is.EqualTo("Option 1"));

            Assert.That(model.SpecialValues[1].Selected, Is.False);
            Assert.That(model.SpecialValues[1].Value, Is.EqualTo(2));
            Assert.That(model.SpecialValues[1].Title, Is.EqualTo("Option 2"));

            Assert.That(model.IsSpecialValue, Is.True);
        }

        [Test]
        public void Init_special_values_for_answered_with_special_value_integer_question()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(Id.g2, specialValues: Create.Entity.Options(1, 2)));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerNumericIntegerQuestion(Id.gF, entityIdentity.Id, entityIdentity.RosterVector, DateTime.Now, 2);

            var optionsModel = Create.ViewModel.FilteredOptionsViewModel(entityIdentity, questionnaire, interview);
            var model = Create.ViewModel.SpecialValues(optionsModel, interviewRepository: Setup.StatefulInterviewRepository(interview));

            // Act
            model.Init(interviewId, entityIdentity, Mock.Of<IQuestionStateViewModel>());

            //Assert
            Assert.That(model.SpecialValues.Count, Is.EqualTo(2));
            Assert.That(model.SpecialValues[0].Selected, Is.False);
            Assert.That(model.SpecialValues[0].Value, Is.EqualTo(1));
            Assert.That(model.SpecialValues[0].Title, Is.EqualTo("Option 1"));

            Assert.That(model.SpecialValues[1].Selected, Is.True);
            Assert.That(model.SpecialValues[1].Value, Is.EqualTo(2));
            Assert.That(model.SpecialValues[1].Title, Is.EqualTo("Option 2"));

            Assert.That(model.IsSpecialValue, Is.True);
        }

        [Test]
        public void Init_non_special_value_selected_Should_not_show_special_options()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(Id.g2, specialValues: Create.Entity.Options(1, 2)));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerNumericIntegerQuestion(Id.gF, entityIdentity.Id, entityIdentity.RosterVector, DateTime.Now, 10);

            var optionsModel = Create.ViewModel.FilteredOptionsViewModel(entityIdentity, questionnaire, interview);
            var model = Create.ViewModel.SpecialValues(optionsModel, interviewRepository: Setup.StatefulInterviewRepository(interview));


            // Act
            model.Init(interviewId, entityIdentity, Mock.Of<IQuestionStateViewModel>());

            //Assert
            Assert.That(model.SpecialValues, Is.Empty);
        }

        [Test]
        public void Set_not_special_values_answer_for_unanswered_integer_question()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(Id.g2, specialValues: Create.Entity.Options(1, 2)));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var optionsModel = Create.ViewModel.FilteredOptionsViewModel(entityIdentity, questionnaire, interview);
            var model = Create.ViewModel.SpecialValues(optionsModel, interviewRepository: Setup.StatefulInterviewRepository(interview));

            model.Init(interviewId, entityIdentity, Mock.Of<IQuestionStateViewModel>());

            // Act
            interview.AnswerNumericIntegerQuestion(Id.gF, entityIdentity.Id, entityIdentity.RosterVector, DateTime.Now, 300);
            model.SetAnswer(300);

            //Assert
            Assert.That(model.SpecialValues.Count, Is.EqualTo(0));
            Assert.That(model.IsSpecialValue, Is.False);
        }

        [Test]
        public void Set_special_values_for_answered_with_special_value_integer_question()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(Id.g2, specialValues: Create.Entity.Options(1, 2)));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var optionsModel = Create.ViewModel.FilteredOptionsViewModel(entityIdentity, questionnaire, interview);
            var model = Create.ViewModel.SpecialValues(optionsModel, interviewRepository: Setup.StatefulInterviewRepository(interview));

            model.Init(interviewId, entityIdentity, Mock.Of<IQuestionStateViewModel>());
            interview.AnswerNumericIntegerQuestion(Id.gF, entityIdentity.Id, entityIdentity.RosterVector, DateTime.Now, 300);
            model.SetAnswer(300);

            // Act
            interview.AnswerNumericIntegerQuestion(Id.gF, entityIdentity.Id, entityIdentity.RosterVector, DateTime.Now, 2);
            model.SetAnswer(2);

            //Assert
            Assert.That(model.SpecialValues.Count, Is.EqualTo(2));
            Assert.That(model.IsSpecialValue, Is.True);
        }

        [Test]
        public void Remove_answered_with_special_value_integer_question()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(Id.g2, specialValues: Create.Entity.Options(1, 2)));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerNumericIntegerQuestion(Id.gF, entityIdentity.Id, entityIdentity.RosterVector, DateTime.Now, 2);

            var optionsModel = Create.ViewModel.FilteredOptionsViewModel(entityIdentity, questionnaire, interview);
            var model = Create.ViewModel.SpecialValues(optionsModel, interviewRepository: Setup.StatefulInterviewRepository(interview));

            model.Init(interviewId, entityIdentity, Mock.Of<IQuestionStateViewModel>());
            
            // Act
            interview.RemoveAnswer(entityIdentity.Id, entityIdentity.RosterVector, Id.gF, DateTime.Now);
            model.ClearSelectionAndShowValues().WaitAndUnwrapException();

            //Assert
            Assert.That(model.SpecialValues.Count, Is.EqualTo(2));
            Assert.That(model.IsSpecialValue, Is.Null);
        }

        [Test]
        public void IsSpecialValueSelected_shoud_react_only_integer_values()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericRealQuestion(Id.g2, specialValues: Create.Entity.Options(1, 2)));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var optionsModel = Create.ViewModel.FilteredOptionsViewModel(entityIdentity, questionnaire, interview);
            var model = Create.ViewModel.SpecialValues(optionsModel, interviewRepository: Setup.StatefulInterviewRepository(interview));

            model.Init(interviewId, entityIdentity, Mock.Of<IQuestionStateViewModel>());

            // Act
            var isSpecialValueSelectedReal = model.IsSpecialValueSelected(2.2m);
            var isSpecialValueSelectedInt = model.IsSpecialValueSelected(2.00m);

            //Assert
            Assert.That(isSpecialValueSelectedReal, Is.False);
            Assert.That(isSpecialValueSelectedInt, Is.True);
        }
    }
}
