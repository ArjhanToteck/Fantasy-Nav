using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
            DrawWay(way, mapWorldHeight, mapWorldWidth);
        }

        foreach (OsmNode node in osmData.nodes)
        {
            // add node as child
            DrawNode(node, mapWorldHeight, mapWorldWidth);
        }
    }

    void DrawWay(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
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

    void DrawNode(OsmNode node, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        Texture2D nodeTexture = null;

        // switch node type (not an actual switch because more than one property used)
        if (node.tags.ContainsKey("crossing") || node.tags.ContainsKey("traffic_sign"))
        {
            // can be Sign0, Sign1, Sign2
            int variant = GetRandomIntFromId(node.id, 3);
            nodeTexture = (Texture2D)GD.Load($"res://Images/Sign{variant}.svg");
            GD.Print("sign", node.id, $"res://Images/Sign{variant}.svg");
        }

        // draw node
        if (nodeTexture != null)
        {
            Vector2 position = WorldToGamePosition(node.latitude, node.longitude, osmData.minLatitude, osmData.minLongitude, mapWorldHeight, mapWorldWidth);
            DrawIconAtPoint(nodeTexture, position);
        }
    }

    void DrawBuilding(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
    {
        // get sprite for icon
        Texture2D buildingTexture = null;

        // switch building type (not an actual switch because more than one property used)
        if (way.tags.TryGetValue("building", out string building))
        {
            if (building == "religious" || building == "church" || way.tags.ContainsKey("religion"))
            {
                buildingTexture = (Texture2D)GD.Load("res://Images/Cathedral.svg");
            }
            else if (building == "house")
            {
                buildingTexture = (Texture2D)GD.Load("res://Images/House.svg");
            }
            else if (building == "school" || building == "kindergarten" || building == "college" || building == "university")
            {
                buildingTexture = (Texture2D)GD.Load("res://Images/University.svg");
            }
            else if (building == "apartments")
            {
                buildingTexture = (Texture2D)GD.Load("res://Images/City.svg");
            }
            else if (building == "shed")
            {
                buildingTexture = (Texture2D)GD.Load("res://Images/Shed.svg");
            }
            else if (building == "garage" || building == "carport")
            {
                buildingTexture = (Texture2D)GD.Load("res://Images/Caravan.svg");
            }
            else if (building == "tower" || building == "water_tower" || building == "transformer_tower")
            {
                // can be Tower0 or Tower1
                int variant = GetRandomIntFromId(way.id, 2);
                buildingTexture = (Texture2D)GD.Load($"res://Images/Tower{variant}.svg");
                GD.Print("tower", way.id);
            }
            else if (building == "farm")
            {
                buildingTexture = (Texture2D)GD.Load("res://Images/Farm.svg");
            }
            else if (building == "gatehouse")
            {
                buildingTexture = (Texture2D)GD.Load("res://Images/Gate.svg");
            }
            else if (building == "ruins" || building == "construction")
            {
                buildingTexture = (Texture2D)GD.Load("res://Images/Ruins.svg");
            }
            else if (building == "tent")
            {
                buildingTexture = (Texture2D)GD.Load("res://Images/Tent.svg");
            }
            else if (building == "windmill")
            {
                buildingTexture = (Texture2D)GD.Load("res://Images/Windmill.svg");
            }
            else if (way.tags.TryGetValue("man_made", out string manMade))
            {
                if (manMade == "water_well")
                {
                    buildingTexture = (Texture2D)GD.Load("res://Images/Well.svg");
                }
            }
        }
        // TODO: create default buildings

        // draw building
        if (buildingTexture != null)
        {
            // calculate position
            Vector2 centerPoint = Vector2.Zero;
            Vector2[] points = GetPointsFromWay(way, mapWorldHeight, mapWorldWidth);

            // add all points together
            foreach (Vector2 point in points)
            {
                centerPoint += point;
            }

            // divide x and y by number of points to get mean
            centerPoint = centerPoint / new Vector2(points.Length, points.Length);

            DrawIconAtPoint(buildingTexture, centerPoint);
        }
    }

    void DrawRoad(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
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

    void DrawSurface(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
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

    void DrawIconAtPoint(Texture2D texture, Vector2 position)
    {
        if (texture == null)
        {
            return;
        }

        // load building scene
        PackedScene buildingScene = (PackedScene)ResourceLoader.Load("res://Scenes/Icon.tscn");
        Sprite2D buildingSprite = (Sprite2D)buildingScene.Instantiate();
        buildingSprite.Position = position;
        buildingSprite.Texture = texture;

        AddChild(buildingSprite);
    }

    int GetRandomIntFromId(string id, int max)
    {
        SHA256 sha256 = SHA256.Create();

        // hash id string to use as seed
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(id));

        // convert has to int
        int seed = BitConverter.ToInt32(hashBytes, 0);

        return new Random(seed).Next(max);
    }

    Vector2[] GetPointsFromWay(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth)
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

    Vector2 WorldToGamePosition(decimal latitude, decimal longitude, decimal minLatitude, decimal minLongitude, decimal mapWorldHeight, decimal mapWorldWidth)
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