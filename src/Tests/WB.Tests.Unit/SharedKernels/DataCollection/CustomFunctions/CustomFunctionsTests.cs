using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.V2.CustomFunctions;

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

        #region Tests

        [Test]
        public void Test_InRange()
        {
            Assert.IsTrue(Extensions.InRange(1, 0, 10));
            Assert.IsFalse(Extensions.InRange(1, 3, 10));
            Assert.IsFalse(Extensions.InRange(13, 3, 10));

            Assert.IsTrue(Extensions.InRange(1.0, 0.0, 10.0));
            Assert.IsFalse(Extensions.InRange(1.0, 3.0, 10.0));
            Assert.IsFalse(Extensions.InRange(13.0, 3.0, 10.0));

            decimal? ten = 10;
            decimal? three = 3;
            decimal? one = 1;
            decimal? zero = 0;
            decimal? thirteen = 13;

            Assert.IsTrue(Extensions.InRange(one, zero, ten));
            Assert.IsFalse(Extensions.InRange(one, three, ten));
            Assert.IsFalse(Extensions.InRange(thirteen, three, ten));

            Assert.IsTrue(one.InRange(zero, ten));
            Assert.IsFalse(one.InRange(three, ten));
            Assert.IsFalse(thirteen.InRange(three, ten));
        }

        [Test]
        public void Test_InRangeDate()
        {
            DateTime? date1 = new DateTime(2001, 1, 1);
            DateTime? date2 = new DateTime(2002, 2, 2);
            DateTime? date3 = new DateTime(2003, 3, 3);

            Assert.IsTrue(date2.InRange(date1, date3));
            Assert.IsFalse(date1.InRange(date2, date3));
            Assert.IsFalse(date3.InRange(date1, date2));

            DateTime? date0 = null;
            Assert.IsFalse(date0.InRange(date1, date3));
            Assert.IsFalse(date2.InRange(null, date3));
            Assert.IsFalse(date2.InRange(date1, null));

        }


        [Test]
        public void Test_InList()
        {
            long? d = 2;
            Assert.IsFalse(d.InList());
            Assert.IsTrue(Extensions.InList(1, 1, 2, 3, 4));
            Assert.IsFalse(Extensions.InList(0, 1, 2, 3, 4));
            Assert.IsFalse(Extensions.InList(null, 1, 2, 3, 4));
            Assert.IsTrue(Extensions.InList(null, 1, 2, 3, 4, null));
        }

        [Test]
        public void Test_InListStr()
        {
            string name = "Washington";
            Assert.IsTrue(name.InList("Jackson", "Washington", "Bush"));
            Assert.IsFalse(name.InList("Jackson", "Clinton", "Bush"));
            Assert.IsFalse(String.Empty.InList("Jackson", "Clinton", "Bush"));
            Assert.IsFalse(name.InList());
        }

        [Test]
        public void Test_InListDouble()
        {
            double? d = 2.0;
            Assert.IsFalse(d.InList());
            Assert.IsTrue(Extensions.InList(1.0, 1.0, 2.0, 3.0, 4.0));
            Assert.IsFalse(Extensions.InList(0.0, 1.0, 2.0, 3.0, 4.0));
            Assert.IsFalse(Extensions.InList(null, 1.0, 2.0, 3.0, 4.0));
            Assert.IsTrue(Extensions.InList(null, 1.0, 2.0, 3.0, 4.0, null));
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
            Assert.IsTrue(Extensions.InList(one, one, two, three, four));
            Assert.IsFalse(Extensions.InList(zero, one, two, three, four));
            Assert.IsFalse(Extensions.InList(null, one, two, three, four));
            Assert.IsTrue(Extensions.InList(null, one, two, three, four, null));
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

            decimal[] empty2 = new decimal[0];
            Assert.IsFalse(empty2.ContainsAll(1));
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

            decimal[] empty2 = new decimal[0];
            Assert.AreEqual(0, empty2.CountValues(2, 3, 4));

        }

        [Test]
        public void Test_CenturyMonthCode()
        {
            Assert.AreEqual(11, new BaseFunctions().CenturyMonthCode(11, 1900));
            Assert.AreEqual(1383, new BaseFunctions().CenturyMonthCode(3, 2015));

            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(11, null));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(null, 2015));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(null, null));

            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(13, 2015));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(0, 2015));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(6, 1812));
        }

        [Test]
        public void Test_CenturyMonthCodeDouble()
        {
            Assert.AreEqual(11, new BaseFunctions().CenturyMonthCode(11.0, 1900.0));
            Assert.AreEqual(1383, new BaseFunctions().CenturyMonthCode(3.0, 2015.0));

            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(11.0, null));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(null, 2015.0));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(null, null));

            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(13.0, 2015.0));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(0.0, 2015.0));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(6.0, 1812.0));

            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(2.2, 2015.0));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(6.0, 1812.3));
        }

        [Test]
        public void Test_CenturyMonthCodeDecimal()
        {
            decimal yr2015 = 2015;
            decimal yr1900 = 1900;
            decimal yr1812 = 1812;

            decimal m11 = 11;

            Assert.AreEqual(11, new BaseFunctions().CenturyMonthCode(m11, yr1900));
            Assert.AreEqual(1383, new BaseFunctions().CenturyMonthCode(3, yr2015));

            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(m11, null));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(null, yr2015));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(null, null));

            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(13, yr2015));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(0, yr2015));
            Assert.AreEqual(-1, new BaseFunctions().CenturyMonthCode(6, yr1812));
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
        public void Test_IsMilitaryTime()
        {
            Assert.IsTrue(new BaseFunctions().IsMilitaryTime("0600"));
            Assert.IsTrue(new BaseFunctions().IsMilitaryTime("2323"));
            Assert.IsTrue(new BaseFunctions().IsMilitaryTime("0600"));
            Assert.IsTrue(new BaseFunctions().IsMilitaryTime("2323"));

            Assert.IsFalse(new BaseFunctions().IsMilitaryTime(""));
            Assert.IsFalse(new BaseFunctions().IsMilitaryTime("0090"));
            Assert.IsFalse(new BaseFunctions().IsMilitaryTime("2500"));
            Assert.IsFalse(new BaseFunctions().IsMilitaryTime("2525"));
            Assert.IsFalse(new BaseFunctions().IsMilitaryTime("545")); // leading zeroes are required
            Assert.IsFalse(new BaseFunctions().IsMilitaryTime("5:45")); // delimiters are not allowed
            Assert.IsFalse(new BaseFunctions().IsMilitaryTime("15.3")); // only digits are allowed
        }

        [Test]
        public void Test_IsMilitaryTimeZ()
        {
            Assert.IsTrue(new BaseFunctions().IsMilitaryTimeZ("0600Z"));
            Assert.IsTrue(new BaseFunctions().IsMilitaryTimeZ("2323J"));
            Assert.IsTrue(new BaseFunctions().IsMilitaryTimeZ("0600Z"));
            Assert.IsTrue(new BaseFunctions().IsMilitaryTimeZ("2323J"));

            Assert.IsFalse(new BaseFunctions().IsMilitaryTimeZ(""));
            Assert.IsFalse(new BaseFunctions().IsMilitaryTimeZ("0090A"));
            Assert.IsFalse(new BaseFunctions().IsMilitaryTimeZ("2500Z"));
            Assert.IsFalse(new BaseFunctions().IsMilitaryTimeZ("2525B"));
            Assert.IsFalse(new BaseFunctions().IsMilitaryTimeZ("0630q")); // small letters not allowed, capital letters are required
            Assert.IsFalse(new BaseFunctions().IsMilitaryTimeZ("0630")); // time zone is required
            Assert.IsFalse(new BaseFunctions().IsMilitaryTimeZ("545A")); // leading zeroes are required
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
        public void Test_FullYearsSince()
        {
            DateTime? d1 = new DateTime(2000, 1, 1);
            DateTime? d2 = new DateTime(1990, 1, 1);

            Assert.AreEqual(10, d1.FullYearsSince(d2));
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
        public void Test_IsIntegerNumber()
        {
            Assert.AreEqual(true, "12".IsIntegerNumber());
            Assert.AreEqual(true, "-120".IsIntegerNumber());
            Assert.AreEqual(false, "12.5".IsIntegerNumber());
            Assert.AreEqual(false, "abc".IsIntegerNumber());
            Assert.AreEqual(false, "".IsIntegerNumber());
        }

        [Test]
        public void Test_IsNumber()
        {
            Assert.AreEqual(true, "3.1415".IsNumber());
            Assert.AreEqual(true, "-3.1415".IsNumber());
            Assert.AreEqual(false, "3.14.15".IsNumber());
            Assert.AreEqual(false, "3FA2".IsNumber());
            Assert.AreEqual(false, "".IsNumber());
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
        public void Test_IsAlphaLatinOrDelimiter()
        {
            Assert.IsTrue("".IsAlphaLatinOrDelimiter());
            Assert.IsTrue("ABC".IsAlphaLatinOrDelimiter());
            Assert.IsTrue("xyz".IsAlphaLatinOrDelimiter());
            Assert.IsTrue("ABCxyz".IsAlphaLatinOrDelimiter());
            Assert.IsTrue("abc.".IsAlphaLatinOrDelimiter());
            Assert.IsFalse("abc(def)gh".IsAlphaLatinOrDelimiter());
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
            var p1 = new GeoLocation(38.9047, -77.0164, 15, 15);
            var p2 = new GeoLocation(39.9500, -75.1667, 15, 15);

            var d = Extensions.GpsDistance(p1, p2);
            Assert.IsTrue(Math.Abs(196800 - d) < 100);   // meters

            p1.Latitude = 36.12;
            p1.Longitude = -86.67;
            p2.Latitude = 33.94;
            p2.Longitude = -118.4;

            Assert.IsTrue(Math.Abs(2887259.95060711 - p1.GpsDistance(p2)) < 0.001);
        }

        [Test]
        public void Test_GpsDistanceCoord()
        {
            var p1 = new GeoLocation(38.9047, -77.0164, 15, 15);
            var d = Extensions.GpsDistance(p1, 39.9500, -75.1667);
            Assert.IsTrue(Math.Abs(196800 - d) < 100); // meters
        }


        [Test]
        public void Test_GpsDistanceKm()
        {
            var p1 = new GeoLocation(38.9047, -77.0164, 15, 15);
            var p2 = new GeoLocation(39.9500, -75.1667, 15, 15);

            var d = Extensions.GpsDistanceKm(p1, p2);
            Assert.IsTrue(Math.Abs(196.8 - d) < 0.1);   // kilometers

            p1.Latitude = 36.12;
            p1.Longitude = -86.67;
            p2.Latitude = 33.94;
            p2.Longitude = -118.4;

            Assert.IsTrue(Math.Abs(2887.25995060711 - p1.GpsDistanceKm(p2)) < 1);
        }

        [Test]
        public void Test_GpsDistanceCoordKm()
        {
            var p1 = new GeoLocation(38.9047, -77.0164, 15, 15);
            var d = Extensions.GpsDistanceKm(p1, 39.9500, -75.1667);
            Assert.IsTrue(Math.Abs(196.8 - d) < 0.1); // kilometers
        }

        [Test]
        public void Test_InRectangle()
        {
            var point = new GeoLocation(20, 20, 0, 0);
            Assert.IsTrue(point.InRectangle(30, 0, 10, 40));   // Ok
            Assert.IsFalse(point.InRectangle(15, 0, 10, 40));  // point too far North
            Assert.IsFalse(point.InRectangle(30, 25, 10, 40));  // point too far West
            Assert.IsFalse(point.InRectangle(30, 0, 25, 40));  // point too far South
            Assert.IsFalse(point.InRectangle(30, 0, 10, 15));  // point too far East
        }


        [Test]
        public void Test_Bmi()
        {
            Assert.AreEqual(24.98, (double)new BaseFunctions().Bmi(68, 1.65), 0.05);
        }

        #endregion

        #region ZSCORES TESTS

        [Test]
        public void Test_Zscores()
        {
            var delta = 0.1;

            Assert.AreEqual(2, ZScore.Bmifa(20, false, 18.7), delta);
            Assert.AreEqual(2, ZScore.Hcfa(20, false, 49.4), delta);
            Assert.AreEqual(2, ZScore.Lhfa(20, false, 88.7), delta);
            Assert.AreEqual(2, ZScore.Wfa(20, false, 13.7), delta);

            Assert.AreEqual(2, ZScore.Wfl(99.5, false, 18.0), delta);
            Assert.AreEqual(2, ZScore.Ssfa(20, true, 9.0), delta);
            Assert.AreEqual(2, ZScore.Acfa(20, true, 17.4), delta);
            Assert.AreEqual(2, ZScore.Tsfa(50, true, 12.9), delta);
            Assert.AreEqual(2, ZScore.Wfh(85, true, 13.8), delta);

        }

        [Test]
        public void Test_Bmia2()
        {
            var ht = 1.00;
            var wt = 12.8;
            Assert.AreEqual(-2, ZScore.Bmifa(50, false, wt, ht), 0.02);

            wt = 17.7;
            ht = 1.00;
            Assert.AreEqual(1, ZScore.Bmifa(16, true, wt, ht), 0.02);
        }



        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestForNullsInSex()
        {
            ZScore.Ssfa(12, null, 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestForNullsInLen()
        {
            ZScore.Wfl(null, true, 0);
        }


        #region WFH

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_ZscoresWfh_When_height_is_null()
        {
            ZScore.Wfh(null, true, 10);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ZscoresWfh()
        {
            ZScore.Wfh(0, true, -10);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ZscoresWfh2()
        {
            ZScore.Wfh(49, true, 14);
        }

        #endregion

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ZscoresTsfa2()
        {
            ZScore.Tsfa(-3, true, 14);
        }

        #region WFL
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ZscoresWfl2()
        {
            ZScore.Wfl(-3, true, 14);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ZscoresWfl3()
        {
            ZScore.Wfl(3, true, -14);
        }

        #endregion

        #region SSFA
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSsfaForNullsInAge()
        {
            ZScore.Ssfa(null, true, 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSsfaForNullsInSex()
        {
            ZScore.Ssfa(20, null, 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestSsfaForRangeMonths()
        {
            ZScore.Ssfa(-3, true, 14);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ZscoresSsfa4()
        {
            ZScore.Ssfa(5, true, -10);
        }
        #endregion

        #region ACFA
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ZscoresAcfa2()
        {
            ZScore.Acfa(-3, true, 14);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ZscoresAcfa3()
        {
            ZScore.Acfa(1, true, 14); // still out of range
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ZscoresAcfa4()
        {
            ZScore.Acfa(5, true, -10);
        }

        #endregion

        #region HCFA

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ZscoresHcfa2()
        {
            ZScore.Hcfa(-3, false, 48);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ZscoresHcfa3()
        {
            ZScore.Hcfa(3, true, -48);
        }

        #endregion

        #endregion

    }
}