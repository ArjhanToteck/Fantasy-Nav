extends TextureRect

class_name BackgroundTexture

func _process(_delta: float) -> void:
	var viewport_size: Vector2 = get_viewport_rect().size
	size = get_viewport_rect().size
	position = - viewport_size / 2