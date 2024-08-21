using System;

public class OpenStreetMapApiRequest
{
	public double minLatitude;
	public double minLongitude;
	public double maxLatitude;
	public double maxLongitude;
	public Action<string> callback;

	public OpenStreetMapApiRequest(double minLatitude, double minLongitude, double maxLatitude, double maxLongitude, Action<string> callback)
	{
		this.minLatitude = minLatitude;
		this.minLongitude = minLongitude;
		this.maxLatitude = maxLatitude;
		this.maxLongitude = maxLongitude;
		this.callback = callback;
	}
}