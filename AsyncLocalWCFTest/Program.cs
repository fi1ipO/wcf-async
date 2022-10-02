using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using AsyncLocalWCFTest;
using ServiceReference1;

// Construct the soap client from address and binding
var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
EndpointAddress endpointAddress = new EndpointAddress("https://www.w3schools.com/xml/tempconvert.asmx");

var soapClient = new TempConvertSoapClient(binding, endpointAddress);
soapClient.Endpoint.EndpointBehaviors.Add(new SoapClientEndpointBehavior());

// Populate async local value => there will be corresponding entry in ExecutionContext
var userIdDelegatingHandler = new UserIdDelegatingHandler();
userIdDelegatingHandler.SetContext(Guid.NewGuid().ToString());

var tasks = new List<Task>();

// Run at least two tasks in parallel using the same soapClient instance
for (int i = 0; i < 2; i++)
    tasks.Add(soapClient.CelsiusToFahrenheitAsync("10"));

// Await them all
await Task.WhenAll(tasks);

// Expected result in console:
// Including X-User-ID header with value: 62f6a8b3-3c0c-4dcc-a6db-477c29663e95
// Including X-User-ID header with value: 62f6a8b3-3c0c-4dcc-a6db-477c29663e95

// Actual result:
// Including X-User-ID header with value: 62f6a8b3-3c0c-4dcc-a6db-477c29663e95
// Including X-User-ID header with value:

// After the first initialization of the TempConvertSoapClient any further calls to process request simultaneously works as expected
tasks = new List<Task>();

// Client is already initialized - the number of threads doesn't matter here
for (int i = 0; i < 3; i++)
    tasks.Add(soapClient.CelsiusToFahrenheitAsync("10"));

// Await them all again
await Task.WhenAll(tasks);

// Console output:
// Including X-User-ID header with value: 62f6a8b3-3c0c-4dcc-a6db-477c29663e95
// Including X-User-ID header with value: 62f6a8b3-3c0c-4dcc-a6db-477c29663e95
// Including X-User-ID header with value: 62f6a8b3-3c0c-4dcc-a6db-477c29663e95

Console.ReadKey();


public class SoapClientEndpointBehavior : IEndpointBehavior
{
    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
        bindingParameters.Add(new Func<HttpClientHandler, HttpMessageHandler>(_ => new UserIdDelegatingHandler()));
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }
    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
    public void Validate(ServiceEndpoint endpoint) { }
}