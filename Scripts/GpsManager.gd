class_name GpsManager

extends Node

var gpsProvider
var locationFailed = false

@export var info_text: RichTextLabel

signal location_failed
signal update_location(latitude: int, longitude: int)

# TODO: find a way to do this on pc mac and linux
func _ready():
	# call permissions check when requesting permissions
	get_tree().on_request_permissions_result.connect(permissionsCheck)

	var allowed = OS.request_permissions()
	
	# check if we got gps permission
	if allowed:
		enableGPS()
	elif (not locationFailed):
		locationFailed = true
		info_text.text = "Location permission is required"
		location_failed.emit()

func permissionsCheck(permName, wasGranted):
	if permName == "android.permission.ACCESS_FINE_LOCATION" and wasGranted == true:
		enableGPS()
	elif (not locationFailed):
		locationFailed = true
		info_text.text = "Location permission is required"
		location_failed.emit()

func enableGPS():
	# check if we have gps provider
	if Engine.has_singleton("PraxisMapperGPSPlugin"):
		# get gps provider from plugin
		gpsProvider = Engine.get_singleton("PraxisMapperGPSPlugin")
		
		# connect and listen
		gpsProvider.onLocationUpdates.connect(gpsListener)
		gpsProvider.StartListening()
	elif (not locationFailed):
		locationFailed = true
		info_text.text = "Location access request failed"
		location_failed.emit()

func gpsListener(data):
	print("update location")
	# pass location to Map.cs
	update_location.emit(data["latitude"], data["longitude"])
