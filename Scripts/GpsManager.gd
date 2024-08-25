class_name GpsManager

extends Node

var gpsProvider
var locationFailed = false

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
		$"../Camera2D/Panel/InfoText".text = "Location permission is required"
		$"..".LocationFailed()

func permissionsCheck(permName, wasGranted):
	if permName == "android.permission.ACCESS_FINE_LOCATION" and wasGranted == true:
		enableGPS()
	elif (not locationFailed):
		locationFailed = true
		$"../Camera2D/Panel/InfoText".text = "Location permission is required"
		$"..".LocationFailed()

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
		$"../Camera2D/Panel/InfoText".text = "Location access request failed"
		$"..".LocationFailed()

func gpsListener(data):
	print("update location")
	# pass location to Map.cs
	$"..".UpdateLocation(data["latitude"], data["longitude"])
