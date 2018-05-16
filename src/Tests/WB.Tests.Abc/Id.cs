using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Abc
{
    internal class Id
    {
        public static readonly Guid gA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        public static readonly Guid gB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        public static readonly Guid gC = Guid.Parse("cccccccccccccccccccccccccccccccc");
        public static readonly Guid gD = Guid.Parse("dddddddddddddddddddddddddddddddd");
        public static readonly Guid gE = Guid.Parse("eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee");
        public static readonly Guid gF = Guid.Parse("ffffffffffffffffffffffffffffffff");
        public static readonly Guid g1 = Guid.Parse("11111111111111111111111111111111");
        public static readonly Guid g2 = Guid.Parse("22222222222222222222222222222222");
        public static readonly Guid g3 = Guid.Parse("33333333333333333333333333333333");
        public static readonly Guid g4 = Guid.Parse("44444444444444444444444444444444");
        public static readonly Guid g5 = Guid.Parse("55555555555555555555555555555555");
        public static readonly Guid g6 = Guid.Parse("66666666666666666666666666666666");
        public static readonly Guid g7 = Guid.Parse("77777777777777777777777777777777");
        public static readonly Guid g8 = Guid.Parse("88888888888888888888888888888888");
        public static readonly Guid g9 = Guid.Parse("99999999999999999999999999999999");
        public static readonly Guid g10 = Guid.Parse("11111111111111110000000000000000");

        public static readonly Identity Identity1 = Create.Entity.Identity(1, RosterVector.Empty);
        public static readonly Identity Identity2 = Create.Entity.Identity(2, RosterVector.Empty);
        public static readonly Identity Identity3 = Create.Entity.Identity(3, RosterVector.Empty);
        public static readonly Identity Identity4 = Create.Entity.Identity(4, RosterVector.Empty);
        public static readonly Identity Identity5 = Create.Entity.Identity(5, RosterVector.Empty);
        public static readonly Identity Identity6 = Create.Entity.Identity(6, RosterVector.Empty);
        public static readonly Identity Identity7 = Create.Entity.Identity(7, RosterVector.Empty);
        public static readonly Identity Identity8 = Create.Entity.Identity(8, RosterVector.Empty);
        public static readonly Identity Identity9 = Create.Entity.Identity(9, RosterVector.Empty);
        public static readonly Identity Identity10 = Create.Entity.Identity(g10, RosterVector.Empty);
        public static readonly Identity Identity11 = Create.Entity.Identity(12, RosterVector.Empty);

        public static readonly Identity IdentityA = Create.Entity.Identity(gA, RosterVector.Empty);
        public static readonly Identity IdentityB = Create.Entity.Identity(gB, RosterVector.Empty);
        public static readonly Identity IdentityC = Create.Entity.Identity(gC, RosterVector.Empty);
        public static readonly Identity IdentityD = Create.Entity.Identity(gD, RosterVector.Empty);

        public static readonly Identity IdentityA_0 = Create.Entity.Identity(gA, new RosterVector(new[] { 0.0m }));
        public static readonly Identity IdentityB_0 = Create.Entity.Identity(gB, new RosterVector(new[] { 0.0m }));
        public static readonly Identity IdentityC_0 = Create.Entity.Identity(gC, new RosterVector(new[] { 0.0m }));
    }
}
