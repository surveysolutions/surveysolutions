using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Ninject;

namespace Questionnaire.Core.Web.Binding
{

    public class GenericBinderResolver : DefaultModelBinder
    {
        private static readonly Type BinderType = typeof(IModelBinder<>);
        private IKernel _kernel;
        public GenericBinderResolver(IKernel kernel)
        {
            _kernel = kernel;
        }
        public override object BindModel(ControllerContext controllerContext,
                                         ModelBindingContext bindingContext)
        {
            var originalMetadata = bindingContext.ModelMetadata;

            bindingContext.ModelMetadata = new ModelMetadata(
                ModelMetadataProviders.Current,
                originalMetadata.ContainerType,
                () => null,  //Forces model to null
                originalMetadata.ModelType,
                originalMetadata.PropertyName
                );

            Type genericBinderType = BinderType.MakeGenericType(bindingContext.ModelType);

            var binder = _kernel.TryGet(genericBinderType) as IModelBinder;
            if (null != binder) return binder.BindModel(controllerContext, bindingContext);

            return base.BindModel(controllerContext, bindingContext);
        }
    }

    public interface IModelBinder<T> : IModelBinder
    {

    }

    public abstract class ModelBinder<T> : IModelBinder<T>
    {
        public abstract T BindModel(ControllerContext controllerContext,
                                       ModelBindingContext bindingContext);

        object IModelBinder.BindModel(ControllerContext controllerContext,
                                      ModelBindingContext bindingContext)
        {
            return BindModel(controllerContext, bindingContext);
        }
    }
}
