// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StoredObject.cs" company="">
//   
// </copyright>
// <summary>
//   The stored object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.DenormalizerStorage
{
    /// <summary>
    /// The stored object.
    /// </summary>
    public class StoredObject
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredObject"/> class.
        /// </summary>
        public StoredObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredObject"/> class.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        public StoredObject(object data, string id)
        {
            this.Data = data;
            this.Id = id;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        #endregion
    }
}