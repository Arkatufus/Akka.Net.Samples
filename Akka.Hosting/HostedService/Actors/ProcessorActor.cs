using Akka.Actor;
using Akka.Event;

namespace HostedService.Actors;

public class ProcessorActor: ReceiveActor
{
    private readonly string _id;
    private ILogger _log;

    public ProcessorActor(IServiceProvider provider, ILoggerFactory loggerFactory, string id)
    {
        _id = id;
        _log = loggerFactory.CreateLogger($"{typeof(ProcessorActor)}[{_id}]");

        ReceiveAsync<Message>(async msg =>
        {
            if(msg.Id != _id)
                _log.LogError("[processor-{processorId}] Id mismatch. Expected: [{expectedId}], received: [{receivedId}]", _id, _id, msg.Id);

            _log.LogInformation("[processor-{processorId}] Received payload [{payload}]", _id, msg.Payload);
            
            await Task.Delay(200); // Fake processing
            Sender.Tell(msg.Payload);
        });
    }
}