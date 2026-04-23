using HL7.Tea.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL7.Tea.tests
{
    [TestClass]
    public class HL7Tests
    {
        const string TEST_MSG = @"MSH|^~\&|ADM|RCH|||202403270202||ADT^A03|4425797|P|2.4||||NE
EVN|A03|202403270202|||UNKNOWN^RUN^MIDNT^^^^^^^^^^U|20240326
PID|1||RC123^^^^MR^RCH~9872360649^^^^HCN^RCH~123^^^^PI^RCH~E456^^^^EMR^RCH~66292A7E8541^^^^PT^RCH||MICROTEST^CRIT^ONE^^^^L~OPENHOUSE^CRIT^ONE^^^^A||19690414|F|||123 MAIN ST^^Seattle^WA^12345^CAN||(896)321-4545^PRN^CELL||ENG|||RC1234/24|3452353232
PV1|1|O|RC.ERZ1|||ER-ERIN1^ERERINZ1-1^IRM|LABTESTG1^Labtest^Generic Doc1^Doc2^^^MD^^^^^^XX|||||||||||RCR||CL|||||||||||||||||||RCH||DIS|||202309081450|202403260001|
GT1|ABC||Doe^Jane
ZFH|CVC||F||test1@gmail.com
ZFH|CVC||F||test2@gmail.com|hi^there";

        [TestMethod]
        public void TestValidation1()
        {
            // Should throw ValueError / ArgumentException for invalid path
            Assert.ThrowsException<ArgumentException>(() => new HL7Path("1.2"));
        }

        [TestMethod]
        public void TestValidation2()
        {
            Assert.ThrowsException<ArgumentException>(() => new HL7Path("P1.2"));
        }

        [TestMethod]
        public void TestValidation3()
        {
            Assert.ThrowsException<ArgumentException>(() => new HL7Path("PID1.2"));
        }

        [TestMethod]
        public void TestIsField()
        {
            var hp = new HL7Path("PID-1");
            Assert.IsTrue(hp.IsField);
            Assert.IsFalse(hp.IsSubField);

            hp = new HL7Path("PID-1.2");
            Assert.IsFalse(hp.IsField);
            Assert.IsTrue(hp.IsSubField);
        }

        [TestMethod]
        public void TestFieldRetrieval()
        {
            var hp = new HL7Path("PID-1");
            Assert.AreEqual(1, hp.Field);

            hp = new HL7Path("PID-2.3");
            Assert.AreEqual(2, hp.Field);
            Assert.AreEqual(3, hp.SubField);
        }


    }
}