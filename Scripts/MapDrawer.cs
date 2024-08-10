using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Xml.Linq;

public partial class MapDrawer : Node2D
{
    Vector2 mapGameSize = new Vector2(5000, 5000);

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
                GD.Print("adding child");
                AddChild(wayNode);
            }
        }

        // TODO: also draw nodes for things like stop signs, put little dots or images or something
    }

    public Node2D CreateWayNode(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        // check if invisible
        if (!way.visible)
        {
            return null;
        }

        // check if building
        if (way.tags.ContainsKey("building"))
        {
            return CreateBuilding(way, mapWorldHeight, mapWorldWidth);
        }
        // check if road
        else if (way.tags.ContainsKey("highway"))
        {
            return CreateRoad(way, mapWorldHeight, mapWorldWidth);
        }
        // check if land
        else if (way.tags.ContainsKey("landuse") || way.tags.ContainsKey("leisure"))
        {
            return CreateLand(way, mapWorldHeight, mapWorldWidth);
        }

        // we don't care if it's not what on of these
        return null;
    }

    public Polygon2D CreateBuilding(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        return null;
    }

    public Line2D CreateRoad(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        if (way.tags.TryGetValue("highway", out string highway))
        {
            // exclude footways and paths
            if (highway == "footway" || highway == "path")
            {
                return null;
            }

            // TODO: also add dirt road for smaller highways (need to make dirt road look cleaner too)

            GD.Print(GetPointsFromWay(way, mapWorldHeight, mapWorldWidth));
            // create stone road
            return new Line2D()
            {
                Points = GetPointsFromWay(way, mapWorldHeight, mapWorldWidth),
                Texture = (Texture2D)GD.Load("res://Images/StoneRoad.png"),
                TextureMode = Line2D.LineTextureMode.Tile,
                TextureRepeat = TextureRepeatEnum.Enabled,
                Width = 25,
                VisibilityLayer = 2
            };
        }

        return null;
    }

    public Polygon2D CreateLand(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        if (way.tags.TryGetValue("landuse", out string landuse))
        {
            if (landuse == "grass")
            {
                // make grass green
                return new Polygon2D()
                {
                    Polygon = GetPointsFromWay(way, mapWorldHeight, mapWorldWidth),
                    VisibilityLayer = 3,
                    Color = Colors.SeaGreen
                };
            }
        }
        else if (way.tags.TryGetValue("leisure", out string leisure))
        {
            if (leisure == "park")
            {
                // make park green
                return new Polygon2D()
                {
                    Polygon = GetPointsFromWay(way, mapWorldHeight, mapWorldWidth),
                    VisibilityLayer = 1,
                    Color = Colors.SeaGreen
                };
            }
        }
        else if (way.tags.TryGetValue("water", out string water))
        {
            // make water blue
            return new Polygon2D()
            {
                Polygon = GetPointsFromWay(way, mapWorldHeight, mapWorldWidth),
                VisibilityLayer = 1,
                Color = Colors.MediumBlue
            };
        }

        return null;
    }

    public Vector2[] GetPointsFromWay(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        // get points from node positions
        Vector2[] points = way.nodeChildren
            .Select((node) =>
            {
                // convert latitude and longitude to in-game position
                return WorldToGamePosition(node.latitude, node.longitude, osmData.minLatitude, osmData.minLongitude, mapWorldHeight, mapWorldWidth);
            })
            .ToArray();

        return points;
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