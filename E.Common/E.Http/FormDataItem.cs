namespace E.Http
{
    public class FormDataItem
    {
        public string Name;
        public string Value;
        public bool IsFile;
        public string FileName;
        public byte[] Content;
        public int ContentLength;

        public FormDataItem(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public FormDataItem(string name, string filename, byte[] content, int length)
        {
            IsFile = true;
            Name = name;
            FileName = filename;
            Content = content;
            ContentLength = length;
        }
    }
}
