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
    public class ModificationsTests
    {
        const string TEST_MSG = @"MSH|^~\&|ADM|RCH|||202403270202||ADT^A03|4425797|P|2.4||||NE
EVN|A03|202403270202|||UNKNOWN^RUN^MIDNT^^^^^^^^^^U|20240326
PID|1||RC123^^^^MR^RCH~9872360649^^^^HCN^RCH~123^^^^PI^RCH~E456^^^^EMR^RCH~66292A7E8541^^^^PT^RCH||MICROTEST^CRIT^ONE^^^^L~OPENHOUSE^CRIT^ONE^^^^A||19690414|F|||123 MAIN ST^^Seattle^WA^12345^CAN||(896)321-4545^PRN^CELL||ENG|||RC1234/24|3452353232
PV1|1|O|RC.ERZ1|||ER-ERIN1^ERERINZ1-1^IRM|LABTESTG1^Labtest^Generic Doc1^Doc2^^^MD^^^^^^XX|||||||||||RCR||CL|||||||||||||||||||RCH||DIS|||202309081450|202403260001|
GT1|ABC||Doe^Jane
ZFH|CVC||F||test1@gmail.com
ZFH|CVC||F||test2@gmail.com|hi^there";

        [TestMethod]
        public void TestFieldDeletion()
        {
            // Arrange
            var msg = new HL7Message(TEST_MSG);

            // Act
            msg.RemoveField("ZFH-1");
            var res = msg.GetFieldAll("ZFH-1");

            // Assert
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual("", res[0]);
            Assert.AreEqual("", res[1]);
            var parts = msg.Content.Split();
            Assert.IsTrue(parts.Last().StartsWith("ZFH||"));
        }

        [TestMethod]
        public void TestRemoveSegment()
        {
            // Arrange
            var msg = new HL7Message(TEST_MSG);

            // Act
            msg.RemoveSegment("ZFH");

            // Assert
            Assert.AreEqual(5, msg.Segments.Count);
            Assert.IsFalse(msg.Content.Contains("ZFH"));
        }

        [TestMethod]
        public void TestRemoveSegmentWithIndex()
        {
            // Arrange
            var msg = new HL7Message(TEST_MSG);

            // Act
            msg.RemoveSegment("ZFH", 0);

            // Assert
            Assert.AreEqual(6, msg.Segments.Count);
            Assert.AreEqual("ZFH", msg.Segments[5].Name);
        }
    }
}