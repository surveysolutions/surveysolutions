using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.V8
{
    public abstract class AbstractInterviewExpressionStateV8 : AbstractInterviewExpressionStateV7, IInterviewExpressionStateV8
    {
        protected AbstractInterviewExpressionStateV8() {}

        protected AbstractInterviewExpressionStateV8(IDictionary<string, IExpressionExecutableV8> interviewScopes, Dictionary<string, List<string>> siblingRosters, IInterviewProperties interviewProperties)
            : this(interviewScopes.AsEnumerable(), siblingRosters, interviewProperties) {}

        protected AbstractInterviewExpressionStateV8(IEnumerable<KeyValuePair<string, IExpressionExecutableV8>> interviewScopes, Dictionary<string, List<string>> siblingRosters, IInterviewProperties interviewProperties)
            : base(
                interviewScopes.ToDictionary<KeyValuePair<string, IExpressionExecutableV8>, string, IExpressionExecutableV7>(
                    item => item.Key,
                    item => item.Value),
                siblingRosters,
                interviewProperties) {}

        private IDictionary<string, IExpressionExecutableV8> interviewScopes;

        public new IDictionary<string, IExpressionExecutableV8> InterviewScopes
            => this.interviewScopes ?? (this.interviewScopes = this.InitializeInterviewScopes());

        private IDictionary<string, IExpressionExecutableV8> InitializeInterviewScopes()
            => new TwoWayDictionaryAdapter<string, IExpressionExecutableV7, IExpressionExecutableV8>(
                () => base.InterviewScopes, ConvertExpressionExecutableV7ToV8, ConvertExpressionExecutableV8ToV7);

        private static IExpressionExecutableV7 ConvertExpressionExecutableV8ToV7(IExpressionExecutableV8 expressionExecutableV8)
            => expressionExecutableV8;

        private static IExpressionExecutableV8 ConvertExpressionExecutableV7ToV8(IExpressionExecutableV7 expressionExecutableV7)
        {
            if (expressionExecutableV7 == null)
                return null;

            if (expressionExecutableV7 is IExpressionExecutableV8)
                return (IExpressionExecutableV8) expressionExecutableV7;

            throw new NotSupportedException($"Interview scope expression executable V7 ({expressionExecutableV7.GetType().FullName}) cannot be converted to V8");
        }

        public new IEnumerable<IExpressionExecutableV8> GetRosterInstances(Identity[] rosterKey, Guid scopeId)
            => base.GetRosterInstances(rosterKey, scopeId)?.Cast<IExpressionExecutableV8>();

        public new virtual EnablementChanges ProcessEnablementConditions()
            => EnablementChanges.Union(
                this.InterviewScopes
                    .Values
                    .OrderBy(x => x.GetLevel()) // order by scope depth starting from top because conditionally lower scope could depend only from upper scope
                    .Select(scope => scope.ProcessEnablementConditions()));

        protected new virtual IExpressionExecutableV8 GetRosterByIdAndVector(Guid questionId, decimal[] rosterVector)
            => (IExpressionExecutableV8) base.GetRosterByIdAndVector(questionId, rosterVector);

        public void DisableStaticTexts(IEnumerable<Identity> staticTextsToDisable)
        {
            foreach (Identity staticText in staticTextsToDisable)
            {
                var targetLevel = this.GetRosterByIdAndVector(staticText.Id, staticText.RosterVector);

                targetLevel?.DisableStaticText(staticText.Id);
            }
        }

        public void EnableStaticTexts(IEnumerable<Identity> staticTextsToEnable)
        {
            foreach (Identity staticText in staticTextsToEnable)
            {
                var targetLevel = this.GetRosterByIdAndVector(staticText.Id, staticText.RosterVector);

                targetLevel?.EnableStaticText(staticText.Id);
            }
        }

        public void DeclareStaticTextValid(IEnumerable<Identity> validStaticTexts)
        {
            foreach (Identity staticText in validStaticTexts)
            {
                var targetLevel = this.GetRosterByIdAndVector(staticText.Id, staticText.RosterVector);

                targetLevel?.DeclareStaticTextValid(staticText.Id);
            }
        }

        void IInterviewExpressionStateV8.ApplyStaticTextFailedValidations(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions)
        {
            foreach (var failedValidationCondition in failedValidationConditions)
            {
                var targetLevel = this.GetRosterByIdAndVector(failedValidationCondition.Key.Id, failedValidationCondition.Key.RosterVector);

                (targetLevel as IExpressionExecutableV8)?.ApplyStaticTextFailedValidations(failedValidationCondition.Key.Id, failedValidationCondition.Value);
            }
        }

        IInterviewExpressionStateV8 IInterviewExpressionStateV8.Clone() => (IInterviewExpressionStateV8) this.Clone();
    }
}