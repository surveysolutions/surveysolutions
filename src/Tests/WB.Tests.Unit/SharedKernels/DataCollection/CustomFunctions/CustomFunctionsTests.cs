using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.CustomFunctions;

namespace WB.Tests.Unit.SharedKernels.DataCollection.CustomFunctions
{
    [TestFixture]
    public class UnitTest1
    {
        // For all methods using params[], the indication is that 16383 is the limit:
        // http://stackoverflow.com/questions/12658883/what-is-the-maximum-number-of-parameters-that-a-c-sharp-method-can-be-defined-as

        private decimal[] _mc123;
        private decimal[] _mc3;

        [SetUp]
        public void Init()
        {
            _mc123 = new decimal[] { 1, 2, 3 };
            _mc3 = new decimal[] { 3 };
        }

        [Test]
        public void Test_InRange()
        {
            Assert.IsTrue(StaticFunctions.InRange(1, 0, 10));
            Assert.IsFalse(StaticFunctions.InRange(1, 3, 10));
            Assert.IsFalse(StaticFunctions.InRange(13, 3, 10));

            Assert.IsTrue(StaticFunctions.InRange(1.0, 0.0, 10.0));
            Assert.IsFalse(StaticFunctions.InRange(1.0, 3.0, 10.0));
            Assert.IsFalse(StaticFunctions.InRange(13.0, 3.0, 10.0));

            decimal? ten = 10;
            decimal? three = 3;
            decimal? one = 1;
            decimal? zero = 0;
            decimal? thirteen = 13;

            Assert.IsTrue(StaticFunctions.InRange(one, zero, ten));
            Assert.IsFalse(StaticFunctions.InRange(one, three, ten));
            Assert.IsFalse(StaticFunctions.InRange(thirteen, three, ten));

            Assert.IsTrue(one.InRange(zero, ten));
            Assert.IsFalse(one.InRange(three, ten));
            Assert.IsFalse(thirteen.InRange(three, ten));
        }

        [Test]
        public void Test_InList()
        {
            long? d = 2;
            Assert.IsFalse(d.InList());
            Assert.IsTrue(StaticFunctions.InList(1, 1, 2, 3, 4));
            Assert.IsFalse(StaticFunctions.InList(0, 1, 2, 3, 4));
            Assert.IsFalse(StaticFunctions.InList(null, 1, 2, 3, 4));
            Assert.IsTrue(StaticFunctions.InList(null, 1, 2, 3, 4, null));
        }

        [Test]
        public void Test_InListDouble()
        {
            double? d = 2.0;
            Assert.IsFalse(d.InList());
            Assert.IsTrue(StaticFunctions.InList(1.0, 1.0, 2.0, 3.0, 4.0));
            Assert.IsFalse(StaticFunctions.InList(0.0, 1.0, 2.0, 3.0, 4.0));
            Assert.IsFalse(StaticFunctions.InList(null, 1.0, 2.0, 3.0, 4.0));
            Assert.IsTrue(StaticFunctions.InList(null, 1.0, 2.0, 3.0, 4.0, null));
        }

        [Test]
        public void Test_InListDecimal()
        {
            decimal? d = 2;
            decimal? zero = 0;
            decimal? one = 1;
            decimal? two = 2;
            decimal? three = 3;
            decimal? four = 4;
            Assert.IsFalse(d.InList());
            Assert.IsTrue(StaticFunctions.InList(one, one, two, three, four));
            Assert.IsFalse(StaticFunctions.InList(zero, one, two, three, four));
            Assert.IsFalse(StaticFunctions.InList(null, one, two, three, four));
            Assert.IsTrue(StaticFunctions.InList(null, one, two, three, four, null));
        }

        [Test]
        public void Test_ContainsAny()
        {
            Assert.IsTrue(_mc123.ContainsAny(2, 0));
            Assert.IsFalse(_mc123.ContainsAny(5, 10));

            Assert.IsTrue(_mc123.ContainsAny(2));

            Assert.IsTrue(_mc123.ContainsAny(null));
            Assert.IsTrue(_mc123.ContainsAny(new decimal[0]));

            decimal[] empty = null;
            Assert.IsFalse(empty.ContainsAny(2));
            Assert.IsFalse(empty.ContainsAny(null));

            empty = new decimal[0];
            Assert.IsFalse(empty.ContainsAny(2));
        }

