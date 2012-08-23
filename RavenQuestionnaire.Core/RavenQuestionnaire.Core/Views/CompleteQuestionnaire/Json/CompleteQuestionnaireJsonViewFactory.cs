using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json
{
    public class CompleteQuestionnaireJsonViewFactory : IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>
    {
        private IDenormalizerStorage<CompleteQuestionnaireDocument> store;

        public CompleteQuestionnaireJsonViewFactory(IDenormalizerStorage<CompleteQuestionnaireDocument> store)
        {
            this.store = store;
        }

        #region IViewFactory<CompleteQuestionnaireViewInputModel,CompleteQuestionnaireViewV> Members

        public CompleteQuestionnaireJsonView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc =
                    this.store.GetByGuid(Guid.Parse(input.CompleteQuestionnaireId));
                ICompleteGroup group = null;
                if (input.CurrentGroupPublicKey.HasValue)
                    group = doc.FindGroupByKey(input.CurrentGroupPublicKey.Value, input.PropagationKey);
                return new CompleteQuestionnaireJsonView(doc, group);
            }
            return null;
        }

        #endregion
    }
    
    public class CompleteQuestionnaireMobileViewFactory : IViewFactory<CompleteQuestionnaireViewInputModel, CompleteGroupMobileView>
    {
        private IDenormalizerStorage<CompleteQuestionnaireDocument> store;

        public CompleteQuestionnaireMobileViewFactory(IDenormalizerStorage<CompleteQuestionnaireDocument> store)
        {
            this.store = store;
        }

        #region IViewFactory<CompleteQuestionnaireViewInputModel,CompleteQuestionnaireViewV> Members

        public CompleteGroupMobileView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc =
                    this.store.GetByGuid(Guid.Parse(input.CompleteQuestionnaireId));
                ICompleteGroup group = null;
                var rout = new List<NodeWithLevel>();
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    Stack<NodeWithLevel> treeStack = new Stack<NodeWithLevel>();
                    treeStack.Push(new NodeWithLevel(doc, 0));
                    while (treeStack.Count>0)
                    {
                        var node = treeStack.Pop();
                        group = ProceedGroup(node.Group, input.CurrentGroupPublicKey.Value, input.PropagationKey);
                        UpdateNavigation(rout, node);
                        if (group != null)
                            break;
                        var subGroups = node.Group.Children.OfType<ICompleteGroup>().ToArray();
                        for (int i = subGroups.Length - 1; i >= 0; i--)
                            treeStack.Push(new NodeWithLevel(subGroups[i], node.Level + 1));
                    }
                }
                if (group == null)
                    group = doc.Children.OfType<ICompleteGroup>().First();
                var executor = new CompleteQuestionnaireConditionExecutor(doc.QuestionHash);
                executor.Execute(group);
                var validator = new CompleteQuestionnaireValidationExecutor(doc.QuestionHash);
                validator.Execute(group);
                if (input.PropagationKey.HasValue)
                    return new PropagatedGroupMobileView(doc, group, CompileNavigation(rout,group));
                return new CompleteGroupMobileView(doc, (CompleteGroup)group, CompileNavigation(rout, group));
            }
            return null;
        }
        protected class NodeWithLevel
        {
            public NodeWithLevel(ICompleteGroup group, int level)
            {
                this.Group = group;
                this.Level = level;
            }

            public ICompleteGroup Group { get;private set; }
            public int Level { get; private set; }
        }
        #endregion
        protected ScreenNavigation CompileNavigation(List<NodeWithLevel> rout, ICompleteGroup group)
        {
            ScreenNavigation navigation=new ScreenNavigation();
            navigation.PublicKey = group.PublicKey;
            navigation.CurrentScreenTitle = group.Title;
            rout = rout.Take(rout.Count - 1).ToList();
            navigation.BreadCumbs = rout.Select(n => new CompleteGroupHeaders(n.Group)).ToList();
            var parent = rout.Last();
            List<ICompleteGroup> groupNeighbors;
            int indexOfTarget;
            if(group.PropogationPublicKey.HasValue)
            {
                groupNeighbors = parent.Group.Children.OfType<ICompleteGroup>().Where(g=>g.PublicKey==group.PublicKey && g.PropogationPublicKey.HasValue).ToList();
                indexOfTarget = groupNeighbors.FindIndex(0,
                                                         g =>
                                                         g.PropogationPublicKey == group.PropogationPublicKey);
            }
            else
            {
                groupNeighbors = parent.Group.Children.OfType<ICompleteGroup>().Where(g =>  !g.PropogationPublicKey.HasValue).ToList();
                indexOfTarget = groupNeighbors.FindIndex(0,
                                                         g =>
                                                         g.PublicKey == group.PublicKey);
            }
            
            if (indexOfTarget > 0)
                navigation.PrevScreen = new CompleteGroupHeaders(groupNeighbors[indexOfTarget - 1]);
            if (indexOfTarget < groupNeighbors.Count - 1)
                navigation.NextScreen = new CompleteGroupHeaders(groupNeighbors[indexOfTarget + 1]);
            return navigation;
        }

        protected void UpdateNavigation(List<NodeWithLevel> navigations, NodeWithLevel node)
        {
            navigations.RemoveAll(n => n.Level >= node.Level);
            navigations.Add(node);
        }

        protected ICompleteGroup ProceedGroup(ICompleteGroup node, Guid publicKey, Guid? propagationKey)
        {
            if (node.PublicKey != publicKey)
                return null;
            if (propagationKey.HasValue && node.PropogationPublicKey != propagationKey.Value)
                return null;
            return node;
        }
    }
}