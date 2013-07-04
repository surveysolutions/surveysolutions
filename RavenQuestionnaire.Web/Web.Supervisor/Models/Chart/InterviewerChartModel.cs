namespace Web.Supervisor.Models.Chart
{
    using System.Collections.Generic;
    using System.Linq;

    using Core.Supervisor.Views.Index;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The interviewer chart model.
    /// </summary>
    public class InterviewerChartModel : ChartModelBase
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
            IEnumerable<PieData> piePreData = this.GetPiePreData(model);

            this.CalcPieData(piePreData);

            this.BarData = new List<object>
                {
                    new
                        {
                            name = "Total", 
                            data = model.Items.Select(t => t.Total).ToArray(), 
                            stack = "total", 
                            color = this.StatusColors["Total"]
                        }, 
                    new
                        {
                            name = SurveyStatus.Unassign.Name, 
                            data = model.Items.Select(t => t.Unassigned).ToArray(), 
                            stack = "parts", 
                            color = this.StatusColors[SurveyStatus.Unassign.Name]
                        }, 
                    new
                        {
                            name = SurveyStatus.Initial.Name, 
                            data = model.Items.Select(t => t.Initial).ToArray(), 
                            stack = "parts", 
                            color = this.StatusColors[SurveyStatus.Initial.Name]
                        }, 
                    new
                        {
                            name = SurveyStatus.Redo.Name, 
                            data = model.Items.Select(t => t.Redo).ToArray(), 
                            stack = "parts", 
                            color = this.StatusColors[SurveyStatus.Redo.Name]
                        }, 
                    new
                        {
                            name = SurveyStatus.Error.Name, 
                            data = model.Items.Select(t => t.Error).ToArray(), 
                            stack = "parts", 
                            color = this.StatusColors[SurveyStatus.Error.Name]
                        }, 
                    new
                        {
                            name = SurveyStatus.Complete.Name, 
                            data = model.Items.Select(t => t.Completed).ToArray(), 
                            stack = "parts", 
                            color = this.StatusColors[SurveyStatus.Complete.Name]
                        }, 
                    new
                        {
                            name = SurveyStatus.Approve.Name, 
                            data = model.Items.Select(t => t.Approved).ToArray(), 
                            stack = "parts", 
                            color = this.StatusColors[SurveyStatus.Approve.Name]
                        }
                };
            this.ScatterData =
                new List<object>(
                    model.Items.Select(
                        (t, index) =>
                        new
                            {
                                name = t.Title, 
                                color = GetColor(index), 
                                data = new[] { new[] { t.Initial + t.Redo, t.Completed + t.Approved } }
                            }).ToArray());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get pie pre data array 
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// Array of PieData
        /// </returns>
        private IEnumerable<PieData> GetPiePreData(IndexView model)
        {
            PieData[] piePreData =
                model.Items.Select(
                    (t, index) =>
                    new PieData
                        {
                            y = t.Total, 
                            color = GetColor(index), 
                            name = t.Title, 
                            title = t.Title.Acronim(), 
                            categories =
                                new[]
                                    {
                                        SurveyStatus.Unassign.Name, SurveyStatus.Initial.Name, SurveyStatus.Redo.Name, 
                                        SurveyStatus.Complete.Name, SurveyStatus.Approve.Name, SurveyStatus.Error.Name
                                    }, 
                            data = new[] { t.Unassigned, t.Initial, t.Redo, t.Completed, t.Approved, t.Error }, 
                        }).ToArray();
            return piePreData;
        }

        #endregion
    }
}