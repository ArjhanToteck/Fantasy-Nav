# MapChunk.gd
extends Node2D
class_name MapChunk

var parent_map: Map
var osm_data: OsmData
var min_latitude: float
var min_longitude: float
var max_latitude: float
var max_longitude: float

# --- Center coordinates ---
func get_center_latitude() -> float:
    return (min_latitude + max_latitude) / 2

func get_center_longitude() -> float:
    return (min_longitude + max_longitude) / 2


# --- Draw the map ---
func draw_map() -> void:
    # draw relations
    for relation in osm_data.relations:
        draw_relation(relation)

    # draw ways
    for way in osm_data.ways:
        draw_way(way)

    # draw nodes
    for node in osm_data.nodes:
        draw_icon(node)


# --- Draw a relation ---
func draw_relation(relation: OsmRelation) -> void:
    # check if invisible
    if not relation.visible:
        return

    # draw each outer way
    for way in relation.outer_boundaries:
        # TODO: the relation has extra tags needed by the way
        draw_way(way, relation.tags)

    # TODO: draw inner boundaries too (but they might be kinda weird)


# --- Draw a way ---
func draw_way(way: OsmWay, extra_tags: Dictionary = {}) -> void:
    # check if invisible
    if not way.visible:
        return

    # draw way
    draw_icon(way, extra_tags)
    draw_road(way, extra_tags)
    draw_surface(way, extra_tags)


# draw an icon for a node or way
func draw_icon(element: OsmElement, extra_tags: Dictionary = {}) -> void:
    var icon_texture: Texture2D = null
    var tags: Dictionary = element.tags.duplicate() # duplicate to avoid modifying original

    # add extra keys if needed
    for key in extra_tags.keys():
        if not tags.has(key):
            tags[key] = extra_tags[key]

    # switch building type
    if tags.has("building"):
        var building = tags["building"]
        if building in ["religious", "church"] or tags.has("religion"):
            icon_texture = load("res://Images/Cathedral.svg") as Texture2D
        elif building in ["house", "terrace", "detached", "semidetached_house", "bungalow", "manor", "villa"]:
            var variant = get_random_int_from_id(element.id, 2)
            icon_texture = load("res://Images/House%d.svg" % variant) as Texture2D
        elif building in ["school", "kindergarten", "college", "university"]:
            icon_texture = load("res://Images/University.svg") as Texture2D
        elif building == "apartments":
            icon_texture = load("res://Images/City.svg") as Texture2D
        elif building == "shed":
            icon_texture = load("res://Images/Shed.svg") as Texture2D
        elif building in ["garage", "carport", "static_caravan"]:
            icon_texture = load("res://Images/Caravan.svg") as Texture2D
        elif building in ["tower", "water_tower", "transformer_tower"]:
            var variant = get_random_int_from_id(element.id, 2)
            icon_texture = load("res://Images/Tower%d.svg" % variant) as Texture2D
        elif building == "farm":
            icon_texture = load("res://Images/Farm.svg") as Texture2D
        elif building == "gatehouse":
            icon_texture = load("res://Images/Gate.svg") as Texture2D
        elif building in ["ruins", "construction"]:
            icon_texture = load("res://Images/Ruins.svg") as Texture2D
        elif building == "tent":
            var variant = get_random_int_from_id(element.id, 2)
            icon_texture = load("res://Images/Tent%d.svg" % variant) as Texture2D
        elif building == "windmill":
            icon_texture = load("res://Images/Windmill.svg") as Texture2D
        elif building == "hotel":
            icon_texture = load("res://Images/Inn.svg") as Texture2D
        else:
            var variant = get_random_int_from_id(element.id, 5)
            icon_texture = load("res://Images/Building%d.svg" % variant) as Texture2D

    # TODO: replicate all the other tag checks (artwork_type, memorial, amenity, man_made, natural, attraction, leisure, tourism, historic, traffic signs)
    # (can be done similarly to building logic)

    # draw building if we have a texture
    if icon_texture != null:
        var draw_point = Vector2.ZERO

        if element is OsmWay:
            var points = get_points_from_way(element)
            draw_point = Vector2.ZERO
            for p in points:
                draw_point += p
            draw_point /= float(points.size())
        elif element is OsmNode:
            draw_point = parent_map.WorldToGamePosition(element.latitude, element.longitude, min_latitude, min_longitude)

        draw_icon_at_point(icon_texture, draw_point)


