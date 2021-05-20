using BestHTTP;

namespace Spaces.Core.RestAPI
{
    public delegate void RestEntryCallback(bool error, string reply);

    public class RestEntryRequestData { }

    public class RestEntryResponseData { }

    public class RestEntry
    {
        public HTTPMethods entryType;
        public int retryAttempt = 0;
        public string fullUrl;
        public RestManagerCallback managerCallback;
        public HTTPRequest request;
        public byte[] data;
    }
}