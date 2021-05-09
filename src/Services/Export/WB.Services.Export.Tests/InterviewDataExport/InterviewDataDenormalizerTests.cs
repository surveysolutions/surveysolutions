using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using NpgsqlTypes;
using NUnit.Framework;
using WB.Services.Export.Events;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Infrastructure.EventSourcing;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Tests.InterviewDataExport
{
    [UseApprovalSubdirectory("InterviewDataDenormalizerTests-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [TestOf(typeof(InterviewDataDenormalizer))]
    public class InterviewDataDenormalizerTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public async Task when_get_interview_created_event_should_raise_interview_add_command()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.CreatedInterview(interviewId).ToPublishedEvent<InterviewCreated>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo(interviewId));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_get_interview_created_on_client_event_should_raise_interview_add_command()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.InterviewOnClientCreated(interviewId).ToPublishedEvent<InterviewOnClientCreated>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo(interviewId));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_variable_changed_with_special_double_value_should_be_able_to_write_it_to_db()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.VariablesChanged(interviewId,
                    Create.Identity(doubleVariableId),
                    Double.NegativeInfinity.ToString(CultureInfo.InvariantCulture))
                .ToPublishedEvent<VariablesChanged>());

            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);


            NpgsqlParameter commandParameter = (NpgsqlParameter)command.Parameters[0];
            Assert.That(commandParameter.NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Double));
            Assert.That(commandParameter.Value, Is.EqualTo(Double.NegativeInfinity));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_variable_changed_with_long_value_as_double_should_be_able_to_write_it_to_db()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.VariablesChanged(interviewId,
                    Create.Identity(longVariableId), (double)42)
                .ToPublishedEvent<VariablesChanged>());

            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);

            NpgsqlParameter commandParameter = (NpgsqlParameter)command.Parameters[0];
            Assert.That(commandParameter.NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Bigint));
            Assert.That(commandParameter.Value, Is.EqualTo(42));
            Assert.That(commandParameter.Value.GetType(), Is.EqualTo(typeof(long)));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_variable_changed_with_double_value_as_long_should_be_able_to_write_it_to_db()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.VariablesChanged(interviewId,
                    Create.Identity(doubleVariableId), (long)42)
                .ToPublishedEvent<VariablesChanged>());

            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);

            NpgsqlParameter commandParameter = (NpgsqlParameter)command.Parameters[0];
            Assert.That(commandParameter.NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Double));
            Assert.That(commandParameter.Value, Is.EqualTo((double)42));
            Assert.That(commandParameter.Value.GetType(), Is.EqualTo(typeof(double)));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_date_question_is_answered_should_use_answer_date_as_answer()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            var originDate = new DateTimeOffset(2010, 5, 31, 10, 1, 1, TimeSpan.FromHours(-2));
            await denormalizer.Handle(Create.Event.DateTimeQuestionAnswered(interviewId,
                    Create.Identity(dateQuestionId),
                    originDate: originDate,
                    answer: originDate.UtcDateTime // pretend that tablet was in the UTC timezone
                )
                .ToPublishedEvent<DateTimeQuestionAnswered>());

            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);


            NpgsqlParameter commandParameter = (NpgsqlParameter)command.Parameters[0];
            Assert.That(commandParameter.NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Timestamp));
            Assert.That(commandParameter.Value, Is.EqualTo(originDate.UtcDateTime));
        }

        [Test]
        public async Task when_get_add_roster_on_client_event_should_raise_interview_add_command()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.RosterInstancesAdded(interviewId, rosterId, new RosterVector(1, 2), 1).ToPublishedEvent<RosterInstancesAdded>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo(interviewId));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(new RosterVector(1, 2, 1)));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_get_remove_roster_on_client_event_should_raise_interview_add_command()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.RosterInstancesRemoved(interviewId, rosterId, new RosterVector(1, 2), 1).ToPublishedEvent<RosterInstancesRemoved>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo(interviewId));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(new RosterVector(1, 2, 1)));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_get_answer_text_question_event_should_raise_interview_update_command()
        {
            string answer = "answer";
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.TextQuestionAnswered(textQuestionId, answer, interviewId).ToPublishedEvent<TextQuestionAnswered>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo(answer));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(interviewId));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_get_answer_int_question_event_should_raise_interview_update_command()
        {
            int answer = 777;
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.NumericIntegerQuestionAnswered(intQuestionId, answer, interviewId).ToPublishedEvent<NumericIntegerQuestionAnswered>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo(answer));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(interviewId));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_get_answer_real_question_event_should_raise_interview_update_command()
        {
            decimal answer = (decimal)777.77;

            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.NumericRealQuestionAnswered(realQuestionId, answer, interviewId, rosterVector).ToPublishedEvent<NumericRealQuestionAnswered>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo(answer));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(interviewId));
            Assert.That(command.Parameters[2].Value, Is.EqualTo(rosterVector));

            Approvals.Verify(command.CommandText);
        }


        [Test]
        public async Task when_get_answer_text_list_question_event_should_raise_interview_update_command()
        {
            Tuple<decimal, string>[] answer = new[] { new Tuple<decimal, string>(55, "55"), };
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.TextListQuestionAnswered(textListQuestionId, answer, interviewId).ToPublishedEvent<TextListQuestionAnswered>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo("[{\"Value\":55.0,\"Answer\":\"55\"}]"));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(interviewId));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_update_enable_question_event_should_raise_interview_update_command()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.QuestionsEnabled(interviewId, new[] { Create.Identity(intQuestionId, rosterVector) }).ToPublishedEvent<QuestionsEnabled>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo(true));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(interviewId));
            Assert.That(command.Parameters[2].Value, Is.EqualTo(rosterVector));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_update_disable_question_event_should_raise_interview_update_command()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.QuestionsDisabled(interviewId, new[] { Create.Identity(intQuestionId, rosterVector) }).ToPublishedEvent<QuestionsDisabled>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo(false));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(interviewId));
            Assert.That(command.Parameters[2].Value, Is.EqualTo(rosterVector));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_update_invalid_question_event_should_raise_interview_update_command()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            var failedValidationConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();
            failedValidationConditions.Add(Create.Identity(intQuestionId, rosterVector), new List<FailedValidationCondition>()
            {
                new FailedValidationCondition(2),
                new FailedValidationCondition(7),
            });

            await denormalizer.Handle(Create.Event.AnswersDeclaredInvalid(interviewId, failedValidationConditions).ToPublishedEvent<AnswersDeclaredInvalid>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo(new[] { 2, 7 }));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(interviewId));
            Assert.That(command.Parameters[2].Value, Is.EqualTo(rosterVector));

            Approvals.Verify(command.CommandText);
        }

        [Test]
        public async Task when_update_valid_question_event_should_raise_interview_update_command()
        {
            DbCommand command = null;
            var denormalizer = CreateInterviewDataDenormalizer(c => command = c);

            await denormalizer.Handle(Create.Event.AnswersDeclaredValid(interviewId, new[] { Create.Identity(intQuestionId, rosterVector) }).ToPublishedEvent<AnswersDeclaredValid>());
            await denormalizer.SaveStateAsync(CancellationToken.None);

            Assert.That(command, Is.Not.Null);
            Assert.That(command.CommandText, Is.Not.Null);
            Assert.That(command.Parameters[0].Value, Is.EqualTo(DBNull.Value));
            Assert.That(command.Parameters[1].Value, Is.EqualTo(interviewId));
            Assert.That(command.Parameters[2].Value, Is.EqualTo(rosterVector));

            Approvals.Verify(command.CommandText);
        }

        private InterviewDataDenormalizer CreateInterviewDataDenormalizer(Action<DbCommand> funcToSaveCommand)
        {
            var questionnaireDocument = SetupQuestionnaireDocumentWithAllEntities();

            ITenantContext tenantContext = Mock.Of<ITenantContext>(t => t.Tenant == new TenantInfo("http://test","","tenant_name", TenantInfo.DefaultWorkspace));
            IQuestionnaireStorage questionnaireStorage = 
                Mock.Of<IQuestionnaireStorage>(s => s.GetQuestionnaireAsync(It.IsAny<QuestionnaireIdentity>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()) == Task.FromResult(questionnaireDocument));
            object value = questionnaireDocument.QuestionnaireId;
            IMemoryCache memoryCache = Mock.Of<IMemoryCache>(mc => mc.TryGetValue(It.IsAny<object>(), out value) == true);
            ILogger<InterviewDataDenormalizer> logger = Mock.Of<ILogger<InterviewDataDenormalizer>>();
            var interviewReference = Create.Entity.InterviewReference();
            IInterviewReferencesStorage interviewReferencesStorage = Mock.Of<IInterviewReferencesStorage>(r 
                => r.FindAsync(It.IsAny<Guid>()) == new ValueTask<InterviewReference>(Task.FromResult(interviewReference)));
            IInterviewDataExportBulkCommandBuilder commandBuilder = new InterviewDataExportBulkCommandBuilder(new InterviewDataExportBulkCommandBuilderSettings());
            Mock<ICommandExecutor> commandExecutor = new Mock<ICommandExecutor>();
            commandExecutor.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<DbCommand>(), It.IsAny<CancellationToken>()))
                .Returns<DbCommand, CancellationToken>((c, ct) => Task.CompletedTask)
                .Callback((DbCommand c, CancellationToken ct) => funcToSaveCommand.Invoke(c));

            return new InterviewDataDenormalizer(tenantContext, questionnaireStorage, memoryCache, commandBuilder, logger, interviewReferencesStorage, commandExecutor.Object);
        }

        private QuestionnaireDocument SetupQuestionnaireDocumentWithAllEntities()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId, questionnaireVersion, questionnaireVariable,
                children:
                Create.Group(sectionId, "section", children: new IQuestionnaireEntity[]
                {
                    Create.TextQuestion(textQuestionId, variable: "text_q"),
                    Create.NumericIntegerQuestion(intQuestionId, "int_q"),
                    Create.Roster(rosterId, "roster1", children:
                        Create.NumericRealQuestion(realQuestionId, "real_q")
                    ),
                    Create.Group(groupId, children: new IQuestionnaireEntity[]
                    {
                        Create.TextListQuestion(textListQuestionId, "text_list_q")
                    }),
                    Create.Variable(doubleVariableId, VariableType.Double, "double_variabe"),
                    Create.Variable(longVariableId, VariableType.LongInteger, "long_variabe"),
                    Create.DateTimeQuestion(timestampQuestionId, variable: "timestamp", isTimestamp: true),
                    Create.DateTimeQuestion(dateQuestionId, variable: "date", isTimestamp: false),
                })
            );
            return questionnaireDocument;
        }

        private readonly RosterVector rosterVector = new RosterVector(1, 2);
        private readonly Guid interviewId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        private readonly Guid questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private readonly int questionnaireVersion = 17;
        private readonly string questionnaireVariable = "questionnaireVariable";
        private readonly Guid sectionId = Guid.Parse("11111111-1111-1111-1111-111111111112");
        private readonly Guid groupId = Guid.Parse("11111111-1111-1111-1111-111111111113");
        private readonly Guid rosterId = Guid.Parse("11111111-1111-1111-1111-111111111114");
        private readonly Guid textQuestionId = Guid.Parse("21111111-1111-1111-1111-111111111111");
        private readonly Guid intQuestionId = Guid.Parse("31111111-1111-1111-1111-111111111111");
        private readonly Guid realQuestionId = Guid.Parse("41111111-1111-1111-1111-111111111111");
        private readonly Guid textListQuestionId = Guid.Parse("51111111-1111-1111-1111-111111111111");
        private readonly Guid doubleVariableId = Guid.Parse("61111111-1111-1111-1111-111111111111");
        private readonly Guid longVariableId = Guid.Parse("62111111-1111-2111-1111-111111111111");
        private readonly Guid timestampQuestionId = Guid.Parse("71111111-1111-1111-1111-111111111111");
        private readonly Guid dateQuestionId = Guid.Parse("81111111-1111-1111-1111-111111111111");
    }
}
