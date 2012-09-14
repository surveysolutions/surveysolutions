// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewerStatisticsFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The interviewers factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Interviewer
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities;
    using Main.Core.Utility;

    using RavenQuestionnaire.Core.Denormalizers;

    /// <summary>
    /// Interviewer statistics factory
    /// </summary>
    public class InterviewerStatisticsFactory : IViewFactory<InterviewerStatisticsInputModel, InterviewerStatisticsView>
    {
        #region Fields

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<InterviewerStatisticsItem> stat;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerStatisticsFactory"/> class. 
        /// Initializes a new instance of the <see cref="InterviewersViewFactory"/> class.
        /// </summary>
        /// <param name="users">
        /// The users.
        /// </param>
        public InterviewerStatisticsFactory(IDenormalizerStorage<InterviewerStatisticsItem> users)
        {
            this.stat = users;
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
        public InterviewerStatisticsView Load(InterviewerStatisticsInputModel input)
        {
            var s = this.stat.GetByGuid(input.UserId);
            if (s == null || s.StatusesByCQ.Count == 0)
                return new InterviewerStatisticsView(
                    input.UserId,
                    input.UserName, //fix this
                    input.Order,
                    new List<InterviewerStatisticsViewItem>(),
                    input.Page,
                    input.PageSize,
                    0);

            IQueryable<InterviewerStatisticsViewItem> items = s.GetTableRows().AsQueryable();
            if (input.Orders.Count > 0)
            {
                items = input.Orders[0].Direction == OrderDirection.Asc
                            ? items.OrderBy(input.Orders[0].Field)
                            : items.OrderByDescending(input.Orders[0].Field);
            }

            items = items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
            return new InterviewerStatisticsView(
                s.Id, s.Name, input.Order, items.ToList(), input.Page, input.PageSize, items.Count());
        }

        #endregion
    }
}