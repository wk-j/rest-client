using Sprache;
using System.Linq;

namespace RestClient {
    public static class RequestParser {
        private static Method ParseMethod(string input) =>
            input switch {
                "POST" => Method.Post,
                "PUT" => Method.Put,
                "DELETE" => Method.Delete,
                "GET" => Method.Get,
                _ => Method.X
            };

        private static readonly Parser<string> XMethod =
            Parse.String("POST")
                .Or(Parse.String("DELETE"))
                .Or(Parse.String("PUT"))
                .Or(Parse.String("GET"))
                .Text();

        private static readonly Parser<string> XUrl =
            Parse.AnyChar
                .Except(Parse.Char('\n'))
                .Many()
                .Text();

        private static readonly Parser<char> XSpace = Parse.Char(' ');

        private static readonly Parser<string> XLine =
            Parse.WhiteSpace
                .Many()
                .Text();

        private static readonly Parser<string> XBody =
            Parse.AnyChar.Many().Text();

        private static readonly Parser<Header> XHeader =
            from key in Parse.AnyChar
                .Except(Parse.Char(':')
                .Or(Parse.Char('{')))
                .Many()
                .Text()
            from colon in Parse.String(":")
                .Or(Parse.String(": "))
                .Text()
            from value in Parse.AnyChar
                .Except(Parse.Char('\n'))
                .Many()
                .Text()
            select new Header {
                Key = key.Trim(),
                Value = value.Trim()
            };

        public static RequestInfo XRequest(string text) {
            var lines = text.Split('\n').Where(x => !x.TrimStart().StartsWith("//"));
            var clean = string.Join('\n', lines);

            var parser =
                from method in XMethod
                from empty in XSpace
                from url in XUrl
                from headers in XHeader.Many()
                from space in XLine
                from body in XBody
                select new RequestInfo {
                    Method = ParseMethod(method),
                    Url = url,
                    Headers = headers.ToArray(),
                    Body = body
                };

            return parser.Parse(clean);
        }
    }
}
