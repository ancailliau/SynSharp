using System;

namespace Synsharp.Types;

public class CryptoX509Cert : GUID
{
    protected CryptoX509Cert(string value) : base(value)
    {
    }

    public static CryptoX509Cert Convert(object o)
    {
        if (o is string str)
            return new CryptoX509Cert(str);
        
        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(CryptoX509Cert).FullName}'");
    }
}