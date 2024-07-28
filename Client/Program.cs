// See https://aka.ms/new-console-template for more information
using Grpc.Core;
using Grpc.Net.Client;
using gRPC_Project;
using System.Linq.Expressions;

Console.WriteLine("Hello, World!");

var option = new GrpcChannelOptions()
{

};

using var channel = GrpcChannel.ForAddress("https://localhost:7197",option);
var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);
/*Unary(client);*/
/*ClientStreaming(client);*/
ServerStreaming(client);
/*BiDirectional(client);*/

Console.ReadLine();
 
void Unary(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var request = new Request() { Content = "Hello World!" };
    var response = client.Unary(request, deadline: DateTime.UtcNow.AddSeconds(5));
}
async void ClientStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using var call = client.ClientStream();
    for(var i = 0; i < 100; i++)
    {
        await call.RequestStream.WriteAsync(new Request() { Content = i.ToString() });
    }
    await call.RequestStream.CompleteAsync();
    Response response = await call;
    Console.WriteLine($"{response.Message}");
}

async void ServerStreaming(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var cancellationToken = new CancellationTokenSource();
    using var streamingCall = client.ServerStream(new Request() { Content = "Hello!" });

    await foreach (var response in streamingCall.ResponseStream.ReadAllAsync(cancellationToken.Token))
    {
        Console.WriteLine(response.Message);
        if(response.Message.Contains("2"))
        {
            cancellationToken.Cancel();
        }
    }
}

async void BiDirectional(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using (var call = client.BiDirectional())
    {
        var request = new Request();
        for (var i = 0; i < 10; i++)
        {
            request.Content = i.ToString();
            Console.WriteLine(request.Content);
            await call.RequestStream.WriteAsync(new Request());
        }
        while(await call.ResponseStream.MoveNext())
        {
            var message = call.ResponseStream.Current; 
            Console.WriteLine(message);
        }

        await call.RequestStream.CompleteAsync();

    }
}