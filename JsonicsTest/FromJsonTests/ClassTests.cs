using Jsonics;
using NUnit.Framework;

namespace JsonicsTests.FromJsonTests
{
    [TestFixture]
    public class ClassTests
    {
        [Test]
        public void FromJson_TwoProperties_ReturnsClassInstance()
        {
            //arrange
            var jsonConverter = JsonFactory.Compile<TwoProperties>();

            //act
            var instance = jsonConverter.FromJson("{\"FirstName\":\"Ob\\t Won\",\"LastName\":\"Ken\\noby\",\"Age\":60,\"PowerFactor\":104.6789,\"IsJedi\":true}");

            //assert
            Assert.That(instance, Is.Not.Null);
        }

        public class TwoProperties
        {
            public int First
            {
                get;
                set;
            }

            public int Secon
            {
                get;
                set;
            }
        }

        [Test]
        public void FromJson_TestClass_PropertiesSetCorrectly()
        {
            //arrange
            var jsonConverter = JsonFactory.Compile<TwoProperties>();

            //act
            var instance = jsonConverter.FromJson("{\"First\":1,\"Secon\":2}");

            //assert
            Assert.That(instance.First, Is.EqualTo(1));
            Assert.That(instance.Secon, Is.EqualTo(2));
        }

        [TestCase("null")]
        [TestCase(" null")]
        [TestCase("\tnull")]
        public void FromJson_Null_ReturnsNull(string json)
        {
            //arrange
            var jsonConverter = JsonFactory.Compile<TwoProperties>();

            //act
            var instance = jsonConverter.FromJson(json);

            //assert
            Assert.That(instance, Is.Null);
        }

        public class ThreeProperties
        {
            public int First
            {
                get;
                set;
            }

            public int Second
            {
                get;
                set;
            }

            public int Third
            {
                get;
                set;
            }
        }

        [Test]
        public void FromJson_ThreeProperties_PropertiesSetCorrectly()
        {
            //arrange
            var jsonConverter = JsonFactory.Compile<ThreeProperties>();

            //act
            var instance = jsonConverter.FromJson("{\"First\":1,\"Second\":2,\"Third\":3}");

            //assert
            Assert.That(instance.First, Is.EqualTo(1));
            Assert.That(instance.Second, Is.EqualTo(2));
            Assert.That(instance.Third, Is.EqualTo(3));
        }

        public class CollisionProperties
        {
            public int AAA
            {
                get;
                set;
            }

            public int AAB
            {
                get;
                set;
            }

            public int BAA
            {
                get;
                set;
            }
        }

        [Test]
        public void FromJson_HashCollision_PropertiesSetCorrectly()
        {
            //arrange
            var jsonConverter = JsonFactory.Compile<CollisionProperties>();

            //act
            var instance = jsonConverter.FromJson("{\"AAA\":1,\"AAB\":2,\"BAA\":3}");

            //assert
            Assert.That(instance.AAA, Is.EqualTo(1));
            Assert.That(instance.AAB, Is.EqualTo(2));
            Assert.That(instance.BAA, Is.EqualTo(3));
        }

        public class TwoStrings
        {
            public string First;
            public string Second;
        }

        [Test]
        public void SpaceAfterLastProperty_CorrectlyDeserialized()
        {
            //arrange
            var json = "{\"First\":\"ok\",\"Second\":\"asdf\" }";
            IJsonConverter<TwoStrings> converter = JsonFactory.Compile<TwoStrings>();

            //act
            var result = converter.FromJson(json);

            //assert
            Assert.That(result.First, Is.EqualTo("ok"));
            Assert.That(result.Second, Is.EqualTo("asdf"));
        }
    }
}