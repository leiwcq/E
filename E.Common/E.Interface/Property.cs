using System.Collections.Generic;
using System.Runtime.Serialization;

namespace E.Interface
{
    [DataContract]
    public class Property
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }
    }

    [CollectionDataContract(ItemName = "Property")]
    public class Properties : List<Property>
    {
        public Properties() { }
        public Properties(IEnumerable<Property> collection) : base(collection) { }
    }
}