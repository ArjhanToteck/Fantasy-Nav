using Godot;
using System;
using System.Collections.Generic;

public class OsmWay : OsmElement
{
    public List<string> nodeChildIDs;
    public List<OsmNode> nodeChildren;
    public List<OsmTag> tags;
}
