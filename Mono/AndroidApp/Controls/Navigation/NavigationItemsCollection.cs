// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NavigationItemsCollection.cs" company="">
//   
// </copyright>
// <summary>
//   The navigation items collection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AndroidApp.Controls.Navigation
{
    using System;
    using System.Collections.Generic;

    using Android.Content;

    using AndroidApp.Core.Model.Authorization;

    /// <summary>
    /// The navigation items collection.
    /// </summary>
    public class NavigationItemsCollection : List<NavigationItem>
    {
        #region Fields

        /// <summary>
        /// The selected item index.
        /// </summary>
        public int? SelectedItemIndex;

        /// <summary>
        /// The context.
        /// </summary>
        private readonly Context context;

        /// <summary>
        /// The membership.
        /// </summary>
        private readonly IAuthentication membership;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationItemsCollection"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public NavigationItemsCollection(Context context)
        {
            this.context = context;

            this.membership = CapiApplication.Membership;

            if (this.membership.IsLoggedIn)
            {
                this.Add(new NavigationItem(this.Dashboard, "Dashboard"));
                if (context is DashboardActivity)
                {
                    this.SelectedItemIndex = 0;
                }

                this.Add(new NavigationItem(this.Synchronization, "Synchronization"));
                if (context is SynchronizationActivity)
                {
                    this.SelectedItemIndex = 1;
                }

                this.Add(new NavigationItem(this.Settings, "Settings"));
                if (context is SettingsActivity)
                {
                    this.SelectedItemIndex = 2;
                }

                this.Add(new NavigationItem(this.LogOff, "LogOff"));
            }
            else
            {
                this.Add(new NavigationItem(this.Login, "LogIn"));
                if (context is LoginActivity)
                {
                    this.SelectedItemIndex = 0;
                }

                this.Add(new NavigationItem(this.Synchronization, "Synchronization"));
                if (context is SynchronizationActivity)
                {
                    this.SelectedItemIndex = 1;
                }

                this.Add(new NavigationItem(this.Settings, "Settings"));
                if (context is SettingsActivity)
                {
                    this.SelectedItemIndex = 2;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The dashboard.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected bool Dashboard(object sender, EventArgs e)
        {
            /*if (this.context is DashboardActivity || this.context is LoginActivity)
            {
                return true;
            }

            this.context.StartActivity(
                this.membership.IsLoggedIn ? 
                typeof(DashboardActivity) : 
                typeof(LoginActivity));
            */
            this.context.StartActivity(typeof(SynchronizationActivity));
            return true;
        }

        /// <summary>
        /// The log off.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected bool LogOff(object sender, EventArgs e)
        {
            this.membership.LogOff();
            this.context.StartActivity(typeof(LoginActivity));
            return true;
        }

        /// <summary>
        /// The synchronization.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected bool Synchronization(object sender, EventArgs e)
        {
            this.context.StartActivity(typeof(SynchronizationActivity));

            return true;
        }

        /// <summary>
        /// The login.
        /// </summary>
        /// <param name="arg1">
        /// The arg 1.
        /// </param>
        /// <param name="arg2">
        /// The arg 2.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool Login(object arg1, EventArgs arg2)
        {
            this.context.StartActivity(typeof(LoginActivity));
            return true;
        }

        /// <summary>
        /// The settings.
        /// </summary>
        /// <param name="arg1">
        /// The arg 1.
        /// </param>
        /// <param name="arg2">
        /// The arg 2.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool Settings(object arg1, EventArgs arg2)
        {
            this.context.StartActivity(typeof(SettingsActivity));
            return true;
        }

        #endregion
    }
}