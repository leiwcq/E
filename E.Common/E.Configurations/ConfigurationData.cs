using System;
using System.Runtime.Serialization;

namespace E.Configurations
{
    public interface IConfigurationData
    {
    }

    public interface IConfigurationData<T>: IConfigurationData
    {
        T Configuration { get; set; }
    }

    [Serializable]
    [DataContract(Name = "Root")]
    public class ConfigurationData<T>: IConfigurationData<T>
    {
        [DataMember(Name = "Configuration")]
        public T Configuration { get; set; }
    }
}
