using Godot;
using System;
using System.Globalization;
using System.Text;
using System.Web;

/// <summary>
/// Used to interact with the Open Street Map API to fetch map information.
/// </summary>
public partial class OpenStreetMapAPI : Node
{
	private const string apiUrl = "https://www.openstreetmap.org/api/0.6/map?bbox=";

	private HttpRequest httpRequest;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		httpRequest = GetNode<HttpRequest>("HTTPRequest");
	}

	public void FetchMap(Vector2 position, Vector2 size, Action<string> callback)
	{
		// calculate bounds
		float minLatitude = position.Y - (size.Y / 2);
		float minLongitude = position.X - (size.X / 2);
		float maxLatitude = position.Y + (size.Y / 2);
		float maxLongitude = position.X + (size.X / 2);

		// fetch from bounds
		FetchMap(minLatitude, minLongitude, maxLatitude, maxLongitude, callback);
	}

	public void FetchMap(float minLatitude, float minLongitude, float maxLatitude, float maxLongitude, Action<string> callback)
	{
		// format request parameters
		string boundsString = minLatitude.ToString(CultureInfo.InvariantCulture) + "," + minLongitude.ToString(CultureInfo.InvariantCulture) + "," + maxLatitude.ToString(CultureInfo.InvariantCulture) + "," + maxLongitude.ToString(CultureInfo.InvariantCulture);

		// handle request
		httpRequest.RequestCompleted += OnRequestCompleted;

		// make http request
		httpRequest.Request(apiUrl + boundsString);
		GD.Print(apiUrl + boundsString);

		void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
		{
			// remove self from event listener
			httpRequest.RequestCompleted -= OnRequestCompleted;

			// get response string (osm file)
			string osmResponse = Encoding.UTF8.GetString(body);

			// make callback
			callback.Invoke(osmResponse);
		}
	}
}
