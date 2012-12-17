// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireConditionExecuteCollector.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire condition executor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.ExpressionExecutors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.ExpressionExecutors.ExpressionExtentions;

    using NCalc;

    /// <summary>
    /// The complete questionnaire condition execute collector.
    /// </summary>
    public class CompleteQuestionnaireConditionExecuteCollector
    {
        #region Fields

        /// <summary>
        /// The stack depth limit.
        /// </summary>
        private const int StackDepthLimit = 0x64; //// stackoverflow insurance

        /// <summary>
        /// The hash.
        /// </summary>
        private readonly ICompleteQuestionnaireDocument doc;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireConditionExecuteCollector"/> class. 
        /// </summary>
        /// <param name="doc">
        /// The hash.
        /// </param>
        public CompleteQuestionnaireConditionExecuteCollector(ICompleteQuestionnaireDocument doc)
        {
            this.doc = doc;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get hash key.
        /// </summary>
        /// <param name="g">
        /// The g.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetGroupHashKey(ICompleteGroup g)
        {
            return g.PropagationPublicKey.HasValue
                                         ? string.Format("{0}{1}", g.PublicKey, g.PropagationPublicKey.Value)
                                         : g.PublicKey.ToString();
        }

        /// <summary>
        /// The execute condition after answer.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="resultQuestionsStatus">
        /// The result questions status.
        /// </param>
        /// <param name="resultGroupsStatus">
        /// The result groups status.
        /// </param>
        public void ExecuteConditionAfterAnswer(
            ICompleteQuestion question,
            Dictionary<string, bool?> resultQuestionsStatus,
            Dictionary<string, bool?> resultGroupsStatus)
        {
            
            // TODO: 
            // move this into design time
            // more efficient collect all dependencies and then set the values
            // in template of questionnaire
            var dependentQuestions = new Dictionary<Guid, List<Guid>>();
            var dependentGroups = new Dictionary<Guid, List<Guid>>();
            ExpressionDependencyBuilder.HandleTree(this.doc, dependentQuestions, dependentGroups);
            // /////

            // do we need to collect all items or only from the first level?
            if (dependentQuestions.ContainsKey(question.PublicKey))
            {
                // TODO: create topologicaly sorted list of dependencies
                this.CollectQuestionStates(question, resultQuestionsStatus, dependentQuestions);
            }

            if (dependentGroups.ContainsKey(question.PublicKey))
            {
                this.CollectGroupStates(question, resultGroupsStatus, resultQuestionsStatus, dependentGroups);
            }
        }

        /// <summary>
        /// The get question in scope.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <returns>
        /// The <see cref="ICompleteQuestion"/>.
        /// </returns>
        private ICompleteQuestion GetQuestionInScope(IComposite element, Guid target)
        {
            ICompleteQuestion result = null;
            IComposite parent = element.Parent;

            // go through hierarchy with the scope
            while (parent != null)
            {
                var item = parent as ICompleteGroup;
                if (item == null)
                {
                    break;
                }

                // do not look into propagate subgroups
                var questions =
                    parent.Find<ICompleteQuestion>(
                        q => q.PublicKey == target && q.PropagationPublicKey == item.PropagationPublicKey).ToList();

                if (questions.Any())
                {
                    result = questions.FirstOrDefault(); // ignore more than one result
                    break;
                }

                parent = item.Parent;
            }

            return result;
        }

        /// <summary>
        /// The collect conditional group states.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="resultGroupsStatus">
        /// The result groups status.
        /// </param>
        /// <param name="resultQuestionsStatus">
        /// The result questions status.
        /// </param>
        private void CollectConditionalGroupStates(
            ICompleteGroup group,
            Dictionary<string, bool?> resultGroupsStatus,
            Dictionary<string, bool?> resultQuestionsStatus)
        {
            if (string.IsNullOrEmpty(group.ConditionExpression))
            {
                return;
            }

            const int StackDepth = 1;

            this.CollectGroups(
                group,
                StackDepth,
                resultGroupsStatus,
                resultQuestionsStatus);
        }

        /// <summary>
        /// The execute condition.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        /// <param name="currentStack">
        /// The current stack.
        /// </param>
        /// <param name="resultQuestionsStatus">
        /// The result questions status.
        /// </param>
        /// <returns>
        /// The <see cref="bool?"/>.
        /// </returns>
        private bool? ExecuteCondition(IConditional element, int currentStack, Dictionary<string, bool?> resultQuestionsStatus)
        {
            var expression = new Expression(element.ConditionExpression);

            expression.EvaluateParameter += (name, args) =>
            {
                Guid nameGuid = Guid.Parse(name);
                var item = element as IComposite;

                // find question in the scope of current item
                var targetQuestion = this.GetQuestionInScope(item, nameGuid);
                
                if (targetQuestion != null && !string.IsNullOrWhiteSpace(targetQuestion.ConditionExpression))
                {
                    string tempHashKey = targetQuestion.PropagationPublicKey.HasValue
                                         ? string.Format("{0}{1}", targetQuestion.PublicKey, targetQuestion.PropagationPublicKey.Value)
                                         : targetQuestion.PublicKey.ToString();
                    if (!resultQuestionsStatus.ContainsKey(tempHashKey))
                    {
                        this.CollectQuestionsRecursive(targetQuestion, currentStack, resultQuestionsStatus);
                    }

                    var resultQuestionsStatu = resultQuestionsStatus[tempHashKey];
                    if (resultQuestionsStatu != null && resultQuestionsStatu.Value != true)
                    {
                        args.Result = null;
                        return;
                    }
                }

                if (targetQuestion == null)
                {
                    args.Result = null;
                    return;
                }

                args.Result = targetQuestion.GetAnswerObject();
            };

            expression.EvaluateFunction += ExtensionFunctions.EvaluateFunctionContains; ////support for multioption

            //// if condition is failed to execute question or group have to be active to avoid impossible to complete survey 
            //// we could treat null as success
            bool? result = null;
            try
            {
                result = (bool)expression.Evaluate();
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        /// <summary>
        /// The collect questions recursive.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="currentStack">
        /// The current stack.
        /// </param>
        /// <param name="resultQuestionsStatus">
        /// The result questions status.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        private void CollectQuestionsRecursive(
            IConditional question, 
            int currentStack, 
            Dictionary<string, bool?> resultQuestionsStatus)
        {
            if (string.IsNullOrEmpty(question.ConditionExpression))
            {
                return;
            }
            
            var q = question as ICompleteQuestion;
            if (q == null)
            {
                throw new InvalidOperationException("Wrong type of object.");
            }

            string questionHashKey = q.PropagationPublicKey.HasValue
                                         ? string.Format("{0}{1}", q.PublicKey, q.PropagationPublicKey.Value)
                                         : q.PublicKey.ToString();
            if (resultQuestionsStatus.ContainsKey(questionHashKey))
            {
                    return;
            }
            
            if (currentStack++ >= StackDepthLimit)
            {
                throw new Exception("Unsupported depth of expression.");
            }

            var newState = this.ExecuteCondition(question, currentStack, resultQuestionsStatus);

            if (!resultQuestionsStatus.ContainsKey(questionHashKey))
            {
                resultQuestionsStatus.Add(questionHashKey, newState);
            }
        }
        
        /// <summary>
        /// The collect groups.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="currentStack">
        /// The current stack.
        /// </param>
        /// <param name="resultGroupStatus">
        /// The result group status.
        /// </param>
        /// <param name="resultQuestionsStatus">
        /// The result questions status.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        private void CollectGroups(
            IConditional group,
            int currentStack,
            Dictionary<string, bool?> resultGroupStatus,
            Dictionary<string, bool?> resultQuestionsStatus)
        {
            if (string.IsNullOrEmpty(group.ConditionExpression))
            {
                return;
            }

            var g = group as ICompleteGroup;
            if (g == null)
            {
                throw new InvalidOperationException("Wrong type of object.");
            }

            string groupHashKey = GetGroupHashKey(g);

            if (resultQuestionsStatus.ContainsKey(groupHashKey))
            {
                return;
            }

            if (currentStack++ >= StackDepthLimit)
            {
                throw new Exception("Unsupported depth of expression.");
            }

            var newState = this.ExecuteCondition(group, currentStack, resultQuestionsStatus);

            if (!resultGroupStatus.ContainsKey(groupHashKey))
            {
                resultGroupStatus.Add(groupHashKey, newState);
            }
        }

        /// <summary>
        /// The collect question states.
        /// </summary>
        /// <param name="autoQuestion">
        /// The auto question.
        /// </param>
        /// <param name="resultQuestionsStatus">
        /// The result questions status.
        /// </param>
        /// <param name="conditionQuestionDependencies">
        /// The condition question dependencies.
        /// </param>
        private void CollectQuestionStates(
            ICompleteQuestion autoQuestion,
            Dictionary<string, bool?> resultQuestionsStatus,
            Dictionary<Guid, List<Guid>> conditionQuestionDependencies)
        {
            // iterates over all items depending from this question
            // generaly we hav to evaluate only directrly dependent questions
            // and their dependents but not the all questions
            // todo: build tree and split execution into 2 ways
            foreach (var item in conditionQuestionDependencies[autoQuestion.PublicKey])
            {
                // get all dependent questions in subset
                // it can be more then one item 
                Guid item1 = item;

                IEnumerable<ICompleteQuestion> elementsToExecute;
                if (autoQuestion.PropagationPublicKey == null)
                {
                    // source question is not from propagation group
                    elementsToExecute = this.doc.Find<ICompleteQuestion>(q => q.PublicKey == item1);
                }
                else
                {
                    // find all groups by propagation key and go through
                    IEnumerable<ICompleteQuestion> tempElementsToExecute = new List<ICompleteQuestion>();
                    var groupsToSearch = this.doc.Find<ICompleteGroup>(g => g.PropagationPublicKey == autoQuestion.PropagationPublicKey);
                    
                    foreach (var completeGroup in groupsToSearch)
                    {
                        tempElementsToExecute = tempElementsToExecute.Concat(completeGroup.Find<ICompleteQuestion>(q => q.PublicKey == item1)).ToList();
                    }

                    elementsToExecute = tempElementsToExecute.Distinct();
                }

                foreach (var questions in elementsToExecute)
                {
                    this.CollectConditionalQuestionStates(questions, resultQuestionsStatus);
                }
            }
        }

        /// <summary>
        /// The collect conditional question states.
        /// </summary>
        /// <param name="questionToEvaluate">
        /// The question to evaluate.
        /// </param>
        /// <param name="resultQuestionsStatus">
        /// The result questions status.
        /// </param>
        private void CollectConditionalQuestionStates(
           ICompleteQuestion questionToEvaluate,
           Dictionary<string, bool?> resultQuestionsStatus)
        {
            if (string.IsNullOrEmpty(questionToEvaluate.ConditionExpression))
            {
                return;
            }

            //// stack overflow 
            const int StackDepth = 1;

            this.CollectQuestionsRecursive(
                questionToEvaluate,
                StackDepth,
                resultQuestionsStatus);
        }

        /// <summary>
        /// The collect group states.
        /// </summary>
        /// <param name="autoQuestion">
        /// The auto question.
        /// </param>
        /// <param name="resultGroupsStatus">
        /// The result groups status.
        /// </param>
        /// <param name="resultQuestionsStatus">
        /// The result questions status.
        /// </param>
        /// <param name="conditionGroupDependencies">
        /// The condition group dependencies.
        /// </param>
        private void CollectGroupStates(
            ICompleteQuestion autoQuestion,
            Dictionary<string, bool?> resultGroupsStatus,
            Dictionary<string, bool?> resultQuestionsStatus,
            Dictionary<Guid, List<Guid>> conditionGroupDependencies)
        {
            // iterates over all dependent items  
            foreach (var item in conditionGroupDependencies[autoQuestion.PublicKey])
            {
                // get all dependent questions in subset
                Guid item1 = item;
                var elementsToExecute = autoQuestion.PropagationPublicKey == null
                                        ? this.doc.Find<ICompleteGroup>(q => q.PublicKey == item1)
                                        : this.doc.Find<ICompleteGroup>(
                                            q =>
                                            q.PublicKey == item1
                                            && q.PropagationPublicKey == autoQuestion.PropagationPublicKey);

                foreach (var group in elementsToExecute)
                {
                    this.CollectConditionalGroupStates(group, resultGroupsStatus, resultQuestionsStatus);
                }
            }
        }

        #endregion
    }
}