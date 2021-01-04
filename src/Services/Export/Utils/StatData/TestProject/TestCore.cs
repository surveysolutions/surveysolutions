using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatData.Core;

namespace TestProject
{
    [TestClass]
    public class TestCore
    {
        [TestMethod]
        [Description("Test reading variable names")]
        public void TestMethod4()
        {
            var t1 = new TabStreamDataQuery("benchmark8.tab");
            Assert.AreEqual(2, t1.GetVarNames().Length);
            var vname1 = t1.GetVarNames()[0];
            var vname2 = t1.GetVarNames()[1];
            Assert.AreEqual("name", vname1);
            Assert.AreEqual("agegroup", vname2);
        }

        [TestMethod]
        [Description("Test verifying the valuesets are identical")]
        public void TestUtil1()
        {
            var s1 = new ValueSet();
            s1.Add(1,"One");
            s1.Add(-1,"Neg one");

            var s2 = new ValueSet();
            s2.Add(-1,"Neg one");
            s2.Add(1,"One");
            
            Assert.AreEqual(true, Util.ValueSetsAreIdentical(s1,s2));

            s2.Add(0,"Zero");

            Assert.AreEqual(false, Util.ValueSetsAreIdentical(s1, s2));

            s1.Add(0, "Zero");
            Assert.AreEqual(true, Util.ValueSetsAreIdentical(s1, s2));

            Assert.AreEqual(true, Util.ValueSetsAreIdentical(s1, s1));
            Assert.AreEqual(true, Util.ValueSetsAreIdentical(s2, s2));
        }

        [TestMethod]
        [Description("Test detecting identical value sets")]
        public void TestUtil2()
        {
            var s1 = new ValueSet();
            s1.Add(1, "One");
            s1.Add(-1, "Neg one");
            var s2 = new ValueSet();
            s2.Add(-1, "Neg one");
            s2.Add(1, "One");
            var meta = DatasetMeta.FromVarlist("A B C D E F G");
            meta.AssociateValueSet("A", s1);
            meta.AssociateValueSet("B", s2);
            meta.AssociateValueSet("C", s1);
            s2.Add(2, "Two");
            meta.AssociateValueSet("D", s2);

            var result = meta.GetValueSetsGroups();
            Assert.AreEqual(2, result.Count); //  two because only s1 and s2, modifying s2 affects both B and D since what is stored are the pointers!!

            var s3 = new ValueSet();
            s3.Add(1, "Zero");
            s3.Add(-1, "Zero");
            meta.AssociateValueSet("G", s3);
            meta.AssociateValueSet("E", s3);
            meta.AssociateValueSet("F", s3);
            
            result = meta.GetValueSetsGroups();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("A", result["A"][0]);
            Assert.AreEqual("C", result["A"][1]);
            Assert.AreEqual("B", result["B"][0]);
            Assert.AreEqual("D", result["B"][1]);
            Assert.AreEqual("G", result["G"][0]);
            Assert.AreEqual("E", result["G"][1]);
            Assert.AreEqual("F", result["G"][2]);
        }
        
        [TestMethod]
        [Description("Test not possible to attach a null-label, instead variable must be removed from association list.")]
        public void TestUtil3()
        {
            const string vname = "x";
            var meta = new DatasetMeta(new[] {new DatasetVariable(vname)});
            meta.AssociateValueSet(vname, new ValueSet());
            meta.AssociateValueSet(vname, null);

            var l = meta.GetLabelledVariables();
            Assert.AreEqual(0, l.Count);
        }

    }
}
