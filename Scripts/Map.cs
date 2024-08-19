using Godot;
using System;

public partial class Map : Node2D
{
    private readonly Vector2 defaultLocation = new Vector2(10.286926f, 53.652949f);
    private readonly Vector2 worldMapSize = new Vector2(0.001f, 0.001f);
    private Vector2 gameMapSize;

    private ChunkGrid chunkGrid;
    private OpenStreetMapAPI openStreetMapAPI;
    private Camera2D camera;

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
        openStreetMapAPI = GetNode<OpenStreetMapAPI>("OpenStreetMapAPI");
        camera = GetNode<Camera2D>("Camera2D");

        // adjust map size for screen size
        gameMapSize = Vector2.One * GetViewportRect().Size.Y * 1.25f;

        // center camera
        camera.Position = new Vector2(gameMapSize.X / 2, gameMapSize.Y / 2);

        DrawMap();
    }

    void DrawMap()
    {
        chunkGrid = new ChunkGrid()
        {
            topLeft = CreateChunk(
                // left
                currentLatitude - worldMapSize.X,

                // top
                currentLongitude + worldMapSize.Y,

                new Vector2(-gameMapSize.X, -gameMapSize.Y)
            ),

            topCenter = CreateChunk(
                // center
                currentLatitude,

                // top
                currentLongitude + worldMapSize.Y,

                new Vector2(0, -gameMapSize.Y)
            ),

            topRight = CreateChunk(
                // right
                currentLatitude + worldMapSize.X,

                // top
                currentLongitude + worldMapSize.Y,

                new Vector2(gameMapSize.X, -gameMapSize.Y)
            ),

            centerLeft = CreateChunk(
                // left
                currentLatitude - worldMapSize.X,

                // center
                currentLongitude,

                new Vector2(-gameMapSize.X, 0)
            ),

            center = CreateChunk(
                // center
                currentLatitude,

                // center
                currentLongitude,

                Vector2.Zero
            ),

            centerRight = CreateChunk(
                // right
                currentLatitude - worldMapSize.X,

                // center
                currentLongitude,

                new Vector2(gameMapSize.X, 0)
            ),

            bottomLeft = CreateChunk(
                // left
                currentLatitude + worldMapSize.X,

                // bottom
                currentLongitude,

                new Vector2(-gameMapSize.X, gameMapSize.Y)
            ),

            bottomCenter = CreateChunk(
                // center
                currentLatitude,

                // bottom
                currentLongitude,

                new Vector2(0, gameMapSize.Y)
            ),

            bottomRight = CreateChunk(
                // right
                currentLatitude + worldMapSize.X,

                // bottom
                currentLongitude,

                new Vector2(gameMapSize.X, gameMapSize.Y)
            )
        };
    }

    MapChunk CreateChunk(double latitude, double longitude, Vector2 gameLocation)
    {
        // load chunk scene
        PackedScene chunkScene = (PackedScene)ResourceLoader.Load("res://Scenes/MapChunk.tscn");
        MapChunk mapChunk = (MapChunk)chunkScene.Instantiate();

        // get osm data and callback to draw chunk
        Action<string> callback = (osmResponse) =>
        {
            OsmData osmData = OsmData.FromRawOsm(osmResponse);
            mapChunk.DrawMap(osmData, gameMapSize);
        };

        // get map data
        // TODO: queue requests so all chunks can be loaded in order
        openStreetMapAPI.FetchMap(latitude, longitude, worldMapSize, callback);

        // position chunk
        mapChunk.Position = gameLocation;

        // include chunk in scene
        AddChild(mapChunk);

        return mapChunk;
    }
}