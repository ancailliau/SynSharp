using System;

namespace Synsharp.Types;

public class InetDnsA : Comp<Types.InetFqdn, Types.InetIPv4>
{
    public InetDnsA(InetFqdn fqdn, InetIPv4 ipv4) : base(new Tuple<InetFqdn, InetIPv4>(fqdn, ipv4))
    {
    }
    
    public InetDnsA(Tuple<InetFqdn, InetIPv4> value) : base(value)
    {
    }

    public static InetDnsA Convert(object o)
    {
        if (o is Object[] { Length: 2 } array)
        {
            var item1 = InetFqdn.Convert(array[0]);
            var item2 = InetIPv4.Convert(array[1]);
            return new InetDnsA(item1, item2);
        }

        throw new NotImplementedException(o.GetType().FullName);
    }
}