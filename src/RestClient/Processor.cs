using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.Text;

namespace RestClient {
    public class Processor {
        private readonly CommandOptions _options;

        public Processor(CommandOptions options) {
            _options = options;
        }

        private async Task PrintResponse(HttpResponseMessage result) {
            var body = await result.Content.ReadAsStringAsync();
            var headers = result.Headers.GetEnumerator();
            var contentHeaders = result.Content.Headers.GetEnumerator();

            if (_options.ShowHeader) {
                while (headers.MoveNext()) {
                    var current = headers.Current;
                    Console.WriteLine($"{current.Key}: {string.Join(", ", current.Value)}");
                }

                while (contentHeaders.MoveNext()) {
                    var current = contentHeaders.Current;
                    Console.WriteLine($"{current.Key}: {string.Join(", ", current.Value)}");
                }
                Console.WriteLine();
            }

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

        private (bool, string) GetHeaderValue(Header[] headers, string key) {
            var h = Array.Find(headers, x => x.Key == key);
            if (h == null) {
                return (true, "text/plain");
            }
            return (true, h.Value);
        }

        public async Task ProcessGet(HttpClient client, RequestInfo info) {
            var url = info.Url;
            var result = await client.GetAsync(url);
            await PrintResponse(result);
        }

        public async Task ProcessPost(HttpClient client, RequestInfo info) {
            var url = info.Url;
            var headers = info.Headers;
            var body = info.Body;

            var (_, contentType) = GetHeaderValue(headers, "Content-Type");
            var (authorizationOk, authorization) = GetHeaderValue(headers, "Authorization");

            if (authorizationOk) {
                var tokens = authorization.Trim().Split(' ');
                var scheme = tokens[0].Trim();
                var token = tokens[1].Trim();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    scheme,
                    token
                );
            }

            var content = new StringContent(body, Encoding.UTF8, contentType);
            var response = await client.PostAsync(url, content);
            await PrintResponse(response);
        }
    }
}