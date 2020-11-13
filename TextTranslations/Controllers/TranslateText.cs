using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TextTranslations.Controllers
{
    [ApiController]
    [Route("api/text")]
    public class TranslateText : ControllerBase
    {
        static string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");
        static string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");

        string uriBase = endpoint + "vision/v3.0/ocr";
        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null)
            {
                return this.StatusCode(StatusCodes.Status404NotFound, "file not found");
            }
            string details = await MakeAnalysisRequest(file);
            return Ok(details);
        }

        [NonAction]
        private async Task<string> MakeAnalysisRequest(IFormFile image)
        {

            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
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

                return e.Message;
            }
        }
        private byte[] GetImageAsByteArray(string fileName)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binary = new BinaryReader(fileStream);
                return binary.ReadBytes((int)fileStream.Length);

            }
        }
    }
}
