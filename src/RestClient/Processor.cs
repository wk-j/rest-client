using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.Text;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace RestClient {
    public class Processor {
        private readonly CommandOptions _options;

        public Processor(CommandOptions options) {
            _options = options;
        }

        private string UnescapeUnicode(string input) {
            return Regex.Unescape(input);
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


            if (_options.ShowStatus) {
                AnsiConsole.Markup($"[blue]{result.StatusCode}[/]\n");
                Console.WriteLine();
            }

            var ok = result.Content.Headers.TryGetValues("Content-Type", out var contentType);
            if (ok && contentType.Any(x => x.Contains("application/json"))) {
                var obj = JsonSerializer.Deserialize<dynamic>(body);
                var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions {
                    WriteIndented = true
                });
                var pretty = UnescapeUnicode(json);
                Console.WriteLine(pretty);
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

        private void AppendHeader(HttpClient client, RequestInfo info) {
            var headers = info.Headers;

            var (authorizationOk, authorization) = GetHeaderValue(headers, "Authorization");
            var (acceptOk, accept) = GetHeaderValue(headers, "Accept");

            if (authorizationOk) {
                var tokens = authorization.Trim().Split(' ');
                var scheme = tokens[0].Trim();
                var token = tokens[1].Trim();

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    scheme,
                    token
                );
            }

            if (acceptOk) {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(accept));
            }

            var customHeaders = headers
                .Where(x => x.Key != "Content-Type")
                .Where(x => x.Key != "Accept")
                .Where(x => x.Key != "Authorization");

            foreach (var header in customHeaders) {
                Console.WriteLine("Custom header {0}:{1}", header.Key, header.Value);
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        public async Task ProcessGet(HttpClient client, RequestInfo info) {
            var url = info.Url;
            AppendHeader(client, info);
            var result = await client.GetAsync(url);
            await PrintResponse(result);
        }

        public async Task ProcessPost(HttpClient client, RequestInfo info) {
            var url = info.Url;
            var headers = info.Headers;
            var body = info.Body;
            AppendHeader(client, info);

            var (contentTypeOk, contentType) = GetHeaderValue(headers, "Content-Type");

            var content = new StringContent(body, Encoding.UTF8, contentTypeOk ? contentType : "text/plain");
            var response = await client.PostAsync(url, content);
            await PrintResponse(response);
        }
    }
}