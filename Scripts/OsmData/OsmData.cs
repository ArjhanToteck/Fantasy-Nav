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
    public List<OsmNode> nodes;
    public List<OsmWay> ways;
    public List<OsmRelation> relations;

    public static OsmData FromRawOsm(string rawOsm)
    {
        // parse osm as xml
        XDocument xmlDocument = XDocument.Parse(rawOsm);

        // parse boundaries
        XElement boundsElement = xmlDocument.Descendants("bounds").FirstOrDefault();
        float minLatitude = 0, minLongitude = 0, maxLatitude = 0, maxLongitude = 0;

        if (boundsElement != null)
        {
            minLatitude = (float)boundsElement.Attribute("minlat");
            minLongitude = (float)boundsElement.Attribute("minlon");
            maxLatitude = (float)boundsElement.Attribute("maxlat");
            maxLongitude = (float)boundsElement.Attribute("maxlon");
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
                    latitude = (float)nodeElement.Attribute("lat"),
                    longitude = (float)nodeElement.Attribute("lon")
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
                List<string> nodeChildIds = wayElement.Descendants("nd")
                    .Select(nodeReferenceElement => (string)nodeReferenceElement.Attribute("ref"))
                    .ToList();

                // get node children for each id
                List<OsmNode> nodeChildren = nodeChildIds
                    .Select(id => nodes.Find(node => node.id == id))
                    .Where(node => node != null)
                    .ToList();

                // create way with retrieved data
                return new OsmWay
                {
                    id = (string)wayElement.Attribute("id"),
                    visible = (bool)wayElement.Attribute("visible"),
                    tags = tags,
                    nodeChildIds = nodeChildIds,
                    nodeChildren = nodeChildren,
                };
            })
            .ToList();

        List<OsmRelation> relations = xmlDocument.Descendants("relation")
        .Select((relationElement) =>
        {
            // create dictionary for tags
            Dictionary<string, string> tags = relationElement.Descendants("tag")
                .ToDictionary(
                    tagElement => (string)tagElement.Attribute("k"),
                    tagElement => (string)tagElement.Attribute("v")
                );

            // get ids for referenced child nodes
            List<string> outerBoundaryIds = new List<string>();
            List<string> innerBoundaryIds = new List<string>();

            // loop through ways in relation
            foreach (var member in relationElement.Descendants("member"))
            {
                // skip non ways
                string type = (string)member.Attribute("type");
                if (type != "way")
                {
                    continue;
                }

                // get role and id
                string role = (string)member.Attribute("role");
                string refId = (string)member.Attribute("ref");

                if (refId != null)
                {
                    // separate by inner/outer
                    if (role == "outer")
                    {
                        outerBoundaryIds.Add(refId);
                    }
                    else if (role == "inner")
                    {
                        innerBoundaryIds.Add(refId);
                    }
                }
            }

            // get way children for each outer id
            List<OsmWay> outerBoundaries = outerBoundaryIds
                .Select(id => ways.Find(way => way.id == id))
                .Where(way => way != null)
                .ToList();

            // get way children for each inner id
            List<OsmWay> innerBoundaries = innerBoundaryIds
                .Select(id => ways.Find(way => way.id == id))
                .Where(way => way != null)
                .ToList();

            // create relation with retrieved data
            return new OsmRelation
            {
                id = (string)relationElement.Attribute("id"),
                visible = (bool)relationElement.Attribute("visible"),
                tags = tags,
                innerBoundaries = innerBoundaries,
                innerBoundaryIds = innerBoundaryIds,
                outerBoundaries = outerBoundaries,
                outerBoundaryIds = outerBoundaryIds
            };
        })
        .ToList();

        return new OsmData()
        {
            nodes = nodes,
            ways = ways,
            relations = relations
        };
    }
}
