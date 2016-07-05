using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V4;

namespace WB.Core.SharedKernels.DataCollection.V5
{
    public abstract class AbstractConditionalLevelInstanceV5<T> : AbstractConditionalLevelInstanceV4<T> where T : IExpressionExecutableV5, IExpressionExecutableV2, IExpressionExecutable
    {
        protected new Func<Identity[], Guid, IEnumerable<IExpressionExecutableV5>> GetInstances { get; private set; }
        protected new Dictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV5>> RosterGenerators { get; set; }

        protected AbstractConditionalLevelInstanceV5(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, 
            IEnumerable<IExpressionExecutableV5>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies, 
            Dictionary<Guid, Guid[]> structuralDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.GetInstances = getInstances;
            this.RosterGenerators = new Dictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV5>>();
        }

        protected AbstractConditionalLevelInstanceV5(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid,
            IEnumerable<IExpressionExecutableV5>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies, IInterviewProperties properties)
            : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.GetInstances = getInstances;
            this.Quest = properties;
        }

        public void UpdateYesNoAnswer(Guid questionId, YesNoAnswersOnly answers)
        {
            if (this.QuestionYesNoAnswerUpdateMap.ContainsKey(questionId))
                this.QuestionYesNoAnswerUpdateMap[questionId].Invoke(answers);
        }

        public bool IsAnswered(YesNoAnswers answer)
        {
            return !IsAnswerEmpty(answer);
        }

        public override IExpressionExecutableV2 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV2>> getInstances)
        {
            return null;
        }

        public IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances)
        {
            return null;
        }

        public abstract IExpressionExecutableV5 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV5>> getInstances);

        public new IExpressionExecutableV5 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey)
        {
            return this.RosterGenerators[rosterId].Invoke(rosterVector, rosterIdentityKey);
        }
    }
}