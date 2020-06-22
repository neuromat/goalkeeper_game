using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("ConfigCollection")]
public class GKGConfigContainer
{
    [XmlArray("Items")]
    [XmlArrayItem("Item")]
    public List<GKGConfig> configItems = new List<GKGConfig>();

    public static GKGConfigContainer Load()
    {
        TextAsset _xml = Resources.Load<TextAsset>("GKGConfig/GKGConfig");
        XmlSerializer serializer = new XmlSerializer(typeof(GKGConfigContainer));
        StringReader reader = new StringReader(_xml.text);
        GKGConfigContainer items = serializer.Deserialize(reader) as GKGConfigContainer;
        reader.Close();

        return items;
    }
}