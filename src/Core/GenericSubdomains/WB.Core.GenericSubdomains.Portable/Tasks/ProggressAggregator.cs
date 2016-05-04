using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable.Tasks
{
    public class ProggressAggregator
    {
        private readonly Dictionary<Progress<int>, WeightedProgress> progresses = new Dictionary<Progress<int>, WeightedProgress>();

        public event EventHandler<int> ProgressChanged;

        public void Add(Progress<int> progress, double weight)
        {
            this.progresses[progress] = new WeightedProgress {ProgressWeight = weight};

            progress.ProgressChanged += (sender, progressArg) =>
            {
                WeightedProgress weightedProgress = this.progresses[(Progress<int>) sender];
                weightedProgress.LastReportedProgress = progressArg;

                var progressToReport = (int)this.progresses.Values.Sum(x => x.LastReportedProgress * x.ProgressWeight);
                this.OnProgressChanged(progressToReport);
            };
        }

        protected virtual void OnProgressChanged(int progress)
        {
            this.ProgressChanged?.Invoke(this, progress);
        }
    }

    internal class WeightedProgress
    {
        public int LastReportedProgress { get; set; }
        public double ProgressWeight { get; set; }
    }
}