// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusChartModel.cs" company="The World Bank">
//   Status Chart Model
// </copyright>
// <summary>
//   Chart model for status page
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace Web.Supervisor.Models.Chart
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    using Core.Supervisor.Views.Status;

    /// <summary>
    /// Chart model for status page
    /// </summary>
    public class StatusChartModel : ChartModelBase
    {
        public StatusChartModel(StatusView view)
            : base()
        {
            this.BarData.Add(
                new
                    {
                        name = "Total",
                        data = view.Items.Select(t => t.Items.Sum(kvp => kvp.Value)).ToArray(),
                        stack = "total"
                    });
            foreach (var header in view.Headers)
            {
                this.BarData.Add(
                    new
                        {
                            name = header.Title.Acronim(),
                            title = header.Title,
                            data = view.Items.Select(t => t.Items[header]).ToArray(),
                            stack = "parts"
                        });
            }

            var piePreData = this.GetPiePreData(view);

            this.CalcPieData(piePreData);
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
        private IEnumerable<PieData> GetPiePreData(StatusView model)
        {
            var piePreData =
                model.Items.Select(
                    (t, index) =>
                    new PieData
                        {
                            y = t.Items.Values.Sum(),
                            color = this.GetColor(index),
                            name = t.User.Name,
                            title = t.User.Name,
                            categories = t.Items.Keys.Select(k => k.Title.Acronim()).ToArray(),
                            data = t.Items.Values.ToArray()
                        }).ToArray();
            return piePreData;
        }
    }
}