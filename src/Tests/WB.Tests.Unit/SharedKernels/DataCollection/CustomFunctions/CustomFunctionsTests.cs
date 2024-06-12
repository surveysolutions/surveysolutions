using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.V2.CustomFunctions;
using WB.Core.SharedKernels.DataCollection.V5.CustomFunctions;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage.CustomFunctions;
using Extensions = WB.Core.SharedKernels.DataCollection.V2.CustomFunctions.Extensions;

namespace WB.Tests.Unit.SharedKernels.DataCollection.CustomFunctions
{
    [TestFixture]
    internal class CustomFunctionsTests
    {
        // For all methods using params[], the indication is that 16383 is the limit:
        // http://stackoverflow.com/questions/12658883/what-is-the-maximum-number-of-parameters-that-a-c-sharp-method-can-be-defined-as

        private int[] _mc123;
        private int[] _mc3;

        [SetUp]
        public void Init()
        {
            _mc123 = new int[] { 1, 2, 3 };
            _mc3 = new int[] { 3 };
        }

        #region Tests

        [Test]
        public void Test_InRange()
        {
            ClassicAssert.IsTrue(1.InRange(0, 10));
            ClassicAssert.IsFalse(1.InRange(3, 10));
            ClassicAssert.IsFalse(13.InRange(3, 10));

            ClassicAssert.IsTrue( ((double?)1d).InRange(0.0, 10.0));
            ClassicAssert.IsFalse(((double?)1d).InRange(3.0, 10.0));
            ClassicAssert.IsFalse(((double?)13d).InRange(3.0, 10.0));

            decimal? ten = 10;
            decimal? three = 3;
            decimal? one = 1;
            decimal? zero = 0;
            decimal? thirteen = 13;

            ClassicAssert.IsTrue(one.InRange(zero, ten));
            ClassicAssert.IsFalse(one.InRange(three, ten));
            ClassicAssert.IsFalse(thirteen.InRange(three, ten));

            ClassicAssert.IsTrue(one.InRange(zero, ten));
            ClassicAssert.IsFalse(one.InRange(three, ten));
            ClassicAssert.IsFalse(thirteen.InRange(three, ten));

            ClassicAssert.IsFalse(((int?)null).InRange(0, 10));
            ClassicAssert.IsFalse(((int?)1).InRange(null, 10));
            ClassicAssert.IsFalse(((int?)1).InRange(2, (int?)null));
            
            ClassicAssert.IsFalse(((long?)null).InRange(0, 10));
            ClassicAssert.IsFalse(((long?)1).InRange(null, 10));
            ClassicAssert.IsFalse(((long?)1).InRange(2, (int?)null));

            ClassicAssert.IsFalse(((double?)null).InRange(0, 10));
            ClassicAssert.IsFalse(((double?)1).InRange(null, 10));
            ClassicAssert.IsFalse(((double?)1).InRange(2, (int?)null));
            
            ClassicAssert.IsFalse(((decimal?)null).InRange(0, 10));
            ClassicAssert.IsFalse(((decimal?)1).InRange(null, 10));
            ClassicAssert.IsFalse(((decimal?)1).InRange(2, (int?)null));
            
            
            ClassicAssert.IsFalse(((long?)11).InRange(0, 10));
            ClassicAssert.IsFalse(((double?)11).InRange(0, 10));
        }
        
        [Test]
        public void Test_InRangeDate()
        {
            DateTime? date1 = new DateTime(2001, 1, 1);
            DateTime? date2 = new DateTime(2002, 2, 2);
            DateTime? date3 = new DateTime(2003, 3, 3);

            ClassicAssert.IsTrue(date2.InRange(date1, date3));
            ClassicAssert.IsFalse(date1.InRange(date2, date3));
            ClassicAssert.IsFalse(date3.InRange(date1, date2));

            DateTime? date0 = null;
            ClassicAssert.IsFalse(date0.InRange(date1, date3));
            ClassicAssert.IsFalse(date2.InRange(null, date3));
            ClassicAssert.IsFalse(date2.InRange(date1, null));
        }


        [Test]
        public void Test_InList()
        {
            long? d = 2;
            ClassicAssert.IsFalse(d.InList());
            ClassicAssert.IsTrue(Extensions.InList(1, 1, 2, 3, 4));
            ClassicAssert.IsFalse(Extensions.InList(0, 1, 2, 3, 4));
            ClassicAssert.IsFalse(Extensions.InList(null, 1, 2, 3, 4));
            ClassicAssert.IsTrue(Extensions.InList(null, 1, 2, 3, 4, null));
        }

        [Test]
        public void Test_InListStr()
        {
            string name = "Washington";
            ClassicAssert.IsTrue(name.InList("Jackson", "Washington", "Bush"));
            ClassicAssert.IsFalse(name.InList("Jackson", "Clinton", "Bush"));
            ClassicAssert.IsFalse(String.Empty.InList("Jackson", "Clinton", "Bush"));
            ClassicAssert.IsFalse(name.InList());
        }

        [Test]
        public void Test_InListDouble()
        {
            double? d = 2.0;
            ClassicAssert.IsFalse(d.InList());
            ClassicAssert.IsTrue(Extensions.InList(1.0, 1.0, 2.0, 3.0, 4.0));
            ClassicAssert.IsFalse(Extensions.InList(0.0, 1.0, 2.0, 3.0, 4.0));
            ClassicAssert.IsFalse(Extensions.InList(null, 1.0, 2.0, 3.0, 4.0));
            ClassicAssert.IsTrue(Extensions.InList(null, 1.0, 2.0, 3.0, 4.0, null));
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
            ClassicAssert.IsFalse(d.InList());
            ClassicAssert.IsTrue(Extensions.InList(one, one, two, three, four));
            ClassicAssert.IsFalse(Extensions.InList(zero, one, two, three, four));
            ClassicAssert.IsFalse(Extensions.InList(null, one, two, three, four));
            ClassicAssert.IsTrue(Extensions.InList(null, one, two, three, four, null));
        }

        [Test]
        public void Test_ContainsAny()
        {
            ClassicAssert.IsTrue(_mc123.ContainsAny(2, 0));
            ClassicAssert.IsFalse(_mc123.ContainsAny(5, 10));

            ClassicAssert.IsTrue(_mc123.ContainsAny(2));

            ClassicAssert.IsTrue(_mc123.ContainsAny(null));
            ClassicAssert.IsTrue(_mc123.ContainsAny(new int[0]));

            decimal[] empty = null;
            ClassicAssert.IsFalse(empty.ContainsAny(2));
            ClassicAssert.IsFalse(empty.ContainsAny(null));

            empty = new decimal[0];
            ClassicAssert.IsFalse(empty.ContainsAny(2));
        }

