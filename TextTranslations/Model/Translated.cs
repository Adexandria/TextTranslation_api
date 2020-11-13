using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TextTranslations.Model
{
    public class Translated
    { 
        [JsonProperty("translations")]
        public IList<Translations> translations { get; set; }
    }
}
