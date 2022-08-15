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

public class Time : SynapseType
{
    private DateTime _value;
    private Time(DateTime value)
    {
        _value = value;
    }

    public static implicit operator DateTime(Time d) => d._value;
    public static implicit operator Time(DateTime d) => new Time(d);

    public override string ToString()
    {
        return _value.ToString();
    }

    public override string GetCoreValue()
    {
        return _value.ToString();
    }

    public static Time Parse(DateTime s)
    {
        return new Time(s);
    }

    public static Time Convert(object o)
    {
        if (o is Int64 int64)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(int64).UtcDateTime;
        }

        if (o is Int32 int32)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(int32).UtcDateTime;
        }

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(CryptoX509Cert).FullName}'");
    }
}