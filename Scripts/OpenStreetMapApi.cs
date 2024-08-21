using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

/// <summary>
/// Used to interact with the Open Street Map API to fetch map information.
/// </summary>
public partial class OpenStreetMapApi : Node
{
	public bool makingRequest = false;

	private const string apiUrl = "https://www.openstreetmap.org/api/0.6/map?bbox=";

	private HttpRequest httpRequest;
	private Queue<OpenStreetMapApiRequest> requestQueue = new Queue<OpenStreetMapApiRequest>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		httpRequest = GetNode<HttpRequest>("HTTPRequest");
	}

	public void FetchMap(double latitude, double longitude, float size, Action<string> callback)
	{
		// calculate bounds
		double minLatitude = latitude - size / 2;
		double minLongitude = longitude - size / 2;
		double maxLatitude = latitude + size / 2;
		double maxLongitude = longitude + size / 2;

		// fetch from bounds
		FetchMap(minLatitude, minLongitude, maxLatitude, maxLongitude, callback);
	}

	public void FetchMap(OpenStreetMapApiRequest request)
	{
		FetchMap(request.minLatitude, request.minLongitude, request.maxLatitude, request.maxLongitude, request.callback);
	}

	public void FetchMap(double minLatitude, double minLongitude, double maxLatitude, double maxLongitude, Action<string> callback)
	{
		// make sure we're not busy
		if (makingRequest)
		{
			// add to queue to do later
			requestQueue.Enqueue(new OpenStreetMapApiRequest(minLatitude, minLongitude, maxLatitude, maxLongitude, callback));
			return;
		}

		makingRequest = true;

		// format request parameters
		string boundsString = minLongitude.ToString(CultureInfo.InvariantCulture) + "," + minLatitude.ToString(CultureInfo.InvariantCulture) + "," + maxLongitude.ToString(CultureInfo.InvariantCulture) + "," + maxLatitude.ToString(CultureInfo.InvariantCulture);

		// handle request
		httpRequest.RequestCompleted += OnRequestCompleted;

		// make http request
		httpRequest.Request(apiUrl + boundsString);
		GD.Print(apiUrl + boundsString);

		void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
		{
			// no longer busy
			makingRequest = false;

			// call next in queue and dequeue
			if (requestQueue.Count > 0)
			{
				FetchMap(requestQueue.Dequeue());
			}

			// remove self from event listener
			httpRequest.RequestCompleted -= OnRequestCompleted;

			// get response string (osm file)
			string osmResponse = Encoding.UTF8.GetString(body);

			// make callback
			callback.Invoke(osmResponse);
		}
	}
}
