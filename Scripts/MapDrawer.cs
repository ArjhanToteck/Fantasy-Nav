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
            // add way node as child (godot node not osm node)
            Node2D wayNode = CreateWayNode(way, mapWorldHeight, mapWorldWidth);
            if (wayNode != null)
            {
                AddChild(wayNode);
            }
        }

        // TODO: also draw nodes for things like stop signs, put little dots or images or something
    }

    public Node2D CreateWayNode(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        ElementDrawSettings drawSettings = ElementDrawSettings.GetWaySettings(way);

        // don't draw what we don't want to draw
        if (drawSettings == null)
        {
            return null;
        }

        Node2D wayNode;

        // get points from node positions
        Vector2[] points = way.nodeChildren
            .Select((node) =>
            {
                // convert latitude and longitude to in-game position
                return WorldToGamePosition(node.latitude, node.longitude, osmData.minLatitude, osmData.minLongitude, mapWorldHeight, mapWorldWidth);
            })
            .ToArray();

        // check if the way goes back to the beginning, meaning its a polygon
        if (way.nodeChildIDs[0] == way.nodeChildIDs.Last())
        {
            Polygon2D wayPolygon = new Polygon2D
            {
                // set polygon vertices to node positions
                Polygon = points,
                Color = drawSettings.color,
                Texture = drawSettings.texture
            };

            // return this new polygon
            wayNode = wayPolygon;
        }
        else // way is a line
        {
            Line2D wayLine = new Line2D
            {
                // set line points to node positions
                Points = points,
                DefaultColor = drawSettings.color,
                Texture = drawSettings.texture
            };

            // return this new line
            wayNode = wayLine;
        }

        wayNode.VisibilityLayer = drawSettings.visibilityLayer;

        return wayNode;
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