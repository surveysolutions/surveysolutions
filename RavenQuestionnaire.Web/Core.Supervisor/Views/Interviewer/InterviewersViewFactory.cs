// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewersViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The interviewers view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.DenormalizerStorage;

namespace Core.Supervisor.Views.Interviewer
{
    using System;
    using System.Linq;
    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;


    /// <summary>
    /// The interviewers view factory.
    /// </summary>
    public class InterviewersViewFactory : IViewFactory<InterviewersInputModel, InterviewersView>
    {
        #region Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewersViewFactory"/> class.
        /// </summary>
        /// <param name="users">
        /// The users.
        /// </param>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public InterviewersViewFactory(
            IDenormalizerStorage<UserDocument> users,
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession)
        {
            this.users = users;
            this.documentItemSession = documentSession;
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
        /// The RavenQuestionnaire.Core.Views.User.InterviewersView.
        /// </returns>
        public InterviewersView Load(InterviewersInputModel input)
        {
            Func<UserDocument, bool> query =
                x => x.Supervisor != null && x.Supervisor.Id == input.Id;

            var queryResult = this.users.Query().Where(query).AsQueryable().OrderUsingSortExpression(input.Order);

            var items = queryResult.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).Select(
                    x =>
                    new InterviewersItem(
                        x.PublicKey, 
                        x.UserName, 
                        x.Email, 
                        x.CreationDate, 
                        x.IsLocked));
            
            return new InterviewersView(
                input.Page, input.PageSize, queryResult.Count(), items, input.Id);
        }

        #endregion
    }
}