        [Test]
        public void Test_ContainsOnly()
        {
            Assert.IsFalse(_mc123.ContainsOnly(1));   // False because also contains 2 and 3
            Assert.IsFalse(_mc123.ContainsOnly(2));   // False because also contains 1 and 3
            Assert.IsFalse(_mc123.ContainsOnly(3));   // False because also contains 1 and 2
            Assert.IsFalse(_mc123.ContainsOnly(1, 2));   // False because also contains 3
            Assert.IsTrue(_mc123.ContainsOnly(1, 2, 3)); // True because contains each of the items and no other items.

            Assert.IsFalse(_mc3.ContainsOnly(1)); // False because contains a different item
            Assert.IsTrue(_mc3.ContainsOnly(3)); // True because contains exactly this item

            decimal[] empty = null;
            Assert.IsFalse(empty.ContainsOnly(2));     // nothing does not contain 2
            //Assert.IsFalse(empty.ContainsOnly(null));       // nothing consists of nothing
        }

        [Test]
        public void Test_ContainsAll()
        {
            Assert.IsTrue(_mc123.ContainsAll(1));
            Assert.IsTrue(_mc123.ContainsAll(2));
            Assert.IsTrue(_mc123.ContainsAll(3));

            Assert.IsTrue(_mc123.ContainsAll(1, 2));
            Assert.IsTrue(_mc123.ContainsAll(1, 3));
            Assert.IsTrue(_mc123.ContainsAll(2, 3));

            Assert.IsTrue(_mc123.ContainsAll(1, 2, 3));

            // Order does not matter!
            Assert.IsTrue(_mc123.ContainsAll(2, 1));
            Assert.IsTrue(_mc123.ContainsAll(3, 1));
            Assert.IsTrue(_mc123.ContainsAll(3, 2));

            Assert.IsTrue(_mc123.ContainsAll(3, 2, 1));

            Assert.IsFalse(_mc123.ContainsAll(3, 9));

            Assert.IsTrue(_mc123.ContainsAll(null));
            Assert.IsTrue(_mc123.ContainsAll(new decimal[0]));


            decimal[] empty = null;
            Assert.IsFalse(empty.ContainsAll(1));
        }

        [Test]
        public void Test_IsAnyOf()
        {
            decimal? educ = 4;
            Assert.IsTrue(educ.IsAnyOf(2, 3, 4));
            Assert.IsFalse(educ.IsAnyOf(5, 6, 7));

            decimal? empty = null;
            Assert.IsFalse(empty.IsAnyOf(2));
        }

        [Test]
        public void Test_IsNoneOf()
        {
            decimal? educ = 4;
            Assert.IsTrue(educ.IsNoneOf(2, 3, 11)); // bacause value of educ is not blacklisted
            Assert.IsFalse(educ.IsNoneOf(2, 3, 4)); // because 4 is blacklisted but is the current value of educ
            Assert.IsTrue(educ.IsNoneOf());         // because the blacklist is empty
            Assert.IsTrue(educ.IsNoneOf(null));

            decimal? none = null;
            Assert.IsTrue(none.IsNoneOf(2, 3, 4));
        }

        [Test]
        public void Test_CountValue()
        {
            decimal? q1 = 2;
            decimal? q2 = 3;
            decimal? q3 = 1;
            decimal? q4 = 2;
            decimal? q5 = 2;
            decimal? q6 = 3;
            decimal? q7 = 2;
            decimal? q8 = null;


            Assert.AreEqual(1, new BaseFunctions().CountValue(1, q1, q2, q3, q4, q5, q6, q7));
            Assert.AreEqual(4, new BaseFunctions().CountValue(2, q1, q2, q3, q4, q5, q6, q7));
            Assert.AreEqual(2, new BaseFunctions().CountValue(3, q1, q2, q3, q4, q5, q6, q7));
            Assert.AreEqual(0, new BaseFunctions().CountValue(4, q1, q2, q3, q4, q5, q6, q7));
            Assert.AreEqual(4, new BaseFunctions().CountValue(2, q1, q2, q3, q4, q5, q6, q7, q8));
        }

