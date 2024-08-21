using Godot;
using System;
using System.Diagnostics;

public partial class Map : Node2D
{
    private const float worldChunkSize = 0.001f;
    private float gameChunkSize = 1.25f;

    private ChunkGrid chunkGrid;

    private OpenStreetMapApi openStreetMapAPI;
    private Camera2D camera;

    private double currentLatitude = 53.652949f;
    private double currentLongitude = 10.286926f;
    private bool readyCalled = false;
    private bool locationFailed = false;
    private bool initialDraw = false;

    public void LocationFailed()
    {
        locationFailed = true;

        // sometimes we know if the location failed before _Ready is even called because of how gdscript interoperability works
        if (readyCalled)
        {
            FirstMapDraw();
        }
    }
    public void UpdateLocation(double latitude, double longitude)
    {
        // update coordinates
        currentLatitude = latitude;
        currentLongitude = longitude;

        if (!initialDraw)
        {
            // perform initial draw
            initialDraw = true;
            FirstMapDraw();

            return;
        }

        // TODO: move the map here, load and unload chunks
    }

    public override void _Ready()
    {
        readyCalled = true;

        // get node references
        openStreetMapAPI = GetNode<OpenStreetMapApi>("OpenStreetMapApi");
        camera = GetNode<Camera2D>("Camera2D");

        // adjust map size for screen size
        gameChunkSize *= GetViewportRect().Size.Y;

        // center camera
        camera.Position = Vector2.One * (gameChunkSize / 2);

        if (locationFailed)
        {
            FirstMapDraw();
        }
    }

    void FirstMapDraw()
    {
        chunkGrid = new ChunkGrid()
        {
            Center = CreateChunk(
                // center
                currentLatitude,

                // center
                currentLongitude,

                Vector2.Zero
            ),

            CenterLeft = CreateChunk(
                // center
                currentLatitude,

                // left
                currentLongitude - worldChunkSize,

                new Vector2(-gameChunkSize, 0)
            ),

            CenterRight = CreateChunk(
                // center
                currentLatitude,

                // right
                currentLongitude + worldChunkSize,

                new Vector2(gameChunkSize, 0)
            ),

            TopCenter = CreateChunk(
                // top
                currentLatitude + worldChunkSize,

                // center
                currentLongitude,

                new Vector2(0, -gameChunkSize)
            ),

            BottomCenter = CreateChunk(
                // bottom
                currentLatitude - worldChunkSize,

                // center
                currentLongitude,

                new Vector2(0, gameChunkSize)
            ),

            TopLeft = CreateChunk(
                // top
                currentLatitude + worldChunkSize,

                // left
                currentLongitude - worldChunkSize,

                new Vector2(-gameChunkSize, -gameChunkSize)
            ),

            TopRight = CreateChunk(
                // top
                currentLatitude + worldChunkSize,

                // right
                currentLongitude + worldChunkSize,

                new Vector2(gameChunkSize, -gameChunkSize)
            ),

            BottomLeft = CreateChunk(
                // bottom
                currentLatitude - worldChunkSize,

                // left
                currentLongitude - worldChunkSize,

                new Vector2(-gameChunkSize, gameChunkSize)
            ),

            BottomRight = CreateChunk(
                // bottom
                currentLatitude - worldChunkSize,

                // right
                currentLongitude + worldChunkSize,

                new Vector2(gameChunkSize, gameChunkSize)
            )
        };
    }

    MapChunk CreateChunk(double latitude, double longitude, Vector2 gameLocation)
    {
        // load chunk scene
        PackedScene chunkScene = (PackedScene)ResourceLoader.Load("res://Scenes/MapChunk.tscn");

        // create chunk
        MapChunk mapChunk = (MapChunk)chunkScene.Instantiate();
        mapChunk.gameChunkSize = gameChunkSize;
        mapChunk.worldChunkSize = worldChunkSize;

        // position chunk
        mapChunk.Position = gameLocation;

        // get osm data and callback to draw chunk
        Action<string> callback = (osmResponse) =>
        {
            // create data from raw xml
            OsmData osmData = OsmData.FromRawOsm(osmResponse);

            mapChunk.osmData = osmData;
            // draw map
            mapChunk.DrawMap();
        };

        // get map data
        openStreetMapAPI.FetchMap(latitude, longitude, worldChunkSize, callback);

        // include chunk in scene
        AddChild(mapChunk);

        return mapChunk;
    }
}