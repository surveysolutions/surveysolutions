// -----------------------------------------------------------------------
// <copyright file="StoredObject.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Main.DenormalizerStorage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class StoredObject
    {
        public StoredObject()
        {
        }

        public StoredObject(object data, string id)
        {
            Data = data;
            Id = id;
        }

        public object Data { get; set; }
        public string Id { get; set; }
    
    }

}
