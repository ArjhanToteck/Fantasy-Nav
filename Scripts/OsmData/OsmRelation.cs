using Godot;
using System;
using System.Collections.Generic;

public class OsmRelation : OsmElement
{
	public List<OsmWay> innerBoundaries;
	public List<string> innerBoundaryIds;
	public List<OsmWay> outerBoundaries;
	public List<string> outerBoundaryIds;
}