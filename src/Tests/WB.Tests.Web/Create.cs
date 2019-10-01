using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Web.TestFactories;

namespace WB.Tests.Web
{
    internal class Create
    {
        public static readonly AggregateRootFactory AggregateRoot = new AggregateRootFactory();
        public static readonly EntityFactory Entity = new EntityFactory();

        public static readonly ServiceFactory Service = new ServiceFactory();

        public static readonly CommandFactory Command = new CommandFactory();

        public static readonly OtherFactory Other = new OtherFactory();

        public static readonly StorageFactory Storage = new StorageFactory();

        public static Identity Identity(Guid? id = null, params int[] rosterVector) => Entity.Identity(id, rosterVector);

        public static RosterVector RosterVector(params int[] coordinates) => Entity.RosterVector(coordinates);
    }
}
