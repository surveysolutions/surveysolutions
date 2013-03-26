// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestsContext.cs" company="">
//   
// </copyright>
// <summary>
//   The tests context.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AndroidMain.Core.Tests.AndroidSpecificTests
{
    using System;

    using Android.App;
    using Android.Content;
    using Android.Runtime;

    /// <summary>
    /// The tests context.
    /// </summary>
    [Application]
    public class TestsContext : Application
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsContext"/> class.
        /// </summary>
        public TestsContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsContext"/> class.
        /// </summary>
        /// <param name="javaReference">
        /// The java reference.
        /// </param>
        /// <param name="transfer">
        /// The transfer.
        /// </param>
        protected TestsContext(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the current context.
        /// </summary>
        public static Context CurrentContext { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on create.
        /// </summary>
        public override void OnCreate()
        {
            base.OnCreate();

            CurrentContext = this;
        }

        #endregion
    }
}