using System;
using Synsharp.Forms;

namespace Synsharp.Types;

public class InetDnsAnswer : GUID
{
    protected InetDnsAnswer() : base(null)
    {
    }
    protected InetDnsAnswer(string value) : base(value)
    {
    }
    public static InetDnsAnswer Convert(object o)
    {
        if (o is string s)
            return new InetDnsAnswer(s);
        
        throw new NotImplementedException(o.GetType().FullName);
    }
}