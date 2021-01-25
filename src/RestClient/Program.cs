﻿using System.IO;
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
        /// <returns></returns>
        private static async Task Main(FileInfo file, bool header = false) {
            var definition = File.ReadAllLines(file.FullName)
                .Where(x => !x.TrimStart().StartsWith("//"));

            var request = RequestParser.XRequest(string.Concat(definition));

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
