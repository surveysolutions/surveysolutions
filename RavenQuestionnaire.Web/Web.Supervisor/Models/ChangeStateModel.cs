// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeStateModel.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   Defines the ChangeStateModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Web.Supervisor.Models
{
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// Define changestate model
    /// </summary>
    public class ChangeStateModel
    {


        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets TemplateId.
        /// </summary>
        public string TemplateId { get; set; }

    }
}