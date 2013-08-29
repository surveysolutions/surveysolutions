namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Utility;
    using Main.Core.View.Group;

    /// <summary>
    /// The group with rout.
    /// </summary>
    public class ScreenWithRout
    {
        /// <summary>
        /// The navigation.
        /// </summary>
        private ScreenNavigation navigation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenWithRout"/> class.
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
        /// <param name="scope">
        /// The scope.
        /// </param>
        public ScreenWithRout(CompleteQuestionnaireStoreDocument doc, Guid? publicKey, Guid? propagationKey, QuestionScope scope)
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
            this.MenuItems = new List<DetailsMenuItem>();
            {
                var treeStack = new Stack<NodeWithLevel>();
                treeStack.Push(new NodeWithLevel(doc, 0));
                while (treeStack.Count > 0)
                {
                    NodeWithLevel node = treeStack.Pop();

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

                    group = this.ProceedScreen(node);

                    if (group == null)
                    {
                        continue;
                    }

                    var item = new DetailsMenuItem(doc, node);
                       
                    this.MenuItems.Add(item);
                }
            }
        }

        /// <summary>
        /// Gets the navigation.
        /// </summary>
        public ScreenNavigation Navigation
        {
            get
            {
                return this.navigation ?? (this.navigation = this.CompileNavigation());
            }
        }

        /// <summary>
        /// Gets the current rout.
        /// </summary>
        public IEnumerable<NodeWithLevel> CurrentRout { get; private set; }

        /// <summary>
        /// Gets the group.
        /// </summary>
        public ICompleteGroup Group { get; private set; }

        /// <summary>
        /// Gets or sets Scope.
        /// </summary>
        public QuestionScope Scope { get; set; }

        /// <summary>
        /// Gets or sets MenuItems.
        /// </summary>
        public List<DetailsMenuItem> MenuItems { get; set; }

        /// <summary>
        /// The compile navigation.
        /// </summary>
        /// <returns>
        /// The <see cref="ScreenNavigation"/>.
        /// </returns>
        protected ScreenNavigation CompileNavigation()
        {
            return new ScreenNavigation();
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
        /// The proceed group.
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <returns>
        /// The ICompleteGroup.
        /// </returns>
        private ICompleteGroup ProceedScreen(NodeWithLevel node)
        {
            if (node.Group.Propagated != Propagate.None && !node.Group.PropagationPublicKey.HasValue)
            {
                return null;
            }

            return node.Group;
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
    }
}
