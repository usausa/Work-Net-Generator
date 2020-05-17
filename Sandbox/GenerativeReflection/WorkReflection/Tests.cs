namespace WorkReflection
{
    using System;
    using System.Diagnostics;

    public class SampleDataTest
    {
        public static void Test()
        {
            TestConstructor();
            TestConstructor2();

            TestIntValueAccessor();
            TestStringValueAccessor();
        }

        private static void TestConstructor()
        {
            var metadata = new SampleDataTypeMetadata();
            var activator = metadata.FindActivator(typeof(Data).GetConstructor(Type.EmptyTypes));

            var obj = activator.Create();

            Debug.Assert(obj != null);
        }

        private static void TestConstructor2()
        {
            var metadata = new SampleDataTypeMetadata();
            var activator = metadata.FindActivator(typeof(Data).GetConstructor(new[] { typeof(int), typeof(string) }));

            var obj = (Data)activator.Create(1, "abc");

            Debug.Assert(obj != null);
            Debug.Assert(obj.IntValue == 1);
            Debug.Assert(obj.StringValue == "abc");
        }

        private static void TestIntValueAccessor()
        {
            var metadata = new SampleDataTypeMetadata();
            var accessor = metadata.FindAccessor(typeof(Data).GetProperty(nameof(Data.IntValue)));

            var obj = new Data();
            accessor.SetValue(obj, 1);
            Debug.Assert((int)accessor.GetValue(obj) == 1);
            accessor.SetValue(obj, null);
            Debug.Assert((int)accessor.GetValue(obj) == default);
        }

        private static void TestStringValueAccessor()
        {
            var metadata = new SampleDataTypeMetadata();
            var accessor = metadata.FindAccessor(typeof(Data).GetProperty(nameof(Data.StringValue)));

            var obj = new Data();
            accessor.SetValue(obj, "abc");
            Debug.Assert((string)accessor.GetValue(obj) == "abc");
            accessor.SetValue(obj, null);
            Debug.Assert((string)accessor.GetValue(obj) == default);
        }
    }
}
