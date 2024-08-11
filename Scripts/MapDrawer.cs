using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

public partial class MapDrawer : Node2D
{
    Vector2 mapGameSize = new Vector2(5000, 5000);
    Color backgroundColor = Color.FromHtml("9d7d44");
    Color outlineColor = new Color(0.36f, 0.19f, 0, 1);

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
            DrawWayNode(way, mapWorldHeight, mapWorldWidth);
        }

        // TODO: also draw nodes for things like stop signs, put little dots or images or something
    }

    public void DrawWayNode(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        // check if invisible
        if (!way.visible)
        {
            return;
        }

        // check if building
        if (way.tags.ContainsKey("building"))
        {
            DrawBuilding(way, mapWorldHeight, mapWorldWidth);
        }
        // check if road
        else if (way.tags.ContainsKey("highway"))
        {
            DrawRoad(way, mapWorldHeight, mapWorldWidth);
        }
        // check if surface
        else if (way.tags.ContainsKey("landuse") || way.tags.ContainsKey("leisure") || way.tags.ContainsKey("water"))
        {
            DrawSurface(way, mapWorldHeight, mapWorldWidth);
        }
    }

    public void DrawBuilding(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        Vector2[] points = GetPointsFromWay(way, mapWorldHeight, mapWorldWidth);
        Vector2 centerPoint = Vector2.Zero;

        // add all points together
        foreach (Vector2 point in points)
        {
            centerPoint += point;
        }

        // divide x and y by number of points to get mean
        centerPoint = centerPoint / new Vector2(points.Length, points.Length);

        // load building scene
        PackedScene buildingScene = (PackedScene)ResourceLoader.Load("res://Scenes/Building.tscn");
        Sprite2D buildingSprite = (Sprite2D)buildingScene.Instantiate();
        buildingSprite.Position = centerPoint;

        // switch building type
        if (way.tags.TryGetValue("building", out string building))
        {
            if (building == "school")
            {
                buildingSprite.Texture = (Texture2D)GD.Load("res://Images/University.svg");
            }
            else if (building == "apartments")
            {
                buildingSprite.Texture = (Texture2D)GD.Load("res://Images/City.svg");
            }
            else if (building == "garage")
            {
                buildingSprite.Texture = (Texture2D)GD.Load("res://Images/Shed.svg");
            }
            else if (way.tags.ContainsKey("religion"))
            {
                buildingSprite.Texture = (Texture2D)GD.Load("res://Images/Cathedral.svg");
            }
        }

        AddChild(buildingSprite);

        // set flow map
        ((ShaderMaterial)buildingSprite.Material).SetShaderParameter("flowMap", (Texture2D)GD.Load("res://Images/FlowMap.jpg"));
    }

    public void DrawRoad(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        if (way.tags.TryGetValue("highway", out string highway))
        {
            // exclude footways and paths
            if (highway == "footway" || highway == "path")
            {
                return;
            }

            DrawLineFromWay(outlineColor, 2, 25, way, mapWorldHeight, mapWorldWidth);
            DrawLineFromWay(backgroundColor, 3, 20, way, mapWorldHeight, mapWorldWidth);

            return;
        }
    }

    public void DrawSurface(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        bool drawSurface = false;
        Color color = Colors.White;
        uint layer = 0;
        float width = 0;

        if (way.tags.TryGetValue("landuse", out string landuse))
        {
            if (landuse == "grass")
            {
                // draw grass
                drawSurface = true;
                color = new Color(0.30f, 0.32f, 0.23f, 1);
                layer = 3;
                width = 25;
            }
        }
        else if (way.tags.TryGetValue("leisure", out string leisure))
        {
            if (leisure == "park")
            {
                // draw park
                drawSurface = true;
                color = new Color(0.30f, 0.32f, 0.23f, 1);
                layer = 1;
                width = 25;
            }
        }
        else if (way.tags.TryGetValue("water", out string water))
        {
            // draw water
            drawSurface = true;
            color = new Color(0.36f, 0.19f, 0, 1);
            layer = 1;
            width = 25;
        }

        // check if we decided to draw land earlier
        if (drawSurface)
        {
            DrawPolygonFromWay(color, layer, way, mapWorldHeight, mapWorldWidth);
            DrawLineFromWay(outlineColor, layer, width, way, mapWorldHeight, mapWorldWidth);
        }
    }

    void DrawLineFromWay(Color color, uint layer, float width, OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        bool closed = false;
        Vector2[] points = GetPointsFromWay(way, mapWorldHeight, mapWorldWidth);

        // check if first and last points match
        if (points[0] == points[points.Length - 1])
        {
            closed = true;
            List<Vector2> pointsList = points.ToList();

            // remove last element (Godot doesn't like having the same one twice instead of doing closed)
            pointsList.RemoveAt(pointsList.Count - 1);

            points = pointsList.ToArray();
        }

        AddChild(new Line2D()
        {
            Points = points,
            Texture = (Texture2D)GD.Load("res://Images/LineTexture.png"),
            TextureMode = Line2D.LineTextureMode.Tile,
            TextureRepeat = TextureRepeatEnum.Enabled,
            Width = width,
            VisibilityLayer = layer,
            DefaultColor = color,
            JointMode = Line2D.LineJointMode.Round,
            Closed = closed
        });
    }

    void DrawPolygonFromWay(Color color, uint layer, OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        AddChild(new Polygon2D()
        {
            Polygon = GetPointsFromWay(way, mapWorldHeight, mapWorldWidth),
            VisibilityLayer = layer,
            Color = color
        });

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