using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntryClient.WcfInfrastructure;
using Moq;

namespace DataEntryClientTests.Stubs
{
    public class ChanelFactoryStub<TInput> : IChanelFactoryWrapper where TInput : class
    {
        private Mock<TInput> mock;

        public ChanelFactoryStub(Mock<TInput> mock)
        {
            this.mock = mock;
        }

        public void Execute<T>(Action<T> handler) where T : class
        {
            T client = GetChanel<T>();
            handler(client);
        }

        public T GetChanel<T>() where T : class
        {
            var result = mock.Object as T;
            if (result == null)
                throw new ArgumentException("invalid chanel type");
            return result;
        }
    }
}
