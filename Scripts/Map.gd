extends Node2D

# Map manager: handles camera, chunks, and OSM data
class_name Map

@export var smoothing_speed: float = 5.0
@export var world_chunk_size: float = 0.004

@export var open_street_map_api: OpenStreetMapApi
@export var camera: Camera2D
@export var info_text: RichTextLabel
@export var info_text_animation_player: AnimationPlayer
@export var map_pin: Sprite2D
var game_chunk_size: float = 5.0
var chunk_grid: ChunkGrid = ChunkGrid.new()


var camera_target: Vector2
var current_latitude: float = 53.652949
var current_longitude: float = 10.286926
var ready_called: bool = false
var location_failed: bool = false
var initial_draw_started: bool = false


# Called when the node enters the scene tree
func _ready() -> void:
	ready_called = true

	# get node references
	open_street_map_api = $OpenStreetMapApi
	camera = $Camera2D
	info_text = camera.get_node("Panel/InfoText")
	info_text_animation_player = camera.get_node("Panel/AnimationPlayer")
	map_pin = camera.get_node("Pin")

	# adjust map size for screen width
	game_chunk_size *= get_viewport_rect().size.x

	# center camera
	camera.position = Vector2.ONE * (game_chunk_size / 2)
	camera_target = camera.position

	if location_failed and not initial_draw_started:
		print("location failed")
		draw_map(current_latitude, current_longitude)


func _process(delta: float) -> void:
	# approach camera target smoothly
	camera.global_position = camera.global_position.lerp(camera_target, float(delta) * smoothing_speed)


func _input(_event: InputEvent) -> void:
	# editor mode controls
	# TODO: remove
	if true: # if Engine.is_editor_hint():
		var direction: Vector2i = Vector2i.ZERO

		# vertical
		if Input.is_key_pressed(Key.KEY_UP):
			direction += Vector2i.DOWN
		elif Input.is_key_pressed(Key.KEY_DOWN):
			direction += Vector2i.UP

		# horizontal
		if Input.is_key_pressed(Key.KEY_LEFT):
			direction += Vector2i.LEFT
		elif Input.is_key_pressed(Key.KEY_RIGHT):
			direction += Vector2i.RIGHT

		# shift grid if needed (debug)
		if direction != Vector2i.ZERO:
			update_location(current_latitude + direction.y * 0.0005, current_longitude + direction.x * 0.0005)


func update_location(latitude: float, longitude: float) -> void:
	print("update location")

	if not initial_draw_started:
		# initial draw
		current_latitude = latitude
		current_longitude = longitude
		draw_map(current_latitude, current_longitude)
		return

	var center_chunk: MapChunk = chunk_grid.get_center_()

	var latitude_delta: float = abs(latitude - current_latitude)
	var longitude_delta: float = abs(longitude - current_longitude)
	var latitude_center_distance: float = abs(latitude - center_chunk.get_center_latitude())
	var longitude_center_distance: float = abs(longitude - center_chunk.get_center_longitude())

	current_latitude = latitude
	current_longitude = longitude

	# redraw completely if moved too far
	if latitude_delta > world_chunk_size or longitude_delta > world_chunk_size \
		or latitude_center_distance > world_chunk_size * 2 or longitude_center_distance > world_chunk_size * 2:
		print("moved too far")
		overwrite_map(current_latitude, current_longitude)
		return

	# check chunk bounds for grid shift
	var shift_direction: Vector2i = Vector2i.ZERO

	if latitude <= center_chunk.min_latitude:
		shift_direction += Vector2i.UP
	elif latitude >= center_chunk.max_latitude:
		shift_direction += Vector2i.DOWN

	if longitude < center_chunk.min_longitude:
		shift_direction += Vector2i.RIGHT
	elif longitude > center_chunk.max_longitude:
		shift_direction += Vector2i.LEFT

	if shift_direction != Vector2i.ZERO:
		chunk_grid.shift(shift_direction)
		center_chunk = chunk_grid.get_center_()
		draw_map(center_chunk.get_center_latitude(), center_chunk.get_center_longitude())
		camera.global_position += Vector2(shift_direction) * game_chunk_size

	# update camera target
	camera_target = world_to_game_position(current_latitude, current_longitude, center_chunk.min_latitude, center_chunk.min_longitude)


