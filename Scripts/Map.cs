using Godot;
using System;

public partial class Map : Node2D
{

    private readonly Vector2 defaultLocation = new Vector2(39.95183f, -75.17171f);
    private readonly Vector2 defaultMapSize = new Vector2(0.01f, 0.01f);

    MapDrawer mapDrawer;
    OpenStreetMapAPI openStreetMapAPI;

    public override void _Ready()
    {

        mapDrawer = GetNode<MapDrawer>("MapDrawer");
        openStreetMapAPI = GetNode<OpenStreetMapAPI>("OpenStreetMapAPI");

        Action<string> callback = (osmResponse) =>
        {
            OsmData osmData = OsmData.FromRawOsm(osmResponse);
            mapDrawer.DrawMap(osmData);
        };

        openStreetMapAPI.FetchMap(defaultLocation, defaultMapSize, callback);
    }
}