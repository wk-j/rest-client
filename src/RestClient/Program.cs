using System;
using System.Collections.Generic;

using static RestClient.RequestParser;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.Text;

namespace RestClient {

    class Program {

        static async Task PrintResponse(HttpResponseMessage result) {
            var body = await result.Content.ReadAsStringAsync();
            var headers = result.Headers.GetEnumerator();
            while (headers.MoveNext()) {
                var current = headers.Current;
                Console.WriteLine($"{current.Key}: {string.Join(", ", current.Value)}");
            }

            var c = result.Content.Headers.GetEnumerator();
            while (c.MoveNext()) {
                var current = c.Current;
                Console.WriteLine($"{current.Key}: {string.Join(", ", current.Value)}");
            }

            Console.WriteLine();

            var ok = result.Content.Headers.TryGetValues("Content-Type", out var contentType);
            if (ok && contentType.Any(x => x.Contains("application/json"))) {
                var obj = JsonSerializer.Deserialize<dynamic>(body);
                var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions {
                    WriteIndented = true
                });
                Console.WriteLine(json);
            } else {
                Console.WriteLine(body);
            }

        }

        private static (bool, string) GetHeaderValue(Header[] headers, string key) {
            var h = headers.FirstOrDefault(x => x.Key == key);
            if (h == null) {
                return (true, "text/plain");
            }
            return (true, h.Value);
        }

        private static async Task ProcessGet(HttpClient client, RequestInfo info) {
            var url = info.Url;
            var result = await client.GetAsync(url);
            await PrintResponse(result);
        }

        private static async Task ProcessPost(HttpClient client, RequestInfo info) {
            var url = info.Url;
            var headers = info.Headers;
            var body = info.Body;
            var (ok, contentType) = GetHeaderValue(headers, "Content-Type");
            var content = new StringContent(body, Encoding.UTF8, contentType);
            var response = await client.PostAsync(url, content);
            await PrintResponse(response);
        }

        static async Task Main(string[] args) {
            // var r = File.ReadAllText("http/register.rest");
            var r = File.ReadAllText("http/register.rest");
            var request = XRequest(r);

            var client = new HttpClient();
            if (request.Method == Method.Get) {
                await ProcessGet(client, request);
            } else if (request.Method == Method.Post) {
                await ProcessPost(client, request);
            }
        }
    }
}
