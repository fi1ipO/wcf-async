namespace AsyncLocalWCFTest
{
    public class UserIdDelegatingHandler : DelegatingHandler
    {
        private static readonly AsyncLocal<ContextHolder> AsyncLocalContextHolder = new();

        public UserIdDelegatingHandler()
        {
            InnerHandler = new HttpClientHandler();
        }

        public void SetContext(string? userId)
        {
            AsyncLocalContextHolder.Value = new ContextHolder(userId);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var userId = AsyncLocalContextHolder.Value?.UserId;

            request.Headers.Add("X-User-ID", userId);

            Console.WriteLine($"Including X-User-ID header with value: {userId}");

            return base.SendAsync(request, cancellationToken);
        }

        class ContextHolder
        {
            public string? UserId { get; }

            public ContextHolder(string? userId)
            {
                UserId = userId;
            }
        }
    }
}
