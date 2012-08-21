#region

using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ViewSnapshot;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json;

#endregion

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile
{
    public class CompleteQuestionnaireMabileViewFactory : IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>
    {
        private readonly IViewSnapshot store;

        public CompleteQuestionnaireMabileViewFactory(IViewSnapshot store)
        {
            this.store = store;
        }

        #region IViewFactory<CompleteQuestionnaireViewInputModel,CompleteQuestionnaireViewV> Members

        public CompleteQuestionnaireMobileView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc = store.ReadByGuid<CompleteQuestionnaireDocument>(Guid.Parse(input.CompleteQuestionnaireId));
                UpdateInputData(doc, input);
                ICompleteGroup group = null;

                var rout = new List<NodeWithLevel>();
                 
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    Stack<NodeWithLevel> treeStack = new Stack<NodeWithLevel>();
                    treeStack.Push(new NodeWithLevel(doc, 0));
                    while (treeStack.Count > 0)
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
                {
                    group = doc.Children.OfType<ICompleteGroup>().First();
                    UpdateNavigation(rout,new NodeWithLevel(doc,0));
                    UpdateNavigation(rout, new NodeWithLevel(group, 1));
                }
                var navigation = CompileNavigation(rout, group);
                var currentScreen = navigation.BreadCumbs.Count > 1
                                        ? navigation.BreadCumbs[1].PublicKey
                                        : group.PublicKey;
                return new CompleteQuestionnaireMobileView(doc, currentScreen, group, CompileNavigation(rout, group));
            }
            return null;
        }

        #endregion

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
        protected void UpdateInputData(CompleteQuestionnaireDocument doc, CompleteQuestionnaireViewInputModel input)
        {
            if (input.CurrentGroupPublicKey.HasValue)
                return;
            if(doc.LastVisitedGroup==null)
                return;
            input.CurrentGroupPublicKey = doc.LastVisitedGroup.GroupKey;
            input.PropagationKey = doc.LastVisitedGroup.PropagationKey;
        }

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