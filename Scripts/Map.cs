using Godot;
using System;

public partial class Map : Node2D
{

    private readonly Vector2 defaultLocation = new Vector2(53.65309f, 10.28713f);
    private readonly Vector2 worldMapSize = new Vector2(0.003f, 0.003f);
    Vector2 gameMapSize = new Vector2(3000, 3000);

    MapDrawer mapDrawer;
    OpenStreetMapAPI openStreetMapAPI;
    Camera2D camera;
    RichTextLabel label;

    public override void _Ready()
    {
        // get node references
        mapDrawer = GetNode<MapDrawer>("MapDrawer");
        openStreetMapAPI = GetNode<OpenStreetMapAPI>("OpenStreetMapAPI");
        camera = GetNode<Camera2D>("Camera2D");
        label = camera.GetNode<RichTextLabel>("RichTextLabel");

        // center camera
        camera.Position = new Vector2(gameMapSize.X / 2, gameMapSize.Y / 2);

        // draw map callback
        Action<string> callback = (osmResponse) =>
        {
            label.Text = "map data fetched";
            OsmData osmData = OsmData.FromRawOsm(osmResponse);
            label.Text = "drawing map";
            mapDrawer.DrawMap(osmData, gameMapSize);
            label.Text = "map drawn";
        };

        label.Text = "fetching map data";

        // get map data
        openStreetMapAPI.FetchMap(defaultLocation, worldMapSize, callback);
    }
}