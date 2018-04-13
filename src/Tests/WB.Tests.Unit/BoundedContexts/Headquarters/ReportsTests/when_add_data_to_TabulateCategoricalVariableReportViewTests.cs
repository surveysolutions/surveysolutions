using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.ReportsTests
{
    [TestFixture]
    public class when_add_data_to_TabulateCategoricalVariableReportViewTests
    {
        private List<Answer> answers;
        private List<GetCategoricalReportItem> rows;
        private readonly Guid firstTeamLead = Id.g1;
        private readonly Guid secondTeamLead = Id.g2;

        readonly int answerNewYork = 1;
        readonly int answerWashington = 2;
        readonly int answerRural = 3;

        private const string interviewerA = "interviewerA";
        private const string interviewerB = "interviewerB";
        private const string interviewerC = "interviewerC";

        [SetUp]
        public void Context()
        {
            this.answers = new List<Answer>
            {
                Create.Entity.Answer("New York", answerNewYork),
                Create.Entity.Answer("Washington", answerWashington),
                Create.Entity.Answer("Rural", answerRural)
            };
            
            GetCategoricalReportItem CreateRowItem(string responsible, int answer, int count, Guid? teamLead = null)
            {
                return new GetCategoricalReportItem
                {
                    TeamLeadName = (teamLead ?? Id.gA).ToString(),
                    ResponsibleName = responsible,
                    Answer = answer,
                    Count = count
                };
            }

            this.rows = new List<GetCategoricalReportItem>
            {
                CreateRowItem(interviewerA, answerNewYork,    10, firstTeamLead),
                CreateRowItem(interviewerA, answerWashington, 20, firstTeamLead),
                CreateRowItem(interviewerA, answerRural,      30, firstTeamLead),

                CreateRowItem(interviewerB, answerWashington, 25, secondTeamLead),
                CreateRowItem(interviewerB, answerRural,      35, secondTeamLead),
                CreateRowItem(interviewerB, answerNewYork,    15, secondTeamLead), // changed order of answers

                CreateRowItem(interviewerC, answerNewYork,    100, firstTeamLead),
                CreateRowItem(interviewerC, answerWashington, 200, firstTeamLead), // missing answer on rural
            };
        }

        [Test]
        public void should_group_result_by_responsible()
        {
            var subject = new CategoricalReportViewBuilder(answers, rows);

            Assert.That(subject.Data.Count, Is.EqualTo(3), "should be equal to number of different responsibles");
        }

        [Test]
        public void should_move_counts_into_proper_columns()
        {
            var subject = new CategoricalReportViewBuilder(answers, rows);

            Assert.That(ValuesFor(interviewerA), Is.EqualTo(new[] { 10, 20, 30 }));
            Assert.That(ValuesFor(interviewerB), Is.EqualTo(new[] { 15, 25, 35 }));
            Assert.That(ValuesFor(interviewerC), Is.EqualTo(new[] { 100, 200, 0 }), "Should fill missing values with zero");

            int[] ValuesFor(string responsibleName) => GetValuesFor(subject, d => d.ResponsibleName == responsibleName);
        }

        private int[] GetValuesFor(CategoricalReportViewBuilder perTeamReport, Func<CategoricalReportViewItem, bool> predicate)
        {
            return perTeamReport.Data.Where(predicate).Select(d => d.Values).SingleOrDefault();
        }
    }
}
