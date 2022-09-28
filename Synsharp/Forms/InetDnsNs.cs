using System;
using System.Net;
using Synsharp.Attribute;
using Synsharp.Types;

namespace Synsharp.Forms;

[SynapseForm("inet:dns:ns")]
public class InetDnsNs : SynapseObject<Types.InetDnsNs>
{
    public InetDnsNs()
    {
        
    }
    public Types.InetFqdn Zone => _value.Value.Item1;
    public Types.InetFqdn NS => _value.Value.Item2;
}