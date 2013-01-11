// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScreenedCompleteQuestionnaireDocument.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the CompleteQuestionnaireStoreDocument type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Documents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Utility;
    using Main.DenormalizerStorage;

    /// <summary>
    /// The complete questionnaire store document.
    /// </summary>
    [SmartDenormalizer]
    public class ScreenedCompleteQuestionnaireDocument
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenedCompleteQuestionnaireDocument"/> class.
        /// </summary>
        public ScreenedCompleteQuestionnaireDocument()
        {
            this.CreationDate = DateTime.Now;
            this.StatusChangeComments = new List<ChangeStatusDocument>();
            this.Screens = new Dictionary<Guid, SurveyScreen>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets PublicKey.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the close date.
        /// </summary>
        public DateTime? CloseDate { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets LastEntryDate.
        /// </summary>
        public DateTime LastEntryDate { get; set; }
        
        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        public UserLight Creator { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets Status Change Comments.
        /// </summary>
        public List<ChangeStatusDocument> StatusChangeComments { get; set; }

        /// <summary>
        /// Gets or sets the status change comment.
        /// </summary>
        public string StatusChangeComment { get; set; }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets Screens.
        /// </summary>
        public Dictionary<Guid, SurveyScreen> Screens { get; set; }

        #endregion

        /// <summary>
        /// The op_ explicit.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Raises InvalidOperationException.
        /// </exception>
        public static explicit operator ScreenedCompleteQuestionnaireDocument(QuestionnaireDocument doc)
        {
            var complete = (CompleteQuestionnaireDocument)doc;
            return (ScreenedCompleteQuestionnaireDocument)complete;
        }

        /// <summary>
        /// The op_ explicit.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <returns>
        /// </returns>
        public static explicit operator ScreenedCompleteQuestionnaireDocument(CompleteQuestionnaireDocument doc)
        {
            var result = new ScreenedCompleteQuestionnaireDocument
                {
                    PublicKey = doc.PublicKey, 
                    TemplateId = doc.TemplateId, 
                    Title = doc.Title, 
                    CreationDate = doc.CreationDate, 
                    LastEntryDate = doc.LastEntryDate, 
                    Status = doc.Status, 
                    Creator = doc.Creator, 
                    Responsible = doc.Responsible, 
                    Description = doc.Description 
               };

            result.StatusChangeComments.Add(new ChangeStatusDocument
                    {
                        Status = doc.Status, 
                        Responsible = doc.Creator,
                        ChangeDate = doc.CreationDate // not sure it's correct
                    });

            var stack = new Stack<NodeWithLevel>();
            stack.Push(new NodeWithLevel(doc, 0));
            while (stack.Count > 0)
            {
                NodeWithLevel node = stack.Pop();

                var screen = new SurveyScreen(doc, node);
                result.Screens.Add(screen.Key, screen);

                var subGroups =
                    node.Group.Children.OfType<ICompleteGroup>().GroupBy(g => g.PublicKey).ToDictionary(
                        g => g.Key, g => g.ToList());

                foreach (var kvp in subGroups)
                {
                    var propagationTemplate = kvp.Value.FirstOrDefault(g => g.Propagated != Propagate.None && g.PropagationPublicKey == null);
                    ICompleteGroup group = null;

                    if (propagationTemplate != null)
                    {
                        // creating fake group
                        group = new CompleteGroup(propagationTemplate.Title)
                            {
                                PublicKey = propagationTemplate.PublicKey,
                                Propagated = propagationTemplate.Propagated,
                                Description = propagationTemplate.Description 
                            };

                        foreach (var propagatedGroup in kvp.Value.Where(g => g.PropagationPublicKey.HasValue))
                        {
                            group.Children.Add(propagatedGroup);
                        }
                    }
                    else
                    {
                        group = kvp.Value.FirstOrDefault();
                    }

                    stack.Push(new NodeWithLevel(group, node.Level + 1));
                }
            }

            return result;
        }
   }
}