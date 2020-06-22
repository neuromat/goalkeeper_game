using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
public class GKGConfig
{
    [XmlAttribute("webSite")]
    public string webSite;

    [XmlElement("URL")]
    public string URL;
}