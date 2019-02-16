using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Prism.Mvvm;
using PropertyChanged;

namespace aframe.Updater
{
    [Serializable]
    [AddINotifyPropertyChangedInterface]
    public class ReleaseNote : BindableBase
    {
        private string version = string.Empty;

        [XmlElement(ElementName = "version")]
        public string Version
        {
            get => this.version;
            set
            {
                if (this.SetProperty(ref this.version, value))
                {
                    this.RaisePropertyChanged(nameof(VersionInfo));
                }
            }
        }

        [XmlIgnore]
        public Version VersionInfo =>
            !string.IsNullOrEmpty(this.Version) ?
            new Version(this.Version) :
            null;

        [XmlElement(ElementName = "channel")]
        public ReleaseChannels ReleaseChannel { get; set; } = ReleaseChannels.Stable;

        [XmlElement(ElementName = "timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.MinValue;

        [XmlElement(ElementName = "uri")]
        public string Uri { get; set; } = string.Empty;

        private string description = string.Empty;

        [XmlElement(ElementName = "description")]
        public string Description
        {
            get => this.description;
            set
            {
                var text = string.Empty;

                if (!string.IsNullOrEmpty(value))
                {
                    using (var sr = new StringReader(value))
                    {
                        while (true)
                        {
                            var line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }

                            line = line.Trim();
                            if (string.IsNullOrEmpty(line))
                            {
                                continue;
                            }

                            text += !string.IsNullOrEmpty(text) ?
                                Environment.NewLine + line :
                                line;
                        }
                    }
                }

                this.SetProperty(ref this.description, text);
            }
        }

        private WebClient downloadClient;

        public async Task DownloadAsync(
            string destination,
            DownloadProgressChangedEventHandler progressChangedEventHandler = null,
            AsyncCompletedEventHandler completedEventHandler = null)
        {
            using (this.downloadClient = new WebClient())
            {
                if (progressChangedEventHandler != null)
                {
                    this.downloadClient.DownloadProgressChanged += progressChangedEventHandler;
                }

                if (completedEventHandler != null)
                {
                    this.downloadClient.DownloadFileCompleted += completedEventHandler;
                }

                await this.downloadClient.DownloadFileTaskAsync(
                    this.Uri,
                    destination);
            }

            this.downloadClient = null;
        }

        public void CancelDownload()
            => this.downloadClient?.CancelAsync();
    }
}
