using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public partial class MapDrawer : Node2D
{
    Vector2 mapViewportSize = new Vector2(1080, 920);

    OsmData osmData;

    decimal mapWorldHeight;
    decimal mapWorldWidth;
    decimal scaleFactor;

    public void DrawMap(OsmData osmData)
    {
        this.osmData = osmData;

        mapWorldHeight = osmData.maxLatitude - osmData.minLatitude;
        mapWorldWidth = osmData.maxLongitude - osmData.minLongitude;

        scaleFactor = (decimal)mapViewportSize.X / mapWorldWidth;
        scaleFactor = Math.Min(scaleFactor, (decimal)mapViewportSize.Y / mapWorldHeight);

        GD.Print(scaleFactor);

        _Draw();
    }

    public override void _Draw()
    {
        if (osmData == null) return;

        foreach (OsmNode node in osmData.nodes)
        {
            // calculate new position
            decimal latitude = node.latitude;
            decimal longitude = node.longitude;

            // account for position
            latitude -= osmData.minLatitude;
            longitude -= osmData.minLongitude;

            // account for scale
            latitude *= scaleFactor;
            longitude *= scaleFactor;

            // convert to float and vector2 with inverse Y axis
            Vector2 position = new Vector2((float)longitude, mapViewportSize.Y - (float)latitude);

            // NOTE: some positions will be slightly out of bounds, that is just because they are still included in OpenStreetMap despite the boundaries set
            GD.Print(position);

            // draw node
            DrawCircle(position, 5, Colors.Red);
        }
    }
}