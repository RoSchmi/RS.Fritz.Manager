namespace RS.Fritz.Manager.API
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Item")]
    public sealed record CaptureControl
    {
        [XmlElement(ElementName = "Index")]
        public int Index { get; set; }

        [XmlElement(ElementName = "IPAddress")]

        public string? IPAddress { get; set; }
    }
}