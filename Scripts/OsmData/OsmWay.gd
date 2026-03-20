extends OsmElement
class_name OsmWay

# IDs of child nodes that define this way
var node_child_ids: Array[String] = []

# References to actual child node objects
var node_children: Array[OsmNode] = []