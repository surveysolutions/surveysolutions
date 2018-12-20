using System;

namespace WB.Core.SharedKernels.DataCollection.Portable
{
    public class Section
    {
        public Section(Func<bool> isDisabled, Func<int> countEnabledQuestions, Func<int> enabledAnsweredQuestionsCount)
        {
            __isDisabled = isDisabled;
            this.__countEnabledQuestions = countEnabledQuestions;
            this.__enabledAnsweredQuestionsCount = enabledAnsweredQuestionsCount;
        }

        private readonly Func<bool> __isDisabled;
        public bool IsDisabled() => this.__isDisabled.Invoke();

        private readonly Func<int> __countEnabledQuestions;
        public int GetEnabledQuestionsCount() => this.__countEnabledQuestions.Invoke();

        private readonly Func<int> __enabledAnsweredQuestionsCount;
        public int GetEnabledAnsweredQuestionsCount() => this.__enabledAnsweredQuestionsCount.Invoke();
    }
}
