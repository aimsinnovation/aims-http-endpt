using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aims.EndpointAgent
{
    public class AgentConfigurationReader<T>
    {
        public T Json { get; }

        public AgentConfigurationReader(string path)
        {
            var conf = File.ReadAllText(path);
            var json = JObject.Parse(conf);
            foreach (var item in json.Descendants().ToList().Where(d => d.Path.ToLower().EndsWith("$ref")))
            {
                if (!item.HasValues) continue;
                var token = item.First.ToString().TrimStart('#').TrimStart('/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .Aggregate<string, JToken>(json,
                        (current, t) => Regex.IsMatch(t, @"^\d+$") ? current[int.Parse(t)] : current[t]);
                item.Parent.Replace(token);
            }

            Json = JsonConvert.DeserializeObject<T>(json.ToString(), new JsonSerializerSettings()
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            });
        }
    }
}