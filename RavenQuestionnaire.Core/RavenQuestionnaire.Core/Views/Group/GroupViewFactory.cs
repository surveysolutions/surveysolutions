// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The group view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Group
{
    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities;
    using RavenQuestionnaire.Core.Entities.SubEntities;

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
            QuestionnaireDocument doc = this.store.GetByGuid(input.QuestionnaireId);
            var group = new Questionnaire(doc).Find<Group>(input.PublicKey);
            if (group == null)
            {
                return null;
            }

            return new GroupView(doc, group);
        }

        #endregion
    }
}