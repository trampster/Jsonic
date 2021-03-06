using System;
using System.Collections;
using Jsonics;
using NUnit.Framework;

namespace JsonicsTests.FromJsonTests
{
    public class NullableDateTimeTestCaseData
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData("\"2017-07-25\"", new DateTime(2017,7,25), DateTimeKind.Unspecified);
                yield return new TestCaseData("\"2017-07-25T23:59:58\"", new DateTime(2017,7,25,23,59,58), DateTimeKind.Unspecified);
                var dateTime = new DateTime(2017,7,25,23,59,58).AddMilliseconds(196.329);
                yield return new TestCaseData("\"2017-07-25T23:59:58.196329\"", dateTime, DateTimeKind.Unspecified);
                dateTime = new DateTime(2017,7,25,23,59,58).AddMilliseconds(123.45678);
                yield return new TestCaseData("\"2017-07-25T23:59:58.12345678\"", dateTime, DateTimeKind.Unspecified);
                dateTime = new DateTime(2017,7,25,23,59,58, DateTimeKind.Utc).AddMilliseconds(123.45678);

                //utc
                yield return new TestCaseData("\"2017-07-25T23:59:58.12345678Z\"", dateTime, DateTimeKind.Utc);
                
                //with offset
                var utc = new DateTime(2017,7,25,23,59,58, DateTimeKind.Utc).AddMilliseconds(123.45678).Subtract(new TimeSpan(3,15,0));
                var local = utc.ToLocalTime();
                yield return new TestCaseData("\"2017-07-25T23:59:58.12345678+03:15\"", local, DateTimeKind.Local);

                //whitespace at start
                yield return new TestCaseData(" \"2017-07-25\"", new DateTime(2017,7,25), DateTimeKind.Unspecified);

                //lazy doesn't start at beginning
                yield return new TestCaseData(" \"2017-07-25\"", new DateTime(2017,7,25), DateTimeKind.Unspecified);

                yield return new TestCaseData("null", null, DateTimeKind.Unspecified);
                yield return new TestCaseData(" null", null, DateTimeKind.Unspecified);
            }
        }
    }

    [TestFixture]
    public class NullableDateTimeTests
    {
        IJsonConverter<NullableDateTimeClass> _propertyFactory;
        IJsonConverter<DateTime?> _valueFactory;
        

        public class NullableDateTimeClass
        {
            public DateTime? Property
            {
                get;
                set;
            }
        }

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            _propertyFactory = JsonFactory.Compile<NullableDateTimeClass>();
            _valueFactory = JsonFactory.Compile<DateTime?>();
        }

        [Test, TestCaseSource(typeof(DateTimeTestCaseData), "TestCases")]
        public void NullableDateTimeProperty_CorrectlyDeserialized(string value, DateTime? expectedDateTime, DateTimeKind expectedKind)
        {
            //arrange
            //act
            var result = _propertyFactory.FromJson($"{{\"Property\":{value}}}");

            //assert
            Assert.That(result.Property, Is.EqualTo(expectedDateTime));
            if(expectedDateTime != null)
            {
                Assert.That(result.Property.Value.Kind, Is.EqualTo(expectedKind));
            }
        }

        [Test, TestCaseSource(typeof(DateTimeTestCaseData), "TestCases")]
        public void NullableDateTimeValue_CorrectlyDeserialized(string value, DateTime? expectedDateTime, DateTimeKind expectedKind)
        {
            //arrange
            //act
            DateTime? result = _valueFactory.FromJson($"{value}");

            //assert
            Assert.That(result, Is.EqualTo(expectedDateTime));
            if(expectedDateTime != null)
            {
                Assert.That(result.Value.Kind, Is.EqualTo(expectedKind));
            }
        }
    }
}