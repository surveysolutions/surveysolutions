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
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<UserDocument> users;

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
            IQueryableDenormalizerStorage<UserDocument> users,
            IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession)
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
            int count =
                this.users.Query().Where(u => u.Supervisor != null).Count(u => u.Supervisor.Id == input.Supervisor.Id);
            if (count == 0)
            {
                return new InterviewersView(
                    input.Page, 
                    input.PageSize, 
                    count, 
                    new InterviewersItem[0], 
                    input.Supervisor.Id, 
                    input.Supervisor.Name);
            }

            IQueryable<UserDocument> query =
                this.users.Query().Where(u => u.Supervisor != null).Where(u => u.Supervisor.Id == input.Supervisor.Id);
            IQueryable<InterviewersItem> items =
                query.Select(
                    x =>
                    new InterviewersItem(
                        x.PublicKey, 
                        x.UserName, 
                        x.Email, 
                        x.CreationDate, 
                        x.IsLocked));
            if (input.Orders.Count > 0)
            {
                items = input.Orders[0].Direction == OrderDirection.Asc
                            ? items.OrderBy(input.Orders[0].Field)
                            : items.OrderByDescending(input.Orders[0].Field);
            }

            items = items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
            return new InterviewersView(
                input.Page, input.PageSize, count, items, input.Supervisor.Id, input.Supervisor.Name);
        }

        #endregion
    }
}