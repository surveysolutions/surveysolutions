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
                    // od.Behaviors[i] = new JsonNewSerializerOperationBehavior1(od);
                    //od.Behaviors[i] = new JsonNewSerializerOperationBehavior(od);
                    
                    /*
                    if (od.Behaviors[i].GetType().Name == "DataContractSerializerOperationBehavior")
                    {
                        od.Behaviors[i] = new JsonNewSerializerOperationBehavior1(od);
                    }*/
                    
                    
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
    }

    public class MyClientFormatter : IClientMessageFormatter
    {
            OperationDescription operationDescription;
            JsonNewSerializerOperationBehavior1 serializerOperationBehavior;
            List<Type> knownTypes;

            public MyClientFormatter(OperationDescription operationDescription, JsonNewSerializerOperationBehavior1 serializerOperationBehavior)
            {
                this.operationDescription = operationDescription;
                this.serializerOperationBehavior = serializerOperationBehavior;
                this.knownTypes = new List<Type>();
                foreach (Type type in operationDescription.KnownTypes)
                {
                    this.knownTypes.Add(type);
                }
            }

        #region Implementation of IClientMessageFormatter

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            Message result = Message.CreateMessage(
            messageVersion,
            this.operationDescription.Messages[0].Action,
            new MyOperationBodyWriter(this.operationDescription, this.serializerOperationBehavior, this.knownTypes, parameters));
            return result;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            MessageDescription incomingMessage = this.operationDescription.Messages[1];
            MessagePartDescription returnPart = incomingMessage.Body.ReturnValue;
            XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents();
            if (incomingMessage.Body.WrapperName != null)
            {
                bool isEmptyElement = bodyReader.IsEmptyElement;
                bodyReader.ReadStartElement(incomingMessage.Body.WrapperName, incomingMessage.Body.WrapperNamespace);
                if (isEmptyElement) return null;
            }

            XmlObjectSerializer returnValueSerializer = this.serializerOperationBehavior.CreateSerializer(
                returnPart.Type, returnPart.Name, returnPart.Namespace, this.knownTypes);
            object result = returnValueSerializer.ReadObject(bodyReader, false);

            if (incomingMessage.Body.WrapperName != null)
            {
                bodyReader.ReadEndElement();
            }

            return result;
        }


        class MyOperationBodyWriter : BodyWriter
        {
            OperationDescription operationDescription;
            JsonNewSerializerOperationBehavior1 serializerOperationBehavior;
            object[] operationParameters;
            IList<Type> knownTypes;

            public MyOperationBodyWriter(OperationDescription operationDescription, JsonNewSerializerOperationBehavior1 serializerOperationBehavior, IList<Type> knownTypes, object[] operationParameters)
                : base(true)
            {
                this.operationDescription = operationDescription;
                this.serializerOperationBehavior = serializerOperationBehavior;
                this.knownTypes = knownTypes;
                this.operationParameters = operationParameters;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                MessageDescription outgoingMessage = this.operationDescription.Messages[0];
                if (outgoingMessage.Body.WrapperName != null)
                {
                    writer.WriteStartElement(outgoingMessage.Body.WrapperName, outgoingMessage.Body.WrapperNamespace);
                }

                foreach (var bodyPart in outgoingMessage.Body.Parts)
                {
                    XmlObjectSerializer serializer = this.serializerOperationBehavior.CreateSerializer(bodyPart.Type, bodyPart.Name, bodyPart.Namespace, this.knownTypes);
                    serializer.WriteObject(writer, this.operationParameters[bodyPart.Index]);
                }

                if (outgoingMessage.Body.WrapperName != null)
                {
                    writer.WriteEndElement();
                }
            }
        }

        #endregion
    }

            /// <summary>
        /// The json new serializer operation behavior.
        /// </summary>
        public class JsonNewSerializerOperationBehavior : DataContractSerializerOperationBehavior
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
                var _knownTypes = new List<Type>
                    {
                        typeof(ListOfAggregateRootsForImportMessage),
                        typeof(EventSyncMessage),
                        typeof(ImportSynchronizationMessage)
                    };

                foreach (var knownType in _knownTypes)
                {
                    if (!knownTypes.Contains(knownType)) knownTypes.Add(knownType);
                }

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
                var _knownTypes = new List<Type>
                    {
                        typeof(ListOfAggregateRootsForImportMessage),
                        typeof(EventSyncMessage),
                        typeof(ImportSynchronizationMessage)
                    };

                foreach (var knownType in _knownTypes)
                {
                    if (!knownTypes.Contains(knownType)) knownTypes.Add(knownType);
                }

                return new JsonSerrializer(type, base.CreateSerializer(type, name, ns, knownTypes));
            }

        }

        /// <summary>
        /// The json new serializer operation behavior.
        /// </summary>
        public class JsonNewSerializerOperationBehavior1 : IOperationBehavior
        {
            private OperationDescription operationDec;



            /// <summary>
            /// Initializes a new instance of the <see cref="JsonNewSerializerOperationBehavior"/> class.
            /// </summary>
            /// <param name="operation">
            /// The operation.
            /// </param>
            public JsonNewSerializerOperationBehavior1(OperationDescription operation)
            {
                this.operationDec = operation;
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
            public XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
            {
                var _knownTypes = new List<Type>
                    {
                        typeof(ListOfAggregateRootsForImportMessage),
                        typeof(EventSyncMessage),
                        typeof(ImportSynchronizationMessage)
                    };

               /* foreach (var knownType in _knownTypes)
                {
                    if (!knownTypes.Contains(knownType)) knownTypes.Add(knownType);
                }*/

                return new JsonSerrializer(type, new DataContractSerializer(type, name, ns, knownTypes));
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
            public XmlObjectSerializer CreateSerializer(
                Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
            {
                var _knownTypes = new List<Type>
                    {
                        typeof(ListOfAggregateRootsForImportMessage),
                        typeof(EventSyncMessage),
                        typeof(ImportSynchronizationMessage)
                    };

                /*foreach (var knownType in _knownTypes)
                {
                    if (!knownTypes.Contains(knownType)) knownTypes.Add(knownType);
                }*/

                return new JsonSerrializer(type, new DataContractSerializer(type, name, ns, knownTypes));
            }

            #endregion

            #region Implementation of IOperationBehavior

            public void Validate(OperationDescription operationDescription)
            {

            }

            public void ApplyDispatchBehavior(
                OperationDescription operationDescription, DispatchOperation dispatchOperation)
            {

            }

            public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
            {
                clientOperation.Formatter = new MyClientFormatter(operationDescription, this);
                clientOperation.SerializeRequest = !IsUntypedMessage(operationDescription.Messages[0]);
                clientOperation.DeserializeReply = operationDescription.Messages.Count > 1
                                                   && !IsUntypedMessage(operationDescription.Messages[1]);
            }

            public void AddBindingParameters(
                OperationDescription operationDescription, BindingParameterCollection bindingParameters)
            {

            }


            private bool IsUntypedMessage(MessageDescription md)
            {
                return (md.Body.ReturnValue != null && md.Body.Parts.Count == 0
                        && md.Body.ReturnValue.Type == typeof(Message))
                       ||
                       (md.Body.ReturnValue == null && md.Body.Parts.Count == 1
                        && md.Body.Parts[0].Type == typeof(Message));
            }


            #endregion
        }
    
}