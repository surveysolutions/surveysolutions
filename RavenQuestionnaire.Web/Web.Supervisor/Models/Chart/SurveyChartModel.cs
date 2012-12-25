// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyChartModel.cs" company="">
//   
// </copyright>
// <summary>
//   The interviewer chart model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Models
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    using Core.Supervisor.Views.Index;
    using Core.Supervisor.Views.Summary;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The interviewer chart model.
    /// </summary>
    public class SurveyChartModel : ChartModelBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerChartModel"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public SurveyChartModel(SummaryView model) 
            : base()
        {
            var piePreData = this.GetPiePreData(model);

            this.CalcPieData(piePreData);

            this.BarData = new List<object>
                {
                    new
                        {
                            name = "Total",
                            data = model.Items.Select(t => t.Total).ToArray(),
                            stack = "total",
                            color = this.statusColors["Total"]
                        },
                    new
                        {
                            name = SurveyStatus.Initial.Name,
                            data = model.Items.Select(t => t.Initial).ToArray(),
                            stack = "parts",
                            color = this.statusColors[SurveyStatus.Initial.Name]
                        },
                    new
                        {
                            name = SurveyStatus.Redo.Name,
                            data = model.Items.Select(t => t.Redo).ToArray(),
                            stack = "parts",
                            color = this.statusColors[SurveyStatus.Redo.Name]
                        },
                    new
                        {
                            name = SurveyStatus.Error.Name,
                            data = model.Items.Select(t => t.Error).ToArray(),
                            stack = "parts",
                            color = this.statusColors[SurveyStatus.Error.Name]
                        },
                    new
                        {
                            name = SurveyStatus.Complete.Name,
                            data = model.Items.Select(t => t.Completed).ToArray(),
                            stack = "parts",
                            color = this.statusColors[SurveyStatus.Complete.Name]
                        },
                    new
                        {
                            name = SurveyStatus.Approve.Name,
                            data = model.Items.Select(t => t.Approved).ToArray(),
                            stack = "parts",
                            color = this.statusColors[SurveyStatus.Approve.Name]
                        }
                };
            this.ScatterData =
                new List<object>(
                    model.Items.Select(
                        (t, index) =>
                        new
                            {
                                name = t.User.Name,
                                color = GetColor(index),
                                data = new[] { new[] { t.Initial + t.Redo, t.Completed + t.Approved } }
                            }).ToArray());
        }

        /// <summary>
        /// Get pie pre data array 
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// Array of PieData
        /// </returns>
        private IEnumerable<PieData> GetPiePreData(SummaryView model)
        {
            return model.Items.Select(
                    (t, index) =>
                    new PieData
                        {
                            y = t.Total,
                            color = this.GetColor(index),
                            name = t.User.Name,
                            title = t.User.Name,
                            categories = new[]
                            {
                                SurveyStatus.Initial.Name, 
                                SurveyStatus.Redo.Name, 
                                SurveyStatus.Complete.Name,
                                SurveyStatus.Approve.Name, 
                                SurveyStatus.Error.Name
                            },
                            data = new[]
                                {
                                    t.Initial, 
                                    t.Redo, 
                                    t.Completed, 
                                    t.Approved, 
                                    t.Error
                                },
                        }).ToArray();
        }

        #endregion
    }
}