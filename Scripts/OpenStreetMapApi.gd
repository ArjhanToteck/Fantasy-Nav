extends Node

# Used to interact with the Open Street Map API to fetch map information.
class_name OpenStreetMapApi

@export var api_url: String = "https://www.openstreetmap.org/api/0.6/map?bbox="
@export var http_request: HTTPRequest

var making_request: bool = false
var request_queue: Array[OpenStreetMapApiRequest] = []

# private
var _current_callback = null


func clear_request_queue() -> void:
    request_queue.clear()


func fetch_map_from_point(latitude: float, longitude: float, size: float, callback: Callable) -> void:
    # calculate bounds
    var min_latitude: float = latitude - size / 2.0
    var min_longitude: float = longitude - size / 2.0
    var max_latitude: float = latitude + size / 2.0
    var max_longitude: float = longitude + size / 2.0

    # fetch from bounds
    _fetch_map_from_bounds(min_latitude, min_longitude, max_latitude, max_longitude, callback)


func _fetch_map_from_request(request: OpenStreetMapApiRequest) -> void:
    _fetch_map_from_bounds(request.min_latitude, request.min_longitude, request.max_latitude, request.max_longitude, request.callback)


func _fetch_map_from_bounds(min_latitude: float, min_longitude: float, max_latitude: float, max_longitude: float, callback: Callable) -> void:
    # make sure we're not busy
    if making_request:
        # add to queue to do later
        request_queue.append(OpenStreetMapApiRequest.new(min_latitude, min_longitude, max_latitude, max_longitude, callback))
        return

    making_request = true

    # format request parameters
    var bounds_string: String = str(min_longitude, ",", min_latitude, ",", max_longitude, ",", max_latitude)

    # make http request
    http_request.request(api_url + bounds_string)
    print(api_url + bounds_string)

    # store current callback for the signal
    _current_callback = callback


# this should be connected to the http request's request signal
func _on_http_request_request_completed(_result: int, _response_code: int, _headers: PackedStringArray, body: PackedByteArray):
	# TODO: should probably check for request failure and shit
    # call next in queue and dequeue
    if request_queue.size() > 0:
        _fetch_map_from_request(request_queue.pop_front())

    # get response string (osm file)
    var osm_response: String = body.get_string_from_utf8()

    # make callback
    if _current_callback != null and _current_callback is Callable:
        _current_callback.call(osm_response)
        _current_callback = null
    
	# no longer busy
    making_request = false

    # move to next request if applicable
    if !request_queue.is_empty():
        var request: OpenStreetMapApiRequest = request_queue.pop_front()
        _fetch_map_from_request(request)


class OpenStreetMapApiRequest:
    var min_latitude: float
    var min_longitude: float
    var max_latitude: float
    var max_longitude: float
    var callback: Callable

    func _init(_min_latitude: float, _min_longitude: float, _max_latitude: float, _max_longitude: float, _callback: Callable) -> void:
        self.min_latitude = _min_latitude
        self.min_longitude = _min_longitude
        self.max_latitude = _max_latitude
        self.max_longitude = _max_longitude
        self.callback = _callback