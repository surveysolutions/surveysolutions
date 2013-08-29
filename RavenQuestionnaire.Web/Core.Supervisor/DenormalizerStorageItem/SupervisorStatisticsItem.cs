using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Core.Supervisor.DenormalizerStorageItem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SupervisorStatisticsItem : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SupervisorStatisticsItem"/> class.
        /// </summary>
        public SupervisorStatisticsItem()
        {
            this.Surveys = new List<Guid>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupervisorStatisticsItem"/> class.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        public SupervisorStatisticsItem(CompleteQuestionnaireDocument document)
            : this()
        {
            this.User = document.Responsible ?? new UserLight(Guid.Empty, string.Empty);
            this.Template = new TemplateLight(document.TemplateId, document.Title);
            this.Status = document.Status;
        }

        /// <summary>
        /// Gets or sets User.
        /// </summary>
        public UserLight User { get; set; }

        /// <summary>
        /// Gets or sets Template.
        /// </summary>
        public TemplateLight Template { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets Surveys.
        /// </summary>
        public List<Guid> Surveys { get; set; }
    }
}