        [Test]
        public void Test_ContainsOnly()
        {
            ClassicAssert.IsFalse(_mc123.ContainsOnly(1));   // False because also contains 2 and 3
            ClassicAssert.IsFalse(_mc123.ContainsOnly(2));   // False because also contains 1 and 3
            ClassicAssert.IsFalse(_mc123.ContainsOnly(3));   // False because also contains 1 and 2
            ClassicAssert.IsFalse(_mc123.ContainsOnly(1, 2));   // False because also contains 3
            ClassicAssert.IsTrue(_mc123.ContainsOnly(1, 2, 3)); // True because contains each of the items and no other items.

            ClassicAssert.IsFalse(_mc3.ContainsOnly(1)); // False because contains a different item
            ClassicAssert.IsTrue(_mc3.ContainsOnly(3)); // True because contains exactly this item

            decimal[] empty = null;
            ClassicAssert.IsFalse(empty.ContainsOnly(2));     // nothing does not contain 2
            //ClassicAssert.IsFalse(empty.ContainsOnly(null));       // nothing consists of nothing
        }

        [Test]
        public void Test_ContainsAll()
        {
            ClassicAssert.IsTrue(_mc123.ContainsAll(1));
            ClassicAssert.IsTrue(_mc123.ContainsAll(2));
            ClassicAssert.IsTrue(_mc123.ContainsAll(3));

            ClassicAssert.IsTrue(_mc123.ContainsAll(1, 2));
            ClassicAssert.IsTrue(_mc123.ContainsAll(1, 3));
            ClassicAssert.IsTrue(_mc123.ContainsAll(2, 3));

            ClassicAssert.IsTrue(_mc123.ContainsAll(1, 2, 3));

            // Order does not matter!
            ClassicAssert.IsTrue(_mc123.ContainsAll(2, 1));
            ClassicAssert.IsTrue(_mc123.ContainsAll(3, 1));
            ClassicAssert.IsTrue(_mc123.ContainsAll(3, 2));

            ClassicAssert.IsTrue(_mc123.ContainsAll(3, 2, 1));

            ClassicAssert.IsFalse(_mc123.ContainsAll(3, 9));

            ClassicAssert.IsTrue(_mc123.ContainsAll((decimal[])null));
            ClassicAssert.IsTrue(_mc123.ContainsAll((int[])null));
            
            ClassicAssert.IsTrue(_mc123.ContainsAll(new decimal[0]));


            decimal[] empty = null;
            ClassicAssert.IsFalse(empty.ContainsAll(1));

            decimal[] empty2 = new decimal[0];
            ClassicAssert.IsFalse(empty2.ContainsAll(1));
        }


        [Test]
        public void Test_IsNoneOf()
        {
            decimal? educ = 4;
            ClassicAssert.IsTrue(educ.IsNoneOf(2, 3, 11)); // bacause value of educ is not blacklisted
            ClassicAssert.IsFalse(educ.IsNoneOf(2, 3, 4)); // because 4 is blacklisted but is the current value of educ
            ClassicAssert.IsTrue(educ.IsNoneOf());         // because the blacklist is empty
            ClassicAssert.IsTrue(educ.IsNoneOf(null));

            decimal? none = null;
            ClassicAssert.IsTrue(none.IsNoneOf(2, 3, 4));
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


            ClassicAssert.AreEqual(1, new AbstractConditionalLevelInstanceFunctions().CountValue(1, q1, q2, q3, q4, q5, q6, q7));
            ClassicAssert.AreEqual(4, new AbstractConditionalLevelInstanceFunctions().CountValue(2, q1, q2, q3, q4, q5, q6, q7));
            ClassicAssert.AreEqual(2, new AbstractConditionalLevelInstanceFunctions().CountValue(3, q1, q2, q3, q4, q5, q6, q7));
            ClassicAssert.AreEqual(0, new AbstractConditionalLevelInstanceFunctions().CountValue(4, q1, q2, q3, q4, q5, q6, q7));
            ClassicAssert.AreEqual(4, new AbstractConditionalLevelInstanceFunctions().CountValue(2, q1, q2, q3, q4, q5, q6, q7, q8));
        }

        [Test]
        public void Test_CountValues()
        {
            var _mc123_1 = new decimal[] { 1, 2, 3 };

            ClassicAssert.AreEqual(1, _mc123_1.CountValues(1));
            ClassicAssert.AreEqual(1, _mc123_1.CountValues(2));
            ClassicAssert.AreEqual(1, _mc123_1.CountValues(3));
            ClassicAssert.AreEqual(2, _mc123_1.CountValues(1, 2));
            ClassicAssert.AreEqual(2, _mc123_1.CountValues(1, 3));
            ClassicAssert.AreEqual(3, _mc123_1.CountValues(1, 2, 3));
            ClassicAssert.AreEqual(3, _mc123_1.CountValues(1, 2, 3, 4));
            ClassicAssert.AreEqual(0, _mc123_1.CountValues());
            ClassicAssert.AreEqual(0, _mc123_1.CountValues(null));

            decimal[] empty = null;
            ClassicAssert.AreEqual(0, empty.CountValues(1, 2, 3));

            decimal[] empty2 = new decimal[0];
            ClassicAssert.AreEqual(0, empty2.CountValues(2, 3, 4));

        }

