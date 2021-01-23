using System;
using System.Collections.Generic;

using static RestClient.RequestParser;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

namespace RestClient {

    class Program {

        static async Task ProcessGet(HttpClient client, string url) {
            var result = await client.GetAsync(url);
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
                // Console.WriteLine(body);
            }

        }

        static async Task Main(string[] args) {
            // var r = File.ReadAllText("http/register.rest");
            var r = File.ReadAllText("http/users.rest");
            var request = XRequest(r);

            var client = new HttpClient();
            if (request.Method == Method.Get) {
                await ProcessGet(client, request.Url);
            }
        }
    }
}
