using System;

namespace Synsharp.Types;

public class CryptoX509SAN : Comp<Str,Str>
{
    protected CryptoX509SAN(Tuple<Str, Str> value) : base(value)
    {
    }
}