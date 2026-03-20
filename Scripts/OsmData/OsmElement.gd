# Base class for OSM elements (node, way, relation)
class_name OsmElement
extends RefCounted

var id: String
var visible: bool = true
var tags: Dictionary = {}