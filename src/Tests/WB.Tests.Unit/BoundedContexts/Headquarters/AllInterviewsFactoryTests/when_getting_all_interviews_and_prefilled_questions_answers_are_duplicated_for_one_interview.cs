using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.AllInterviewsFactoryTests
{
    internal class when_getting_interviews_and_prefilled_questions_answers_are_duplicated_for_one_interview
    {
        // KP-7369 this test & fix were added because we did not have STR but has duplicates on dev and in production
        // therefore consequences were fixed but not the origins

        Establish context = () =>
        {
            var interview1 = Guid.Parse("11111111111111111111111111111111");
            var interview2 = Guid.Parse("22222222222222222222222222222222");
            var interview3 = Guid.Parse("33333333333333333333333333333333");

            var questionA = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionB = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var interviewSummaryReader = Stub<IQueryableReadSideRepositoryReader<InterviewSummary>>.Returning(
                new List<InterviewSummary>
                {
                    Create.Entity.InterviewSummary(interviewId: interview1),
                    Create.Entity.InterviewSummary(interviewId: interview2),
                    Create.Entity.InterviewSummary(interviewId: interview3),
                });

            var answersReader = Stub<IQueryableReadSideRepositoryReader<QuestionAnswer>>.Returning(
                new List<QuestionAnswer>
                {
                    Create.Entity.QuestionAnswer(interviewId: interview1, questionId: questionA),
                    Create.Entity.QuestionAnswer(interviewId: interview1, questionId: questionB),

                    Create.Entity.QuestionAnswer(interviewId: interview2, questionId: questionA),
                    Create.Entity.QuestionAnswer(interviewId: interview2, questionId: questionB),
                    Create.Entity.QuestionAnswer(interviewId: interview2, questionId: questionA),
                    Create.Entity.QuestionAnswer(interviewId: interview2, questionId: questionB),
                    Create.Entity.QuestionAnswer(interviewId: interview2, questionId: questionA),
                    Create.Entity.QuestionAnswer(interviewId: interview2, questionId: questionB),

                    Create.Entity.QuestionAnswer(interviewId: interview3, questionId: questionA),
                    Create.Entity.QuestionAnswer(interviewId: interview3, questionId: questionB),
                });

            factory = Create.Service.AllInterviewsFactory(
                interviewSummaryReader: interviewSummaryReader,
                answersReader: answersReader);
        };

        Because of = () =>
            interviewsView = factory.Load(Create.Entity.AllInterviewsInputModel());

        It should_return_interviews_count_same_as_got_from_repository = () =>
            interviewsView.Items.Count().ShouldEqual(3);

        It should_return_actual_prefilled_question_answers_count_for_each_interview_without_duplicates = () =>
            interviewsView.Items.ShouldEachConformTo(item => item.FeaturedQuestions.Count() == 2);

        private static AllInterviewsFactory factory;
        private static AllInterviewsView interviewsView;
    }
}
