﻿using Newtonsoft.Json.Linq;

namespace Ncqrs.Eventing.Storage.Serialization
{
    /// <summary>
    /// Translates a serialized event between an intermediate (common) format
    /// and a raw format for storage.
    /// </summary>
    /// <typeparam name="T">The type of the raw data.</typeparam>
    public interface IEventTranslator<T>
    {
        /// <summary>
        /// Translates from the raw format to the intermediate (common) format.
        /// </summary>
        /// <param name="obj">The event to translate.</param>
        /// <returns><paramref name="obj"/> translated to the common format.</returns>
        /// <seealso cref="StoredEvent{T}.Clone{TOther}"/>
        StoredEvent<JObject> TranslateToCommon(StoredEvent<T> obj);

        /// <summary>
        /// Translates from the intermediate (common) format to the raw format.
        /// </summary>
        /// <param name="obj">The event to translate.</param>
        /// <returns><paramref name="obj"/> translated to the raw format.</returns>
        /// <seealso cref="StoredEvent{T}.Clone{TOther}"/>
        StoredEvent<T> TranslateToRaw(StoredEvent<JObject> obj);
    }
}