        [Test]
        public void Test_CountValues()
        {
            Assert.AreEqual(1, _mc123.CountValues(1));
            Assert.AreEqual(1, _mc123.CountValues(2));
            Assert.AreEqual(1, _mc123.CountValues(3));
            Assert.AreEqual(2, _mc123.CountValues(1, 2));
            Assert.AreEqual(2, _mc123.CountValues(1, 3));
            Assert.AreEqual(3, _mc123.CountValues(1, 2, 3));
            Assert.AreEqual(3, _mc123.CountValues(1, 2, 3, 4));
            Assert.AreEqual(0, _mc123.CountValues());
            Assert.AreEqual(0, _mc123.CountValues(null));

            decimal[] empty = null;
            Assert.AreEqual(0, empty.CountValues(1, 2, 3));

        }

        [Test]
        public void Test_CmCode()
        {
            Assert.AreEqual(11, new BaseFunctions().CmCode(11, 1900));
            Assert.AreEqual(1383, new BaseFunctions().CmCode(3, 2015));

            Assert.AreEqual(-1, new BaseFunctions().CmCode(11, null));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(null, 2015));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(null, null));

            Assert.AreEqual(-1, new BaseFunctions().CmCode(13, 2015));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(0, 2015));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(6, 1812));
        }

        [Test]
        public void Test_CmCodeDouble()
        {
            Assert.AreEqual(11, new BaseFunctions().CmCode(11.0, 1900.0));
            Assert.AreEqual(1383, new BaseFunctions().CmCode(3.0, 2015.0));

            Assert.AreEqual(-1, new BaseFunctions().CmCode(11.0, null));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(null, 2015.0));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(null, null));

            Assert.AreEqual(-1, new BaseFunctions().CmCode(13.0, 2015.0));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(0.0, 2015.0));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(6.0, 1812.0));

            Assert.AreEqual(-1, new BaseFunctions().CmCode(2.2, 2015.0));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(6.0, 1812.3));
        }

        [Test]
        public void Test_CmCodeDecimal()
        {
            decimal yr2015 = 2015;
            decimal yr1900 = 1900;
            decimal yr1812 = 1812;

            decimal m11 = 11;

            Assert.AreEqual(11, new BaseFunctions().CmCode(m11, yr1900));
            Assert.AreEqual(1383, new BaseFunctions().CmCode(3, yr2015));

            Assert.AreEqual(-1, new BaseFunctions().CmCode(m11, null));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(null, yr2015));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(null, null));

            Assert.AreEqual(-1, new BaseFunctions().CmCode(13, yr2015));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(0, yr2015));
            Assert.AreEqual(-1, new BaseFunctions().CmCode(6, yr1812));
        }

        [Test]
        public void Test_IsDate()
        {
            Assert.IsTrue(new BaseFunctions().IsDate((decimal)2010, 12, 31));
            Assert.IsFalse(new BaseFunctions().IsDate((decimal)2010, 2, 31));

            Assert.IsFalse(new BaseFunctions().IsDate(null, (decimal)12, 31));
            Assert.IsFalse(new BaseFunctions().IsDate((decimal)2010, null, 31));
            Assert.IsFalse(new BaseFunctions().IsDate((decimal)2010, 12, null));

            Assert.IsFalse(new BaseFunctions().IsDate((decimal)2010, 12, null));
            Assert.IsFalse(new BaseFunctions().IsDate((decimal)2010.2, 12, 10));
            Assert.IsFalse(new BaseFunctions().IsDate((decimal)2010, (decimal)12.2, 10));
            Assert.IsFalse(new BaseFunctions().IsDate((decimal)2010, 12, (decimal)10.2));
        }

        [Test]
        public void Test_IsDateDouble()
        {
            Assert.IsTrue(new BaseFunctions().IsDate(2010.0, 12, 31));
            Assert.IsFalse(new BaseFunctions().IsDate(2010.0, 2, 31));

            Assert.IsFalse(new BaseFunctions().IsDate(null, 12, 31.0));
            Assert.IsFalse(new BaseFunctions().IsDate(2010.0, null, 31));
            Assert.IsFalse(new BaseFunctions().IsDate(2010.0, 12, null));

            Assert.IsFalse(new BaseFunctions().IsDate(2010.0, 12, null));
            Assert.IsFalse(new BaseFunctions().IsDate(2010.2, 12, 10));
            Assert.IsFalse(new BaseFunctions().IsDate(2010, 12.2, 10));
            Assert.IsFalse(new BaseFunctions().IsDate(2010, 12, 10.2));
        }

        [Test]
        public void Test_IsMilTime()
        {
            Assert.IsTrue(new BaseFunctions().IsMilTime("0600"));
            Assert.IsTrue(new BaseFunctions().IsMilTime("2323"));
            Assert.IsTrue(new BaseFunctions().IsMilTime("0600"));
            Assert.IsTrue(new BaseFunctions().IsMilTime("2323"));

            Assert.IsFalse(new BaseFunctions().IsMilTime(""));
            Assert.IsFalse(new BaseFunctions().IsMilTime("0090"));
            Assert.IsFalse(new BaseFunctions().IsMilTime("2500"));
            Assert.IsFalse(new BaseFunctions().IsMilTime("2525"));
            Assert.IsFalse(new BaseFunctions().IsMilTime("545")); // leading zeroes are required
            Assert.IsFalse(new BaseFunctions().IsMilTime("5:45")); // delimiters are not allowed
            Assert.IsFalse(new BaseFunctions().IsMilTime("15.3")); // only digits are allowed
        }

        [Test]
        public void Test_IsMilTimeZ()
        {
            Assert.IsTrue(new BaseFunctions().IsMilTimeZ("0600Z"));
            Assert.IsTrue(new BaseFunctions().IsMilTimeZ("2323J"));
            Assert.IsTrue(new BaseFunctions().IsMilTimeZ("0600Z"));
            Assert.IsTrue(new BaseFunctions().IsMilTimeZ("2323J"));

            Assert.IsFalse(new BaseFunctions().IsMilTimeZ(""));
            Assert.IsFalse(new BaseFunctions().IsMilTimeZ("0090A"));
            Assert.IsFalse(new BaseFunctions().IsMilTimeZ("2500Z"));
            Assert.IsFalse(new BaseFunctions().IsMilTimeZ("2525B"));
            Assert.IsFalse(new BaseFunctions().IsMilTimeZ("0630q")); // small letters not allowed, capital letters are required
            Assert.IsFalse(new BaseFunctions().IsMilTimeZ("0630")); // time zone is required
            Assert.IsFalse(new BaseFunctions().IsMilTimeZ("545A")); // leading zeroes are required
        }

        [Test]
        public void Test_FullYearsBetween()
        {
            DateTime? d1 = new DateTime(2001, 09, 12);
            DateTime? d2 = new DateTime(2015, 03, 20);
            DateTime? d3 = new DateTime(2014, 02, 20);
            DateTime? d4 = new DateTime(2015, 12, 31);

            Assert.AreEqual(13, new BaseFunctions().FullYearsBetween(d1, d2));
            Assert.AreEqual(1, new BaseFunctions().FullYearsBetween(d3, d2));
            Assert.AreEqual(0, new BaseFunctions().FullYearsBetween(d2, d4));

            Assert.AreEqual(-9998, new BaseFunctions().FullYearsBetween(d4, d2));
            Assert.AreEqual(-9999, new BaseFunctions().FullYearsBetween(d4, null));
            Assert.AreEqual(-9999, new BaseFunctions().FullYearsBetween(null, d4));
            Assert.AreEqual(-9999, new BaseFunctions().FullYearsBetween(null, null));
        }

        [Test]
        public void Test_SelectKish1949()
        {
            Assert.AreEqual(5, new BaseFunctions().SelectKish1949(8, 5));
            Assert.AreEqual(1, new BaseFunctions().SelectKish1949(8, 1));
            Assert.AreEqual(2, new BaseFunctions().SelectKish1949(3, 20));

            Assert.AreEqual(-9999, new BaseFunctions().SelectKish1949(8, null)); // undefined household size
            Assert.AreEqual(-9998, new BaseFunctions().SelectKish1949(10, 3)); // invalid table number
            Assert.AreEqual(-9997, new BaseFunctions().SelectKish1949(8, 0));
        }

        [Test]
        public void Test_SelectKishIlo()
        {
            Assert.AreEqual(1, new BaseFunctions().SelectKishIlo(121, 8));
            Assert.AreEqual(1, new BaseFunctions().SelectKishIlo(1210, 8));
            Assert.AreEqual(5, new BaseFunctions().SelectKishIlo(555, 5));
            Assert.AreEqual(1, new BaseFunctions().SelectKishIlo(121, 12));
            Assert.AreEqual(-9997, new BaseFunctions().SelectKishIlo(121, 0));
            Assert.AreEqual(-9998, new BaseFunctions().SelectKishIlo(0, 1));
            Assert.AreEqual(-9999, new BaseFunctions().SelectKishIlo(121, null));
        }

        [Test]
        public void Test_Concat()
        {
            Assert.AreEqual("George Washington", new BaseFunctions().Concat("George", " ", "Washington"));
            Assert.AreEqual("Washington", new BaseFunctions().Concat("Washington"));
            Assert.AreEqual("", new BaseFunctions().Concat(null));
            Assert.AreEqual("George", new BaseFunctions().Concat("", "George", "", null));
        }

        [Test]
        public void Test_IsLike()
        {
            Assert.IsTrue("abcdefgh".IsLike("abcdefgh"));
            Assert.IsTrue("abcdefgh".IsLike("ab?defgh"));
            Assert.IsTrue("abcdefgh".IsLike("a*h"));
            Assert.IsTrue("abcdefgh".IsLike("a*"));
            Assert.IsTrue("abcdefgh".IsLike("*h"));
            Assert.IsTrue("abcdefgh".IsLike("ab?d*"));
            Assert.IsTrue("abcdefgh".IsLike("ab????gh"));
            Assert.IsTrue("abcdefgh".IsLike("*abcdefgh"));
            Assert.IsTrue("abcdefgh".IsLike("abcdefgh*"));

            Assert.IsFalse("abcdefgh".IsLike("bacdefgh"));
            Assert.IsFalse("abcdefgh".IsLike("?abcdefgh"));
            Assert.IsFalse("abcdefgh".IsLike("abcdefgh?"));
            Assert.IsFalse("abcdefgh".IsLike("?c?"));
            Assert.IsTrue("".IsLike(""));
            Assert.IsFalse("".IsLike("*"));

            Assert.IsFalse("abcdefgh".IsLike("abcde*efgh"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_IsLike2()
        {
            "abcdefgh".IsLike("*cdef*");
        }

        [Test]
        public void Test_IsLike3()
        {
            Assert.IsTrue("abc".IsLike("abc"));
            Assert.IsFalse("abc".IsLike("Abc"));
            Assert.IsTrue("abc".IsLike("a?c"));
            Assert.IsTrue("abc".IsLike("a*c"));
            Assert.IsTrue("abc".IsLike("a*"));
            Assert.IsTrue("abc".IsLike("*bc"));
            Assert.IsFalse("abc".IsLike("?abc"));
        }

        [Test]
        public void Test_Left()
        {
            Assert.AreEqual(null, "abcdefgh".Left(null));
            Assert.AreEqual("", "abcdefgh".Left(0));
            Assert.AreEqual("ab", "abcdefgh".Left(2));
            Assert.AreEqual("abcdefgh", "abcdefgh".Left(222));
            Assert.AreEqual("", "".Left(2));
            Assert.AreEqual("", "abcdefg".Left(-2));
        }

        [Test]
        public void Test_LeftDouble()
        {
            double? n = null;
            Assert.AreEqual(null, "abcdefgh".Left(n));
            Assert.AreEqual("", "abcdefgh".Left(0.0));
            Assert.AreEqual("ab", "abcdefgh".Left(2.0));
            Assert.AreEqual("abcdefgh", "abcdefgh".Left(222.0));
            Assert.AreEqual("", "".Left(2.0));
            Assert.AreEqual("", "abcdefg".Left(-2.0));
        }

        [Test]
        public void Test_LeftDecimal()
        {
            decimal? n = null;

            Assert.AreEqual(null, "abcdefgh".Left(n));
            Assert.AreEqual("", "abcdefgh".Left((decimal)0));
            Assert.AreEqual("ab", "abcdefgh".Left((decimal)2));
            Assert.AreEqual("abcdefgh", "abcdefgh".Left((decimal)222));
            Assert.AreEqual("", "".Left((decimal)2));
            Assert.AreEqual("", "abcdefg".Left((decimal)-2));
        }


        [Test]
        public void Test_Right()
        {
            Assert.AreEqual(null, "abcdefgh".Right(null));
            Assert.AreEqual("", "abcdefgh".Right(0));
            Assert.AreEqual("gh", "abcdefgh".Right(2));
            Assert.AreEqual("abcdefgh", "abcdefgh".Right(222));
            Assert.AreEqual("", "".Right(2));
            Assert.AreEqual("", "abcde".Right(-2));
        }

        [Test]
        public void Test_RightDouble()
        {
            double? n = null;
            Assert.AreEqual(null, "abcdefgh".Right(n));
            Assert.AreEqual("", "abcdefgh".Right(0.0));
            Assert.AreEqual("gh", "abcdefgh".Right(2.0));
            Assert.AreEqual("abcdefgh", "abcdefgh".Right(222.0));
            Assert.AreEqual("", "".Right(2.0));
        }

        [Test]
        public void Test_RightDecimal()
        {
            decimal? n = null;

            Assert.AreEqual(null, "abcdefgh".Right(n));
            Assert.AreEqual("", "abcdefgh".Right((decimal)0));
            Assert.AreEqual("gh", "abcdefgh".Right((decimal)2));
            Assert.AreEqual("abcdefgh", "abcdefgh".Right((decimal)222));
            Assert.AreEqual("", "".Right((decimal)2));
        }


        [Test]
        public void Test_IsAlphaLatin()
        {
            Assert.IsTrue("".IsAlphaLatin());
            Assert.IsTrue("ABC".IsAlphaLatin());
            Assert.IsTrue("xyz".IsAlphaLatin());
            Assert.IsTrue("ABCxyz".IsAlphaLatin());
            Assert.IsFalse("abc.".IsAlphaLatin());
        }

        [Test]
        public void Test_IsAlphaLatinOrDelim()
        {
            Assert.IsTrue("".IsAlphaLatinOrDelim());
            Assert.IsTrue("ABC".IsAlphaLatinOrDelim());
            Assert.IsTrue("xyz".IsAlphaLatinOrDelim());
            Assert.IsTrue("ABCxyz".IsAlphaLatinOrDelim());
            Assert.IsTrue("abc.".IsAlphaLatinOrDelim());
            Assert.IsFalse("abc(def)gh".IsAlphaLatinOrDelim());
        }

        [Test]
        public void Test_ConsistsOf()
        {
            Assert.IsTrue("abcdefgabcdefg".ConsistsOf("gfedcba"));
            Assert.IsFalse("987".ConsistsOf("01"));
            Assert.IsTrue("George Washington".ConsistsOf("Georg Washint"));
            Assert.IsFalse("George Washington".ConsistsOf("Geor washint"));

            string tst = null;
            Assert.IsTrue(tst.ConsistsOf("ABCDEF"));
        }

        [Test]
        public void Test_GpsDistance()
        {
            var p1 = new GeoLocation(38.9047, -77.0164,0,0);
            var p2 = new GeoLocation(39.9500, -75.1667,0,0);

            var d = StaticFunctions.GpsDistance(p1, p2);
            Assert.IsTrue(Math.Abs(196800 - d) < 100);

            p1.Latitude = 36.12;
            p1.Longitude = -86.67;
            p2.Latitude = 33.94;
            p2.Longitude = -118.4;

            Assert.IsTrue(Math.Abs(2887259.95060711 - p1.GpsDistance(p2)) < 0.001);
        }


        [Test]
        public void Test_Zscores()
        {
            var delta = 0.1;

            Assert.AreEqual(2, new BaseFunctions().Bmi(20, false, 18.7), delta);
            Assert.AreEqual(2, new BaseFunctions().Hcfa(20, false, 49.4), delta);
            Assert.AreEqual(2, new BaseFunctions().Lhfa(20, false, 88.7), delta);
            Assert.AreEqual(2, new BaseFunctions().Wfa(20, false, 13.7), delta);

            Assert.AreEqual(2, new BaseFunctions().Wfl(99.5, false, 18.0), delta);
            Assert.AreEqual(2, new BaseFunctions().Ssfa(20, true, 9.0), delta);
            Assert.AreEqual(2, new BaseFunctions().Acfa(20, true, 17.4), delta);
            Assert.AreEqual(2, new BaseFunctions().Tsfa(50, true, 12.9), delta);
            Assert.AreEqual(2, new BaseFunctions().Wfh(85, true, 13.8), delta);

        }
    }
}
