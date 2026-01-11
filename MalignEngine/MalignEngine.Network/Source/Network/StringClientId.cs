using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine.Network;

public class StringClientId : ClientId
{
    public override string StringRepresentation => _clientId;

    private readonly string _clientId;

    public StringClientId(string id)
    {
        _clientId = id;
    }
}
