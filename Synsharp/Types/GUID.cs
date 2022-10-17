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

namespace Synsharp.Types;

public class GUID : SynapseType
{
    protected bool Equals(GUID other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((GUID)obj);
    }

    public override int GetHashCode()
    {
        return (_value != null ? _value.GetHashCode() : 0);
    }

    private string _value;
    protected GUID(string value)
    {
        _value = value;
    }

    public static implicit operator string(GUID d) => d._value;
    public static implicit operator GUID(string d) => new GUID(d);

    public override string ToString()
    {
        return _value;
    }

    public override string GetEscapedCoreValue()
    {
        return string.IsNullOrEmpty(_value) ? "*" : _value;
    }

    public override string GetCoreValue()
    {
        return _value.ToString();
    }

    public static GUID Parse(string s)
    {
        return new GUID(s);
    }
}