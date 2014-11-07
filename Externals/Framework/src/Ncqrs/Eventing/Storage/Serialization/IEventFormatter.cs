namespace Ncqrs.Eventing.Storage.Serialization
{
    /// <summary>
    /// Serializes/deserializes strongly typed events for storage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is generally intended to use an intermediate format such
    /// as <see cref="System.Xml.Linq.XDocument"/> or <see cref="Newtonsoft.Json.Linq.JObject"/>
    /// rather than a raw string or binary blob.
    /// </para>
    /// 
    /// <para>
    /// Using an intermediate format that is easily manipulatable allows
    /// for easier handling of old verisons of events (see <see cref="IEventConverter"/>).
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of the serialized data.</typeparam>
    public interface IEventFormatter<T>
    {
        /// <summary>
        /// Serializes a strongly typed event.
        /// </summary>
        /// <returns>A serialized representation of <paramref name="theEvent"/>.</returns>
        T Serialize(object theEvent, out string eventName);

        /// <summary>
        /// De-serializes a serialized event to a strongly typed event.
        /// </summary>
        /// <param name="obj">The serialized event to be de-serialized.</param>
        /// <param name="eventName">A name of serialized event type.</param>
        /// <returns>A strongly typed event from <paramref name="obj"/>.</returns>
        object Deserialize(T obj, string eventName);
    }
}
