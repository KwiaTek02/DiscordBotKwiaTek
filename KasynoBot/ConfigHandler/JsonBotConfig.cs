using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KasynoBot.ConfigHandler
{
    public sealed class JsonBotConfig
    {
        public string DiscordBotToken { get; set; }
        public string DiscordBotPrefix { get; set; }

        public async Task ReadJSON()
        {
            using (StreamReader sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JSONStruct obj = JsonConvert.DeserializeObject<JSONStruct>(json);

                this.DiscordBotToken = obj.token;
                this.DiscordBotPrefix = obj.prefix;

            }
            
        }
    }

    internal sealed class JSONStruct
    {
        public string token { get; set; }
        public string prefix { get; set; }
    }
}
