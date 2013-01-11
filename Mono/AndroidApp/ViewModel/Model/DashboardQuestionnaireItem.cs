// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DashboardQuestionnaireItem.cs" company="">
//   
// </copyright>
// <summary>
//   The dashboard questionnaire item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AndroidApp.ViewModel.Model
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;

    using AndroidApp.ViewModel.QuestionnaireDetails;

    using Cirrious.MvvmCross.Commands;
    using Cirrious.MvvmCross.ViewModels;

    /// <summary>
    /// The dashboard questionnaire item.
    /// </summary>
    public class DashboardQuestionnaireItem : MvxViewModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardQuestionnaireItem"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="properties">
        /// The properties.
        /// </param>
        public DashboardQuestionnaireItem(Guid publicKey, string status, IList<FeaturedItem> properties)
        {
            this.PublicKey = publicKey;
            this.Status = status;
            this.Properties = properties;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public IList<FeaturedItem> Properties { get; private set; }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        public Guid PublicKey { get; private set; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Gets the view detail command.
        /// </summary>
        public ICommand ViewDetailCommand
        {
            get
            {
                return
                    new MvxRelayCommand(
                        () =>
                        this.RequestNavigate<QuestionnaireScreenViewModel>(
                            new { completeQuestionnaireId = this.PublicKey.ToString() }));
            }
        }

        #endregion
    }
}