﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewerStatisticsFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The interviewers factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;
using RavenQuestionnaire.Core.Views.Survey;

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
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerStatisticsFactory"/> class. 
        /// Initializes a new instance of the <see cref="InterviewersViewFactory"/> class.
        /// </summary>
        /// <param name="users">
        /// The users.
        /// </param>
        public InterviewerStatisticsFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession, IDenormalizerStorage<UserDocument> users)
        {
            this.documentItemSession = documentItemSession;
            this.users = users;
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
            var user = this.users.GetByGuid(input.UserId);
            if (user == null)
                return null;
            var questionnairesGroupedByTemplate =
                BuildItems(
                    this.documentItemSession.Query().Where(q => q.Responsible.Id == input.UserId).GroupBy(
                        x => x.TemplateId)).AsQueryable();

            var retval = new InterviewerStatisticsView(input.UserId, user.UserName, input.Order,
                                                       new List<InterviewerStatisticsViewItem>(), input.Page,
                                                       input.PageSize, questionnairesGroupedByTemplate.Count());
            if (input.Orders.Count > 0)
            {
                questionnairesGroupedByTemplate = input.Orders[0].Direction == OrderDirection.Asc
                                                      ? questionnairesGroupedByTemplate.OrderBy(input.Orders[0].Field)
                                                      : questionnairesGroupedByTemplate.OrderByDescending(
                                                          input.Orders[0].Field);
            }

            retval.Items =
                questionnairesGroupedByTemplate.Skip((input.Page - 1)*input.PageSize).Take(input.PageSize).ToList();
            return retval;
        }

        protected IEnumerable<InterviewerStatisticsViewItem> BuildItems(IQueryable<IGrouping<Guid, CompleteQuestionnaireBrowseItem>> grouped)
        {
            foreach (var templateGroup in grouped)
            {
                var first = templateGroup.FirstOrDefault();
                yield
                    return new InterviewerStatisticsViewItem(first.Responsible.Id, first.Responsible.Name,
                                                             first.QuestionnaireTitle,
                                                             templateGroup.Key,
                                                             templateGroup.Count(
                                                                 q =>
                                                                 q.Status.PublicId ==
                                                                 SurveyStatus.Initial.PublicId),
                                                             templateGroup.Count(
                                                                 q =>
                                                                 q.Status.PublicId == SurveyStatus.Error.PublicId),
                                                             templateGroup.Count(
                                                                 q =>
                                                                 q.Status.PublicId ==
                                                                 SurveyStatus.Complete.PublicId),
                                                             templateGroup.Count(
                                                                 q =>
                                                                 q.Status.PublicId ==
                                                                 SurveyStatus.Approve.PublicId),
                                                                 templateGroup.Count(
                                                                 q =>
                                                                 q.Status.PublicId ==
                                                                 SurveyStatus.Redo.PublicId)
                        );
            }
        }

        #endregion
    }
}