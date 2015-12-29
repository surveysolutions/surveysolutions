using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable.Tasks
{
    public class ProggressAggregator
    {
        private readonly Dictionary<Progress<int>, WeightedProgress> progresses = new Dictionary<Progress<int>, WeightedProgress>();

        public event ProgressChanged ProgressChanged;

        public void Add(Progress<int> progress, double weight)
        {
            progress.ProgressChanged += (sender, progressArg) =>
            {
                WeightedProgress weightedProgress = this.progresses[(Progress<int>) sender];
                weightedProgress.LastReportedProgress = progressArg;

                var progressToReport = (int)progresses.Values.Sum(x => x.LastReportedProgress * x.ProgressWeight);
                this.OnProgressChanged(progressToReport);
            };

            this.progresses[progress] = new WeightedProgress {ProgressWeight = weight};
        }

        protected virtual void OnProgressChanged(int progress)
        {
            this.ProgressChanged?.Invoke(this, progress);
        }
    }

    public delegate void ProgressChanged(object sender, int progress);


    internal class WeightedProgress
    {
        public int LastReportedProgress;
        public double ProgressWeight;
    }
}