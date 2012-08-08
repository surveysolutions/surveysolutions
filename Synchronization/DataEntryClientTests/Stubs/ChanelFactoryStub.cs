using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntryClient.WcfInfrastructure;
using Moq;

namespace DataEntryClientTests.Stubs
{
    public class ChanelFactoryStub: IChanelFactoryWrapper
    {
        private IEnumerable<object> mocks;

        public ChanelFactoryStub(object mock)
        {
            this.mocks = new []{mock};
        }
        public ChanelFactoryStub(IEnumerable<object> mocks)
        {
            this.mocks = mocks;
        }
        public void Execute<T>(string baseAdress, Action<T> handler) where T : class
        {
            T client = GetChanel<T>(baseAdress);
            handler(client);
        }

        public T GetChanel<T>(string baseAdress) where T : class
        {
            var result = mocks.OfType<Mock<T>>().FirstOrDefault().Object;
            if (result == null)
                throw new ArgumentException("invalid chanel type");
            return result;
        }
    }
}
