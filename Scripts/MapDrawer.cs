using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public partial class MapDrawer : Node2D
{
    Vector2 mapGameSize = new Vector2(1080, 920);

    OsmData osmData;

    public void DrawMap(OsmData osmData)
    {
        this.osmData = osmData;

        // get size of map in world units
        decimal mapWorldHeight = osmData.maxLatitude - osmData.minLatitude;
        decimal mapWorldWidth = osmData.maxLongitude - osmData.minLongitude;

        foreach (OsmWay way in osmData.ways)
        {
            // TODO: shade polygons by tag
            Polygon2D wayPolygon = new Polygon2D();

            // get vertices from node positions
            Vector2[] vertices = way.nodeChildren
                .Select((node) =>
                {
                    // convert latitude and longitude to in-game position
                    return WorldToGamePosition(node.latitude, node.longitude, osmData.minLatitude, osmData.minLongitude, mapWorldHeight, mapWorldWidth);
                })
                .ToArray();

            // set polygon vertices
            wayPolygon.Polygon = vertices;

            // add polygon as child
            AddChild(wayPolygon);
        }
    }

    public Vector2 WorldToGamePosition(decimal latitude, decimal longitude, decimal minLatitude, decimal minLongitude, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        // calculate scale factor for world to map
        decimal scaleFactor = (decimal)mapGameSize.X / mapWorldWidth;
        scaleFactor = Math.Min(scaleFactor, (decimal)mapGameSize.Y / mapWorldHeight);

        // account for position
        latitude -= minLatitude;
        longitude -= minLongitude;

        // account for scale
        latitude *= scaleFactor;
        longitude *= scaleFactor;

        // convert to float and vector2 with inverse Y axis
        return new Vector2((float)longitude, mapGameSize.Y - (float)latitude);
    }
}