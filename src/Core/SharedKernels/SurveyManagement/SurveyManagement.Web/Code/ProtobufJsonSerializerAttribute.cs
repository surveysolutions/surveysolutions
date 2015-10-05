using System;
using System.Web.Http.Controllers;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public class ProtobufJsonSerializerAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            //controllerSettings.ParameterBindingRules.Insert(0,
            //    parameterDescriptor => new FromUriOrBodyParameterBinding(parameterDescriptor));

            controllerSettings.Formatters.Insert(0, new ProtobufJsonFormatter());
        }
    }
}