using System;
using System.Collections.Concurrent;

public class GenericCustomMockState
{
    public ConcurrentDictionary<string, (int PersonType, int PersonId)> AcceptedEventIds { get; } =
        new ConcurrentDictionary<string, (int PersonType, int PersonId)>(StringComparer.OrdinalIgnoreCase);

    public ConcurrentQueue<GenericCustomReceivedRequest> Received { get; } =
        new ConcurrentQueue<GenericCustomReceivedRequest>();
}
