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

public class Comp<T1,T2> : SynapseType where T1: SynapseType where T2: SynapseType
{
    private Tuple<T1,T2> _value;
    public Tuple<T1, T2> Value => _value;

    protected Comp(Tuple<T1,T2> value)
    {
        _value = value;
    }

    private Comp(T1 item1, T2 item2)
    {
        _value = new Tuple<T1, T2>(item1, item2);
    }

    public static implicit operator Tuple<T1,T2>(Comp<T1,T2> d) => d._value;
    public static implicit operator Comp<T1,T2>(Tuple<T1,T2> d) => new Comp<T1,T2>(d);

    public override string ToString()
    {
        return $"({string.Join(",", _value)})";
    }

    public override string GetCoreValue()
    {
        return $"({_value.Item1.GetCoreValue()},{_value.Item2.GetCoreValue()})";
    }

    public static Comp<T1,T2> Parse(string s)
    {
        throw new NotImplementedException();
    }

    public static Comp<T1,T2> Convert(object o)
    {
        if (o is Object[] { Length: 2 } array)
        {
            var item1 = (T1) SynapseConverter.Convert(array[0].GetType(), typeof(T1), array[0]);
            var item2 = (T2) SynapseConverter.Convert(array[1].GetType(), typeof(T2), array[1]);
            return new Comp<T1, T2>(item1, item2);
        }
        
        throw new NotImplementedException();
    }

    protected bool Equals(Comp<T1, T2> other)
    {
        return Equals(_value, other._value);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Comp<T1, T2>)obj);
    }

    public override int GetHashCode()
    {
        return (_value != null ? _value.GetHashCode() : 0);
    }
}