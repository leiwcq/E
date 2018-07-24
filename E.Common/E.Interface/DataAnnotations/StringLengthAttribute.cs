namespace E.Interface.DataAnnotations
{
    public class StringLengthAttribute : AttributeBase
    {
        public const int MAX_TEXT = int.MaxValue;
        public int MinimumLength { get; set; }
        public int MaximumLength { get; set; }

        public StringLengthAttribute(int maximumLength)
        {
            MaximumLength = maximumLength;
        }

        public StringLengthAttribute(int minimumLength, int maximumLength)
        {
            MinimumLength = minimumLength;
            MaximumLength = maximumLength;
        }
    }
}