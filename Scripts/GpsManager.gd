class_name GpsManager

extends Node

var gpsProvider

func _ready():
	# call permissions check when requesting permissions
	get_tree().on_request_permissions_result.connect(permissionsCheck)

	var allowed = OS.request_permissions()
	
	# check if we got gps permission
	if allowed:
		enableGPS()

func permissionsCheck(permName, wasGranted):
	if permName == "android.permission.ACCESS_FINE_LOCATION" and wasGranted == true:
		enableGPS()

func enableGPS():	
	# get gps provider from plugin
	gpsProvider = Engine.get_singleton("PraxisMapperGPSPlugin")

	if gpsProvider != null:
		gpsProvider.onLocationUpdates.connect(gpsListener)
		gpsProvider.StartListening()

func gpsListener(data):
	# pass location to Map.cs
	$"..".UpdateLocation(data["latitude"], data["longitude"])
