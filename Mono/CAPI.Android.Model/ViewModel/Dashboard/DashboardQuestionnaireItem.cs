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
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Commands;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    /// <summary>
    /// The dashboard questionnaire item.
    /// </summary>
    public class DashboardQuestionnaireItem : MvxViewModel
    {
        #region Constructors and Destructors

        
        public DashboardQuestionnaireItem(Guid publicKey, Guid surveyKey, SurveyStatus status, IList<FeaturedItem> properties)
        {
            this.PublicKey = publicKey;
            this.status = status;
            this.Properties = properties;
            this.SurveyKey = surveyKey;
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
        public Guid SurveyKey { get; private set; }
        /// <summary>
        /// Gets the status.
        /// </summary>
        public SurveyStatus Status
        {
            get { return status; }
        }

        private SurveyStatus status;
        
     

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
        public void SetStatus(SurveyStatus newStatus)
        {
            if (newStatus == this.status)
                return;
            this.status = newStatus;
            this.RaisePropertyChanged("Status");
        }
    }
}