using Godot;
using System;

public partial class Map : Node2D
{

    readonly Vector2 defaultLocation = new Vector2(10.286926f, 53.652949f);
    readonly Vector2 worldMapSize = new Vector2(0.003f, 0.003f);

    Vector2 gameMapSize;

    MapDrawer mapDrawer;
    OpenStreetMapAPI openStreetMapAPI;
    Camera2D camera;

    double currentLatitude;
    double currentLongitude;
    bool mapDrawn = false;

    public void UpdateLocation(double latitude, double longitude)
    {
        if (mapDrawn) return;
        mapDrawn = true;

        currentLatitude = latitude;
        currentLongitude = longitude;

        DrawMap();
    }

    public override void _Ready()
    {
        // get node references
        mapDrawer = GetNode<MapDrawer>("MapDrawer");
        openStreetMapAPI = GetNode<OpenStreetMapAPI>("OpenStreetMapAPI");
        camera = GetNode<Camera2D>("Camera2D");

        // adjust map size for screen size
        gameMapSize = Vector2.One * GetViewportRect().Size.Y * 3.5f;

        // center camera
        camera.Position = new Vector2(gameMapSize.X / 2, gameMapSize.Y / 2);
    }

    void DrawMap()
    {
        // draw map callback
        Action<string> callback = (osmResponse) =>
        {
            OsmData osmData = OsmData.FromRawOsm(osmResponse);
            mapDrawer.DrawMap(osmData, gameMapSize);
        };

        // get map data
        openStreetMapAPI.FetchMap(currentLatitude, currentLongitude, worldMapSize, callback);
    }
}