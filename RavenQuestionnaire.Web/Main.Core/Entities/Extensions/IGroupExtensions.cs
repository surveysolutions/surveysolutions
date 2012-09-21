// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGroupExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class IGroupExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The move item.
        /// </summary>
        /// <param name="root">
        /// The root.
        /// </param>
        /// <param name="itemPublicKey">
        /// The item public key.
        /// </param>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="after">
        /// The after.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public static bool MoveItem(this IComposite root, Guid itemPublicKey, Guid? groupKey, Guid? after)
        {

            return MoveItem(root, itemPublicKey, groupKey, after, root);
        }
        private static bool MoveItem(this IComposite root, Guid itemPublicKey, Guid? groupKey, Guid? after, IComposite parent)
        {

            if (root.Move(root.Children, itemPublicKey, groupKey, after, parent))
            {
                return true;
            }

            return root.Children.Any(@group => MoveItem(group, itemPublicKey, groupKey, after, parent));
        }
        #endregion

        #region Methods

        /// <summary>
        /// The move.
        /// </summary>
        /// <param name="root">
        /// The root.
        /// </param>
        /// <param name="groups">
        /// The groups.
        /// </param>
        /// <param name="itemPublicKey">
        /// The item public key.
        /// </param>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="after">
        /// The after.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        private static bool Move(
            this IComposite root, List<IComposite> groups, Guid itemPublicKey, Guid? groupKey, Guid? after, IComposite parent)
        {
            IComposite moveble = groups.FirstOrDefault(g => g.PublicKey == itemPublicKey);
            if (moveble == null)
            {
                return false;
            }

            if (groupKey.HasValue)
            {
                //var moveToGroup = root.Find<Group>((Guid)groupKey);
                var moveToGroup = parent.Find<Group>((Guid)groupKey);
                if (moveToGroup != null)
                {
                    groups.Remove(moveble);
                    moveToGroup.Insert(moveble, after);
                    return true;
                }
            }

            if (!after.HasValue)
            {
                groups.Remove(moveble);
                groups.Insert(0, moveble);
                return true;
            }

            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].PublicKey == after.Value)
                {
                    groups.Remove(moveble);
                    if (i < groups.Count)
                    {
                        groups.Insert(i + 1, moveble);
                    }
                    else
                    {
                        groups.Add(moveble);
                    }

                    return true;
                }
            }

            throw new ArgumentException(string.Format("target item doesn't exists -{0}", after));
        }

        #endregion
    }
}