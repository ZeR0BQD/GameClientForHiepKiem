using System.Xml.Serialization;

namespace GameClient
{
    [XmlRoot(ElementName = "Position")]
    public class Position
    {
        [XmlAttribute(AttributeName = "MapId")]
        public int MapId { get; set; }

        [XmlAttribute(AttributeName = "PosX")]
        public int PosX { get; set; }

        [XmlAttribute(AttributeName = "PosY")]
        public int PosY { get; set; }
    }
}
