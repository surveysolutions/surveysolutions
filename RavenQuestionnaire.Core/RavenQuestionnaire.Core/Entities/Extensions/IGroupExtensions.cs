// -----------------------------------------------------------------------
// <copyright file="IGroupExtensions.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Entities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class IGroupExtensions
    {
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
            if (root.Move(root.Children, itemPublicKey, groupKey, after))
            {
                return true;
            }

            return root.Children.Any(@group => MoveItem(group, itemPublicKey, groupKey, after));
        }


        /// <summary>
        /// The move.
        /// </summary>
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
        private static bool Move(this IComposite root,List<IComposite> groups, Guid itemPublicKey, Guid? groupKey, Guid? after)
        {
            IComposite moveble = groups.FirstOrDefault(g => g.PublicKey == itemPublicKey);
            if (moveble == null)
            {
                return false;
            }

            if (groupKey.HasValue)
            {
                var moveToGroup = root.Find<Group>((Guid)groupKey);
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
    }
}
