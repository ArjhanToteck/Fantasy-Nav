# Stores the data to create a map in the Open Street Map format, including nodes, ways, and relations.
class_name OsmData
extends RefCounted

var nodes: Array[OsmNode] = []
var ways: Array[OsmWay] = []
var relations: Array[OsmRelation] = []

static func from_raw_osm(raw_osm: String) -> OsmData:
    var parser := XMLParser.new()
    var data := OsmData.new()
    
    if parser.open_buffer(raw_osm.to_utf8_buffer()) != OK:
        push_error("Failed to parse OSM XML")
        return data

    var current_nodes: Array[OsmNode] = []
    var current_ways: Array[OsmWay] = []
    
    while true:
        var type := parser.read()

        # nodes
        if type == XMLParser.NODE_ELEMENT and parser.get_node_name() == "node":
            var node := OsmNode.new()
            node.id = get_attribute_by_name(parser, "id")
            node.visible = get_attribute_by_name(parser, "visible") == "true"
            node.latitude = float(get_attribute_by_name(parser, "lat"))
            node.longitude = float(get_attribute_by_name(parser, "lon"))

            # parse tags for node
            var tags := {}
            if not parser.is_empty():
                var depth = parser.get_node_depth()
                while parser.read() != XMLParser.NODE_ELEMENT_END or parser.get_node_depth() > depth:
                    if parser.get_node_type() == XMLParser.NODE_ELEMENT and parser.get_node_name() == "tag":
                        var k = get_attribute_by_name(parser, "k")
                        var v = get_attribute_by_name(parser, "v")
                        tags[k] = v
            node.tags = tags
            current_nodes.append(node)

        # ways
        elif type == XMLParser.NODE_ELEMENT and parser.get_node_name() == "way":
            var way := OsmWay.new()
            way.id = get_attribute_by_name(parser, "id")
            way.visible = get_attribute_by_name(parser, "visible") == "true"
            var node_child_ids: Array[String] = []
            var node_children: Array[OsmNode] = []
            var tags := {}
            if not parser.is_empty():
                var depth = parser.get_node_depth()
                while parser.read() != XMLParser.NODE_ELEMENT_END or parser.get_node_depth() > depth:
                    if parser.get_node_type() == XMLParser.NODE_ELEMENT:
                        if parser.get_node_name() == "tag":
                            var k = get_attribute_by_name(parser, "k")
                            var v = get_attribute_by_name(parser, "v")
                            tags[k] = v
                        elif parser.get_node_name() == "nd":
                            var ref_id = get_attribute_by_name(parser, "ref")
                            node_child_ids.append(ref_id)
            way.tags = tags
            way.node_child_ids = node_child_ids
            # link actual nodes
            for ref_id in node_child_ids:
                for n in current_nodes:
                    if n.id == ref_id:
                        node_children.append(n)
                        break
            way.node_children = node_children
            current_ways.append(way)

        # relations
        elif type == XMLParser.NODE_ELEMENT and parser.get_node_name() == "relation":
            var rel := OsmRelation.new()
            rel.id = get_attribute_by_name(parser, "id")
            rel.visible = get_attribute_by_name(parser, "visible") == "true"
            var tags := {}
            var outer_ids: Array[String] = []
            var inner_ids: Array[String] = []

            if not parser.is_empty():
                var depth = parser.get_node_depth()
                while parser.read() != XMLParser.NODE_ELEMENT_END or parser.get_node_depth() > depth:
                    if parser.get_node_type() == XMLParser.NODE_ELEMENT:
                        if parser.get_node_name() == "tag":
                            var k = get_attribute_by_name(parser, "k")
                            var v = get_attribute_by_name(parser, "v")
                            tags[k] = v
                        elif parser.get_node_name() == "member":
                            var type_attr = get_attribute_by_name(parser, "type")
                            var role = get_attribute_by_name(parser, "role")
                            var ref_id = get_attribute_by_name(parser, "ref")
                            if type_attr != "way" or ref_id == "":
                                continue
                            if role == "outer":
                                outer_ids.append(ref_id)
                            elif role == "inner":
                                inner_ids.append(ref_id)

            rel.tags = tags
            # link actual ways
            rel.outer_boundaries = []
            for oid in outer_ids:
                for w in current_ways:
                    if w.id == oid:
                        rel.outer_boundaries.append(w)
                        break
            rel.inner_boundaries = []
            for iid in inner_ids:
                for w in current_ways:
                    if w.id == iid:
                        rel.inner_boundaries.append(w)
                        break
            rel.outer_boundary_ids = outer_ids
            rel.inner_boundary_ids = inner_ids

            data.relations.append(rel)

    data.nodes = current_nodes
    data.ways = current_ways

    return data


# helper to get attribute value by name
static func get_attribute_by_name(parser: XMLParser, name: String) -> String:
    for i in range(parser.get_attribute_count()):
        if parser.get_attribute_name(i) == name:
            return parser.get_attribute_value(i)
    return ""