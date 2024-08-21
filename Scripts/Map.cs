using Godot;
using System;
using System.Diagnostics;

public partial class Map : Node2D
{
    private const float worldChunkSize = 0.0015f;
    private float gameChunkSize = 2.25f;

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
        GD.Print("location failed");

        locationFailed = true;

        // sometimes we know if the location failed before _Ready is even called because of how gdscript interoperability works
        if (readyCalled)
        {
            DrawMap();
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
            DrawMap();

            return;
        }

        // move map to current location
        OsmData centerOsm = chunkGrid.Center.osmData;
        camera.Position = WorldToGamePosition(latitude, longitude, centerOsm.minLatitude, centerOsm.minLongitude);

        // TODO: check here if we are completely out of bounds of grid, erase grid, and then redraw

        // check if we are out of chunk bounds and shift grid towards movement
        Vector2I shiftDirection = Vector2I.Zero;

        // latitude
        if (latitude < centerOsm.minLatitude)
        {
            shiftDirection += Vector2I.Down;
        }
        else if (latitude > centerOsm.minLatitude)
        {
            shiftDirection += Vector2I.Up;
        }

        // longitude
        if (longitude < centerOsm.minLongitude)
        {
            shiftDirection += Vector2I.Left;
        }
        else if (longitude > centerOsm.maxLongitude)
        {
            shiftDirection += Vector2I.Right;
        }

        // shift grid if needed
        if (shiftDirection != Vector2I.Zero)
        {
            chunkGrid.Shift(shiftDirection);
            DrawMap();
        }
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
            DrawMap();
        }
    }

    private void DrawMap()
    {
        chunkGrid ??= new ChunkGrid();

        chunkGrid.Center ??= CreateChunk(
            // center
            currentLatitude,

            // center
            currentLongitude,

            Vector2.Zero
        );

        chunkGrid.CenterLeft ??= CreateChunk(
            // center
            currentLatitude,

            // left
            currentLongitude - worldChunkSize,

            new Vector2(-gameChunkSize, 0)
        );

        chunkGrid.CenterRight ??= CreateChunk(
            // center
            currentLatitude,

            // right
            currentLongitude + worldChunkSize,

            new Vector2(gameChunkSize, 0)
        );

        chunkGrid.TopCenter ??= CreateChunk(
            // top
            currentLatitude + worldChunkSize,

            // center
            currentLongitude,

            new Vector2(0, -gameChunkSize)
        );

        chunkGrid.BottomCenter ??= CreateChunk(
            // bottom
            currentLatitude - worldChunkSize,

            // center
            currentLongitude,

            new Vector2(0, gameChunkSize)
        );

        chunkGrid.TopLeft ??= CreateChunk(
            // top
            currentLatitude + worldChunkSize,

            // left
            currentLongitude - worldChunkSize,

            new Vector2(-gameChunkSize, -gameChunkSize)
        );

        chunkGrid.TopRight ??= CreateChunk(
            // top
            currentLatitude + worldChunkSize,

            // right
            currentLongitude + worldChunkSize,

            new Vector2(gameChunkSize, -gameChunkSize)
        );

        chunkGrid.BottomLeft ??= CreateChunk(
            // bottom
            currentLatitude - worldChunkSize,

            // left
            currentLongitude - worldChunkSize,

            new Vector2(-gameChunkSize, gameChunkSize)
        );

        chunkGrid.BottomRight ??= CreateChunk(
            // bottom
            currentLatitude - worldChunkSize,

            // right
            currentLongitude + worldChunkSize,

            new Vector2(gameChunkSize, gameChunkSize)
        );
    }

    private MapChunk CreateChunk(double latitude, double longitude, Vector2 gameLocation)
    {
        // load chunk scene
        PackedScene chunkScene = (PackedScene)ResourceLoader.Load("res://Scenes/MapChunk.tscn");

        // create chunk
        MapChunk mapChunk = (MapChunk)chunkScene.Instantiate();
        mapChunk.parentMap = this;

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

    public Vector2 WorldToGamePosition(double latitude, double longitude, double minLatitude, double minLongitude)
    {
        // calculate scale factor for world to map
        double scaleFactor = gameChunkSize / worldChunkSize;

        // account for position
        latitude -= minLatitude;
        longitude -= minLongitude;

        // account for scale
        latitude *= scaleFactor;
        longitude *= scaleFactor;

        // convert to float and vector2 with inverse Y axis
        Vector2 worldPosition = new Vector2((float)longitude, (float)(gameChunkSize - latitude));

        return worldPosition;
    }
}