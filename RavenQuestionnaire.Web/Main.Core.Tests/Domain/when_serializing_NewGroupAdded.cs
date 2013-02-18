// --------------------------------------------------------------------------------------------------------------------
// <copyright file="when_serializing_NewGroupAdded.cs" company="">
//   
// </copyright>
// <summary>
//   The when_serializing_ new group added.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Tests.Domain
{
    using System;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.Questionnaire;

    using Ncqrs.Spec;

    /// <summary>
    /// The when_serializing_ new group added.
    /// </summary>
    public class when_serializing_NewGroupAdded : JsonEventSerializationFixture<NewGroupAdded>
    {
        #region Methods

        /// <summary>
        /// The given event.
        /// </summary>
        /// <returns>
        /// The ???.
        /// </returns>
        protected override NewGroupAdded GivenEvent()
        {
            return new NewGroupAdded
                {
                    PublicKey = Guid.NewGuid(), 
                    GroupText = "text", 
                    ParentGroupPublicKey = Guid.NewGuid(), 
                    Paropagateble = Propagate.None, 
                    ConditionExpression = string.Empty,
                    Description = string.Empty
                };
        }

        #endregion
    }
}