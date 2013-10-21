using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;


namespace WB.Core.BoundedContexts.Supervisor.Tests.Views
{
    [Subject(typeof(InterviewDataExportFactory))]
    internal class when_requesing_data_by_top_level_of_interview_with_2_approved_empty_interviews_and_template_contains_2_questions : InterviewDataExportFactoryTestContext
    {
        private Establish context = () =>
        {
            interviewDataExportFactory = CreateInterviewDataExportFactoryForQuestionnarieCreatedByMethod(
                () =>
                    CreateQuestionnaireDocument(new Dictionary<string, Guid>
                    {
                        { "q1", Guid.NewGuid() },
                        { "q2", Guid.NewGuid() }
                    }),
                CreateInterviewData,
                2);
        };

        Because of = () =>
           result = interviewDataExportFactory.Load(new InterviewDataExportInputModel(Guid.NewGuid(), 1, null));

        private It should_records_count_equals_2 = () =>
            result.Records.Length.ShouldEqual(2);

        private It should__first_record_have_0_answers= () =>
           result.Records[0].Questions.Count.ShouldEqual(0);

        private It should_first_record_id_equals_0 = () =>
            result.Records[0].RecordId.ShouldEqual(0);

        private It should_second_record_id_equals_1 = () =>
            result.Records[1].RecordId.ShouldEqual(1);

        private It should_header_column_count_be_equal_2 = () =>
           result.Header.Count().ShouldEqual(2);

        private static InterviewDataExportFactory interviewDataExportFactory;
        private static InterviewDataExportView result;
    }
}
