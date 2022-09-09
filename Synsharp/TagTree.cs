using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Synsharp;

public class TagTree : IEnumerable<string>
{
    public Dictionary<string, TagTree> Children { get; set; }

    public TagTree()
    {
        Children = new Dictionary<string, TagTree>();
    }

    public void Add(string tag)
    {
        var tags = tag.Split('.', 2);
        if (tags.Length > 0)
        {
            if (!Children.ContainsKey(tags[0]))
            {
                Children.Add(tags[0], new TagTree());
            }

            if (tags.Length > 1)
                Children[tags[0]].Add(tags[1]);
        }
    }
    
    public void Add(string[] tags)
    {
        foreach (var t in tags)
        {
            Add(t);
        }
    }
    
    public void Add(string tag, params string[] tags)
    {
        Add(tag);
        Add(tags);
    }

    private static class Visitor
    {
        public static HashSet<string> Visit(TagTree tree)
        {
            var collectedTags = new HashSet<string>();
            Visit(tree, "", collectedTags);
            return collectedTags;
        }

        private static void Visit(TagTree tree, string prefix, HashSet<string> accumulator)
        {
            if (!tree.Children.Any() & !string.IsNullOrEmpty(prefix))
            {
                accumulator.Add(prefix);
            }
            else
            {
                foreach (var child in tree.Children)
                {
                    Visit(child.Value, (string.IsNullOrEmpty(prefix) ? child.Key : (prefix + "." + child.Key)), accumulator);
                }
            }
        }
    }

    public IEnumerator<string> GetEnumerator()
    {
        foreach (var child in Visitor.Visit(this))
        {
            yield return child;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}