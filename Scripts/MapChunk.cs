using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MapChunk : Node2D
{
    public OsmData osmData;
    public float gameChunkSize;
    public float worldChunkSize;

    public void DrawMap()
    {
        // draw ways
        foreach (OsmWay way in osmData.ways)
        {
            // add way node as child (godot node not osm node)
            DrawWay(way);
        }

        // draw nodes
        foreach (OsmNode node in osmData.nodes)
        {
            // add node as child
            DrawIcon(node);
        }
    }

    void DrawWay(OsmWay way)
    {
        // check if invisible
        if (!way.visible)
        {
            return;
        }

        // draw way
        DrawIcon(way);
        DrawRoad(way);
        DrawSurface(way);
    }

    void DrawIcon(OsmElement element)
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
            else if (building == "house" || building == "terrace" || building == "detached" || building == "semidetached_house" || building == "bungalow" || building == "	manor" || building == "villa")
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
            else if (building == "garage" || building == "carport" || building == "static_caravan")
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
            else if (building == "hotel")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Inn.svg");
            }
            else
            {
                // default building
                // can be Building0-4
                int variant = GetRandomIntFromId(element.id, 5);
                iconTexture = (Texture2D)GD.Load($"res://Images/Building{variant}.svg");
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
            else if (manMade == "mineshaft")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Mine.svg");
            }
        }

        // natural
        if (element.tags.TryGetValue("natural", out string natural))
        {
            if (natural == "peak")
            {
                // can be Mountain0-Mountain5
                int variant = GetRandomIntFromId(element.id, 6);
                iconTexture = (Texture2D)GD.Load($"res://Images/Mountain{variant}.svg");
            }
            else if (natural == "hill")
            {
                // can be Hill 0-2
                int variant = GetRandomIntFromId(element.id, 3);
                iconTexture = (Texture2D)GD.Load($"res://Images/Hill{variant}.svg");
            }
        }

        // attraction
        if (element.tags.TryGetValue("attraction", out string attraction))
        {
            if (attraction == "maze")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Maze.svg");
            }
        }

        // leisure
        if (element.tags.TryGetValue("leisure", out string leisure))
        {
            if (leisure == "maze")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Maze.svg");
            }
        }

        // tourism
        if (element.tags.TryGetValue("tourism", out string tourism))
        {
            if (tourism == "hotel")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Inn.svg");
            }
        }

        // historic
        if (element.tags.TryGetValue("historic", out string historic))
        {
            if (historic == "mine" || historic == "mine_shaft")
            {
                iconTexture = (Texture2D)GD.Load("res://Images/Mine.svg");
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
                Vector2[] points = GetPointsFromWay((OsmWay)element);

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
                position = WorldToGamePosition(node.latitude, node.longitude, osmData.minLatitude, osmData.minLongitude);
            }

            DrawIconAtPoint(iconTexture, position);
        }
    }

    void DrawRoad(OsmWay way)
    {
        if (way.tags.TryGetValue("highway", out string highway))
        {
            // exclude footways and paths
            if (highway == "footway" || highway == "path")
            {
                return;
            }

            DrawLineFromWay((Texture2D)GD.Load("res://Images/Road.png"), 4, 40, way);

            return;
        }
    }

    void DrawSurface(OsmWay way)
    {
        bool drawSurface = false;
        Color color = Colors.White;
        int layer = 0;

        /*
        // yeah we're not doing buildings anymore
        if (way.tags.ContainsKey("building"))
        {
            // draw building outline
            drawSurface = true;
            color = Color.FromHtml("89652c");
            layer = 5;
        }
        else*/
        if (way.tags.ContainsKey("water"))
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
            /*else if (landuse == "residential")
            {
                // draw sidewalk
                drawSurface = true;
                color = Color.FromHtml("94743b");
                layer = 3;
            }*/
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
            else if (natural == "wetland")
            {
                // draw grass
                drawSurface = true;
                color = Color.FromHtml("997c3e");
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
            DrawPolygonFromWay(color, layer, way);
            DrawLineFromWay((Texture2D)GD.Load("res://Images/Outline.png"), layer, 8, way);
        }
    }

    void DrawLineFromWay(Texture2D texture, int layer, float width, OsmWay way)
    {
        bool closed = false;
        Vector2[] points = GetPointsFromWay(way);

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

    void DrawPolygonFromWay(Color color, int layer, OsmWay way)
    {
        AddChild(new Polygon2D()
        {
            Polygon = GetPointsFromWay(way),
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
        return new Random(unchecked((int)long.Parse(id))).Next(max);
    }

    Vector2[] GetPointsFromWay(OsmWay way)
    {
        // get points from node positions
        Vector2[] points = way.nodeChildren
            .Select((node) =>
            {
                // convert latitude and longitude to in-game position
                return WorldToGamePosition(node.latitude, node.longitude, osmData.minLatitude, osmData.minLongitude);
            })
            .ToArray();

        return points;
    }

    Vector2 WorldToGamePosition(double latitude, double longitude, double minLatitude, double minLongitude)
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