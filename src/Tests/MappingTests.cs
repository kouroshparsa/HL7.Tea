using HL7.Tea.core;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;


namespace HL7.Tea.tests
{
    [TestClass]
    public class MappingTests
    {
        const string TEST_MSG = @"MSH|^~\&|ADM|RCH|||202403270202||ADT^A03|4425797|P|2.4||||NE
EVN|A03|202403270202|||UNKNOWN^RUN^MIDNT^^^^^^^^^^U|20240326
PID|1||RC123^^^^MR^A~9872360649^^^^HCN^B~123^^^^PI^A~E456^^^^EMR^A~66292A7E8541^^^^PT^B||MICROTEST^CRIT^ONE^^^^L~OPENHOUSE^CRIT^ONE^^^^A||19690414|F|||123 MAIN ST^^Seattle^WA^12345^CAN||(896)321-4545^PRN^CELL||ENG|||RC1234/24|3452353232
PV1|1|O|RC.ERZ1|||ER-ERIN1^ERERINZ1-1^IRM|LABTESTG1^Labtest^Generic Doc1^Doc2^^^MD^^^^^^XX|||||||||||RCR||CL|||||||||||||||||||RCH||DIS|||202309081450|202403260001|
GT1|ABC||Doe^Jane
ZFH|CVC||F||test1@gmail.com
ZFH|ABC||F||test2@gmail.com|hi";

        [TestMethod]
        public void TestDirectMap()
        {
            var src = new Message(TEST_MSG);
            var dst = new Message();
            Mapper.DirectMap(src, dst, new List<string> { "ZFH-1", "ZFH-2", "ZFH-4", "ZFH-5", "MSH", "PID", "PV1-1" });
            var exp = @"MSH|^~\&|ADM|RCH|||202403270202||ADT^A03|4425797|P|2.4||||NE
PID|1||RC123^^^^MR^A~9872360649^^^^HCN^B~123^^^^PI^A~E456^^^^EMR^A~66292A7E8541^^^^PT^B||MICROTEST^CRIT^ONE^^^^L~OPENHOUSE^CRIT^ONE^^^^A||19690414|F|||123 MAIN ST^^Seattle^WA^12345^CAN||(896)321-4545^PRN^CELL||ENG|||RC1234/24|3452353232
PV1|1
ZFH|CVC||||test1@gmail.com
ZFH|ABC||||test2@gmail.com";
            var act = dst.Content.Replace("\r", "\r\n");
            Assert.AreEqual(exp, act);
        }

        [TestMethod]
        public void TestConditionalMapByField()
        {
            // Arrange
            var src = new Message(TEST_MSG);
            var dst = new Message();

            Mapper.DirectMap(src, dst, new List<string> { "MSH" });
            dst.SetField("PID-3", "");

            Func<object, bool> pid3MappingCondition = pid3 =>
                Helpers.GetSubfield((string)pid3, 6) == "A";

            // Act
            Mapper.ConditionalMap(src, dst, "PID-3", pid3MappingCondition);

            var exp =
    @"MSH|^~\&|ADM|RCH|||202403270202||ADT^A03|4425797|P|2.4||||NE
PID|||RC123^^^^MR^A~123^^^^PI^A~E456^^^^EMR^A";

            var act = dst.Content.Replace("\r", "\r\n");

            // Assert
            Assert.AreEqual(exp, act);
        }

        [TestMethod]
        public void TestConditionalMapBySegment()
        {
            // Arrange
            var src = new Message(TEST_MSG);
            var dst = new Message();

            Mapper.DirectMap(src, dst, new List<string> { "MSH" });

            Func<object, bool> zfhMappingCondition = zfh =>
                (((Segment)zfh)).GetFieldOne("ZFH-1") == "ABC";

            // Act
            Mapper.ConditionalMap(src, dst, "ZFH", zfhMappingCondition);

            var exp =
    @"MSH|^~\&|ADM|RCH|||202403270202||ADT^A03|4425797|P|2.4||||NE
ZFH|ABC||F||test2@gmail.com|hi";

            var act = dst.Content.Replace("\r", "\r\n");

            // Assert
            Assert.AreEqual(exp, act);
        }
    }
}