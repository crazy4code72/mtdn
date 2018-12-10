namespace VotingWeb.Helper
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    public static class ConfigFileReader
    {
        private static readonly string ConfigFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "config.json");

        /// <summary>
        /// Read the json file and return the deserialized json content
        /// </summary>
        /// <returns>Dictionary of config entries</returns>
        public static Dictionary<string, string> GetConfigFromJsonFile()
        {
            var jsonData = File.ReadAllText(ConfigFilePath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
        }
    }
}
