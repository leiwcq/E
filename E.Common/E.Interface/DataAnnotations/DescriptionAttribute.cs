namespace E.Interface.DataAnnotations
{
    public class DescriptionAttribute : AttributeBase
    {
        public string Description { get; set; }

        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}