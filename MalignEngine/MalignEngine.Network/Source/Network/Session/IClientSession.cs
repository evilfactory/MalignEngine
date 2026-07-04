using MalignEngine.Network;

namespace MalignEngine;

public interface IClientSession
{
    ClientId ClientId { get; }
}