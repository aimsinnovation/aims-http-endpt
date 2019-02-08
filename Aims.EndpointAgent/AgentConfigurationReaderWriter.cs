using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Aims.EndpointAgent
{
    public class AgentConfigurationReaderWriter<T>
    {
        private readonly string _path;

        public AgentConfigurationReaderWriter(string path)
        {
            _path = path;
        }

        public T Read()
        {
            string conf = File.ReadAllText(_path);
            T configurationModel = JsonConvert.DeserializeObject<T>(conf);
            return configurationModel;
        }

        public void Write(T model)
        {
            string jsonAgentConfigurationModel = JsonConvert.SerializeObject(model, Formatting.Indented);
            File.WriteAllText(_path, jsonAgentConfigurationModel);
        }
    }
}