namespace RestClient {
    public class RequestInfo {
        public string Url { set; get; }
        public Header[] Headers { set; get; }
        public Method Method { set; get; }
        public string Body { set; get; }
    }
}
