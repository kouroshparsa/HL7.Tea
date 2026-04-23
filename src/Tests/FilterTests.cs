using HL7.Tea.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StackExchange.Redis;


namespace HL7.Tea.tests
{
    [TestClass]
    public class FilterTests
    {
        const string TEST_MSG = @"MSH|^~\&|ADM|RCH|||202403270202||ADT^A03|4425797|P|2.4||||NE
EVN|A03|202403270202|||UNKNOWN^RUN^MIDNT^^^^^^^^^^U|20240326
PV1|1|O|RC.ERZ1|||ER-ERIN1^ERERINZ1-1^IRM|LABTESTG1^Labtest^Generic Doc1^Doc2^^^MD^^^^^^XX|||||||||||RCR||CL|||||||||||||||||||RCH||DIS|||202309081450|202403260001|
GT1|ABC||Doe^Jane
ZFH|CVC||F||test1@gmail.com
ZFH|ABC||F||test2@gmail.com|hi";

        [TestMethod]
        public void TestFilterIn()
        {
            var msg = new HL7Message(TEST_MSG);

            string spec = @"{
  ""filters"": {
    ""and"": [
      {
        ""field"": ""MSH-9.1"",
        ""operator"": ""eq"",
        ""value"": ""ADT""
      },
      {
        ""field"": ""MSH-9.2"",
        ""operator"": ""in"",
        ""value"": [""A02"", ""A03"", ""A04""]
      }
    ]
  }
}";
            
            var filterFn = FilterCompiler.BuildFilter(spec);
            bool result = filterFn(msg);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestFilterInCache()
        {
            var msg = new HL7Message(TEST_MSG);

            string spec = @"{
  ""filters"": {
    ""and"": [
      {
        ""field"": ""MSH-9.1"",
        ""operator"": ""eq"",
        ""value"": ""ADT""
      },
      {
        ""field"": ""MSH-9.2"",
        ""operator"": ""in_cache"",
        ""value"": ""MyTable""
      }
    ]
  }
}";


            var dbMock = new Mock<IDatabase>();
            dbMock.Setup(d => d.StringGet("MyTable", It.IsAny<CommandFlags>())).Returns("A02,A03");

            RedisCache.TestDatabase = dbMock.Object;
            var filterFn = FilterCompiler.BuildFilter(spec);
            bool result = filterFn(msg);
            Assert.IsTrue(result);
        }

    }
}