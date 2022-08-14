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
using System.Net;
using Synsharp.Attribute;

namespace Synsharp.Forms;

[SynapseForm("inet:fqdn")]
public class InetFqdn : SynapseObject<string>
{
}
    
[SynapseForm("inet:dns:a")]
public class InetDnsA : SynapseObject<Tuple<string,IPAddress>>
{
    public InetDnsA()
    {
        AddNorm<object[]>(array =>
        {
            return new Tuple<string, IPAddress>(array[0].ToString(), IPAddress.Parse(array[1].ToString()));
        });
    }
    [SynapseProperty("fqdn")] public InetFqdn FQDN { get; set; }
    [SynapseProperty("ipv4")] public InetIpV4 IPv4 { get; set; }
}