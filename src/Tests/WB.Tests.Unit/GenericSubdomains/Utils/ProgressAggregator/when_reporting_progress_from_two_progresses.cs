using System;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable.Tasks;

namespace WB.Tests.Unit.GenericSubdomains.Utils.ProgressAggregator
{
    public class when_reporting_progress_from_two_progresses
    {
        Establish context = () =>
        {
            proggressAggregator = new ProggressAggregator();

            progress1 = new Progress<int>();
            proggressAggregator.Add((Progress<int>) progress1, 0.5);

            progress2 = new Progress<int>();
            proggressAggregator.Add((Progress<int>)progress2, 0.5);

            proggressAggregator.ProgressChanged += (sender, progress) =>
              reportedProgress = progress;
        };

        Because of = () =>
        {
            progress1.Report(100);
            progress2.Report(50);
        };

        It should_report_something = () => { reportedProgress.ShouldEqual(75); };

        private static ProggressAggregator proggressAggregator;
        private static IProgress<int> progress1;
        private static IProgress<int> progress2;
        private static int reportedProgress;
    }
}