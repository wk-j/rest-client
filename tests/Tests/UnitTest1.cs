using System;
using System.IO;
using RestClient;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var register = File.ReadAllText("../../../../../http/register.rest");
            var info = RequestParser.XRequest(register);

            var headers = info.Headers;
            Assert.Equal(2, headers.Length);
        }
    }
}
