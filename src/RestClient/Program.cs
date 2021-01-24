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

    class Processor {
        private readonly CommandOptions _options;
        public Processor(CommandOptions options) {
            _options = options;
        }

        async Task PrintResponse(HttpResponseMessage result) {
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

        (bool, string) GetHeaderValue(Header[] headers, string key) {
            var h = headers.FirstOrDefault(x => x.Key == key);
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
            var (ok, contentType) = GetHeaderValue(headers, "Content-Type");
            var content = new StringContent(body, Encoding.UTF8, contentType);
            var response = await client.PostAsync(url, content);
            await PrintResponse(response);
        }

    }

    class Program {

        /// <summary>
        ///
        /// </summary>
        /// <param name="file"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        static async Task Main(FileInfo file, bool header = false) {
            var r = File.ReadAllText(file.FullName);
            var request = XRequest(r);

            var ps = new Processor(new CommandOptions {
                ShowHeader = header
            });

            var client = new HttpClient();
            if (request.Method == Method.Get) {
                await ps.ProcessGet(client, request);
            } else if (request.Method == Method.Post) {
                await ps.ProcessPost(client, request);
            }
        }
    }
}
