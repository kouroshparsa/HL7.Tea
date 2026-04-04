using HL7.Tea.core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HL7.Tea.tests
{
    [TestClass]
    public class TransformerTests
    {
        const string TEST_MSG = @"MSH|^~\&|ADM|RCH|||202403270202||ADT^A03|4425797|P|2.4||||NE
EVN|A03|202403270202|||UNKNOWN^RUN^MIDNT^^^^^^^^^^U|20240326
PID|1||RC123^^^^MR^RCH~9872360649^^^^HCN^RCH~123^^^^PI^RCH~E456^^^^EMR^RCH~66292A7E8541^^^^PT^RCH||MICROTEST^CRIT^ONE^^^^L~OPENHOUSE^CRIT^ONE^^^^A||19690414|F|||123 MAIN ST^^Seattle^WA^12345^CAN||(896)321-4545^PRN^CELL||ENG|||RC1234/24|3452353232
PV1|1|O|RC.ERZ1|||ER-ERIN1^ERERINZ1-1^IRM|LABTESTG1^Labtest^Generic Doc1^Doc2^^^MD^^^^^^XX|||||||||||RCR||CL|||||||||||||||||||RCH||DIS|||202309081450|202403260001|
GT1|ABC||Doe^Jane
ZFH|CVC||F||test1@gmail.com
ZFH|CVC||F||test2@gmail.com|hi^there";

        [TestMethod]
        public void TestRandomNum()
        {
            // Arrange
            var msg = new Message(TEST_MSG);
            var specs = new Dictionary<string, string>
            {
                { "PID-1", "{random_num}" }
            };

            // Act
            msg.Transform(specs);
            var pid1 = msg.GetFieldOne("PID-1");

            // Assert
            Assert.AreEqual(6, pid1.Length);
            Assert.IsTrue(pid1.All(char.IsDigit));
        }

        [TestMethod]
        public void TestNow()
        {
            // Arrange
            var msg = new Message(TEST_MSG);
            msg.AddSegment("OBR");

            var specs = new Dictionary<string, string>
            {
                { "OBR-14", "{now}" }
            };

            // Act
            msg.Transform(specs);
            var obr14 = msg.GetFieldOne("OBR-14");

            // Assert
            Assert.IsTrue(Regex.IsMatch(obr14, @"^\d{12}$"));
        }

        [TestMethod]
        public void TestRandomFirstName()
        {
            var msg = new Message(TEST_MSG);

            var specs = new Dictionary<string, string>
            {
                { "PID-5.2", "{random_first_name}" }
            };

            msg.Transform(specs);
            var firstName = msg.GetFieldOne("PID-5.2");

            Assert.AreNotEqual("CRIT", firstName);
        }

        [TestMethod]
        public void TestRandomLastName()
        {
            var msg = new Message(TEST_MSG);

            var specs = new Dictionary<string, string>
            {
                { "PID-5.1", "{random_last_name}" }
            };

            msg.Transform(specs);
            var lastName = msg.GetFieldOne("PID-5.1");

            Assert.AreNotEqual("MICROTEST", lastName);
        }

        [TestMethod]
        public void TestFieldSubstitutions()
        {
            var msg = new Message(TEST_MSG);

            var specs = new Dictionary<string, string>
            {
                { "PID-1", "prefix-{PID-1}" }
            };

            msg.Transform(specs);
            var pid1 = msg.GetFieldOne("PID-1");

            Assert.AreEqual("prefix-1", pid1);
        }

        [TestMethod]
        public void TestFieldSubstitutions2()
        {
            var msg = new Message(TEST_MSG);

            var specs = new Dictionary<string, string>
            {
                { "GT1-4", "prefix-{GT1-4}" }
            };

            msg.Transform(specs);
            var val = msg.GetFieldOne("GT1-4");

            Assert.AreEqual("prefix-", val);
        }

    }
}