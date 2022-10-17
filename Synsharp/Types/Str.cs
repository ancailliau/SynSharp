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

public class Str : SynapseType
{
    protected string _value;

    protected Str(string value)
    {
        _value = value;
    }

    public static implicit operator string(Str d) => d._value;
    public static implicit operator Str(string d) => new Str(d);

    public override string ToString()
    {
        return _value.ToString();
    }

    public override string GetEscapedCoreValue()
    {
        return StringHelpers.Escape(_value.ToString());
    }

    public override string GetCoreValue()
    {
        return _value.ToString();
    }

    public static Str Parse(string s)
    {
        return new Str(s);
    }
    
    public static Str Convert(object o)
    {
        if (o is string str)
            return Parse(str);

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(Str).FullName}'");
    }
    protected bool Equals(Str other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is string s) return _value == s;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Str)obj);
    }

    public override int GetHashCode()
    {
        return (_value != null ? _value.GetHashCode() : 0);
    }
}