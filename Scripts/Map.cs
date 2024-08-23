using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

public partial class Map : Node2D
{
    private const float worldChunkSize = 0.002f;
    private float gameChunkSize = 2.5f;

    private ChunkGrid chunkGrid = new ChunkGrid();

    private OpenStreetMapApi openStreetMapApi;
    private Camera2D camera;

    private double currentLatitude = 53.652949f;
    private double currentLongitude = 10.286926f;
    private bool readyCalled = false;
    private bool locationFailed = false;
    private bool initialDraw = false;
    private bool updatingLocation = false;

    // TODO: add loading and error indicator
    public override void _Ready()
    {
        readyCalled = true;

        // get node references
        openStreetMapApi = GetNode<OpenStreetMapApi>("OpenStreetMapApi");
        camera = GetNode<Camera2D>("Camera2D");

        // adjust map size for screen size
        gameChunkSize *= GetViewportRect().Size.Y;

        // center camera
        camera.Position = Vector2.One * (gameChunkSize / 2);

        if (locationFailed && !initialDraw)
        {
            DrawMap(currentLatitude, currentLongitude);
        }
    }

    public override void _Input(InputEvent @event)
    {
        Vector2I direction = Vector2I.Zero;

        if (Input.IsKeyPressed(Key.Up))
        {
            direction += Vector2I.Down;
        }
        else if (Input.IsKeyPressed(Key.Down))
        {
            direction += Vector2I.Up;
        }

        if (Input.IsKeyPressed(Key.Left))
        {
            direction += Vector2I.Left;
        }
        else if (Input.IsKeyPressed(Key.Right))
        {
            direction += Vector2I.Right;
        }

        // shift grid if needed
        if (direction != Vector2I.Zero)
        {
            UpdateLocation(currentLatitude + (direction.Y * 0.0005), currentLongitude + (direction.X * 0.0005));
        }
    }

    public void LocationFailed()
    {
        GD.Print("location failed");

        locationFailed = true;

        // sometimes we know if the location failed before _Ready is even called because of how gdscript interoperability works
        if (readyCalled && !initialDraw)
        {
            DrawMap(currentLatitude, currentLongitude);
        }
    }

    public void UpdateLocation(double latitude, double longitude)
    {
        if (updatingLocation)
        {
            GD.Print("already updating location");
            return;
        }

        updatingLocation = true;

        // update coordinates
        currentLatitude = latitude;
        currentLongitude = longitude;

        if (!initialDraw)
        {
            // perform initial draw
            DrawMap(currentLatitude, currentLongitude);

            updatingLocation = false;

            return;
        }

        // move map to current location
        OsmData centerOsm = chunkGrid.Center.osmData;

        // TODO: check here if we are completely out of bounds of grid, erase grid, and then redraw

        // check if we are out of chunk bounds and shift grid towards movement
        Vector2I shiftDirection = Vector2I.Zero;

        // latitude
        if (latitude <= centerOsm.minLatitude)
        {
            shiftDirection += Vector2I.Up;
        }
        else if (latitude >= centerOsm.maxLatitude)
        {
            shiftDirection += Vector2I.Down;
        }

        // longitude
        if (longitude < centerOsm.minLongitude)
        {
            shiftDirection += Vector2I.Right;
        }
        else if (longitude > centerOsm.maxLongitude)
        {
            shiftDirection += Vector2I.Left;
        }

        // load and unload chunks if needed
        if (shiftDirection != Vector2I.Zero)
        {
            // shift grid
            chunkGrid.Shift(shiftDirection);

            // set center to new center after shift
            centerOsm = chunkGrid.Center.osmData;

            // calculate center of whole map
            double centerLatitude = centerOsm.minLatitude + worldChunkSize / 2;
            double centerLongitude = centerOsm.minLongitude + worldChunkSize / 2;

            // draw map
            DrawMap(centerLatitude, centerLongitude);

        }

        camera.Position = WorldToGamePosition(latitude, longitude, centerOsm.minLatitude, centerOsm.minLongitude);

        updatingLocation = false;
    }

    private void DrawMap(double centerLatitude, double centerLongitude)
    {
        initialDraw = true;

        // center
        chunkGrid.Center ??= CreateChunk(
            // center
            centerLatitude,

            // center
            centerLongitude
        );
        // set position
        chunkGrid.Center.Position = Vector2.Zero;

        // center left
        chunkGrid.CenterLeft ??= CreateChunk(
            // center
            centerLatitude,

            // left
            centerLongitude - worldChunkSize
        );
        // set position
        chunkGrid.CenterLeft.Position = new Vector2(-gameChunkSize, 0);

        // center right
        chunkGrid.CenterRight ??= CreateChunk(
            // center
            centerLatitude,

            // right
            centerLongitude + worldChunkSize
        );
        // set position
        chunkGrid.CenterRight.Position = new Vector2(gameChunkSize, 0);

        // top center
        chunkGrid.TopCenter ??= CreateChunk(
            // top
            centerLatitude + worldChunkSize,

            // center
            centerLongitude
        );
        // set position
        chunkGrid.TopCenter.Position = new Vector2(0, -gameChunkSize);

        // bottom center
        chunkGrid.BottomCenter ??= CreateChunk(
            // bottom
            centerLatitude - worldChunkSize,

            // center
            centerLongitude
        );
        // set position
        chunkGrid.BottomCenter.Position = new Vector2(0, gameChunkSize);

        // top left
        chunkGrid.TopLeft ??= CreateChunk(
            // top
            centerLatitude + worldChunkSize,

            // left
            centerLongitude - worldChunkSize
        );
        // set position
        chunkGrid.TopLeft.Position = new Vector2(-gameChunkSize, -gameChunkSize);

        // top right
        chunkGrid.TopRight ??= CreateChunk(
            // top
            centerLatitude + worldChunkSize,

            // right
            centerLongitude + worldChunkSize
        );
        // set position
        chunkGrid.TopRight.Position = new Vector2(gameChunkSize, -gameChunkSize);

        // bottom left
        chunkGrid.BottomLeft ??= CreateChunk(
            // bottom
            centerLatitude - worldChunkSize,

            // left
            centerLongitude - worldChunkSize
        );
        // set position
        chunkGrid.BottomLeft.Position = new Vector2(-gameChunkSize, gameChunkSize);

        // bottom right
        chunkGrid.BottomRight ??= CreateChunk(
            // bottom
            centerLatitude - worldChunkSize,

            // right
            centerLongitude + worldChunkSize
        );
        // set position
        chunkGrid.BottomRight.Position = new Vector2(gameChunkSize, gameChunkSize);
    }

    private MapChunk CreateChunk(double latitude, double longitude)
    {
        // load chunk scene
        PackedScene chunkScene = (PackedScene)ResourceLoader.Load("res://Scenes/MapChunk.tscn");

        // create chunk
        MapChunk mapChunk = (MapChunk)chunkScene.Instantiate();
        mapChunk.parentMap = this;

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
        openStreetMapApi.FetchMap(latitude, longitude, worldChunkSize, callback);

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