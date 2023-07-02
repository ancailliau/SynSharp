using System;

namespace Synsharp.Telepath;

public class SynsharpException : Exception
{
    public SynsharpException()
    {
        
    }

    public SynsharpException(string message): base(message)
    {
        
    }
}

public class SynsharpError : Exception
{
    public SynsharpError()
    {
        
    }

    public SynsharpError(string message): base(message)
    {
        
    }
}