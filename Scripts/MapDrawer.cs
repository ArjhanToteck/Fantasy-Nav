using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

public partial class MapDrawer : Node2D
{
    Vector2 mapGameSize = new Vector2(10000, 10000);

    public void DrawMap(OsmData osmData)
    {
        // get size of map in world units
        decimal mapWorldHeight = osmData.maxLatitude - osmData.minLatitude;
        decimal mapWorldWidth = osmData.maxLongitude - osmData.minLongitude;

        // draw ways
        foreach (OsmWay way in osmData.ways)
        {
            // add way node as child (godot node not osm node)
            DrawWay(way, mapWorldHeight, mapWorldWidth, osmData);
        }

        // draw nodes
        foreach (OsmNode node in osmData.nodes)
        {
            // add node as child
            DrawIcon(node, mapWorldHeight, mapWorldWidth, osmData);
        }
    }

    void DrawWay(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth, OsmData osmData)
    {
        // check if invisible
        if (!way.visible)
        {
            return;
        }

        // draw way
        DrawIcon(way, mapWorldHeight, mapWorldWidth, osmData);
        DrawRoad(way, mapWorldHeight, mapWorldWidth, osmData);
        DrawSurface(way, mapWorldHeight, mapWorldWidth, osmData);
    }

    void DrawIcon(OsmElement element, decimal mapWorldHeight, decimal mapWorldWidth, OsmData osmData)
    {
        // get sprite for icon
        Texture2D iconTexture = null;

        // switch building type
        if (element.tags.TryGetValue("building", out string building))
        {
            if (building == "religious" || building == "church" || element.tags.ContainsKey("religion"))
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Cathedral.svg");
            }
            else if (building == "house")
            {
                // can be House0 or House1
                int variant = GetRandomIntFromId(element.id, 2);
                iconTexture = (Texture2D)GD.Load($"res://Images/House{variant}.svg");
            }
            else if (building == "school" || building == "kindergarten" || building == "college" || building == "university")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/University.svg");
            }
            else if (building == "apartments")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/City.svg");
            }
            else if (building == "shed")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Shed.svg");
            }
            else if (building == "garage" || building == "carport")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Caravan.svg");
            }
            else if (building == "tower" || building == "water_tower" || building == "transformer_tower")
            {
                // can be Tower0 or Tower1
                int variant = GetRandomIntFromId(element.id, 2);
                iconTexture = (Texture2D)GD.Load($"res://Images/Tower{variant}.svg");
            }
            else if (building == "farm")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Farm.svg");
            }
            else if (building == "gatehouse")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Gate.svg");
            }
            else if (building == "ruins" || building == "construction")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Ruins.svg");
            }
            else if (building == "tent")
            {
                // can be Tent0 or Tent1
                int variant = GetRandomIntFromId(element.id, 2);
                iconTexture = (Texture2D)GD.Load($"res://Images/Tent{variant}.svg");
            }
            else if (building == "windmill")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Windmill.svg");
            }
            else
            {
                // default building
            }
        }

        // switch artwork type
        if (element.tags.TryGetValue("artwork_type", out string artworkType))
        {
            if (artworkType == "statue" || artworkType == "sculpture" || artworkType == "stone" || artworkType == "installation" || artworkType == "bust")
            {
                // can be Statue0, Statue1, Statue2, Statue3
                int variant = GetRandomIntFromId(element.id, 4);
                iconTexture = (Texture2D)GD.Load($"res://Images/Statue{variant}.svg");
            }
        }

        // switch memorial type
        if (element.tags.TryGetValue("memorial", out string memorial))
        {
            if (artworkType == "statue" || artworkType == "sculpture" || artworkType == "stone" || artworkType == "obelisk" || artworkType == "bust")
            {
                // can be Statue0, Statue1, Statue2, Statue3
                int variant = GetRandomIntFromId(element.id, 4);
                iconTexture = (Texture2D)GD.Load($"res://Images/Statue{variant}.svg");
            }
        }

        // switch amenity type
        if (element.tags.TryGetValue("amenity", out string amenity))
        {
            if (amenity == "fountain")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Fountain.svg");
            }
        }

        // switch man made type
        if (element.tags.TryGetValue("man_made", out string manMade))
        {
            if (manMade == "water_well")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Well.svg");
            }
        }

        if (element.tags.TryGetValue("natural", out string natural))
        {
            if (natural == "peak")
            {
                // can be Mountain0-Mountain5
                int variant = GetRandomIntFromId(element.id, 6);
                iconTexture = (Texture2D)GD.Load($"res://Images/Mountain{variant}.svg");
            }
        }

        // check if sign
        if (element.tags.ContainsKey("crossing") || element.tags.ContainsKey("traffic_sign"))
        {
            // can be Sign0, Sign1, Sign2
            int variant = GetRandomIntFromId(element.id, 3);
            iconTexture = (Texture2D)GD.Load($"res://Images/Sign{variant}.svg");
        }

        // draw building
        if (iconTexture != null)
        {
            Vector2 position = Vector2.Zero;

            if (element.GetType() == typeof(OsmWay))
            {
                // calculate position as center of way
                Vector2[] points = GetPointsFromWay((OsmWay)element, mapWorldHeight, mapWorldWidth, osmData);

                // add all points together
                foreach (Vector2 point in points)
                {
                    position += point;
                }

                // divide x and y by number of points to get mean
                position = position / new Vector2(points.Length, points.Length);
            }
            else
            {
                OsmNode node = (OsmNode)element;
                position = WorldToGamePosition(node.latitude, node.longitude, osmData.minLatitude, osmData.minLongitude, mapWorldHeight, mapWorldWidth);
            }

            DrawIconAtPoint(iconTexture, position);
        }
    }

    void DrawRoad(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth, OsmData osmData)
    {
        if (way.tags.TryGetValue("highway", out string highway))
        {
            // exclude footways and paths
            if (highway == "footway" || highway == "path")
            {
                return;
            }

            DrawLineFromWay((Texture2D)GD.Load("res://Images/Road.png"), 2, 25, way, mapWorldHeight, mapWorldWidth, osmData);

            return;
        }
    }

    void DrawSurface(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth, OsmData osmData)
    {
        bool drawSurface = false;
        Color color = Colors.White;
        int layer = 0;

        if (way.tags.TryGetValue("water", out string water))
        {
            // draw water
            drawSurface = true;
            color = Color.FromHtml("90784d");
            layer = 1;
        }
        else if (way.tags.TryGetValue("landuse", out string landuse))
        {
            if (landuse == "grass")
            {
                // draw grass
                drawSurface = true;
                color = Color.FromHtml("997c3e");
                layer = 3;
            }
        }

        if (way.tags.TryGetValue("surface", out string surface))
        {
            if (surface == "sand")
            {
                // draw sand
                drawSurface = true;
                color = Color.FromHtml("9c8444");
                layer = 1;
            }
        }

        if (way.tags.TryGetValue("natural", out string natural))
        {
            if (natural == "beach")
            {
                // draw sand
                drawSurface = true;
                color = Color.FromHtml("9c8444");
                layer = 1;
            }
            else if (natural == "sand")
            {
                // draw sand
                drawSurface = true;
                color = Color.FromHtml("9c8444");
                layer = 1;
            }
        }

        if (way.tags.TryGetValue("leisure", out string leisure))
        {
            if (leisure == "park")
            {
                // draw park
                drawSurface = true;
                color = Color.FromHtml("997c3e");
                layer = 1;
            }
            else if (leisure == "pitch")
            {
                // draw park
                drawSurface = true;
                color = Color.FromHtml("8a6a30");
                layer = 1;
            }
        }

        if (way.tags.TryGetValue("parking", out string parking))
        {
            GD.Print("parking", parking);
            if (parking == "surface")
            {
                // draw park
                drawSurface = true;
                color = Color.FromHtml("7e5f29");
                layer = 1;
            }
        }

        // check if we decided to draw land earlier
        if (drawSurface)
        {
            DrawPolygonFromWay(color, layer, way, mapWorldHeight, mapWorldWidth, osmData);
            DrawLineFromWay((Texture2D)GD.Load("res://Images/Outline.png"), layer, 5, way, mapWorldHeight, mapWorldWidth, osmData);
        }
    }

    void DrawLineFromWay(Texture2D texture, int layer, float width, OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth, OsmData osmData)
    {
        bool closed = false;
        Vector2[] points = GetPointsFromWay(way, mapWorldHeight, mapWorldWidth, osmData);

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
            Texture = texture,
            TextureMode = Line2D.LineTextureMode.Tile,
            TextureRepeat = TextureRepeatEnum.Enabled,
            Width = width,
            ZIndex = layer,
            JointMode = Line2D.LineJointMode.Round,
            Closed = closed
        });
    }

    void DrawPolygonFromWay(Color color, int layer, OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth, OsmData osmData)
    {
        AddChild(new Polygon2D()
        {
            Polygon = GetPointsFromWay(way, mapWorldHeight, mapWorldWidth, osmData),
            ZIndex = layer,
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
        PackedScene iconScene = (PackedScene)ResourceLoader.Load("res://Scenes/Icon.tscn");
        Sprite2D iconSprite = (Sprite2D)iconScene.Instantiate();
        iconSprite.Position = position;
        iconSprite.Texture = texture;

        AddChild(iconSprite);
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

    Vector2[] GetPointsFromWay(OsmWay way, decimal mapWorldHeight, decimal mapWorldWidth, OsmData osmData)
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