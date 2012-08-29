// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireJsonViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire json view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.Extensions;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
    using RavenQuestionnaire.Core.ExpressionExecutors;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;

    /// <summary>
    /// The complete questionnaire json view factory.
    /// </summary>
    public class CompleteQuestionnaireJsonViewFactory :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>
    {
        #region Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireJsonViewFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public CompleteQuestionnaireJsonViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
        {
            this.store = store;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json.CompleteQuestionnaireJsonView.
        /// </returns>
        public CompleteQuestionnaireJsonView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (input.CompleteQuestionnaireId != Guid.Empty)
            {
                CompleteQuestionnaireStoreDocument doc = this.store.GetByGuid(input.CompleteQuestionnaireId);
                ICompleteGroup group = null;
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group = doc.FindGroupByKey(input.CurrentGroupPublicKey.Value, input.PropagationKey);
                }

                return new CompleteQuestionnaireJsonView(doc, group);
            }

            return null;
        }

        #endregion
    }

    /// <summary>
    /// The complete questionnaire mobile view factory.
    /// </summary>
    public class CompleteQuestionnaireMobileViewFactory :
        IViewFactory<CompleteQuestionnaireViewInputModel, CompleteGroupMobileView>
    {
        #region Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireMobileViewFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public CompleteQuestionnaireMobileViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
        {
            this.store = store;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile.CompleteGroupMobileView.
        /// </returns>
        public CompleteGroupMobileView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (input.CompleteQuestionnaireId != Guid.Empty)
            {
                CompleteQuestionnaireStoreDocument doc = this.store.GetByGuid(input.CompleteQuestionnaireId);
                ICompleteGroup group = null;
                var rout = new List<NodeWithLevel>();
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    var treeStack = new Stack<NodeWithLevel>();
                    treeStack.Push(new NodeWithLevel(doc, 0));
                    while (treeStack.Count > 0)
                    {
                        NodeWithLevel node = treeStack.Pop();
                        group = this.ProceedGroup(node.Group, input.CurrentGroupPublicKey.Value, input.PropagationKey);
                        this.UpdateNavigation(rout, node);
                        if (group != null)
                        {
                            break;
                        }

                        ICompleteGroup[] subGroups = node.Group.Children.OfType<ICompleteGroup>().ToArray();
                        for (int i = subGroups.Length - 1; i >= 0; i--)
                        {
                            treeStack.Push(new NodeWithLevel(subGroups[i], node.Level + 1));
                        }
                    }
                }

                if (group == null)
                {
                    group = doc.Children.OfType<ICompleteGroup>().First();
                }

                var executor = new CompleteQuestionnaireConditionExecutor(doc.QuestionHash);
                executor.Execute(group);
                var validator = new CompleteQuestionnaireValidationExecutor(doc.QuestionHash);
                validator.Execute(group);
                if (input.PropagationKey.HasValue)
                {
                    return new PropagatedGroupMobileView(doc, group, this.CompileNavigation(rout, group));
                }

                return new CompleteGroupMobileView(doc, (CompleteGroup)group, this.CompileNavigation(rout, group));
            }

            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The compile navigation.
        /// </summary>
        /// <param name="rout">
        /// The rout.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile.ScreenNavigation.
        /// </returns>
        protected ScreenNavigation CompileNavigation(List<NodeWithLevel> rout, ICompleteGroup group)
        {
            var navigation = new ScreenNavigation();
            navigation.PublicKey = group.PublicKey;
            navigation.CurrentScreenTitle = group.Title;
            rout = rout.Take(rout.Count - 1).ToList();
            navigation.BreadCumbs = rout.Select(n => new CompleteGroupHeaders(n.Group)).ToList();
            NodeWithLevel parent = rout.Last();
            List<ICompleteGroup> groupNeighbors;
            int indexOfTarget;
            if (group.PropogationPublicKey.HasValue)
            {
                groupNeighbors =
                    parent.Group.Children.OfType<ICompleteGroup>().Where(
                        g => g.PublicKey == group.PublicKey && g.PropogationPublicKey.HasValue).ToList();
                indexOfTarget = groupNeighbors.FindIndex(0, g => g.PropogationPublicKey == group.PropogationPublicKey);
            }
            else
            {
                groupNeighbors =
                    parent.Group.Children.OfType<ICompleteGroup>().Where(g => !g.PropogationPublicKey.HasValue).ToList();
                indexOfTarget = groupNeighbors.FindIndex(0, g => g.PublicKey == group.PublicKey);
            }

            if (indexOfTarget > 0)
            {
                navigation.PrevScreen = new CompleteGroupHeaders(groupNeighbors[indexOfTarget - 1]);
            }

            if (indexOfTarget < groupNeighbors.Count - 1)
            {
                navigation.NextScreen = new CompleteGroupHeaders(groupNeighbors[indexOfTarget + 1]);
            }

            return navigation;
        }

        /// <summary>
        /// The proceed group.
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteGroup.
        /// </returns>
        protected ICompleteGroup ProceedGroup(ICompleteGroup node, Guid publicKey, Guid? propagationKey)
        {
            if (node.PublicKey != publicKey)
            {
                return null;
            }

            if (propagationKey.HasValue && node.PropogationPublicKey != propagationKey.Value)
            {
                return null;
            }

            return node;
        }

        /// <summary>
        /// The update navigation.
        /// </summary>
        /// <param name="navigations">
        /// The navigations.
        /// </param>
        /// <param name="node">
        /// The node.
        /// </param>
        protected void UpdateNavigation(List<NodeWithLevel> navigations, NodeWithLevel node)
        {
            navigations.RemoveAll(n => n.Level >= node.Level);
            navigations.Add(node);
        }

        #endregion

        /// <summary>
        /// The node with level.
        /// </summary>
        protected class NodeWithLevel
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="NodeWithLevel"/> class.
            /// </summary>
            /// <param name="group">
            /// The group.
            /// </param>
            /// <param name="level">
            /// The level.
            /// </param>
            public NodeWithLevel(ICompleteGroup group, int level)
            {
                this.Group = group;
                this.Level = level;
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets the group.
            /// </summary>
            public ICompleteGroup Group { get; private set; }

            /// <summary>
            /// Gets the level.
            /// </summary>
            public int Level { get; private set; }

            #endregion
        }
    }
}