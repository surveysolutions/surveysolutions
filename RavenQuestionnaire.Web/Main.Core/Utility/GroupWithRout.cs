namespace Main.Core.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View.Group;

    /// <summary>
    /// The group with rout.
    /// </summary>
    public class GroupWithRout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupWithRout"/> class.
        /// </summary>
        /// <param name="currentRout">
        /// The current rout.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        public GroupWithRout(IEnumerable<NodeWithLevel> currentRout, ICompleteGroup group, QuestionScope scope)
        {
            this.CurrentRout = currentRout;
            this.Group = group;
            this.Scope = scope;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupWithRout"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        public GroupWithRout(ICompleteGroup doc, Guid? publicKey, Guid? propagationKey, QuestionScope scope)
        {
            this.Scope = scope;
            var rout = new List<NodeWithLevel>();

            ICompleteGroup group = null;

            if (publicKey.HasValue)
            {
                var treeStack = new Stack<NodeWithLevel>();
                treeStack.Push(new NodeWithLevel(doc, 0));
                while (treeStack.Count > 0)
                {
                    NodeWithLevel node = treeStack.Pop();
                    group = this.ProceedGroup(node.Group, publicKey.Value, propagationKey);

                    this.UpdateNavigation(rout, node);

                    if (group != null)
                    {
                        break;
                    }

                    ICompleteGroup[] subGroups = node.Group.Children.OfType<ICompleteGroup>().ToArray();
                    for (int i = subGroups.Length - 1; i >= 0; i--)
                    {
                        // questions exists, but they are hidden 
                        if (!subGroups[i].HasVisibleItemsForScope(this.Scope))
                        {
                            continue;
                        }

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

            this.CurrentRout = rout;
            this.Group = group;
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

            if (propagationKey.HasValue && node.PropagationPublicKey != propagationKey.Value)
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
        /// The <see cref="ScreenNavigation"/>.
        /// </returns>
        protected ScreenNavigation CompileNavigation()
        {
            var temtNavigation = new ScreenNavigation { PublicKey = this.Group.PublicKey, CurrentScreenTitle = this.Group.Title };

            var rout = this.CurrentRout.Take(this.CurrentRout.Count() - 1).ToList();
            temtNavigation.BreadCumbs = rout.Select(n => new CompleteGroupHeaders(n.Group)).ToList();
            NodeWithLevel parent = rout.Last();
            List<ICompleteGroup> groupNeighbors;
            int indexOfTarget;
            if (this.Group.PropagationPublicKey.HasValue)
            {
                groupNeighbors =
                    parent.Group.Children.OfType<ICompleteGroup>().Where(
                        g => g.PublicKey == this.Group.PublicKey && g.PropagationPublicKey.HasValue).ToList();


                groupNeighbors = groupNeighbors.Where(g => g.Enabled).ToList();
                indexOfTarget = groupNeighbors.FindIndex(0, g => g.PropagationPublicKey == this.Group.PropagationPublicKey);
            }
            else
            {
                groupNeighbors = parent.Group.Children.OfType<ICompleteGroup>()
                    .Where(g => !g.PropagationPublicKey.HasValue)

                    // filter all empty groups or groups with any visible question
                    .Where(g => g.HasVisibleItemsForScope(this.Scope)).ToList(); 

                groupNeighbors = groupNeighbors.Where(g => g.Enabled).ToList();
                indexOfTarget = groupNeighbors.FindIndex(0, g => g.PublicKey == this.Group.PublicKey);
            }

            /*  if (indexOfTarget < 0)
                  throw new InvalidOperationException("groups wasn't founded");*/
            if (indexOfTarget > 0)
            {
                temtNavigation.PrevScreen = new CompleteGroupHeaders(groupNeighbors[indexOfTarget - 1]);
            }

            if (indexOfTarget < groupNeighbors.Count - 1)
            {
                temtNavigation.NextScreen = new CompleteGroupHeaders(groupNeighbors[indexOfTarget + 1]);
            }

            return temtNavigation;
        }

        /// <summary>
        /// Gets the navigation.
        /// </summary>
        public ScreenNavigation Navigation
        {
            get
            {
                if (navigation == null)
                    navigation = CompileNavigation();
                return navigation;
            }
        }

        /// <summary>
        /// Gets the current rout.
        /// </summary>
        public IEnumerable<NodeWithLevel> CurrentRout { get; private set; }

        /// <summary>
        /// The navigation.
        /// </summary>
        private ScreenNavigation navigation;

        /// <summary>
        /// Gets the group.
        /// </summary>
        public ICompleteGroup Group { get; private set; }

        /// <summary>
        /// Gets or sets Scope.
        /// </summary>
        public QuestionScope Scope { get; set; }
    }
}
