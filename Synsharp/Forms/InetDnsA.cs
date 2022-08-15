using System;
using System.Net;
using Synsharp.Attribute;
using Synsharp.Types;

namespace Synsharp.Forms;

[SynapseForm("inet:dns:a")]
public class InetDnsA : SynapseObject<Comp<Str, Types.InetIPv4>>
{
    [SynapseProperty("fqdn")] public InetFqdn FQDN { get; set; }
    [SynapseProperty("ipv4")] public Types.InetIPv4 IPv4 { get; set; }
}