using HL7.Tea.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HL7.Tea.tests
{
    [TestClass]
    public class HelpersTests
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
            var msg = new HL7Message(TEST_MSG);
            var res = msg.GetPatientAge(new DateTime(2026, 1, 1));
            Assert.AreEqual(56, res);
        }


    }
}