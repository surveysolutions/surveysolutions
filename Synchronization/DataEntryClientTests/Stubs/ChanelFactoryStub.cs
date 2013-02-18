// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChanelFactoryStub.cs" company="">
//   
// </copyright>
// <summary>
//   The chanel factory stub.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClientTests.Stubs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Moq;

    using SynchronizationMessages.WcfInfrastructure;

    /// <summary>
    /// The chanel factory stub.
    /// </summary>
    public class ChanelFactoryStub : IChanelFactoryWrapper
    {
        #region Fields

        /// <summary>
        /// The mocks.
        /// </summary>
        private readonly IEnumerable<object> mocks;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChanelFactoryStub"/> class.
        /// </summary>
        /// <param name="mock">
        /// The mock.
        /// </param>
        public ChanelFactoryStub(object mock)
        {
            this.mocks = new[] { mock };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChanelFactoryStub"/> class.
        /// </summary>
        /// <param name="mocks">
        /// The mocks.
        /// </param>
        public ChanelFactoryStub(IEnumerable<object> mocks)
        {
            this.mocks = mocks;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="baseAdress">
        /// The base adress.
        /// </param>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public void Execute<T>(string baseAdress, Action<T> handler) where T : class
        {
            var client = this.GetChanel<T>(baseAdress);
            handler(client);
        }

        /// <summary>
        /// The get chanel.
        /// </summary>
        /// <param name="baseAdress">
        /// The base adress.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public T GetChanel<T>(string baseAdress) where T : class
        {
            var result = this.mocks.OfType<Mock<T>>().FirstOrDefault().Object;
            if (result == null)
            {
                throw new ArgumentException("invalid chanel type");
            }

            return result;
        }

        #endregion
    }
}