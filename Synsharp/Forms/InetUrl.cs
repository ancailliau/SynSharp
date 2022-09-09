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
using Synsharp.Attribute;

namespace Synsharp.Forms;

[SynapseForm("inet:url")]
public class InetUrl : SynapseObject<Types.InetUrl>
{
    protected bool Equals(InetUrl other)
    {
        return base.Equals(other) && Equals(Value, other.Value);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((InetUrl)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public InetUrl() : base()
    {
    }
    public InetUrl(string str) : base()
    {
        SetValue(str);
    }

    [SynapseProperty("fqdn")] public Types.InetFqdn FQDN { get; set; }
    [SynapseProperty("ipv4")] public Types.InetIPv4 IpV4 { get; set; }
    [SynapseProperty("ipv6")] public Types.InetIPv6 IpV6 { get; set; }
    [SynapseProperty("passwd")] public Types.InetPasswd Passwd { get; set; }
    [SynapseProperty("base")] public Types.Str Base { get; set; }
    [SynapseProperty("path")] public Types.Str Path { get; set; }
    [SynapseProperty("params")] public Types.Str Params { get; set; }
    [SynapseProperty("port")] public Types.InetPort Port { get; set; }
    [SynapseProperty("proto")] public Types.Str Proto { get; set; }
    [SynapseProperty("user")] public Types.InetUser User { get; set; }

    public static InetUrl Parse(string str)
    {
        var address = new InetUrl();
        address.SetValue(str);
        return address;
    }
}