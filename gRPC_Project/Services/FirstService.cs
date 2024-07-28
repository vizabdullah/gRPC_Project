using gRPC_Project;
using Grpc.Core;



namespace gRPC_Project.Services
{
    public class FirstService : FirstServiceDefinition.FirstServiceDefinitionBase
    {
        public override Task<Response> Unary(Request request, ServerCallContext context)
        {
            var response = new Response() { Message = request.Content + "from server"};
            return Task.FromResult(response);
        }

        public override async Task<Response> ClientStream(IAsyncStreamReader<Request> requestStream, ServerCallContext context)
        {
            Response response = new Response() { Message = "I got" };
            while (await requestStream.MoveNext())
            {
                var requestPayLoad = requestStream.Current;
                Console.WriteLine(requestPayLoad);
                response.Message = requestPayLoad.ToString();
            }

            return response;
        }

        public override async Task ServerStream(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            for (var i = 0; i < 10; i++)
            {
                if(context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                var response = new Response() { Message = i.ToString() };
                await responseStream.WriteAsync(response);
            }
        }

        public override async Task BiDirectional(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            Response response = new Response() { Message = ""};
            while (await requestStream.MoveNext())
            {
                var requestPayLoad = requestStream.Current;
                response.Message = requestPayLoad.ToString();
                await responseStream.WriteAsync(response);
            }
        }

    }
}
