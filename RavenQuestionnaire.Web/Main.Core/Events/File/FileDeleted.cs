﻿using System;
using Ncqrs.Eventing.Storage;

namespace Main.Core.Events.File
{
    /// <summary>
    /// The file deleted.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:FileDeleted")]
    public class FileDeleted
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        #endregion
    }
}