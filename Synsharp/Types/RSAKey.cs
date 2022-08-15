using System;

namespace Synsharp.Types;

public class RSAKey: Comp<Hex,Int>
{
    protected RSAKey(Tuple<Hex, Int> value) : base(value)
    {
    }
}