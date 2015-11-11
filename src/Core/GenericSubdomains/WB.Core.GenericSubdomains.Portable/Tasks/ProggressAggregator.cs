using System;
using System.Collections.Generic;

namespace WB.Core.GenericSubdomains.Portable.Tasks
{
    public class ProggressAggregator
    {
        private readonly Dictionary<Progress<int>, double> progresses = new Dictionary<Progress<int>, double>();

        public event ProgressChanged ProgressChanged;

        public void Add(Progress<int> progress, double weight)
        {
            progress.ProgressChanged += (sender, progressArg) => OnProgressChanged((int) (this.progresses[(Progress<int>) sender]*progressArg));

            this.progresses[progress] = weight;
        }

        protected virtual void OnProgressChanged(int progress)
        {
            this.ProgressChanged?.Invoke(this, progress);
        }
    }

    public delegate void ProgressChanged(object sender, int progress);
}