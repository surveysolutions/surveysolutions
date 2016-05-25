﻿using System;
using System.Web.Mvc;
using Ninject;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding
{
    /// <summary>
    /// The generic binder resolver.
    /// </summary>
    public class GenericBinderResolver : DefaultModelBinder
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
        private readonly IKernel _kernel;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericBinderResolver"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        public GenericBinderResolver(IKernel kernel)
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

            var binder = this._kernel.TryGet(genericBinderType) as IModelBinder;
            if (null != binder)
            {
                return binder.BindModel(controllerContext, bindingContext);
            }

            return base.BindModel(controllerContext, bindingContext);
        }

        #endregion
    }
}