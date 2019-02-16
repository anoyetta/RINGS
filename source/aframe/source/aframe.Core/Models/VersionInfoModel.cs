using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using aframe.Updater;
using Prism.Mvvm;
using Reactive.Bindings;

namespace aframe.Models
{
    [DataContract]
    public class VersionInfoModel : BindableBase
    {
        public VersionInfoModel() : this(null)
        {
        }

        public VersionInfoModel(
            Assembly assembly)
        {
            this.Assembly.Value = assembly;

            this.Assembly.Subscribe(x =>
            {
                if (x == null)
                {
                    return;
                }

                this.ProductName.Value = x.GetProduct();
                this.Title.Value = x.GetTitle();
                this.Version.Value = x.GetVersion().ToString();
                this.ReleaseChannel.Value = x.GetReleaseChannels();
                this.Timestamp.Value = new FileInfo(x.Location).CreationTime;
                this.Copyright.Value = x.GetCopyright();
            });
        }

        [IgnoreDataMember]
        public ReactiveProperty<Assembly> Assembly { get; } = new ReactiveProperty<Assembly>();

        [DataMember]
        public ReactiveProperty<string> ProductName { get; set; } = new ReactiveProperty<string>();

        [DataMember]
        public ReactiveProperty<string> Title { get; set; } = new ReactiveProperty<string>();

        [DataMember]
        public ReactiveProperty<string> EndPointUri { get; set; } = new ReactiveProperty<string>();

        [DataMember]
        public ReactiveProperty<string> Version { get; set; } = new ReactiveProperty<string>();

        [DataMember]
        public ReactiveProperty<ReleaseChannels> ReleaseChannel { get; set; } = new ReactiveProperty<ReleaseChannels>();

        [DataMember]
        public ReactiveProperty<DateTime> Timestamp { get; set; } = new ReactiveProperty<DateTime>();

        [DataMember]
        public ReactiveProperty<string> Copyright { get; set; } = new ReactiveProperty<string>();

        public override string ToString()
            => $"{this.ProductName.Value} v{this.Version.Value} {this.Timestamp.Value:yyyy-MM-dd HH:m:ss}";
    }
}
