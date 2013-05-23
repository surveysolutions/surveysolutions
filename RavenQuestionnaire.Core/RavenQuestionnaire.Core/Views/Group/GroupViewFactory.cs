// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The group view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.DenormalizerStorage;

namespace RavenQuestionnaire.Core.Views.Group
{
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The group view factory.
    /// </summary>
    public class GroupViewFactory : IViewFactory<GroupViewInputModel, GroupView>
    {
        #region Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireDocument> store;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupViewFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public GroupViewFactory(IDenormalizerStorage<QuestionnaireDocument> store)
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
        /// The RavenQuestionnaire.Core.Views.Group.GroupView.
        /// </returns>
        public GroupView Load(GroupViewInputModel input)
        {
            QuestionnaireDocument doc = this.store.GetById(input.QuestionnaireId);
            var group = doc.Find<Group>(input.PublicKey);
            if (group == null)
            {
                return null;
            }

            return new GroupView(doc, group);
        }

        #endregion
    }
}