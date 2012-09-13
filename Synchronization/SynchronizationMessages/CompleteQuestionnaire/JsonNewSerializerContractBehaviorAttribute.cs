// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonNewSerializerContractBehaviorAttribute.cs" company="">
//   
// </copyright>
// <summary>
//   The json new serializer contract behavior attribute.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SynchronizationMessages.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    using SynchronizationMessages.Synchronization;

    /// <summary>
    /// The json new serializer contract behavior attribute.
    /// </summary>
    public class JsonNewSerializerContractBehaviorAttribute : Attribute, IContractBehavior
    {
        #region Public Methods and Operators

        /// <summary>
        /// The add binding parameters.
        /// </summary>
        /// <param name="contractDescription">
        /// The contract description.
        /// </param>
        /// <param name="endpoint">
        /// The endpoint.
        /// </param>
        /// <param name="bindingParameters">
        /// The binding parameters.
        /// </param>
        public void AddBindingParameters(
            ContractDescription contractDescription, 
            ServiceEndpoint endpoint, 
            BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// The apply client behavior.
        /// </summary>
        /// <param name="contractDescription">
        /// The contract description.
        /// </param>
        /// <param name="endpoint">
        /// The endpoint.
        /// </param>
        /// <param name="clientRuntime">
        /// The client runtime.
        /// </param>
        public void ApplyClientBehavior(
            ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            this.ReplaceSerializerOperationBehavior(contractDescription);
        }

        /// <summary>
        /// The apply dispatch behavior.
        /// </summary>
        /// <param name="contractDescription">
        /// The contract description.
        /// </param>
        /// <param name="endpoint">
        /// The endpoint.
        /// </param>
        /// <param name="dispatchRuntime">
        /// The dispatch runtime.
        /// </param>
        public void ApplyDispatchBehavior(
            ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            this.ReplaceSerializerOperationBehavior(contractDescription);
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="contractDescription">
        /// The contract description.
        /// </param>
        /// <param name="endpoint">
        /// The endpoint.
        /// </param>
        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
            foreach (OperationDescription operation in contractDescription.Operations)
            {
                foreach (MessageDescription message in operation.Messages)
                {
                    this.ValidateMessagePartDescription(message.Body.ReturnValue);
                    foreach (MessagePartDescription part in message.Body.Parts)
                    {
                        this.ValidateMessagePartDescription(part);
                    }

                    foreach (MessageHeaderDescription header in message.Headers)
                    {
                        this.ValidateCustomSerializableType(header.Type);
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The replace serializer operation behavior.
        /// </summary>
        /// <param name="contract">
        /// The contract.
        /// </param>
        private void ReplaceSerializerOperationBehavior(ContractDescription contract)
        {
            foreach (OperationDescription od in contract.Operations)
            {
                for (int i = 0; i < od.Behaviors.Count; i++)
                {
                    var dcsob = od.Behaviors[i] as DataContractSerializerOperationBehavior;
                    if (dcsob != null)
                    {
                        od.Behaviors[i] = new JsonNewSerializerOperationBehavior(od);
                    }
                }
            }
        }

        /// <summary>
        /// The validate custom serializable type.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private void ValidateCustomSerializableType(Type type)
        {
            if (typeof(ICustomSerializable).IsAssignableFrom(type))
            {
                if (!type.IsPublic)
                {
                    throw new InvalidOperationException("Custom serialization is supported in public types only");
                }

                ConstructorInfo defaultConstructor = type.GetConstructor(new Type[0]);
                if (defaultConstructor == null)
                {
                    throw new InvalidOperationException(
                        "Custom serializable types must have a public, parameterless constructor");
                }
            }
        }

        /// <summary>
        /// The validate message part description.
        /// </summary>
        /// <param name="part">
        /// The part.
        /// </param>
        private void ValidateMessagePartDescription(MessagePartDescription part)
        {
            if (part != null)
            {
                this.ValidateCustomSerializableType(part.Type);
            }
        }

        #endregion

        /// <summary>
        /// The json new serializer operation behavior.
        /// </summary>
        private class JsonNewSerializerOperationBehavior : DataContractSerializerOperationBehavior
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="JsonNewSerializerOperationBehavior"/> class.
            /// </summary>
            /// <param name="operation">
            /// The operation.
            /// </param>
            public JsonNewSerializerOperationBehavior(OperationDescription operation)
                : base(operation)
            {
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The create serializer.
            /// </summary>
            /// <param name="type">
            /// The type.
            /// </param>
            /// <param name="name">
            /// The name.
            /// </param>
            /// <param name="ns">
            /// The ns.
            /// </param>
            /// <param name="knownTypes">
            /// The known types.
            /// </param>
            /// <returns>
            /// The System.Runtime.Serialization.XmlObjectSerializer.
            /// </returns>
            public override XmlObjectSerializer CreateSerializer(
                Type type, string name, string ns, IList<Type> knownTypes)
            {
                return new JsonSerrializer(type, base.CreateSerializer(type, name, ns, knownTypes));
            }

            /// <summary>
            /// The create serializer.
            /// </summary>
            /// <param name="type">
            /// The type.
            /// </param>
            /// <param name="name">
            /// The name.
            /// </param>
            /// <param name="ns">
            /// The ns.
            /// </param>
            /// <param name="knownTypes">
            /// The known types.
            /// </param>
            /// <returns>
            /// The System.Runtime.Serialization.XmlObjectSerializer.
            /// </returns>
            public override XmlObjectSerializer CreateSerializer(
                Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
            {
                return new JsonSerrializer(type, base.CreateSerializer(type, name, ns, knownTypes));
            }

            #endregion
        }
    }
}