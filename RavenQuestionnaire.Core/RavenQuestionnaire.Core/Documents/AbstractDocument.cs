using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Documents
{
    public abstract class AbstractDocument
    {
        /// <summary>
        /// Document ID.
        /// </summary>
        public string Id { set; get; }
    }
}
