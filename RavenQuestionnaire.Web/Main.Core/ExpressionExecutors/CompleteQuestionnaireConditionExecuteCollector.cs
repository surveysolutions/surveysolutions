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
    public class CompleteQuestionnaireConditionExecuteCollector : ICompleteQuestionnaireConditionExecuteCollector
    {
        #region Constants

        /// <summary>
        /// The stack depth limit.
        /// </summary>
        private const int StackDepthLimit = 0x128; //// stackoverflow insurance

        #endregion

        #region Fields

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
        /// The collect group hierarhically states.
        /// </summary>
        /// <param name="completeGroup">
        /// The complete group.
        /// </param>
        /// <param name="parentState">
        /// The parent state.
        /// </param>
        /// <param name="resultGroupStatus">
        /// The result group status.
        /// </param>
        /// <param name="resultQuestionsStatus">
        /// The result questions status.
        /// </param>
        public void CollectGroupHierarhicallyStates(
            ICompleteGroup completeGroup, 
            bool parentState, 
            Dictionary<string, bool?> resultGroupStatus, 
            Dictionary<string, bool?> resultQuestionsStatus /*,
            Dictionary<Guid, List<Guid>> conditionGroupDependencies*/)
        {
            string groupHashKey = GetGroupHashKey(completeGroup);

            if (resultGroupStatus.ContainsKey(groupHashKey))
            {
                return;
            }

            bool newState = this.CollectGroupStatus(
                completeGroup, parentState, 1, resultGroupStatus, resultQuestionsStatus);

            foreach (IComposite composite in completeGroup.Children)
            {
                var group = composite as ICompleteGroup;
                if (group != null)
                {
                    this.CollectGroupHierarhicallyStates(group, newState, resultGroupStatus, resultQuestionsStatus);
                    continue;
                }

                var question = composite as ICompleteQuestion;
                if (question != null)
                {
                    this.CollectQuestionsRecursive(question, newState, 1, resultQuestionsStatus);
                }
            }
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
            // in the template of questionnaire
            var dependentQuestions = new Dictionary<Guid, List<Guid>>();
            var dependentGroups = new Dictionary<Guid, List<Guid>>();
            ExpressionDependencyBuilder.HandleTree(this.doc, dependentQuestions, dependentGroups);

            // /////

            // groups state changing could influent to other group and  questions
            // if group of top level is disabled we don't have to evaluate all 
            // dependent conditions
            // but we have to order all groups by their distance from root
            // and start evaluation from the top
            if (dependentGroups.ContainsKey(question.PublicKey))
            {
                this.CollectGroupStates(question, resultGroupsStatus, resultQuestionsStatus, dependentGroups);
            }

            // do we need to collect all items or only from the first level?
            if (dependentQuestions.ContainsKey(question.PublicKey))
            {
                // TODO: create topologicaly sorted list of dependencies
                this.CollectQuestionStates(question, resultQuestionsStatus, dependentQuestions);
            }
        }

        #endregion

        #region Methods

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
            foreach (Guid item in conditionGroupDependencies[autoQuestion.PublicKey])
            {
                // get all dependent questions in subset
                Guid item1 = item;
                IEnumerable<ICompleteGroup> elementsToExecute = autoQuestion.PropagationPublicKey == null
                                                                    ? this.doc.Find<ICompleteGroup>(
                                                                        q => q.PublicKey == item1)
                                                                    : this.doc.Find<ICompleteGroup>(
                                                                        q =>
                                                                        q.PublicKey == item1
                                                                        &&
                                                                        q.PropagationPublicKey
                                                                        == autoQuestion.PropagationPublicKey);

                foreach (ICompleteGroup group in elementsToExecute)
                {
                    var completeItem = @group.GetParent() as ICompleteItem;
                    this.CollectGroupHierarhicallyStates(
                        group, completeItem != null && completeItem.Enabled, resultGroupsStatus, resultQuestionsStatus);
                }
            }
        }

        /// <summary>
        /// The collect group status.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="parentState">
        /// The parent state.
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
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        private bool CollectGroupStatus(
            IConditional group, 
            bool parentState, 
            int currentStack, 
            Dictionary<string, bool?> resultGroupStatus, 
            Dictionary<string, bool?> resultQuestionsStatus)
        {
            var g = group as ICompleteGroup;
            if (g == null)
            {
                throw new InvalidOperationException("Wrong type of object.");
            }

            string groupHashKey = GetGroupHashKey(g);

            if (resultQuestionsStatus.ContainsKey(groupHashKey))
            {
                return resultQuestionsStatus[groupHashKey].Value;
            }

            if (currentStack++ >= StackDepthLimit)
            {
                throw new Exception("Unsupported depth of expression.");
            }

            bool newState;

            if (string.IsNullOrEmpty(group.ConditionExpression))
            {
                newState = parentState;
            }
            else
            {
                newState = parentState && this.ExecuteCondition(group, currentStack, resultQuestionsStatus);
            }

            if (!resultGroupStatus.ContainsKey(groupHashKey))
            {
                resultGroupStatus.Add(groupHashKey, newState);
            }

            return newState;
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
            foreach (Guid item in conditionQuestionDependencies[autoQuestion.PublicKey])
            {
                // get all dependent questions in subset
                // it can be more then one item 
                Guid item1 = item;

                IEnumerable<ICompleteQuestion> elementsToExecute;
                if (autoQuestion.PropagationPublicKey == null)
                {
                    // source question is not from propagation group
                    elementsToExecute = this.doc.Find<ICompleteQuestion>(q => q.PublicKey == item1); //.GetQuestion(item1, null);
                }
                else
                {
                    // find all groups by propagation key and go through
                    List<ICompleteQuestion> tempElementsToExecute = new List<ICompleteQuestion>();
                    IEnumerable<CompleteGroup> groupsToSearch =
                        this.doc.Find<CompleteGroup>(g => g.PropagationPublicKey == autoQuestion.PropagationPublicKey);

                    foreach (CompleteGroup completeGroup in groupsToSearch)
                    {
                        var items = completeGroup.Find<ICompleteQuestion>(q => q.PublicKey == item1);
                        tempElementsToExecute.AddRange(items);
                    }

                    elementsToExecute = tempElementsToExecute.Distinct();
                }

                foreach (ICompleteQuestion question in elementsToExecute)
                {
                    this.CollectQuestionsRecursive(question, true, 1, resultQuestionsStatus);
                }
            }
        }

        /// <summary>
        /// The collect questions recursive.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="parentState">
        /// The parent State.
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
            ICompleteQuestion question, bool parentState, int currentStack, Dictionary<string, bool?> resultQuestionsStatus)
        {
            /*var q = question as IConditional;
            if (q == null)
            {
                throw new InvalidOperationException("Wrong type of object.");
            }*/

            string questionHashKey = question.PropagationPublicKey.HasValue
                                         ? string.Format("{0}{1}", question.PublicKey, question.PropagationPublicKey.Value)
                                         : question.PublicKey.ToString();
            if (resultQuestionsStatus.ContainsKey(questionHashKey))
            {
                return;
            }

            if (currentStack++ >= StackDepthLimit)
            {
                throw new Exception("Unsupported depth of expression.");
            }

            bool newState;

            if (string.IsNullOrEmpty(question.ConditionExpression))
            {
                newState = parentState;
            }
            else
            {
                newState = parentState && this.ExecuteCondition(question, currentStack, resultQuestionsStatus);
            }

            if (!resultQuestionsStatus.ContainsKey(questionHashKey))
            {
                resultQuestionsStatus.Add(questionHashKey, newState);
            }
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
        private bool ExecuteCondition(
            IConditional element, int currentStack, Dictionary<string, bool?> resultQuestionsStatus)
        {
            var expression = new Expression(element.ConditionExpression);

            expression.EvaluateParameter += (name, args) =>
                {
                    Guid nameGuid = Guid.Parse(name);
                    var item = element as IComposite;

                    // find question in the scope of current item
                    ICompleteQuestion targetQuestion = this.GetQuestionInScope(item, nameGuid);

                    if (targetQuestion != null && !string.IsNullOrWhiteSpace(targetQuestion.ConditionExpression))
                    {
                        string tempHashKey = targetQuestion.PropagationPublicKey.HasValue
                                                 ? string.Format(
                                                     "{0}{1}", 
                                                     targetQuestion.PublicKey, 
                                                     targetQuestion.PropagationPublicKey.Value)
                                                 : targetQuestion.PublicKey.ToString();

                        if (!resultQuestionsStatus.ContainsKey(tempHashKey))
                        {
                            this.CollectQuestionsRecursive(
                                targetQuestion, true /*not very good*/, currentStack, resultQuestionsStatus);
                        }

                        bool? resultQuestionsStatu = resultQuestionsStatus[tempHashKey];
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
            bool result = true;
            try
            {
                result = (bool)expression.Evaluate();
            }
            catch
            {
                #warning no exceptions should be ignored without at least writing to log
            }

            return result;
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
            IComposite parent = element;

            // go through hierarchy with the scope
            while (parent != null)
            {
                var item = parent as ICompleteItem;
                if (item == null)
                {
                    break;
                }


                var question = doc.GetQuestion(target, item.PropagationPublicKey);

                if (question != null)
                {
                    return question;
                }

                /*// do not look into propagate subgroups
                List<ICompleteQuestion> questions =
                    doc.Find<ICompleteQuestion>(q => q.PublicKey == target && q.PropagationPublicKey == item.PropagationPublicKey).ToList();

                if (questions.Any())
                {
                    result = questions.FirstOrDefault(); // ignore more than one result
                    break;
                }*/

                parent = parent.GetParent();
            }

            return result;
        }

        #endregion

        /*/// <summary>
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
        }*/
    }
}