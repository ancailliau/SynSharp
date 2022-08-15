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

namespace Synsharp.Forms
{
    [SynapseForm("crypto:x509:cert")]
    public class CryptoX509Cert : SynapseObject<Synsharp.Types.CryptoX509Cert>
    {
        [SynapseProperty("file")] public Synsharp.Types.FileBytes File { get; set; }
        [SynapseProperty("subject")] public Synsharp.Types.Str Subject { get; set; }
        [SynapseProperty("issuer")] public Synsharp.Types.Str Issuer { get; set; }
        [SynapseProperty("issuer:cert")] public Synsharp.Types.CryptoX509Cert IssuerCert { get; set; }
        [SynapseProperty("serial")] public Synsharp.Types.Str Serial { get; set; }
        [SynapseProperty("version")] public Synsharp.Types.Int Version { get; set; }
        [SynapseProperty("validity:notbefore")] public Synsharp.Types.Time NotBefore { get; set; }
        [SynapseProperty("validity:notafter")] public Synsharp.Types.Time NotAfter { get; set; }
        
        [SynapseProperty("md5")] public Synsharp.Types.HashMD5 MD5 { get; set; }
        [SynapseProperty("sha1")] public Synsharp.Types.HashSHA1 SHA1 { get; set; }
        [SynapseProperty("sha256")] public Synsharp.Types.HashSHA256 SHA256 { get; set; }
   
        [SynapseProperty("rsa:key")] public Synsharp.Types.RSAKey RSAKey { get; set; }
        [SynapseProperty("algo")] public Synsharp.Types.ISOOID Algo { get; set; }
        
        [SynapseProperty("signature")] public Synsharp.Types.Hex Signature { get; set; }
        [SynapseProperty("ext:sans")] public Synsharp.Types.Array<Synsharp.Types.CryptoX509SAN> ExtSANs { get; set; }
        
        [SynapseProperty("ext:crls")] public Synsharp.Types.Array<Synsharp.Types.CryptoX509CRL> ExtCRLs { get; set; }
        [SynapseProperty("identities:fqdns")] public Synsharp.Types.Array<Synsharp.Types.InetFqdn> IdentitiesFQDNs { get; set; }
        [SynapseProperty("identities:emails")] public Synsharp.Types.Array<Synsharp.Types.InetEmail> IdentitiesEmails { get; set; }
        [SynapseProperty("identities:ipv4s")] public Synsharp.Types.Array<Synsharp.Types.InetIPv4> IdentitiesIPv4s { get; set; }
        [SynapseProperty("identities:ipv6s")] public Synsharp.Types.Array<Synsharp.Types.InetIPv6> IdentitiesIPv6s { get; set; }

        [SynapseProperty("identities:urls")] public Synsharp.Types.Array<Synsharp.Types.InetUrl> IdentitiesURLs { get; set; }
        [SynapseProperty("crl:urls")] public Synsharp.Types.Array<Synsharp.Types.InetUrl> CRLURLs { get; set; }
        
        [SynapseProperty("selfsigned")] public Synsharp.Types.Bool SelfSigned { get; set; }

    }
}

