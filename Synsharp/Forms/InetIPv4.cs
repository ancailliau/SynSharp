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

using System.Net;
using Synsharp.Attribute;
using Synsharp.Types;

namespace Synsharp.Forms;

[SynapseForm("inet:ipv4")]
public class InetIPv4 : SynapseObject<Types.InetIPv4>
{
    public InetIPv4() : base()
    {
    }

    public InetIPv4(IPAddress s)
    {
        SetValue(s);
    }
    
    public InetIPv4(string s)
    {
        SetValue(s);
    }
    
    public static InetIPv4 Parse(string str)
    {
        var address = new InetIPv4();
        address.SetValue(str);
        return address;
    }
}