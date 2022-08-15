using System;
using Synsharp.Attribute;
using Synsharp.Types;

namespace Synsharp.Tests;

[SynapseForm("_di:document")]
public class SDIDocument : SynapseObject<Str>
{
    public SDIDocument(Guid documentDocumentId)
    {
        SetValue(documentDocumentId.ToString());
    }

    public SDIDocument()
    {
            
    }
}