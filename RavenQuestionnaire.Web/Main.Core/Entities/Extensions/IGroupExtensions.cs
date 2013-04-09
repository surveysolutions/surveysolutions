// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGroupExtensions.cs" company="The World Bank">
//   2012
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

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The i group extensions.
    /// </summary>
    public static class IGroupExtensions
    {
#if MONODROID
        private static readonly AndroidLogger.ILog Logger = AndroidLogger.LogManager.GetLogger(typeof(IGroupExtensions));
#else
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
#endif

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
        [Obsolete]
        public static bool MoveItem(this IComposite root, Guid itemPublicKey, Guid? groupKey, Guid? after)
        {

            return MoveItem(root, itemPublicKey, groupKey, after, root);
        }

        public static void MoveItem(this QuestionnaireDocument questionnaire, Guid itemId, Guid targetGroupId, int targetIndex)
        {
            Guid? idOfItemToPutAfter = GetIdOfItemToPutAfter(questionnaire, itemId, targetGroupId, targetIndex);

            questionnaire.MoveItem(itemId, targetGroupId, idOfItemToPutAfter);
        }

        private static Guid? GetIdOfItemToPutAfter(QuestionnaireDocument questionnaire, Guid idOfItemToMove, Guid targetGroupId, int targetIndex)
        {
            if (targetIndex == 0)
                return null;

            IGroup targetGroup = questionnaire.Find<IGroup>(group => @group.PublicKey == targetGroupId).FirstOrDefault();

            if (targetGroup == null)
            {
                Logger.Warn(string.Format("Failed to correctly move item to group {0} because group is missing.",
                    targetGroupId));

                return null;
            }

            if (targetGroup.Children.Count < targetIndex)
            {
                Logger.Warn(string.Format("Failed to correctly move item to group {0} to index {1} because group has only {2} children.",
                    targetGroupId, targetIndex, targetGroup.Children.Count));

                return null;
            }

            int currentIndexOfMovedItemInTargetGroup = targetGroup.Children.FindIndex(item => item.PublicKey == idOfItemToMove);

            bool willItemMoveAffectIndexOfItemToMoveAfter =
                currentIndexOfMovedItemInTargetGroup > -1 &&
                currentIndexOfMovedItemInTargetGroup < targetIndex;

            if (willItemMoveAffectIndexOfItemToMoveAfter)
                return targetGroup.Children[targetIndex].PublicKey;
            else
                return targetGroup.Children[targetIndex - 1].PublicKey;
        }

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
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
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
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        private static bool Move(this IComposite root, List<IComposite> groups, Guid itemPublicKey, Guid? groupKey, Guid? after, IComposite parent)
        {
            IComposite moveble = groups.FirstOrDefault(g => g.PublicKey == itemPublicKey);
            if (moveble == null)
            {
                return false;
            }

            if (groupKey.HasValue)
            {
                var moveToGroup = parent.Find<Group>((Guid)groupKey);
                if (moveToGroup != null)
                {
                    groups.Remove(moveble);
                    if (moveble is IGroup && after == null)
                    {
                        int index = groups.FindIndex(
                            0, groups.Count - 1, t => t.PublicKey == moveToGroup.PublicKey);
                        groups.Insert(index + 1, moveble);
                    }
                    else
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