# completely redraws map
func overwrite_map(center_latitude: float, center_longitude: float) -> void:
	chunk_grid.clear()
	open_street_map_api.clear_request_queue()
	draw_map(center_latitude, center_longitude)


func draw_map(center_latitude: float, center_longitude: float) -> void:
	# first draw setup
	if not initial_draw_started:
		map_pin.show()
		info_text.text = "Location failed, drawing default map..." if location_failed else "Drawing map..."
		info_text_animation_player.play("PanelFade")

	initial_draw_started = true

	# shorthand for readability
	var c = center_latitude
	var l = center_longitude
	var cw = world_chunk_size
	var gs = game_chunk_size
	var g = chunk_grid.chunks

	# center
	g[1][1] = _ensure_chunk(g[1][1], c, l, Vector2.ZERO)

	# center left / right
	g[1][0] = _ensure_chunk(g[1][0], c, l - cw, Vector2(-gs, 0))
	g[1][2] = _ensure_chunk(g[1][2], c, l + cw, Vector2(gs, 0))

	# top / bottom center
	g[0][1] = _ensure_chunk(g[0][1], c + cw, l, Vector2(0, -gs))
	g[2][1] = _ensure_chunk(g[2][1], c - cw, l, Vector2(0, gs))

	# corners
	g[0][0] = _ensure_chunk(g[0][0], c + cw, l - cw, Vector2(-gs, -gs))
	g[0][2] = _ensure_chunk(g[0][2], c + cw, l + cw, Vector2(gs, -gs))
	g[2][0] = _ensure_chunk(g[2][0], c - cw, l - cw, Vector2(-gs, gs))
	g[2][2] = _ensure_chunk(g[2][2], c - cw, l + cw, Vector2(gs, gs))


# helper to create chunks if they don’t exist
func _ensure_chunk(chunk_ref, latitude_offset: float, longitude_offset: float, _position: Vector2) -> MapChunk:
	if chunk_ref == null:
		chunk_ref = create_chunk(latitude_offset, longitude_offset)
		chunk_ref.position = _position
	return chunk_ref


func create_chunk(latitude: float, longitude: float) -> MapChunk:
	# load chunk scene
	# TODO: probably just make this an exported reference
	var chunk_scene: PackedScene = ResourceLoader.load("res://Scenes/MapChunk.tscn")
	var map_chunk: MapChunk = chunk_scene.instantiate() as MapChunk
	map_chunk.parent_map = self

	# set chunk bounds
	map_chunk.min_latitude = latitude - world_chunk_size / 2
	map_chunk.min_longitude = longitude - world_chunk_size / 2
	map_chunk.max_latitude = latitude + world_chunk_size / 2
	map_chunk.max_longitude = longitude + world_chunk_size / 2

	# fetch osm data with callback
	var chunk_callback = func(osm_response: String, chunk: MapChunk) -> void:
		# TODO: maybe refactor this to OpenStreetMapApi
		# create data from raw xml
		var osm_data: OsmData = OsmData.from_raw_osm(osm_response)
		chunk.osm_data = osm_data

		# draw map
		chunk.draw_map()

	open_street_map_api.fetch_map_from_point(latitude, longitude, world_chunk_size, chunk_callback.bind(map_chunk))
	
	# add chunk to scene
	add_child(map_chunk)

	return map_chunk


func world_to_game_position(latitude: float, longitude: float, min_latitude: float, min_longitude: float) -> Vector2:
	# account for position
	latitude -= min_latitude
	longitude -= min_longitude

	# convert world distance to game distance
	latitude = world_to_game_distance(latitude)
	longitude = world_to_game_distance(longitude)

	return Vector2(float(longitude), float(game_chunk_size - latitude))


func world_to_game_distance(world_distance: float) -> float:
	# calculate scale factor for world to map
	var scale_factor: float = game_chunk_size / world_chunk_size
	return float(world_distance * scale_factor)


func _on_gps_manager_location_failed():
	print("location failed")
	location_failed = true

	# draw map at default location anyway
	if ready_called and not initial_draw_started:
		draw_map(current_latitude, current_longitude)


func _on_gps_manager_update_location(latitude: int, longitude: int):
	update_location(latitude, longitude)
