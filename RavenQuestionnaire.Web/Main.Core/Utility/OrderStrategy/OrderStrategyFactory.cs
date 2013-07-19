namespace Main.Core.Utility.OrderStrategy
{
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The OrderStrategyFactory interface.
    /// </summary>
    public interface IOrderStrategyFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="orderType">
        /// The order type.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Utility.OrderStrategy.IOrderStrategy.
        /// </returns>
        IOrderStrategy Get(Order orderType);

        #endregion
    }

    /// <summary>
    /// The order strategy factory.
    /// </summary>
    public class OrderStrategyFactory : IOrderStrategyFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="orderType">
        /// The order type.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Utility.OrderStrategy.IOrderStrategy.
        /// </returns>
        public IOrderStrategy Get(Order orderType)
        {
            switch (orderType)
            {
                case Order.AsIs:
                    return new AsIsStrategy();
                case Order.Random:
                    return new RandomStrategy();
                case Order.MinMax:
                    return new MinMaxStrategy();
                case Order.MaxMin:
                    return new MaxMinStrategy();
                case Order.AZ:
                    return new AZStrategy();
                case Order.ZA:
                    return new ZAStrategy();
            }

            return new AsIsStrategy();

            // throw new NotImplementedException(string.Format("strategy for {0} is not implemented", orderType));
        }

        #endregion
    }
}