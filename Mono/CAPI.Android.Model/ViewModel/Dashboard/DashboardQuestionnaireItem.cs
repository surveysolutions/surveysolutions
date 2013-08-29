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
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    /// <summary>
    /// The dashboard questionnaire item.
    /// </summary>
    public class DashboardQuestionnaireItem 
    {
        #region Constructors and Destructors

        
        public DashboardQuestionnaireItem(Guid publicKey, Guid surveyKey, SurveyStatus status, IList<FeaturedItem> properties, string title)
        {
            this.PublicKey = publicKey;
            this.status = status;
            this.Properties = properties;
            this.SurveyKey = surveyKey;
            this.Title = title;
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

        public string Title { get; private set; }

        public Guid SurveyKey { get; private set; }
        /// <summary>
        /// Gets the status.
        /// </summary>
        public SurveyStatus Status
        {
            get { return status; }
        }

        private SurveyStatus status;
        
     

        #endregion
    }
}