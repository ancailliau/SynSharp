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
using System.Collections.Generic;
using Synsharp.Attribute;

namespace Synsharp.Forms;

[SynapseForm("crypto:x509:cert")]
public class CryptoX509Cert : SynapseObject<GUID>
{
    [SynapseProperty("file")] public FileBytes File { get; set; }
    [SynapseProperty("subject")] public string Subject { get; set; }
    [SynapseProperty("issuer")] public string Issuer { get; set; }
    [SynapseProperty("issuer:cert")] public CryptoX509Cert IssuerCert { get; set; }
    [SynapseProperty("serial")] public string Serial { get; set; }
    [SynapseProperty("version")] public int Version { get; set; }
    [SynapseProperty("validity:notbefore")] public DateTime NotBefore { get; set; }
    [SynapseProperty("validity:notafter")] public DateTime NotAfter { get; set; }
        
    [SynapseProperty("md5")] public HashMD5 MD5 { get; set; }
    [SynapseProperty("sha1")] public HashSHA1 SHA1 { get; set; }
    [SynapseProperty("sha256")] public HashSHA256 SHA256 { get; set; }
   
    [SynapseProperty("rsa:key")] public RSAKey RSAKey { get; set; }
    [SynapseProperty("algo")] public ISOOID Algo { get; set; }
        
    [SynapseProperty("signature")] public Hex Signature { get; set; }
    [SynapseProperty("ext:sans")] public ISet<CryptoX509SAN> ExtSANs { get; set; }
        
    [SynapseProperty("ext:crls")] public ISet<CryptoX509CRL> ExtCRLs { get; set; }
    [SynapseProperty("identities:fqdns")] public ISet<InetFqdn> IdentitiesFQDNs { get; set; }
    [SynapseProperty("identities:emails")] public ISet<InetEmail> IdentitiesEmails { get; set; }
    [SynapseProperty("identities:ipv4s")] public ISet<InetIpV4> IdentitiesIPv4s { get; set; }
    [SynapseProperty("identities:ipv6s")] public ISet<InetIpV6> IdentitiesIPv6s { get; set; }

    [SynapseProperty("identities:urls")] public ISet<InetUrl> IdentitiesURLs { get; set; }
    [SynapseProperty("crl:urls")] public ISet<InetUrl> CRLURLs { get; set; }
        
    [SynapseProperty("selfsigned")] public bool SelfSigned { get; set; }

}