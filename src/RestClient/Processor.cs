using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.Text;
using Spectre.Console;

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
                    AnsiConsole.Markup($"[green]{current.Key}[/]: ");
                    Console.WriteLine($"{String.Join(", ", current.Value)}");
                }

                while (contentHeaders.MoveNext()) {
                    var current = contentHeaders.Current;
                    AnsiConsole.Markup($"[green]{current.Key}[/]: ");
                    Console.WriteLine($"{String.Join(", ", current.Value)}");
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
                return (false, null);
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

            var (contentTypeOk, contentType) = GetHeaderValue(headers, "Content-Type");
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

            var customHeaders = headers
                .Where(x => x.Key != "Content-Type")
                .Where(x => x.Key != "Authorization");

            foreach (var header in customHeaders) {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            var content = new StringContent(body, Encoding.UTF8, contentTypeOk ? contentType : "text/plain");
            var response = await client.PostAsync(url, content);
            await PrintResponse(response);
        }
    }
}