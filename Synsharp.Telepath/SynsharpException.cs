using System;

namespace Synsharp.Telepath;

class SynsharpException : Exception
{
    public SynsharpException()
    {
        
    }

    public SynsharpException(string message): base(message)
    {
        
    }
}