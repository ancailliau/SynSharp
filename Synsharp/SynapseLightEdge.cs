using System;
using System.Text.RegularExpressions;

namespace Synsharp;

public class SynapseLightEdge
{
    public SynapseObject Source { get; set; }
    public SynapseObject Target { get; set; }
    public string Verb { get; set; }

    public SynapseLightEdge(SynapseObject source, SynapseObject target, string verb)
    {
        Source = source;
        Target = target;

        if (!Regex.Match(verb, @"[a-zA-Z0-9]").Success)
            throw new ArgumentException($"Light edge verb '{verb}' is invalid", nameof(verb));
        Verb = verb;
    }

    protected bool Equals(SynapseLightEdge other)
    {
        return Equals(Source?.Iden, other.Source?.Iden) 
               && Equals(Target?.Iden, other.Target?.Iden) 
               && Verb == other.Verb;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SynapseLightEdge)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Source?.Iden, Target?.Iden, Verb);
    }
}