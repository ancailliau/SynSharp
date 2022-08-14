using System;
using Synsharp.Attribute;

namespace Synsharp.Tests;

[SynapseForm("_di:document")]
public class SDIDocument : SynapseObject<string>
{
    public SDIDocument(Guid documentDocumentId)
    {
        SetValue(documentDocumentId.ToString());
    }

    public SDIDocument()
    {
            
    }
}