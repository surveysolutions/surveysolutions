using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SynchronizationMessages.Synchronization;

namespace SynchronizationMessages.CompleteQuestionnaire
{
    public class ListOfAggregateRootsForImportMessage : ICustomSerializable
    {
        public IList<Guid> Roots { get; set; }

        #region Implementation of ICustomSerializable

        public void WriteTo(Stream stream)
        {
            foreach (Guid guid in Roots)
            {
                FormatHelper.WriteGuid(stream, guid);
            }
        }

        public void InitializeFrom(Stream stream)
        {
            var result = new List<Guid>();
            Guid root = FormatHelper.ReadGuid(stream);
            while (root!=Guid.Empty)
            {
                result.Add(root);
                root = FormatHelper.ReadGuid(stream);
            }
        }

        #endregion
    }
}
