// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DashboardQuestionnaireItem.cs" company="">
//   
// </copyright>
// <summary>
//   The dashboard questionnaire item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows.Input;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Commands;
namespace AndroidApp.Core.Model.ViewModel.Dashboard
{
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
                        RequestNavigate<CompleteQuestionnaireView>(
                            new { publicKey = this.PublicKey.ToString() }));
            }
        }

        #endregion
        public void SetStatus(string newStatus)
        {
            if (newStatus == this.Status)
                return;
            this.Status = newStatus;
            this.RaisePropertyChanged("Status");
        }
    }
}