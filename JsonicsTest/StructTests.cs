using Jsonics;
using NUnit.Framework;

namespace JsonicsTest
{
    [TestFixture]
    public class StructTests
    {
        public struct TestStruct
        {
            public int Num
            {
                get;
                set;
            }
        }

        [Test]
        public void ToJson_Struct_ExpectedJson()
        {
            //arrange
            var converter = JsonFactory.Compile<TestStruct>();
            var testStruct = new TestStruct()
            {
                Num = 42
            };

            //act
            var json = converter.ToJson(testStruct);

            //assert
            Assert.That(json, Is.EqualTo("{\"Num\":42}"));
        }
    }
}