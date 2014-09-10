using System.Web.Mvc;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding
{
    /// <summary>
    /// The model binder.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public abstract class ModelBinder<T> : IModelBinder<T>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The bind model.
        /// </summary>
        /// <param name="controllerContext">
        /// The controller context.
        /// </param>
        /// <param name="bindingContext">
        /// The binding context.
        /// </param>
        /// <returns>
        /// The T.
        /// </returns>
        public abstract T BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext);

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// The bind model.
        /// </summary>
        /// <param name="controllerContext">
        /// The controller context.
        /// </param>
        /// <param name="bindingContext">
        /// The binding context.
        /// </param>
        /// <returns>
        /// The System.Object.
        /// </returns>
        object IModelBinder.BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            return this.BindModel(controllerContext, bindingContext);
        }

        #endregion
    }
}