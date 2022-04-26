using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Services.Export
{
    public class ProgressAggregator
    {
        private readonly Dictionary<ExportProgress, WeightedProgress> progresses = new Dictionary<ExportProgress, WeightedProgress>();

        public event EventHandler<ProgressState>? ProgressChanged;
        private readonly ProgressState state = new ProgressState();

        public void Add(ExportProgress progress, double weight)
        {
            this.progresses[progress] = new WeightedProgress {ProgressWeight = weight};

            progress.ProgressChanged += (sender, progressArg) =>
            {
                lock (state)
                {
                    if (sender != null)
                    {
                        WeightedProgress weightedProgress = this.progresses[(ExportProgress) sender];
                        weightedProgress.LastReportedProgress = progressArg;
                    }

                    state.Percent = (int) this.progresses.Values.Sum(x => (x.LastReportedProgress?.Percent ?? 0) * x.ProgressWeight);

                    //if (this.progresses.Values.Any(v => v.LastReportedProgress?.Eta != null))
                    //{
                    //    state.Eta = this.progresses.Values
                    //        .Where(e => e.LastReportedProgress?.Eta != null)
                    //        .Max(v => v.LastReportedProgress.Eta.Value);
                    //}

                    this.OnProgressChanged(state);
                }
            };
        }

        protected virtual void OnProgressChanged(ProgressState progress)
        {
            this.ProgressChanged?.Invoke(this, progress);
        }
    }
}
