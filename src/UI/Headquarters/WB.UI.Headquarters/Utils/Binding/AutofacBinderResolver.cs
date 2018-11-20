using System;
using System.Web.Mvc;
using Autofac;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding
{
    /// <summary>
    /// The generic binder resolver.
    /// </summary>
    public class AutofacBinderResolver : DefaultModelBinder
    {
        #region Static Fields

        /// <summary>
        /// The binder type.
        /// </summary>
        private static readonly Type BinderType = typeof(IModelBinder<>);

        #endregion

        #region Fields

        /// <summary>
        /// The _kernel.
        /// </summary>
        private readonly IContainer _kernel;

        #endregion

        #region Constructors and Destructors

       public AutofacBinderResolver(IContainer kernel)
        {
            this._kernel = kernel;
        }

        #endregion

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
        /// The System.Object.
        /// </returns>
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ModelMetadata originalMetadata = bindingContext.ModelMetadata;

            bindingContext.ModelMetadata = new ModelMetadata(
                ModelMetadataProviders.Current, 
                originalMetadata.ContainerType, 
                () => null, 
                // Forces model to null
                originalMetadata.ModelType, 
                originalMetadata.PropertyName);

            Type genericBinderType = BinderType.MakeGenericType(bindingContext.ModelType);

            this._kernel.TryResolve(genericBinderType, out var outbinder);

            if (outbinder is IModelBinder binder)
            {
                return binder.BindModel(controllerContext, bindingContext);
            }

            return base.BindModel(controllerContext, bindingContext);
        }

        #endregion
    }
}
