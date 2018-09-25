using System;
using WB.Services.Export.Interview.Entities;

namespace WB.Services.Export.Tests
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

        public static readonly Identity Identity1 = new Identity(g1, RosterVector.Empty);
        public static readonly Identity Identity2 = new Identity(g2, RosterVector.Empty);
        public static readonly Identity Identity3 = new Identity(g3, RosterVector.Empty);
        public static readonly Identity Identity4 = new Identity(g4, RosterVector.Empty);
        public static readonly Identity Identity5 = new Identity(g5, RosterVector.Empty);
        public static readonly Identity Identity6 = new Identity(g6, RosterVector.Empty);
        public static readonly Identity Identity7 = new Identity(g7, RosterVector.Empty);
        public static readonly Identity Identity8 = new Identity(g8, RosterVector.Empty);
        public static readonly Identity Identity9 = new Identity(g9, RosterVector.Empty);
        public static readonly Identity Identity10 = new Identity(g10, RosterVector.Empty);

        public static readonly Identity IdentityA = new Identity(gA, RosterVector.Empty);
        public static readonly Identity IdentityB = new Identity(gB, RosterVector.Empty);
        public static readonly Identity IdentityC = new Identity(gC, RosterVector.Empty);
        public static readonly Identity IdentityD = new Identity(gD, RosterVector.Empty);
    }
}
