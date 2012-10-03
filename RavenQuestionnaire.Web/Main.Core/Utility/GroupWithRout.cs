// -----------------------------------------------------------------------
// <copyright file="GroupWithRout.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.CompleteQuestionnaire;
using Main.Core.View.Group;

namespace Main.Core.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class GroupWithRout
    {
        public GroupWithRout(ICompleteGroup doc, Guid? publicKey, Guid? propagationKey)
        {

            var rout = new List<NodeWithLevel>();
            ICompleteGroup group = null;
            if (publicKey.HasValue)
            {

                var treeStack = new Stack<NodeWithLevel>();
                treeStack.Push(new NodeWithLevel(doc, 0));
                while (treeStack.Count > 0)
                {
                    NodeWithLevel node = treeStack.Pop();
                    group = ProceedGroup(node.Group, publicKey.Value, propagationKey);
                    UpdateNavigation(rout, node);

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
                rout.Add(new NodeWithLevel(doc, 0));

                group = doc.Children.OfType<ICompleteGroup>().First();
                rout.Add(new NodeWithLevel(group, 1));
            }
            currentRout = rout;
            Group = group;
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
        /// The Main.Core.Entities.SubEntities.Complete.ICompleteGroup.
        /// </returns>
        private ICompleteGroup ProceedGroup(ICompleteGroup node, Guid publicKey, Guid? propagationKey)
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
        private void UpdateNavigation(List<NodeWithLevel> navigations, NodeWithLevel node)
        {
            navigations.RemoveAll(n => n.Level >= node.Level);
            navigations.Add(node);
        }
        /// <summary>
        /// The compile navigation.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile.ScreenNavigation.
        /// </returns>
        protected ScreenNavigation CompileNavigation()
        {
            var navigation = new ScreenNavigation();
            navigation.PublicKey = this.Group.PublicKey;
            navigation.CurrentScreenTitle = this.Group.Title;
            var rout = this.currentRout.Take(this.currentRout.Count() - 1).ToList();
            navigation.BreadCumbs = rout.Select(n => new CompleteGroupHeaders(n.Group)).ToList();
            NodeWithLevel parent = rout.Last();
            List<ICompleteGroup> groupNeighbors;
            int indexOfTarget;
            if (this.Group.PropogationPublicKey.HasValue)
            {
                groupNeighbors =
                    parent.Group.Children.OfType<ICompleteGroup>().Where(
                        g => g.PublicKey == this.Group.PublicKey && g.PropogationPublicKey.HasValue).ToList();


                groupNeighbors = groupNeighbors.Where(g => g.Enabled).ToList();
                indexOfTarget = groupNeighbors.FindIndex(0, g => g.PropogationPublicKey == this.Group.PropogationPublicKey);
            }
            else
            {
                groupNeighbors =
                    parent.Group.Children.OfType<ICompleteGroup>().Where(g => !g.PropogationPublicKey.HasValue).ToList();


                groupNeighbors = groupNeighbors.Where(g => g.Enabled).ToList();
                indexOfTarget = groupNeighbors.FindIndex(0, g => g.PublicKey == this.Group.PublicKey);
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

        public ScreenNavigation Navigation
        {
            get
            {
                if (navigation == null)
                    navigation = CompileNavigation();
                return navigation;
            }
        }

        protected IEnumerable<NodeWithLevel> currentRout;
        private ScreenNavigation navigation;
        public ICompleteGroup Group { get; private set; }
    }
}
