using System.Security.Cryptography.X509Certificates;

namespace Synsharp.Telepath;

public class LinkInfo
{
    public X509CertificateCollection? clientCertificates;
}