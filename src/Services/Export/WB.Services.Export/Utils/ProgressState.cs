using System;
using System.Diagnostics;

namespace WB.Services.Export
{
    public class ExportProgress : Progress<ProgressState>
    {
        public void Report(int percent, TimeSpan? eta = null)
        {
            this.OnReport(new ProgressState
            {
                Percent = percent,
                Eta = eta
            });
        }

        public void Report(ProgressState progressState)
        {
            this.OnReport(progressState);
        }
    }

    public class ProgressState
    {
        public int Percent { get; set; }
        public TimeSpan? Eta { get; set; }

        public void Update(TimeSpan totalElapsedTime, long totalItems, long processedItems)
        {
            var speed = totalItems / totalElapsedTime.TotalSeconds;
            Percent = processedItems.PercentOf(totalItems);
            Eta = TimeSpan.FromSeconds((totalItems - processedItems) / speed);
        }
    }
}
