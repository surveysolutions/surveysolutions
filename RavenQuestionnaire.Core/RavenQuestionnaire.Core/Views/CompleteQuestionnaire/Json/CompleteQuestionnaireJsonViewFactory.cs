#region

using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.Storage;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ViewSnapshot;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
using RavenQuestionnaire.Core.Entities.Extensions;
#endregion

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json
{
    public class CompleteQuestionnaireJsonViewFactory : IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>
    {
        private readonly IViewSnapshot store;

        public CompleteQuestionnaireJsonViewFactory(IViewSnapshot store)
        {
            this.store = store;
        }

        #region IViewFactory<CompleteQuestionnaireViewInputModel,CompleteQuestionnaireViewV> Members

        public CompleteQuestionnaireJsonView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc =
                    this.store.ReadByGuid<CompleteQuestionnaireDocument>(Guid.Parse(input.CompleteQuestionnaireId));
                //var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);
             //   var completeQuestionnaireRoot = new Entities.CompleteQuestionnaire(doc);
                ICompleteGroup group = null;
                
                if (input.CurrentGroupPublicKey.HasValue)
                {
                    group = doc.Find<CompleteGroup>(input.CurrentGroupPublicKey.Value);
                }
               
                return new CompleteQuestionnaireJsonView(doc, group);
            }
          /*  if (!string.IsNullOrEmpty(input.TemplateQuestionanireId))
            {
                var doc = documentSession.Load<QuestionnaireDocument>(input.TemplateQuestionanireId);
                return new CompleteQuestionnaireJsonView((CompleteQuestionnaireDocument)doc);
            }*/
            return null;
        }

        #endregion
    }


    public class CompleteQuestionnaireMobileViewFactory : IViewFactory<CompleteQuestionnaireViewInputModel, CompleteGroupMobileView>
    {
        private readonly IViewSnapshot store;

        public CompleteQuestionnaireMobileViewFactory(IViewSnapshot store)
        {
            this.store = store;
        }

        #region IViewFactory<CompleteQuestionnaireViewInputModel,CompleteQuestionnaireViewV> Members

        public CompleteGroupMobileView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc =
                    this.store.ReadByGuid<CompleteQuestionnaireDocument>(Guid.Parse(input.CompleteQuestionnaireId));
                //var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);
                //   var completeQuestionnaireRoot = new Entities.CompleteQuestionnaire(doc);
                ICompleteGroup group = null;

                var navigation = new ScreenNavigation();
                
                if (input.CurrentGroupPublicKey.HasValue)
                {
                   // group = doc.FindGroupByKey(input.CurrentGroupPublicKey.Value, input.PropagationKey);
                    Stack<NodeWithLevel> treeStack = new Stack<NodeWithLevel>();
                    var rout = new List<NodeWithLevel>();
                    treeStack.Push(new NodeWithLevel(doc, 0));
                    while (treeStack.Count>0)
                    {
                        var node = treeStack.Pop();
                        group = ProceedGroup(node.Group, input.CurrentGroupPublicKey.Value, input.PropagationKey);
                        UpdateNavigation(rout, node);

                        if (group != null)
                        {
                            rout.RemoveAt(rout.Count-1);
                            break;
                        }
                        
                        var subGroups = node.Group.Children.OfType<ICompleteGroup>().ToArray();
                        
                        for (int i = subGroups.Length - 1; i >= 0; i--)
                        {
                            treeStack.Push(new NodeWithLevel(subGroups[i], node.Level + 1));
                        }
                    }
                    navigation.BreadCumbs = rout.Select(n => new CompleteGroupHeaders(n.Group)).ToList();
                }
                if (input.PropagationKey.HasValue)
                    return new PropagatedGroupMobileView(doc, group);
                return new CompleteGroupMobileView(doc, (CompleteGroup)group, navigation);
            }
            /*  if (!string.IsNullOrEmpty(input.TemplateQuestionanireId))
              {
                  var doc = documentSession.Load<QuestionnaireDocument>(input.TemplateQuestionanireId);
                  return new CompleteQuestionnaireJsonView((CompleteQuestionnaireDocument)doc);
              }*/
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