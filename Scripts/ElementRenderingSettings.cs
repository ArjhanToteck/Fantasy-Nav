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
public class ElementDrawSettings
{
    public Color color;
    public Texture2D texture;
    public uint visibilityLayer;

    public static ElementDrawSettings GetWaySettings(OsmWay way)
    {
        // skip checking invisible ways
        if (!way.visible)
        {
            return null;
        }

        ElementDrawSettings drawSettings = new ElementDrawSettings();

        // searching tags
        if (way.tags.TryGetValue("highway", out string highway))
        {
            if (highway != "footway" && highway != "path")
            {
                // make roads black
                drawSettings.color = Colors.Black;
                drawSettings.visibilityLayer = 2;
                return drawSettings;
            }
        }

        if (way.tags.TryGetValue("landuse", out string landuse))
        {
            if (landuse == "grass")
            {
                // make grass green
                drawSettings.color = Colors.Green;
                drawSettings.visibilityLayer = 3;
                return drawSettings;
            }
        }

        if (way.tags.TryGetValue("leisure", out string leisure))
        {
            if (leisure == "park")
            {
                // make park green
                drawSettings.color = Colors.Green;
                drawSettings.visibilityLayer = 1;
                return drawSettings;
            }
        }

        if (way.tags.TryGetValue("building", out string building))
        {
            // make buildings gray
            drawSettings.color = Colors.DarkGray;
            drawSettings.visibilityLayer = 4;
            return drawSettings;
        }

        // don't draw by default
        return null;
    }

    public static ElementDrawSettings GetNodeSettings(OsmNode node)
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