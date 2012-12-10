// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewerChartModel.cs" company="">
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

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The interviewer chart model.
    /// </summary>
    public class InterviewerChartModel
    {
        #region Constructors and Destructors



        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerChartModel"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public InterviewerChartModel(IndexView model)
        {
            var piePreData =
                model.Items.Select(
                    (t, index) =>
                    new
                        {
                            y = t.Total,
                            color = this.colors[index],
                            name = t.Title,
                            title = t.Title.Acronim(),
                            categories = new[] { "Unassign", "Initial", "Redo", "Complete", "Approve", "Error" },
                            data = new[] { t.Unassign, t.Initial, t.Redo, t.Complete, t.Approve, t.Error },
                        }).ToArray();

            this.BrowserData = new List<object>();
            this.VersionsData = new List<object>();
            foreach (var data in piePreData)
            {
                this.BrowserData.Add(
                    new
                        {
                            category = data.name,
                            name = data.name,
                            title = data.title,
                            y = data.y,
                            color = data.color
                        });

                for (var j = 0; j < data.data.Length; j++)
                {
                    var brightness = (float)(0.4 - ((float)j / data.data.Length) / 2.5f);
                    Color c = ColorTranslator.FromHtml(data.color);
                    var newColor = ControlPaint.Light(c, brightness);
                    this.VersionsData.Add(new
                        {
                            category = data.name,
                            name = data.categories[j],
                            y = data.data[j],
                            color = ColorTranslator.ToHtml(newColor)
                        });
                }
            }
           
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
                            name = SurveyStatus.Unassign.Name,
                            data = model.Items.Select(t => t.Unassign).ToArray(),
                            stack = "parts",
                            color = this.statusColors[SurveyStatus.Unassign.Name]
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
                            data = model.Items.Select(t => t.Complete).ToArray(),
                            stack = "parts",
                            color = this.statusColors[SurveyStatus.Complete.Name]
                        },
                    new
                        {
                            name = SurveyStatus.Approve.Name,
                            data = model.Items.Select(t => t.Approve).ToArray(),
                            stack = "parts",
                            color = this.statusColors[SurveyStatus.Approve.Name]
                        }
                };
            this.ScatterData =
                model.Items.Select(
                    (t, index) =>
                    new
                        {
                            name = t.Title,
                            color = this.colors[index],
                            data = new[] { new[] { t.Initial + t.Redo, t.Complete + t.Approve } }
                        }).ToArray();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets barData.
        /// </summary>
        public List<object> BarData { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets ScatterData.
        /// </summary>
        public object[] ScatterData { get; set; }

        /// <summary>
        /// The status colors
        /// </summary>
        private Dictionary<string, string> statusColors = new Dictionary<string, string>()
            {
                { SurveyStatus.Unassign.Name, "#c3325f" },
                { SurveyStatus.Initial.Name, "#049cdb" },
                { SurveyStatus.Redo.Name, "#f89406" },
                { SurveyStatus.Error.Name, "#c83025" /*"#6602cc" */ },
                { SurveyStatus.Complete.Name, "#018080" },
                { SurveyStatus.Approve.Name, "#46a546" },
                { "Total", "#333333" },
            };

        /// <summary>
        /// The colors
        /// </summary>
        private string[] colors = new[] { "#049cdb", "#46a546", "#c3325f", "#f89406", "#673301", "#ffc40d", "#400180", "#018080", "#9d261d" };

        public List<object> VersionsData { get; set; }

        public List<object> BrowserData { get; set; }
    }

    /// <summary>
    /// String extensions
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Make acronim from sentence
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The acronim
        /// </returns>
        public static string Acronim(this string str)
        {
            return string.Join(
                string.Empty, str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]));
        }
    }
}