# draw a road from a way
func draw_road(way: OsmWay, extra_tags: Dictionary = {}) -> void:
    var tags = way.tags.duplicate()
    if extra_tags != null:
        for key in extra_tags.keys():
            if not tags.has(key):
                tags[key] = extra_tags[key]

    if tags.has("highway"):
        var highway = tags["highway"]
        if highway in ["footway", "path"]:
            return
        draw_line_from_way(load("res://Images/Road.png") as Texture2D, 4, 40, way)


func draw_surface(way: OsmWay, extra_tags: Dictionary = {}) -> void:
    var surface_visible = false
    var color = Color.WHITE
    var layer = 0

    # make a copy of the way's tags
    var tags = way.tags.duplicate()

    # merge extra tags without overwriting existing ones
    if extra_tags:
        for key in extra_tags.keys():
            if not tags.has(key):
                tags[key] = extra_tags[key]

    # water
    if tags.has("water"):
        surface_visible = true
        color = Color.html("90784d")
        layer = 1
    # landuse
    elif tags.has("landuse"):
        var landuse = tags["landuse"]
        if landuse == "grass":
            surface_visible = true
            color = Color.html("997c3e")
            layer = 3
        # uncomment if you want residential / sidewalk later
        # elif landuse == "residential":
        #     draw_surface_flag = true
        #     color = Color.html("94743b")
        #     layer = 3

    # surface
    if tags.has("surface"):
        var surface = tags["surface"]
        if surface == "sand":
            surface_visible = true
            color = Color.html("9c8444")
            layer = 1

    # natural
    if tags.has("natural"):
        var natural = tags["natural"]
        if natural in ["beach", "sand"]:
            surface_visible = true
            color = Color.html("9c8444")
            layer = 1
        elif natural in ["wetland", "scrub"]:
            surface_visible = true
            color = Color.html("997c3e")
            layer = 1

    # leisure
    if tags.has("leisure"):
        var leisure = tags["leisure"]
        if leisure == "park":
            surface_visible = true
            color = Color.html("997c3e")
            layer = 1
        elif leisure == "pitch":
            surface_visible = true
            color = Color.html("8a6a30")
            layer = 1

    # parking
    if tags.has("parking"):
        var parking = tags["parking"]
        if parking == "surface":
            surface_visible = true
            color = Color.html("7e5f29")
            layer = 1

    # actually draw the surface and outline if needed
    if surface_visible:
        draw_polygon_from_way(color, layer, way)
        draw_line_from_way(load("res://Images/Outline.png"), layer, 8, way)


# --- Helper functions for drawing ---
func draw_line_from_way(texture: Texture2D, layer: int, width: float, way: OsmWay) -> void:
    if not is_instance_valid(self ):
        return

    var closed = false
    var points = get_points_from_way(way)

    if points[0] == points[points.size() - 1]:
        closed = true
        points.remove_at(points.size() - 1)

    var line = Line2D.new()
    line.points = points
    line.texture = texture
    line.texture_mode = Line2D.LINE_TEXTURE_TILE
    line.texture_repeat = true
    line.width = width
    line.z_index = layer
    line.joint_mode = Line2D.LINE_JOINT_ROUND
    line.closed = closed
    add_child(line)


func draw_polygon_from_way(color: Color, layer: int, way: OsmWay) -> void:
    if not is_instance_valid(self ):
        return
    var polygon = Polygon2D.new()
    polygon.polygon = get_points_from_way(way)
    polygon.color = color
    polygon.z_index = layer
    add_child(polygon)


func draw_icon_at_point(texture: Texture2D, draw_point: Vector2) -> void:
    if texture == null or not is_instance_valid(self ):
        return
    var icon_scene = load("res://Scenes/Icon.tscn") as PackedScene
    var sprite = icon_scene.instantiate() as Sprite2D
    sprite.texture = texture
    sprite.position = draw_point
    add_child(sprite)


# --- Random int from string id ---
func get_random_int_from_id(_id: String, _max: int) -> int:
    var rng = RandomNumberGenerator.new()
    rng.seed = int(hash(_id))
    return rng.randi_range(0, _max - 1)


# --- Convert way nodes to Vector2 points ---
func get_points_from_way(way: OsmWay) -> Array:
    var points: Array = []
    for node in way.node_children:
        points.append(parent_map.WorldToGamePosition(node.latitude, node.longitude, min_latitude, min_longitude))
    return points