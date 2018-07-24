using System;
using System.IO;
using System.Reflection;
using System.Text;
using E.Serializer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace E.Configurations
{
    public class AssemblyConfiguration
    {
        protected string BaseDir;
        protected string FileName;
        protected string PathAndFileName;
        protected bool NeedCreateFile;
        protected string DefaultConfigurationJson;
        protected string RootKey;

        public IConfigurationRoot Configuration;

        public IConfigurationSection RootSection;

        /// <inheritdoc />
        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        public AssemblyConfiguration()
            : this(Assembly.GetCallingAssembly().GetName().Name)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        /// <param name="fileName">配置文件名</param>
        public AssemblyConfiguration(string fileName)
            :this(fileName,"Configuration")
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        /// <param name="fileName">配置文件名</param>
        /// <param name="rootKey">根节点名称</param>
        public AssemblyConfiguration(string fileName,string rootKey)
            :this(fileName,rootKey,null)
        {
        }

        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        /// <param name="fileName">配置文件名</param>
        /// <param name="rootKey">根节点名称</param>
        /// <param name="defaultConfigurationData">默认配置文件实体数据</param>
        public AssemblyConfiguration(string fileName, string rootKey, IConfigurationData defaultConfigurationData)
        {
            FileName = fileName;
            BaseDir = Path.Combine(AppContext.BaseDirectory, "Config");
            PathAndFileName = Path.Combine(BaseDir, fileName);
            RootKey = rootKey;

            if (defaultConfigurationData != null)
            {
                if (!File.Exists(PathAndFileName))
                {
                    DefaultConfigurationJson = defaultConfigurationData.ToJson(Formatting.Indented);
                    NeedCreateFile = true;
                }
            }

            Init();
        }

        protected AssemblyConfiguration(string fileName, string rootKey, bool init=false)
        {
            FileName = fileName;
            BaseDir = Path.Combine(AppContext.BaseDirectory, "Config");
            PathAndFileName = Path.Combine(BaseDir, fileName);
            if (string.IsNullOrWhiteSpace(rootKey))
            {
                RootKey = "Configuration";
            }
            else
            {
                RootKey = rootKey;
            }

            if (init)
            {
                Init();
            }
        }

        protected virtual void Init()
        {
            if (!File.Exists(PathAndFileName) && NeedCreateFile)
            {
                if (!string.IsNullOrWhiteSpace(DefaultConfigurationJson))
                {
                    var difo = new DirectoryInfo(BaseDir);
                    if (!difo.Exists) difo.Create();
                    using (var sw = new StreamWriter(PathAndFileName, false, Encoding.UTF8))
                    {
                        sw.Write(DefaultConfigurationJson);
                        sw.Close();
                    }
                }
                else
                {
                    throw new Exception("没有找到配置文件");
                }
            }

            if (File.Exists(PathAndFileName))
            {

                var builder = new ConfigurationBuilder()
                    .SetBasePath(BaseDir)
                    .AddJsonFile(FileName, true, true);
                Configuration = builder.Build();
                RootSection = Configuration.GetSection(RootKey);
            }
        }

        public string Get(string key)
        {
            if (RootSection != null && RootSection.Exists())
                return RootSection[key];
            return string.Empty;
        }
    }

    public class AssemblyConfiguration<T>: AssemblyConfiguration
    {
        /// <inheritdoc />
        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        public AssemblyConfiguration()
            : this(Assembly.GetCallingAssembly().GetName().Name)
        {
        }

        public T Root;

        /// <inheritdoc />
        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        /// <param name="fileName">配置文件名</param>
        public AssemblyConfiguration(string fileName)
            : this(fileName, default(T))
        {
        }

        /// <summary>
        /// 打开自定义配置文件
        /// </summary>
        /// <param name="fileName">配置文件名</param>
        /// <param name="defaultConfigurationData">默认配置文件实体数据</param>
        public AssemblyConfiguration(string fileName, T defaultConfigurationData) : base(fileName, "Configuration",
            false)
        {
            if (defaultConfigurationData != null)
            {
                if (!File.Exists(PathAndFileName))
                {
                    var configData = new ConfigurationData<T>
                    {
                        Configuration = defaultConfigurationData
                    };
                    DefaultConfigurationJson = configData.ToJson(Formatting.Indented);
                    NeedCreateFile = true;
                }
            }
            Init();
            Root = RootSection.Get<T>();
        }
    }
}
