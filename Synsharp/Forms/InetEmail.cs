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

using Synsharp.Attribute;
using Synsharp.Types;

namespace Synsharp.Forms;

[SynapseForm("inet:email")]
public class InetEmail : SynapseObject<Synsharp.Types.InetEmail>
{
    public InetEmail()
    {
    }
    
    public InetEmail(string str)
    {
        SetValue(str);
    }

    [SynapseProperty("user")] public Types.InetUser User { get; private set; }
    [SynapseProperty("fqdn")] public Types.InetFqdn FQDN { get; private set; }
}