using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Xml.Linq;

/// <summary>
/// Instructions on how to render an element on the map (texture, color, etc.).
/// </summary>
public class ElementStyle
{
    public Color color = Colors.White;
    public Texture2D texture;
    public uint visibilityLayer;

    public static ElementStyle GetWayStyle(OsmWay way)
    {
        // skip checking invisible ways
        if (!way.visible)
        {
            return null;
        }

        // searching tags
        if (way.tags.TryGetValue("highway", out string highway))
        {
            if (highway != "footway" && highway != "path")
            {
                // make roads black
                return new ElementStyle()
                {
                    texture = (Texture2D)GD.Load("res://Images/DirtRoad.png"),
                    visibilityLayer = 2
                };
            }
        }

        if (way.tags.TryGetValue("landuse", out string landuse))
        {
            if (landuse == "grass")
            {
                // make grass green
                return new ElementStyle()
                {
                    texture = (Texture2D)GD.Load("res://Images/Grass.png"),
                    visibilityLayer = 3
                };
            }
        }

        if (way.tags.TryGetValue("leisure", out string leisure))
        {
            if (leisure == "park")
            {
                // make park green
                return new ElementStyle()
                {
                    texture = (Texture2D)GD.Load("res://Images/Grass.png"),
                    visibilityLayer = 1
                };
            }
        }

        if (way.tags.TryGetValue("building", out string building))
        {
            // make buildings gray
            return new ElementStyle()
            {
                color = Colors.DarkGray,
                visibilityLayer = 4
            };
        }

        // don't draw by default
        return null;
    }

    public static ElementStyle GetNodeStyle(OsmNode node)
    {
        // skip checking invisible nodes
        if (!node.visible)
        {
            return null;
        }


        // TODO: search tags

        // don't draw by default
        return null;
    }
}