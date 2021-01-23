using System;
using System.Collections.Generic;
using Sprache;
using System.Linq;

using static RestClient.RequestParser;
using System.IO;

namespace RestClient {
    public enum Method { Get, Post, Delete }

    public class Header {
        public string Key { set; get; }
        public string Value { set; get; }
    }

    public class Request {
        public List<Header> Headers { set; get; }
        public Method Method { set; get; }
        public string Body { set; get; }
    }

    public class RequestParser {

        public static readonly Parser<string> XMethod =
            Parse.String("POST")
                .Or(Parse.String("DELETE"))
                .Or(Parse.String("PUT"))
                .Or(Parse.String("GET")).Text();

        public static readonly Parser<string> XUrl =
            Parse.AnyChar.Except(Parse.Char('\n')).Many().Text();

        public static readonly Parser<char> XSpace = Parse.Char(' ');

    }

    class Program {
        static void Main(string[] args) {

            var r = File.ReadAllText("http/register.rest");

            var x =
                from method in XMethod
                from empty in XSpace
                from url in XUrl
                select new {
                    Method = method,
                    Url = url
                };

            var result = x.Parse(r);

            Console.WriteLine(result.Method);
            Console.WriteLine(result.Url);

        }
    }
}
