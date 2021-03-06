using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace JsonicsTests.TestCaseSources
{
    public static class DecimalTestCaseSource
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(0M, "0");
                yield return new TestCaseData(1M, "1");
                yield return new TestCaseData(-1M, "-1");
                yield return new TestCaseData(42M, "42");
                yield return new TestCaseData(-42M, "-42");
                yield return new TestCaseData(-42.42M, "-42.42");
                yield return new TestCaseData(79228162514.264337593543950335M, "79228162514.264337593543950335");
                yield return new TestCaseData(decimal.MaxValue, "79228162514264337593543950335");
                yield return new TestCaseData(decimal.MinValue, "-79228162514264337593543950335");
            }
        }

        public static IEnumerable ToJsonNullableTestCases
        {
            get
            {
                foreach(var testCase in TestCases)
                {
                    yield return testCase;
                }
                yield return new TestCaseData(null, "null");
                
            }
        }

        public static IEnumerable FromJsonNullableTestCases
        {
            get
            {
                foreach(var testCase in TestCases)
                {
                    yield return testCase;
                }
                yield return new TestCaseData(null, " null");
                yield return new TestCaseData(null, "\nnull");
                yield return new TestCaseData(null, "\t null");
            }
        }

        public class NullableDecimalClass
        {
            public decimal? Property 
            {
                get;
                set;
            }
        }
    }
}