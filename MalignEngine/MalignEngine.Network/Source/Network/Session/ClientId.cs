using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine.Network;

public abstract class ClientId
{
    public abstract string StringRepresentation { get; }

    public override bool Equals(object? obj)
    {
        if (obj is ClientId clientId)
        {
            return clientId.StringRepresentation == StringRepresentation;
        }

        return false;
    }

    public static bool operator == (ClientId? a, ClientId? b)
    {
        if (a is null)
        {
            return b is null;
        }

        return a.Equals(b);
    }

    public static bool operator != (ClientId? a, ClientId? b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return StringRepresentation.GetHashCode();
    }
}
