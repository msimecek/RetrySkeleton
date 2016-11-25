using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RetryProtype
{
    public class Program
    {
        private static int maxRetries = 5;
        
        public static void Main()
        {
            for (int i = 0; i < 20; i++)
            {
                CallApiWithRetry(i);
            }
            
            Console.ReadKey();
        }

        private static async Task CallApiWithRetry(int runIteration = 0)
        {
            int retries = 0;

            Stream image = new MemoryStream(File.ReadAllBytes("C:\\Users\\msimecek\\Desktop\\faces.png"));

            while (true)
            {
                try 
                {
                    Console.WriteLine($"iteration {runIteration}, retry {retries}");
                   
                    MemoryStream image2 = new MemoryStream();
                    image.CopyTo(image2);
                    image2.Seek(0, 0);
                    image.Seek(0, 0);

                    await CallApiAsync(image2);
                    return;
                }
                catch (Exception ex) 
                {
                    Console.WriteLine("Throttled.");
                    retries++;

                    if (retries > maxRetries)
                    {
                        throw;
                    }
                }

                await Task.Delay(1000);
            }
        }
        
        private static async Task<bool> CallApiAsync(Stream image)
        {
            using (var client = new HttpClient())
            {
                string apiKey = "keykeykey";
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

                var content = new StreamContent(image);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var httpResponse = await client.PostAsync("https://api.projectoxford.ai/emotion/v1.0/recognize", content);

                image.Dispose();

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var response = await httpResponse.Content.ReadAsStringAsync();
                    //return await httpResponse.Content.ReadAsStringAsync();
                    Console.WriteLine(response);
                    return true;
                }
                else if (httpResponse.StatusCode.ToString() == "429")
                {
                    Console.WriteLine($"Cognitive Services throttling, throwing exception.");
                    throw new Exception(await httpResponse.Content.ReadAsStringAsync());
                }
                else
                {
                    Console.WriteLine($"Cognitive Services error: ({httpResponse.StatusCode}) " + await httpResponse.Content.ReadAsStringAsync());
                }
            }

            return false;
        }
    }
}