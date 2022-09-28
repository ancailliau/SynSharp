using System;

namespace Synsharp.Types;

public class InetDnsNs : Comp<Types.InetFqdn, Types.InetFqdn>
{
    public InetDnsNs(InetFqdn zone, InetFqdn ns) : base(new Tuple<InetFqdn, InetFqdn>(zone, ns))
    {
    }
    
    public InetDnsNs(Tuple<InetFqdn, InetFqdn> value) : base(value)
    {
    }

    public static InetDnsNs Convert(object o)
    {
        if (o is Object[] { Length: 2 } array)
        {
            var item1 = InetFqdn.Convert(array[0]);
            var item2 = InetFqdn.Convert(array[1]);
            return new InetDnsNs(item1, item2);
        }

        throw new NotImplementedException(o.GetType().FullName);
    }
}