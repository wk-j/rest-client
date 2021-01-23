using System;
using System.Collections.Generic;

using static RestClient.RequestParser;
using System.IO;

namespace RestClient {

    class Program {
        static void Main(string[] args) {
            var r = File.ReadAllText("http/register.rest");
            var result = XRequest(r);

            Console.WriteLine(result.Method);
            Console.WriteLine(result.Url);

            foreach (var item in result.Headers) {
                Console.WriteLine($" H {item.Key}:{item.Value}");
            }

            Console.WriteLine(result.Body);
        }
    }
}
