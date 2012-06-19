using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Utility.OrderStrategy
{
    public interface IOrderStrategyFactory
    {
        IOrderStrategy Get(Order orderType);
    }

    public class OrderStrategyFactory : IOrderStrategyFactory
    {
        public IOrderStrategy Get(Order orderType)
        {
            switch (orderType)
            {
                    case Order.AsIs:return new AsIsStrategy();
                    case Order.Random: return new RandomStrategy();
                    case Order.MinMax: return new MinMaxStrategy();
                    case Order.MaxMin: return new MaxMinStrategy();
                    case Order.AZ: return new AZStrategy();
                    case Order.ZA:return new ZAStrategy();
            }
            return new AsIsStrategy();
        //    throw new NotImplementedException(string.Format("strategy for {0} is not implemented", orderType));
        }
    }
}
