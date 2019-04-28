namespace WorkReflection
{
    public class Data
    {
        public int IntValue { get; set; }

        public string StringValue { get; set; }

        public Data()
        {
        }

        public Data(int intValue, string stringValue)
        {
            IntValue = intValue;
            StringValue = stringValue;
        }
    }
}
