namespace Synsharp.Telepath.Messages;

public class SynapseErr : SynapseMessage
{
    public object ErrorSourceLine { get; set; }
    public string ErrorFile { get; set; }
    public long ErrorLineNumber { get; set; }
    public string ErrorName { get; set; }
    public string Message { get; set; }
    public string ErrorType { get; set; }
}