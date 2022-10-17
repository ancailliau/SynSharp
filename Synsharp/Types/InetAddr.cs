/*
 * Copyright 2022 Antoine Cailliau
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Synsharp.Types;

public class InetAddr : SynapseType
{
    private Uri _value;
    private InetAddr(Uri value)
    {
        _value = value;
    }

    public static implicit operator Uri(InetAddr d) => d._value;
    public static implicit operator InetAddr(Uri d) => new InetAddr(d);

    public override string ToString()
    {
        return _value.ToString();
    }

    public override string GetEscapedCoreValue()
    {
        return _value.ToString();
    }

    public override string GetCoreValue()
    {
        return _value.ToString();
    }

    public static InetAddr Parse(string s)
    {
        return new InetAddr(new Uri(s));
    }
}