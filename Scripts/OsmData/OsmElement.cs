using Godot;
using System;
using System.Collections.Generic;

public abstract class OsmElement
{
    public string id;
    public bool visible;
    public Dictionary<string, string> tags;
}
