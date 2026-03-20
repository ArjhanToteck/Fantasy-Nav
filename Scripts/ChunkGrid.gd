# ChunkGrid.gd
extends RefCounted
class_name ChunkGrid

var chunks = [
    [null, null, null],
    [null, null, null],
    [null, null, null]
]


func get_center_():
    return chunks[1][1]


# shift the grid by a direction vector
func shift(direction: Vector2i) -> void:
    # don't do anything if zero
    if direction == Vector2i.ZERO:
        return

    # temporary array to hold the new positions
    var new_chunks = [
        [null, null, null] as Array[MapChunk],
        [null, null, null] as Array[MapChunk],
        [null, null, null] as Array[MapChunk]
    ]

    # loop through the grid and assign chunks to new positions
    for y in range(3):
        for x in range(3):
            # calculate new x and y
            var new_x: int = x + direction.x
            var new_y: int = y + direction.y

            # don't delete if out of bounds
            var chunk: MapChunk = chunks[y][x]
            if chunk == null:
                continue

            if new_x > 2 or new_x < 0 or new_y > 2 or new_y < 0:
                # delete node
                chunk.queue_free()
                continue

            # assign chunk to the new position
            new_chunks[new_y][new_x] = chunk

    # update the chunks with the shifted chunks
    chunks = new_chunks


# clear all chunks from the grid
func clear() -> void:
    # loop through each old chunk to delete them
    for y in range(3):
        for x in range(3):
            var chunk: MapChunk = chunks[y][x]
            if chunk != null:
                # delete node
                chunk.queue_free()
                chunks[y][x] = null