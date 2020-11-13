using Newtonsoft.Json;

namespace TextTranslations.Model
{
    public class Translations
    {
        [JsonProperty("text")]
        public string text { get; set; }

        [JsonProperty("to")]
        public string to { get; set; }
    }
}