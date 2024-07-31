using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

/// <summary>
/// Stores the data to create a map in the Open Street Map format, including nodes, ways, and relations.
/// </summary>
public class OsmData
{
    public decimal minLatitude;
    public decimal minLongitude;
    public decimal maxLatitude;
    public decimal maxLongitude;
    public List<OsmNode> nodes;
    public List<OsmWay> ways;

    // TODO: add relations

    public static OsmData FromRawOsm(string rawOsm)
    {
        // parse osm as xml
        XDocument xmlDocument = XDocument.Parse(rawOsm);

        // parse boundaries
        XElement boundsElement = xmlDocument.Descendants("bounds").FirstOrDefault();
        decimal minLatitude = 0, minLongitude = 0, maxLatitude = 0, maxLongitude = 0;

        if (boundsElement != null)
        {
            minLatitude = (decimal)boundsElement.Attribute("minlat");
            minLongitude = (decimal)boundsElement.Attribute("minlon");
            maxLatitude = (decimal)boundsElement.Attribute("maxlat");
            maxLongitude = (decimal)boundsElement.Attribute("maxlon");
        }

        // nodes
        List<OsmNode> nodes = xmlDocument.Descendants("node")
            .Select((nodeElement) =>
            {
                // create dictionary for tags
                Dictionary<string, string> tags = nodeElement.Descendants("tag")
                    .ToDictionary(
                        tagElement => (string)tagElement.Attribute("k"),
                        tagElement => (string)tagElement.Attribute("v")
                    );

                // create and return node
                return new OsmNode
                {
                    id = (string)nodeElement.Attribute("id"),
                    visible = (bool)nodeElement.Attribute("visible"),
                    tags = tags,
                    latitude = (decimal)nodeElement.Attribute("lat"),
                    longitude = (decimal)nodeElement.Attribute("lon")
                };
            })
            .ToList();

        // ways
        // TODO: load node references too
        List<OsmWay> ways = xmlDocument.Descendants("way")
            .Select((wayElement) =>
            {
                // create dictionary for tags
                Dictionary<string, string> tags = wayElement.Descendants("tag")
                    .ToDictionary(
                        tagElement => (string)tagElement.Attribute("k"),
                        tagElement => (string)tagElement.Attribute("v")
                    );

                // get ids for referenced child nodes
                List<string> nodeChildIDs = wayElement.Descendants("nd")
                .Select(nodeReferenceElement => (string)nodeReferenceElement.Attribute("ref"))
                .ToList();

                // get node children for each id
                List<OsmNode> nodeChildren = nodeChildIDs
                .Select((id) =>
                {
                    // find node with matching id in list
                    return nodes.Find(node => node.id == id);
                })
                .ToList();

                // create way with retrieved data
                return new OsmWay
                {
                    id = (string)wayElement.Attribute("id"),
                    visible = (bool)wayElement.Attribute("visible"),
                    tags = tags,
                    nodeChildIDs = nodeChildIDs,
                    nodeChildren = nodeChildren,
                };
            })
            .ToList();

        return new OsmData()
        {
            minLatitude = minLatitude,
            minLongitude = minLongitude,
            maxLatitude = maxLatitude,
            maxLongitude = maxLongitude,
            nodes = nodes,
            ways = ways
        };
    }
}
