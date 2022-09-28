using System;
using System.Net;
using Synsharp.Attribute;
using Synsharp.Types;

namespace Synsharp.Forms;

[SynapseForm("inet:dns:a")]
public class InetDnsA : SynapseObject<Types.InetDnsA>
{
    public InetDnsA()
    {
        
    }
    public Types.InetFqdn FQDN => _value.Value.Item1;
    public Types.InetIPv4 IPv4 => _value.Value.Item2;
}