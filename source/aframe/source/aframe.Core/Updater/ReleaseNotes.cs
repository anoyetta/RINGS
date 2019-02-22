using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Prism.Mvvm;
using PropertyChanged;

namespace aframe.Updater
{
    [Serializable]
    [AddINotifyPropertyChangedInterface]
    [XmlRoot(ElementName = "release_notes")]
    public class ReleaseNotes : BindableBase
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; } = string.Empty;

        private List<ReleaseNote> notes = new List<ReleaseNote>();

        [XmlElement(ElementName = "note")]
        public ReleaseNote[] Notes
        {
            get => this.notes.ToArray();
            set
            {
                this.notes.Clear();
                this.notes.AddRange(value);
                this.RaisePropertyChanged();
            }
        }

        public void Serialize(
            string fileName)
        {
            var dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(
                fileName,
                this.Serialize(),
                new UTF8Encoding(false));
        }

        public const string DefaultFileName = "RELEASE_NOTES.xml";

        public string Serialize()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                Indent = true,
                NewLineHandling = NewLineHandling.None,
                NewLineOnAttributes = true,
            }))
            {
                var xs = new XmlSerializer(this.GetType());
                xs.Serialize(xw, this, ns);
            }

            sb.Replace("utf-16", "utf-8");

            return sb.ToString();
        }

        public static ReleaseNotes Deserialize(
            string fileNameOrXMLText)
        {
            var source = string.Empty;

            if (File.Exists(fileNameOrXMLText))
            {
                source = File.ReadAllText(fileNameOrXMLText, new UTF8Encoding(false));
            }
            else
            {
                source = fileNameOrXMLText;
            }

            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            var obj = default(ReleaseNotes);
            using (var reader = new StringReader(source))
            {
                var xs = new XmlSerializer(typeof(ReleaseNotes));
                obj = xs.Deserialize(reader) as ReleaseNotes;
            }

            return obj;
        }

        public static async Task<ReleaseNotes> DeserializeAsync(
            Uri uri)
        {
            var source = string.Empty;

            using (var client = new WebClient())
            {
                try
                {
                    client.Encoding = new UTF8Encoding(false);
                    source = await client.DownloadStringTaskAsync(uri);
                }
                catch (WebException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    return null;
                }
            }

            return Deserialize(source);
        }

        public ReleaseNote GetNewerVersion(
            Assembly currentAssembly,
            ReleaseChannels channel = ReleaseChannels.Stable,
            bool isForce = false)
        {
            var currentVersion = currentAssembly.GetVersion();

            ReleaseChannels currentChannel;
            var config = currentAssembly.GetConfiguration();
            if (!Enum.TryParse(config, out currentChannel))
            {
                currentChannel = ReleaseChannels.Stable;
            }

            var note = (
                from x in this.Notes
                where
                x.ReleaseChannel <= channel &&
                x.VersionInfo != null &&
                x.VersionInfo >= currentVersion
                orderby
                x.Version descending,
                x.ReleaseChannel ascending
                select
                x).FirstOrDefault();

            if (note == null)
            {
                return null;
            }

            if (!isForce)
            {
                if (note.VersionInfo == currentVersion &&
                    note.ReleaseChannel == currentChannel)
                {
                    return null;
                }
            }

            return note;
        }
    }
}
