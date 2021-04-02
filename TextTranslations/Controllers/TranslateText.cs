using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TextTranslations.Model;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TextTranslations.Controllers
{
    [ApiController]
    [Route("api/text/{lang}")]
    public class TranslateText : ControllerBase
    {
        static string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");
        static string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");
        string uriBase = endpoint + "vision/v3.0/ocr";

        static string subscriptionKey1 = Environment.GetEnvironmentVariable("TRANSLATOR_SUBSCRIPTION_KEY");
        static string endpoint1 = Environment.GetEnvironmentVariable("TRANSLATOR_ENDPOINT");
        static string location = Environment.GetEnvironmentVariable("LOCATION");

        static string speechKey = Environment.GetEnvironmentVariable("SPEECH_TRANSLATOR_SUBSCRIPTION_KEY");
        static string speechEndpoint = Environment.GetEnvironmentVariable("SPEECH_TRANSLATOR_KEY");
        static string firstpath = Path.GetFullPath("C:\\Users\\HP 650\\source\\repos\\TextTranslations\\Speaker.ssml");
        static string path = Path.GetFullPath("C:\\Users\\HP 650\\source\\repos\\TextTranslations\\Speakers.ssml");
        string uriBase1 = endpoint1+ "translate";

        [HttpPost]
        public async Task<ActionResult<JsonProperty>> Post(IFormFile file,string lang)
        {
            try
            {
                if (lang == null)
                {
                    return BadRequest("language not requested");
                }
                if (file == null)
                {
                return this.StatusCode(StatusCodes.Status404NotFound, "file not found");
                }
                string details = await MakeAnalysisRequest(file);
                Analyze analyze = JsonConvert.DeserializeObject<Analyze>(details);
                var words = analyze.regions.SelectMany(e => e.lines.SelectMany(e => e.words).Select(s => s.text)).ToList();
                string sentence = String.Join(" ", words);
                Texts text = new Texts
                {
                Text = sentence
                };
                List<Texts> texts = new List<Texts>
                {
                text
                };
                var newlang = await Translate(texts, lang);
                List<Translated> translations = JsonConvert.DeserializeObject<List<Translated>>(newlang);
                var speech = await TexttoSpeech(translations.SelectMany(r => r.translations.Select(r => r.text)));
 
                return Ok(speech);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
          
        }

        [NonAction]
        private async Task<string> MakeAnalysisRequest(IFormFile image)
        {

            try
            {
                var client = GetClient(subscriptionKey);
                string uri = uriBase;
                HttpResponseMessage httpResponse;
                byte[] bytedata = GetImageAsByteArray(image.FileName);
                using (ByteArrayContent content = new ByteArrayContent(bytedata))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    httpResponse = await client.PostAsync(uri, content);
                }
                string contentString = await httpResponse.Content.ReadAsStringAsync();
                var newContent = JToken.Parse(contentString).ToString();
                return newContent;


            }
            catch (Exception e)
            {
                string message = "Language format not accepted";
                return message;
            }
        }
        [NonAction]
        private async Task<string> Translate(List<Texts> texts,string lang)
        {

            try
            {
                var client = GetClient(subscriptionKey1);
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region",location);
                var requestparameter = "api-version=3.0&to=" + lang;
                string uri = uriBase1+"?"+requestparameter;
                HttpResponseMessage httpResponse;
                var json = JsonConvert.SerializeObject(texts);
                using (StringContent content = new StringContent(json))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    httpResponse = await client.PostAsync(uri, content);
                }
                string contentString = await httpResponse.Content.ReadAsStringAsync();
                var newContent = JToken.Parse(contentString).ToString();
                return newContent;


            }
            catch (Exception e)
            {

                return e.Message;
            }
        }
   
        private async Task<string> TexttoSpeech(IEnumerable<string> text)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", speechKey);
                client.DefaultRequestHeaders.Add("X-Microsoft-OutputFormat", "raw-24khz-16bit-mono-pcm");
                client.DefaultRequestHeaders.Add("User-Agent", "TranslateSpeech");
                HttpResponseMessage httpResponse;
                var singleString = string.Join("", text.ToArray());
                ReadandWriteText(singleString);
                var str = new FileStream(path,FileMode.Open);
                using (StreamContent content = new StreamContent(str))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/ssml+xml");
                    httpResponse = await client.PostAsync(speechEndpoint, content);
                }
                var contentStream = await httpResponse.Content.ReadAsStringAsync();
                var newContent = JToken.Parse(contentStream).ToString();
                return newContent;

            }
            catch (Exception e)
            {

                return e.Message;
            }
        }
        private HttpClient GetClient(string key) 
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
            return client;
        }
        private byte[] GetImageAsByteArray(string fileName)
        {
            using FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader binary = new BinaryReader(fileStream);
            return binary.ReadBytes((int)fileStream.Length);
        }
        private void ReadandWriteText(string text)
        {
           var st = System.IO.File.ReadAllLines(firstpath);
            StringBuilder s = new StringBuilder();
            for(int i = 0; i < st.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(st[i])) 
                {
                    s.Append(text);
                }
                else 
                {
                    s.Append(st[i]);
                }     
            }
            var str = new StreamWriter(path);
            str.Write(s.ToString());
            str.Close();   
        }
       
    }
}