        [Test]
        public void Test_CenturyMonthCode()
        {
            ClassicAssert.AreEqual(11, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(11, 1900));
            ClassicAssert.AreEqual(1383, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(3, 2015));

            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(11, null));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(null, 2015));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(null, null));

            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(13, 2015));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(0, 2015));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(6, 1812));
        }

        [Test]
        public void Test_CenturyMonthCodeDouble()
        {
            ClassicAssert.AreEqual(11, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(11.0, 1900.0));
            ClassicAssert.AreEqual(1383, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(3.0, 2015.0));

            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(11.0, null));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(null, 2015.0));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(null, null));

            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(13.0, 2015.0));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(0.0, 2015.0));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(6.0, 1812.0));

            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(2.2, 2015.0));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(6.0, 1812.3));
        }

        [Test]
        public void Test_CenturyMonthCodeDecimal()
        {
            decimal yr2015 = 2015;
            decimal yr1900 = 1900;
            decimal yr1812 = 1812;

            decimal m11 = 11;

            ClassicAssert.AreEqual(11, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(m11, yr1900));
            ClassicAssert.AreEqual(1383, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(3, yr2015));

            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(m11, null));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(null, yr2015));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(null, null));

            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(13, yr2015));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(0, yr2015));
            ClassicAssert.AreEqual(-1, new AbstractConditionalLevelInstanceFunctions().CenturyMonthCode(6, yr1812));
        }

        [Test]
        public void Test_IsDate()
        {
            ClassicAssert.IsTrue(new AbstractConditionalLevelInstanceFunctions().IsDate((decimal)2010, 12, 31));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate((decimal)2010, 2, 31));

            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate(null, (decimal)12, 31));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate((decimal)2010, null, 31));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate((decimal)2010, 12, null));

            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate((decimal)2010, 12, null));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate((decimal)2010.2, 12, 10));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate((decimal)2010, (decimal)12.2, 10));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate((decimal)2010, 12, (decimal)10.2));
        }

        [Test]
        public void Test_IsDateDouble()
        {
            ClassicAssert.IsTrue(new AbstractConditionalLevelInstanceFunctions().IsDate(2010.0, 12, 31));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate(2010.0, 2, 31));

            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate(null, 12, 31.0));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate(2010.0, null, 31));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate(2010.0, 12, null));

            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate(2010.0, 12, null));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate(2010.2, 12, 10));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate(2010, 12.2, 10));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsDate(2010, 12, 10.2));
        }

        [Test]
        public void Test_IsMilitaryTime()
        {
            ClassicAssert.IsTrue(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTime("0600"));
            ClassicAssert.IsTrue(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTime("2323"));
            ClassicAssert.IsTrue(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTime("0600"));
            ClassicAssert.IsTrue(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTime("2323"));

            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTime(""));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTime("0090"));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTime("2500"));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTime("2525"));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTime("545")); // leading zeroes are required
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTime("5:45")); // delimiters are not allowed
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTime("15.3")); // only digits are allowed
        }

        [Test]
        public void Test_IsMilitaryTimeZ()
        {
            ClassicAssert.IsTrue(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTimeZ("0600Z"));
            ClassicAssert.IsTrue(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTimeZ("2323J"));
            ClassicAssert.IsTrue(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTimeZ("0600Z"));
            ClassicAssert.IsTrue(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTimeZ("2323J"));

            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTimeZ(""));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTimeZ("0090A"));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTimeZ("2500Z"));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTimeZ("2525B"));
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTimeZ("0630q")); // small letters not allowed, capital letters are required
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTimeZ("0630")); // time zone is required
            ClassicAssert.IsFalse(new AbstractConditionalLevelInstanceFunctions().IsMilitaryTimeZ("545A")); // leading zeroes are required
        }

        [Test]
        [TestCase("2001-09-12", "2015-03-20", ExpectedResult = 13)]
        [TestCase("2014-02-20", "2015-03-20", ExpectedResult = 1)]
        [TestCase("2015-03-20", "2015-12-31", ExpectedResult = 0)]
        [TestCase("2015-12-31", "2015-03-20", ExpectedResult = -9998)]
        [TestCase("2015-12-31", null, ExpectedResult = -9999)]
        [TestCase(null, "2015-12-31", ExpectedResult = -9999)]
        [TestCase(null, null, ExpectedResult = -9999)]

        [TestCase("2000-02-29", "2001-02-28", ExpectedResult = 0)]
        [TestCase("2000-02-29", "2001-03-01", ExpectedResult = 1)]
        [TestCase("2000-02-29", "2004-02-28", ExpectedResult = 3)]
        [TestCase("2000-02-29", "2004-02-29", ExpectedResult = 4)]
        [TestCase("2000-02-29", "2004-03-01", ExpectedResult = 4)]
        [TestCase("1990-11-01", "2022-09-01", ExpectedResult = 31)]
        public int Test_FullYearsBetween(DateTime? first, DateTime? second)
        {
            return new AbstractConditionalLevelInstanceFunctions().FullYearsBetween(first, second);

            
            /*ClassicAssert.AreEqual(level.FullYearsBetween(d11, d12), 0);
            ClassicAssert.AreEqual(level.FullYearsBetween(d11, d13), 1);
            ClassicAssert.AreEqual(level.FullYearsBetween(d11, d14), 3);
            ClassicAssert.AreEqual(level.FullYearsBetween(d11, d15), 4);
            ClassicAssert.AreEqual(level.FullYearsBetween(d11, d16), 4);
            */



            /*
            ClassicAssert.AreEqual(13, level.FullYearsBetween(d1, d2));
            ClassicAssert.AreEqual(1, level.FullYearsBetween(d3, d2));
            ClassicAssert.AreEqual(0, level.FullYearsBetween(d2, d4));

            ClassicAssert.AreEqual(-9998, level.FullYearsBetween(d4, d2));
            ClassicAssert.AreEqual(-9999, level.FullYearsBetween(d4, null));
            ClassicAssert.AreEqual(-9999, level.FullYearsBetween(null, d4));
            ClassicAssert.AreEqual(-9999, level.FullYearsBetween(null, null));
            */

        }

        [Test]
        public void Test_FullYearsSince()
        {
            DateTime? d1 = new DateTime(2000, 1, 1);
            DateTime? d2 = new DateTime(1990, 1, 1);

            ClassicAssert.AreEqual(10, d1.FullYearsSince(d2));
        }

        [Test]
        public void Test_SelectKish1949()
        {
            ClassicAssert.AreEqual(5, new AbstractConditionalLevelInstanceFunctions().SelectKish1949(8, 5));
            ClassicAssert.AreEqual(1, new AbstractConditionalLevelInstanceFunctions().SelectKish1949(8, 1));
            ClassicAssert.AreEqual(2, new AbstractConditionalLevelInstanceFunctions().SelectKish1949(3, 20));

            ClassicAssert.AreEqual(-9999, new AbstractConditionalLevelInstanceFunctions().SelectKish1949(8, null)); // undefined household size
            ClassicAssert.AreEqual(-9998, new AbstractConditionalLevelInstanceFunctions().SelectKish1949(10, 3)); // invalid table number
            ClassicAssert.AreEqual(-9997, new AbstractConditionalLevelInstanceFunctions().SelectKish1949(8, 0));
        }

        [Test]
        public void Test_SelectKishIlo()
        {
            ClassicAssert.AreEqual(1, new AbstractConditionalLevelInstanceFunctions().SelectKishIlo(121, 8));
            ClassicAssert.AreEqual(1, new AbstractConditionalLevelInstanceFunctions().SelectKishIlo(1210, 8));
            ClassicAssert.AreEqual(5, new AbstractConditionalLevelInstanceFunctions().SelectKishIlo(555, 5));
            ClassicAssert.AreEqual(1, new AbstractConditionalLevelInstanceFunctions().SelectKishIlo(121, 12));
            ClassicAssert.AreEqual(-9997, new AbstractConditionalLevelInstanceFunctions().SelectKishIlo(121, 0));
            ClassicAssert.AreEqual(-9998, new AbstractConditionalLevelInstanceFunctions().SelectKishIlo(0, 1));
            ClassicAssert.AreEqual(-9999, new AbstractConditionalLevelInstanceFunctions().SelectKishIlo(121, null));
        }

        [Test]
        public void Test_Concat()
        {
            ClassicAssert.AreEqual("George Washington", new AbstractConditionalLevelInstanceFunctions().Concat("George", " ", "Washington"));
            ClassicAssert.AreEqual("Washington", new AbstractConditionalLevelInstanceFunctions().Concat("Washington"));
            ClassicAssert.AreEqual("", new AbstractConditionalLevelInstanceFunctions().Concat(null));
            ClassicAssert.AreEqual("George", new AbstractConditionalLevelInstanceFunctions().Concat("", "George", "", null));
        }

        [Test]
        public void Test_IsLike()
        {
            ClassicAssert.IsTrue("abcdefgh".IsLike("abcdefgh"));
            ClassicAssert.IsTrue("abcdefgh".IsLike("ab?defgh"));
            ClassicAssert.IsTrue("abcdefgh".IsLike("a*h"));
            ClassicAssert.IsTrue("abcdefgh".IsLike("a*"));
            ClassicAssert.IsTrue("abcdefgh".IsLike("*h"));
            ClassicAssert.IsTrue("abcdefgh".IsLike("ab?d*"));
            ClassicAssert.IsTrue("abcdefgh".IsLike("ab????gh"));
            ClassicAssert.IsTrue("abcdefgh".IsLike("*abcdefgh"));
            ClassicAssert.IsTrue("abcdefgh".IsLike("abcdefgh*"));

            ClassicAssert.IsFalse("abcdefgh".IsLike("bacdefgh"));
            ClassicAssert.IsFalse("abcdefgh".IsLike("?abcdefgh"));
            ClassicAssert.IsFalse("abcdefgh".IsLike("abcdefgh?"));
            ClassicAssert.IsFalse("abcdefgh".IsLike("?c?"));
            ClassicAssert.IsTrue("".IsLike(""));
            ClassicAssert.IsFalse("".IsLike("*"));

            ClassicAssert.IsFalse("abcdefgh".IsLike("abcde*efgh"));
        }

        [Test]
        public void Test_IsLike2()
        {
            Assert.Throws<ArgumentException>(() => "abcdefgh".IsLike("*cdef*"));
        }

        [Test]
        public void Test_IsLike3()
        {
            ClassicAssert.IsTrue("abc".IsLike("abc"));
            ClassicAssert.IsFalse("abc".IsLike("Abc"));
            ClassicAssert.IsTrue("abc".IsLike("a?c"));
            ClassicAssert.IsTrue("abc".IsLike("a*c"));
            ClassicAssert.IsTrue("abc".IsLike("a*"));
            ClassicAssert.IsTrue("abc".IsLike("*bc"));
            ClassicAssert.IsFalse("abc".IsLike("?abc"));
        }

        [Test]
        public void Test_Left()
        {
            ClassicAssert.AreEqual(null, "abcdefgh".Left(null));
            ClassicAssert.AreEqual("", "abcdefgh".Left(0));
            ClassicAssert.AreEqual("ab", "abcdefgh".Left(2));
            ClassicAssert.AreEqual("abcdefgh", "abcdefgh".Left(222));
            ClassicAssert.AreEqual("", "".Left(2));
            ClassicAssert.AreEqual("", "abcdefg".Left(-2));
        }

        [Test]
        public void Test_LeftDouble()
        {
            double? n = null;
            ClassicAssert.AreEqual(null, "abcdefgh".Left(n));
            ClassicAssert.AreEqual("", "abcdefgh".Left(0.0));
            ClassicAssert.AreEqual("ab", "abcdefgh".Left(2.0));
            ClassicAssert.AreEqual("abcdefgh", "abcdefgh".Left(222.0));
            ClassicAssert.AreEqual("", "".Left(2.0));
            ClassicAssert.AreEqual("", "abcdefg".Left(-2.0));
        }

        [Test]
        public void Test_LeftDecimal()
        {
            decimal? n = null;

            ClassicAssert.AreEqual(null, "abcdefgh".Left(n));
            ClassicAssert.AreEqual("", "abcdefgh".Left((decimal)0));
            ClassicAssert.AreEqual("ab", "abcdefgh".Left((decimal)2));
            ClassicAssert.AreEqual("abcdefgh", "abcdefgh".Left((decimal)222));
            ClassicAssert.AreEqual("", "".Left((decimal)2));
            ClassicAssert.AreEqual("", "abcdefg".Left((decimal)-2));
        }


        [Test]
        public void Test_Right()
        {
            ClassicAssert.AreEqual(null, "abcdefgh".Right(null));
            ClassicAssert.AreEqual("", "abcdefgh".Right(0));
            ClassicAssert.AreEqual("gh", "abcdefgh".Right(2));
            ClassicAssert.AreEqual("abcdefgh", "abcdefgh".Right(222));
            ClassicAssert.AreEqual("", "".Right(2));
            ClassicAssert.AreEqual("", "abcde".Right(-2));
        }

        [Test]
        public void Test_RightDouble()
        {
            double? n = null;
            ClassicAssert.AreEqual(null, "abcdefgh".Right(n));
            ClassicAssert.AreEqual("", "abcdefgh".Right(0.0));
            ClassicAssert.AreEqual("gh", "abcdefgh".Right(2.0));
            ClassicAssert.AreEqual("abcdefgh", "abcdefgh".Right(222.0));
            ClassicAssert.AreEqual("", "".Right(2.0));
        }

        [Test]
        public void Test_RightDecimal()
        {
            decimal? n = null;

            ClassicAssert.AreEqual(null, "abcdefgh".Right(n));
            ClassicAssert.AreEqual("", "abcdefgh".Right((decimal)0));
            ClassicAssert.AreEqual("gh", "abcdefgh".Right((decimal)2));
            ClassicAssert.AreEqual("abcdefgh", "abcdefgh".Right((decimal)222));
            ClassicAssert.AreEqual("", "".Right((decimal)2));
        }


        [Test]
        public void Test_IsIntegerNumber()
        {
            ClassicAssert.AreEqual(true, "12".IsIntegerNumber());
            ClassicAssert.AreEqual(true, "-120".IsIntegerNumber());
            ClassicAssert.AreEqual(false, "12.5".IsIntegerNumber());
            ClassicAssert.AreEqual(false, "abc".IsIntegerNumber());
            ClassicAssert.AreEqual(false, "".IsIntegerNumber());
        }

        [Test]
        [SetCulture("en-US")]
        public void Test_IsNumber()
        {
            ClassicAssert.AreEqual(true, "3.1415".IsNumber());
            ClassicAssert.AreEqual(true, "-3.1415".IsNumber());
            ClassicAssert.AreEqual(false, "3.14.15".IsNumber());
            ClassicAssert.AreEqual(false, "3FA2".IsNumber());
            ClassicAssert.AreEqual(false, "".IsNumber());
        }

        [Test]
        public void Test_IsAlphaLatin()
        {
            ClassicAssert.IsTrue("".IsAlphaLatin());
            ClassicAssert.IsTrue("ABC".IsAlphaLatin());
            ClassicAssert.IsTrue("xyz".IsAlphaLatin());
            ClassicAssert.IsTrue("ABCxyz".IsAlphaLatin());
            ClassicAssert.IsFalse("abc.".IsAlphaLatin());
        }

        [Test]
        public void Test_IsAlphaLatinOrDelimiter()
        {
            ClassicAssert.IsTrue("".IsAlphaLatinOrDelimiter());
            ClassicAssert.IsTrue("ABC".IsAlphaLatinOrDelimiter());
            ClassicAssert.IsTrue("xyz".IsAlphaLatinOrDelimiter());
            ClassicAssert.IsTrue("ABCxyz".IsAlphaLatinOrDelimiter());
            ClassicAssert.IsTrue("abc.".IsAlphaLatinOrDelimiter());
            ClassicAssert.IsFalse("abc(def)gh".IsAlphaLatinOrDelimiter());
        }

        [Test]
        public void Test_ConsistsOf()
        {
            ClassicAssert.IsTrue("abcdefgabcdefg".ConsistsOf("gfedcba"));
            ClassicAssert.IsFalse("987".ConsistsOf("01"));
            ClassicAssert.IsTrue("George Washington".ConsistsOf("Georg Washint"));
            ClassicAssert.IsFalse("George Washington".ConsistsOf("Geor washint"));

            string tst = null;
            ClassicAssert.IsTrue(tst.ConsistsOf("ABCDEF"));
        }

        [Test]
        public void Test_GpsDistance()
        {
            var p1 = new GeoLocation(38.9047, -77.0164, 15, 15);
            var p2 = new GeoLocation(39.9500, -75.1667, 15, 15);

            var d = Extensions.GpsDistance(p1, p2);
            ClassicAssert.IsTrue(Math.Abs(196800 - d) < 100);   // meters

            p1.Latitude = 36.12;
            p1.Longitude = -86.67;
            p2.Latitude = 33.94;
            p2.Longitude = -118.4;

            ClassicAssert.IsTrue(Math.Abs(2887259.95060711 - p1.GpsDistance(p2)) < 0.001);
        }

        [Test]
        public void Test_GpsDistanceCoord()
        {
            var p1 = new GeoLocation(38.9047, -77.0164, 15, 15);
            var d = Extensions.GpsDistance(p1, 39.9500, -75.1667);
            ClassicAssert.IsTrue(Math.Abs(196800 - d) < 100); // meters
        }


        [Test]
        public void Test_GpsDistanceKm()
        {
            var p1 = new GeoLocation(38.9047, -77.0164, 15, 15);
            var p2 = new GeoLocation(39.9500, -75.1667, 15, 15);

            var d = Extensions.GpsDistanceKm(p1, p2);
            ClassicAssert.IsTrue(Math.Abs(196.8 - d) < 0.1);   // kilometers

            p1.Latitude = 36.12;
            p1.Longitude = -86.67;
            p2.Latitude = 33.94;
            p2.Longitude = -118.4;

            ClassicAssert.IsTrue(Math.Abs(2887.25995060711 - p1.GpsDistanceKm(p2)) < 1);
        }

        [Test]
        public void Test_GpsDistanceCoordKm()
        {
            var p1 = new GeoLocation(38.9047, -77.0164, 15, 15);
            var d = Extensions.GpsDistanceKm(p1, 39.9500, -75.1667);
            ClassicAssert.IsTrue(Math.Abs(196.8 - d) < 0.1); // kilometers
        }

        [Test]
        public void Test_InRectangle()
        {
            var point = new GeoLocation(20, 20, 0, 0);
            ClassicAssert.IsTrue(point.InRectangle(30, 0, 10, 40));   // Ok
            ClassicAssert.IsFalse(point.InRectangle(15, 0, 10, 40));  // point too far North
            ClassicAssert.IsFalse(point.InRectangle(30, 25, 10, 40));  // point too far West
            ClassicAssert.IsFalse(point.InRectangle(30, 0, 25, 40));  // point too far South
            ClassicAssert.IsFalse(point.InRectangle(30, 0, 10, 15));  // point too far East
        }


        [Test]
        public void Test_Bmi()
        {
            ClassicAssert.AreEqual(24.98, (double)new AbstractConditionalLevelInstanceFunctions().Bmi(68, 1.65), 0.05);
        }

        #endregion

        #region ZSCORES TESTS

        [Test]
        public void Test_Zscores()
        {
            var delta = 0.1;

            ClassicAssert.AreEqual(2, ZScore.Bmifa(20, false, 18.7), delta);
            ClassicAssert.AreEqual(2, ZScore.Hcfa(20, false, 49.4), delta);
            ClassicAssert.AreEqual(2, ZScore.Lhfa(20, false, 88.7), delta);
            ClassicAssert.AreEqual(2, ZScore.Wfa(20, false, 13.7), delta);

            ClassicAssert.AreEqual(2, ZScore.Wfl(99.5, false, 18.0), delta);
            ClassicAssert.AreEqual(2, ZScore.Ssfa(20, true, 9.0), delta);
            ClassicAssert.AreEqual(2, ZScore.Acfa(20, true, 17.4), delta);
            ClassicAssert.AreEqual(2, ZScore.Tsfa(50, true, 12.9), delta);
            ClassicAssert.AreEqual(2, ZScore.Wfh(85, true, 13.8), delta);

        }

        [Test]
        public void Test_Bmia2()
        {
            var ht = 1.00;
            var wt = 12.8;
            ClassicAssert.AreEqual(-2, ZScore.Bmifa(50, false, wt, ht), 0.02);

            wt = 17.7;
            ht = 1.00;
            ClassicAssert.AreEqual(1, ZScore.Bmifa(16, true, wt, ht), 0.02);
        }



        [Test]
        public void TestForNullsInSex()
        {
            Assert.Throws<ArgumentNullException>(() => ZScore.Ssfa(12, null, 0));
        }

        [Test]
        public void TestForNullsInLen()
        {
            Assert.Throws<ArgumentNullException>(() => ZScore.Wfl(null, true, 0));
        }


        #region WFH

        [Test]
        public void Test_ZscoresWfh_When_height_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ZScore.Wfh(null, true, 10));
        }

        [Test]
        public void Test_ZscoresWfh()
        {
            Assert.Throws<ArgumentException>(() => ZScore.Wfh(0, true, -10));
        }

        [Test]
        public void Test_ZscoresWfh2()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ZScore.Wfh(49, true, 14));
        }

        #endregion

        [Test]
        public void Test_ZscoresTsfa2()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ZScore.Tsfa(-3, true, 14));
        }

        #region WFL
        [Test]
        public void Test_ZscoresWfl2()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ZScore.Wfl(-3, true, 14));
        }

        [Test]
        public void Test_ZscoresWfl3()
        {
            Assert.Throws<ArgumentException>(() => ZScore.Wfl(3, true, -14));
        }

        #endregion

        #region SSFA
        [Test]
        public void TestSsfaForNullsInAge()
        {
            Assert.Throws<ArgumentNullException>(() => ZScore.Ssfa(null, true, 0));
        }

        [Test]
        public void TestSsfaForNullsInSex()
        {
            Assert.Throws<ArgumentNullException>(() => ZScore.Ssfa(20, null, 0));
        }

        [Test]
        public void TestSsfaForRangeMonths()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ZScore.Ssfa(-3, true, 14));
        }

        [Test]
        public void Test_ZscoresSsfa4()
        {
            Assert.Throws<ArgumentException>(() => ZScore.Ssfa(5, true, -10));
        }
        #endregion

        #region ACFA
        [Test]
        public void Test_ZscoresAcfa2()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ZScore.Acfa(-3, true, 14));
        }

        [Test]
        public void Test_ZscoresAcfa3()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ZScore.Acfa(1, true, 14)); // still out of range
        }

        [Test]
        public void Test_ZscoresAcfa4()
        {
            Assert.Throws<ArgumentException>(() => ZScore.Acfa(5, true, -10));
        }

        #endregion

        #region HCFA

        [Test]
        public void Test_ZscoresHcfa2()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ZScore.Hcfa(-3, false, 48));
        }

        [Test]
        public void Test_ZscoresHcfa3()
        {
            Assert.Throws<ArgumentException>(() => ZScore.Hcfa(3, true, -48));
        }

        #endregion

        #endregion

        #region EMAIL TESTS 
        // Examples https://en.wikipedia.org/wiki/Email_address#Examples

        [Test]
        public void Test_EmailValid()
        {
            var validEmails = new List<String>();
            validEmails.Add("prettyandsimple@example.com");
            validEmails.Add("very.common@example.com");
            validEmails.Add("disposable.style.email.with+symbol@example.com");
            validEmails.Add("other.email-with-dash@example.com");
            validEmails.Add("\"much.more unusual\"@example.com");
            validEmails.Add("\"very.unusual.@.unusual.com\"@example.com");
            validEmails.Add("\"very.(),:;<>[]\\\".VERY.\\\"very@\\\\ \\\"very\\\".unusual\"@strange.example.com");
            //validEmails.Add("admin@mailserver1");  // ??
            validEmails.Add("#!$%&'*+-/=?^_`{}|~@example.org");
            validEmails.Add("\"()<>[]:,;@\\\\\\\"!#$%&'*+-/=?^_`{}| ~.a\"@example.org");
            validEmails.Add("\" \"@example.org");
            validEmails.Add("üñîçøðé@example.com");
            validEmails.Add("üñîçøðé@üñîçøðé.com");
            validEmails.Add("чебурашка@ящик-с-апельсинами.рф");
            validEmails.Add("甲斐@黒川.日本");
            validEmails.Add("我買@屋企.香港");
            validEmails.Add("δοκιμή@παράδειγμα.δοκιμή");
            validEmails.Add("Pelé@example.com");
            validEmails.Add("\"john..doe\"@example.com");  // Quoted content should be permitted
            //validEmails.Add("postmaster@.");             // Microsoft disallows this. Only one reference on StackOverflow that this may be a valid email. 
            
            foreach (var v in validEmails)
            {
                ClassicAssert.IsTrue(v.IsValidEmail());
            }
        }
        [Test]
        public void Test_EmailInvalid()
        {
            var invalidEmails = new List<String>();
            invalidEmails.Add("Abc.example.com");
            invalidEmails.Add("A@b@c@example.com");
            invalidEmails.Add("a\"b(c)d,e:f;g<h>i[j\\k]l@example.com");
            invalidEmails.Add("just\"not\"right@example.com");
            invalidEmails.Add("this is\"not\\allowed@example.com");
            invalidEmails.Add("this\\ still\\\"not\\\\allowed@example.com");
            invalidEmails.Add("john..doe@example.com");
            invalidEmails.Add("john.doe@example..com");
            invalidEmails.Add(" prettyandsimple@example.com");
            invalidEmails.Add("prettyandsimple@example.com ");
            invalidEmails.Add("abc");

            foreach (var v in invalidEmails)
            {
                ClassicAssert.IsFalse(v.IsValidEmail());
            }
        }

        #endregion

        #region March 2016 functions


        [Test]
        public void Test_IsDateLong()
        {
            ClassicAssert.IsTrue(new LevelFunctions().IsDate((long)2010, 12, 31));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((long)2010, 2, 31));

            ClassicAssert.IsFalse(new LevelFunctions().IsDate(null, (long)12, 31));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((long)2010, null, 31));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((long)2010, 12, null));

            ClassicAssert.IsFalse(new LevelFunctions().IsDate((long)2010, 12, null));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((long)2010, 13, 10));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((long)2010, 12, 32));

            ClassicAssert.IsFalse(new LevelFunctions().IsDate((long)2010, 12, -1));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((long)2010, -12, 1));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((long)-2010, 12, 1));
        }

        [Test]
        public void Test_IsDateInt()
        {
            ClassicAssert.IsTrue(new LevelFunctions().IsDate((int)2010, 12, 31));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((int)2010, 2, 31));

            ClassicAssert.IsFalse(new LevelFunctions().IsDate(null, (int)12, 31));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((int)2010, null, 31));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((int)2010, 12, null));

            ClassicAssert.IsFalse(new LevelFunctions().IsDate((int)2010, 12, null));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((int)2010, 13, 10));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((int)2010, 12, 32));

            ClassicAssert.IsFalse(new LevelFunctions().IsDate((int)2010, 12, -1));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((int)2010, -12, 1));
            ClassicAssert.IsFalse(new LevelFunctions().IsDate((int)-2010, 12, 1));
        }

        [Test]
        public void Test_DaysBetweenDates()
        {
            DateTime? d1 = new DateTime(2016, 03, 01);
            DateTime? d2 = new DateTime(2016, 03, 02);
            DateTime? d3 = new DateTime(2016, 02, 27);
            DateTime? d4 = new DateTime(2000, 01, 01);

            ClassicAssert.AreEqual(1, new LevelFunctions().DaysBetweenDates(d1, d2));
            ClassicAssert.AreEqual(3, new LevelFunctions().DaysBetweenDates(d3, d1));
            ClassicAssert.AreEqual(4, new LevelFunctions().DaysBetweenDates(d3, d2));

            ClassicAssert.AreEqual(5904, new LevelFunctions().DaysBetweenDates(d4, d1)); //confirmed with Gnumeric

            ClassicAssert.AreEqual(-9998, new LevelFunctions().DaysBetweenDates(d2, d1));

            ClassicAssert.AreEqual(-9999, new LevelFunctions().DaysBetweenDates(null, d2));
            ClassicAssert.AreEqual(-9999, new LevelFunctions().DaysBetweenDates(d1, null));
            ClassicAssert.AreEqual(-9999, new LevelFunctions().DaysBetweenDates(null, null));
        }

        [Test]
        public void Test_YearOfCmc()
        {
            var cmc1 = new LevelFunctions().CenturyMonthCode(1, 1900); // jan1900
            var cmc2 = new LevelFunctions().CenturyMonthCode(12, 1900); // dec1900
            var cmc3 = new LevelFunctions().CenturyMonthCode(1, 1901); // jan1901
            var cmc4 = new LevelFunctions().CenturyMonthCode(3, 2016); // mar2016
            var cmc5 = new LevelFunctions().CenturyMonthCode(1, 1946); // jan1946

            ClassicAssert.AreEqual(1900, new LevelFunctions().YearOfCmc(cmc1));
            ClassicAssert.AreEqual(1900, new LevelFunctions().YearOfCmc(cmc2));
            ClassicAssert.AreEqual(1901, new LevelFunctions().YearOfCmc(cmc3));
            ClassicAssert.AreEqual(2016, new LevelFunctions().YearOfCmc(cmc4));
            ClassicAssert.AreEqual(1946, new LevelFunctions().YearOfCmc(cmc5));

            ClassicAssert.AreEqual(-9999, new LevelFunctions().YearOfCmc(-1));
        }

        [Test]
        public void Test_DaysInMonth1()
        {
            var cmc1 = new LevelFunctions().CenturyMonthCode(1, 1900); // jan1900
            var cmc2 = new LevelFunctions().CenturyMonthCode(12, 1900); // dec1900
            var cmc3 = new LevelFunctions().CenturyMonthCode(1, 1901); // jan1901
            var cmc4 = new LevelFunctions().CenturyMonthCode(3, 2016); // mar2016
            var cmc5 = new LevelFunctions().CenturyMonthCode(1, 1946); // jan1946

            var cmc6 = new LevelFunctions().CenturyMonthCode(2, 1900); // feb1900
            var cmc7 = new LevelFunctions().CenturyMonthCode(2, 2000); // feb2000
            var cmc8 = new LevelFunctions().CenturyMonthCode(11, 2016); // nov2016

            ClassicAssert.AreEqual(31, new LevelFunctions().DaysInMonth(cmc1));
            ClassicAssert.AreEqual(31, new LevelFunctions().DaysInMonth(cmc2));
            ClassicAssert.AreEqual(31, new LevelFunctions().DaysInMonth(cmc3));
            ClassicAssert.AreEqual(31, new LevelFunctions().DaysInMonth(cmc4));
            ClassicAssert.AreEqual(31, new LevelFunctions().DaysInMonth(cmc5));
            ClassicAssert.AreEqual(28, new LevelFunctions().DaysInMonth(cmc6));
            ClassicAssert.AreEqual(29, new LevelFunctions().DaysInMonth(cmc7));
            ClassicAssert.AreEqual(30, new LevelFunctions().DaysInMonth(cmc8));

            ClassicAssert.AreEqual(-9999, new LevelFunctions().DaysInMonth(-1));
        }

        [Test]
        public void Test_DaysInMonth3()
        {
            ClassicAssert.AreEqual(31, new LevelFunctions().DaysInMonth(new DateTime(1900, 1, 9)));
            ClassicAssert.AreEqual(31, new LevelFunctions().DaysInMonth(new DateTime(1900, 12, 9)));
            ClassicAssert.AreEqual(31, new LevelFunctions().DaysInMonth(new DateTime(1901, 1, 9)));
            ClassicAssert.AreEqual(31, new LevelFunctions().DaysInMonth(new DateTime(2016, 3, 9)));
            ClassicAssert.AreEqual(31, new LevelFunctions().DaysInMonth(new DateTime(1946, 1, 9)));
            ClassicAssert.AreEqual(28, new LevelFunctions().DaysInMonth(new DateTime(1900, 2, 9)));
            ClassicAssert.AreEqual(29, new LevelFunctions().DaysInMonth(new DateTime(2000, 2, 9)));
            ClassicAssert.AreEqual(30, new LevelFunctions().DaysInMonth(new DateTime(2016, 11, 9)));

            ClassicAssert.AreEqual(-9999, new LevelFunctions().DaysInMonth(null));
        }

        [Test]
        public void Test_MonthOfCmc()
        {
            var cmc1 = new LevelFunctions().CenturyMonthCode(1, 1900); // jan1900
            var cmc2 = new LevelFunctions().CenturyMonthCode(12, 1900); // dec1900
            var cmc3 = new LevelFunctions().CenturyMonthCode(1, 1901); // jan1901
            var cmc4 = new LevelFunctions().CenturyMonthCode(3, 2016); // mar2016
            var cmc5 = new LevelFunctions().CenturyMonthCode(1, 1946); // jan1946

            ClassicAssert.AreEqual(1, new LevelFunctions().MonthOfCmc(cmc1));
            ClassicAssert.AreEqual(12, new LevelFunctions().MonthOfCmc(cmc2));
            ClassicAssert.AreEqual(1, new LevelFunctions().MonthOfCmc(cmc3));
            ClassicAssert.AreEqual(3, new LevelFunctions().MonthOfCmc(cmc4));
            ClassicAssert.AreEqual(1, new LevelFunctions().MonthOfCmc(cmc5));

            ClassicAssert.AreEqual(-9999, new LevelFunctions().MonthOfCmc(-1));
        }

        [Test]
        public void Test_ContainsAnyOtherThan()
        {
            ClassicAssert.IsTrue(_mc123.ContainsAnyOtherThan(2)); // Contains 1, 3
            ClassicAssert.IsTrue(_mc123.ContainsAnyOtherThan(2, 0)); // Contains 1, 3; 0 is irrelevant
            ClassicAssert.IsTrue(_mc123.ContainsAnyOtherThan(5, 10)); // Contains 1,2,3
            ClassicAssert.IsTrue(_mc123.ContainsAnyOtherThan(null)); // Contains 1,2,3
            ClassicAssert.IsTrue(_mc123.ContainsAnyOtherThan(new int[0])); // Contains 1,2,3
            ClassicAssert.IsFalse(_mc123.ContainsAnyOtherThan(1, 2, 3)); // No, does not contain anything else
            ClassicAssert.IsFalse(_mc123.ContainsAnyOtherThan(1, 2, 3, 4, 5)); // No, does not contain anything else, couple of irrelevant options

            int[] empty = null;
            ClassicAssert.IsFalse(empty.ContainsAnyOtherThan(2)); // No, empty does not contain any other
            ClassicAssert.IsFalse(empty.ContainsAnyOtherThan(null));

            empty = new int[0];
            ClassicAssert.IsFalse(empty.ContainsAnyOtherThan(2)); // No, empty does nto contain any other

            var trivial = new int[] { 0 };
            ClassicAssert.IsTrue(trivial.ContainsAnyOtherThan(2)); // Yes, contains 0
        }

        [Test]
        public void Test_BracketIndexLeftDouble()
        {
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexLeft(-1.2, 1, 2, 3));
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexLeft(1.0, 1, 2, 3));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexLeft(1.2, 1, 2, 3));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexLeft(2.0, 1, 2, 3));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexLeft(2.2, 1, 2, 3));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexLeft(3.0, 1, 2, 3));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexLeft(3.3, 1, 2, 3));
        }

        [Test]
        public void Test_BracketIndexRightDouble()
        {
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexRight(-1.2, 1, 2, 3));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexRight(1.0, 1, 2, 3));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexRight(1.2, 1, 2, 3));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexRight(2.0, 1, 2, 3));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexRight(2.2, 1, 2, 3));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexRight(3.0, 1, 2, 3));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexRight(3.3, 1, 2, 3));
        }

        [Test]
        public void Test_BracketIndexLeftDecimal()
        {
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexLeft(-1.2m, 1, 2, 3));
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexLeft(1.0m, 1, 2, 3));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexLeft(1.2m, 1, 2, 3));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexLeft(2.0m, 1, 2, 3));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexLeft(2.2m, 1, 2, 3));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexLeft(3.0m, 1, 2, 3));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexLeft(3.3m, 1, 2, 3));
        }

        [Test]
        public void Test_BracketIndexRightDecimal()
        {
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexRight(-1.2m, 1, 2, 3));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexRight(1.0m, 1, 2, 3));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexRight(1.2m, 1, 2, 3));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexRight(2.0m, 1, 2, 3));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexRight(2.2m, 1, 2, 3));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexRight(3.0m, 1, 2, 3));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexRight(3.3m, 1, 2, 3));
        }

        [Test]
        public void Test_BracketIndexLeftLong()
        {
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexLeft((long)-12, 10, 20, 30));
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexLeft((long)10, 10, 20, 30));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexLeft((long)12, 10, 20, 30));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexLeft((long)20, 10, 20, 30));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexLeft((long)22, 10, 20, 30));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexLeft((long)30, 10, 20, 30));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexLeft((long)33, 10, 20, 30));
        }

        [Test]
        public void Test_BracketIndexRightLong()
        {
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexRight((long)-12, 10, 20, 30));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexRight((long)10, 10, 20, 30));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexRight((long)12, 10, 20, 30));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexRight((long)20, 10, 20, 30));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexRight((long)22, 10, 20, 30));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexRight((long)30, 10, 20, 30));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexRight((long)33, 10, 20, 30));
        }

        [Test]
        public void Test_BracketIndexLeftInt()
        {
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexLeft((int)-12, 10, 20, 30));
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexLeft((int)10, 10, 20, 30));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexLeft((int)12, 10, 20, 30));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexLeft((int)20, 10, 20, 30));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexLeft((int)22, 10, 20, 30));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexLeft((int)30, 10, 20, 30));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexLeft((int)33, 10, 20, 30));
        }

        [Test]
        public void Test_BracketIndexRightInt()
        {
            ClassicAssert.AreEqual(0, new LevelFunctions().BracketIndexRight((int)-12, 10, 20, 30));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexRight((int)10, 10, 20, 30));
            ClassicAssert.AreEqual(1, new LevelFunctions().BracketIndexRight((int)12, 10, 20, 30));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexRight((int)20, 10, 20, 30));
            ClassicAssert.AreEqual(2, new LevelFunctions().BracketIndexRight((int)22, 10, 20, 30));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexRight((int)30, 10, 20, 30));
            ClassicAssert.AreEqual(3, new LevelFunctions().BracketIndexRight((int)33, 10, 20, 30));
        }

        #endregion

    }
}
