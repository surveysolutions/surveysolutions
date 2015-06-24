using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.V3.CustomFunctions
{
    public abstract class AbstractConditionalLevelInstanceV3<T> : AbstractConditionalLevel<T> where T : IExpressionExecutable
    {
        protected AbstractConditionalLevelInstanceV3(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structuralDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
        }

        public bool IsAnswered(string answer)
        {
            return !IsAnswerEmpty(answer);
        }

        public bool IsAnswered<TY>(TY? answer) where TY : struct
        {
            return !IsAnswerEmpty(answer);
        }

        public bool IsAnswered(GeoLocation answer)
        {
            return !IsAnswerEmpty(answer);
        }

        public bool IsAnswered(decimal[] answer)
        {
            return !IsAnswerEmpty(answer);
        }

        public bool IsAnswered(decimal[,] answer)
        {
            return !IsAnswerEmpty(answer);
        }

        public bool IsAnswered(Tuple<decimal, string>[] answer)
        {
            return !IsAnswerEmpty(answer);
        }
        
    }
}