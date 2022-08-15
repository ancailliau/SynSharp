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
using System.Linq;

namespace Synsharp.Types;

public class Array<T> : SynapseType where T: SynapseType
{
    private T[] _value;
    private Array(T[] value)
    {
        _value = value;
    }

    public static implicit operator T[](Array<T> d) => d._value;
    public static implicit operator Array<T>(T[] d) => new Array<T>(d);

    public override string ToString()
    {
        return $"[{string.Join(",", _value.Select(_ => _.ToString()))}]";
    }

    public override string GetCoreValue()
    {
        throw new NotImplementedException();
    }

    public static Array<T> Parse(string s)
    {
        throw new NotImplementedException();
    }
}