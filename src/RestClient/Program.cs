using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Spectre.Console;

namespace RestClient {
    internal static class Program {
        /// <summary>
        ///
        /// </summary>
        /// <param name="file"></param>
        /// <param name="header"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private static async Task Main(FileInfo file, bool header = false, bool status = false) {
            var definition = File.ReadAllText(file.FullName);
            var request = RequestParser.XRequest(definition);

            var ps = new Processor(new CommandOptions {
                ShowHeader = header,
                ShowStatus = status
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
