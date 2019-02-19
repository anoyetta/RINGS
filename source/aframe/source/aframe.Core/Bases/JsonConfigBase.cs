using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Hjson;
using Newtonsoft.Json;

namespace aframe
{
    [JsonObject]
    public abstract class JsonConfigBase :
        INotifyPropertyChanged
    {
        internal static readonly object Locker = new object();

        public JsonConfigBase()
        {
            this.LoadDefaultValues();
        }

        /// <summary>
        /// Singletonインスタンスを返すかまたは設定ファイルをロードして返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instanceContainer">
        /// Singletonインスタンスを格納するコンテナ</param>
        /// <param name="fileName">
        /// 設定ファイルのパス</param>
        /// <returns>
        /// 設定の実装オブジェクト</returns>
        public static T CreateOrGetSingleton<T>(
            ref T instanceContainer,
            string fileName)
            where T : JsonConfigBase, new()
        {
            if (instanceContainer != null)
            {
                return instanceContainer;
            }

            instanceContainer = Load<T>(fileName);
            return instanceContainer;
        }

        private const string EmptyJson = "{}";

        /// <summary>
        /// デフォルト値のオブジェクトを生成する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// すべての設定がデフォルト値である設定オブジェクト</returns>
        public static T GetDefault<T>()
            where T : JsonConfigBase, new()
            => new T();

        /// <summary>
        /// 設定ファイルからロードする（デシリアライズする）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName">
        /// 設定ファイルのパス</param>
        /// <returns>
        /// ロードした設定オブジェクト</returns>
        public static T Load<T>(
            string fileName,
            out bool isFirstLoad) where T : JsonConfigBase, new()
        {
            isFirstLoad = true;

            fileName = SwitchFileName(fileName);

            lock (JsonConfigBase.Locker)
            {
                var json = EmptyJson;

                if (File.Exists(fileName))
                {
                    isFirstLoad = false;
                    json = File.ReadAllText(
                        fileName,
                        new UTF8Encoding(false));
                }

                var data = default(T);

                try
                {
                    // HJSON (コメント付きJSON） -> JSON の変換を行いつつDeserializeする
                    data = JsonConvert.DeserializeObject(
                        HjsonValue.Parse(json).ToString(),
                        typeof(T),
                        new JsonSerializerSettings()
                        {
                            Formatting = Formatting.Indented,
                            DefaultValueHandling = DefaultValueHandling.Ignore,
                        }) as T;
                }
                catch (Exception)
                {
                    data = GetDefault<T>();
                }

                if (data == null)
                {
                    data = GetDefault<T>();
                }

                if (!File.Exists(fileName))
                {
                    data?.Save(fileName);
                }

                return data;
            }
        }

        /// <summary>
        /// 設定ファイルからロードする（デシリアライズする）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName">
        /// 設定ファイルのパス</param>
        /// <returns>
        /// ロードした設定オブジェクト</returns>
        public static T Load<T>(
            string fileName) where T : JsonConfigBase, new()
            => Load<T>(fileName, out bool b);

        public static string SwitchFileName(
            string baseFileName)
        {
#if DEBUG
            const string DebugKeyword = ".debug";

            if (baseFileName.Contains(DebugKeyword))
            {
                return baseFileName;
            }

            return Path.Combine(
                Path.GetDirectoryName(baseFileName),
                Path.GetFileNameWithoutExtension(baseFileName) + DebugKeyword + Path.GetExtension(baseFileName));
#else
            return baseFileName;
#endif
        }

        #region Debug Mode Switcher

        /// <summary>
        /// DEBUGビルドか？
        /// </summary>
#if DEBUG
        public const bool IsDebugBuild = true;
#else
        public const bool IsDebugBuild = false;
#endif

        [JsonProperty(PropertyName = "debug_mode", Order = int.MinValue)]
        public bool DebugMode { get; set; }

        private static readonly KeyValuePair<string, bool> DebugModeDefaultValue =
            new KeyValuePair<string, bool>(nameof(DebugMode), IsDebugBuild);

        public T GetCurrentModeValue<T>(
            IEnumerable<KeyValueContainer> modeSwitchedValues,
            string name)
            => (T)(!this.DebugMode ?
            modeSwitchedValues.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))?.ForReleaseValue :
            modeSwitchedValues.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))?.ForDebugValue);

        #endregion Debug Mode Switcher

        #region Default Values

        [JsonIgnore]
        public abstract Dictionary<string, object> DefaultValues { get; }

        public void LoadDefaultValues()
        {
            if (!DefaultValues.ContainsKey(DebugModeDefaultValue.Key))
            {
                DefaultValues.Add(DebugModeDefaultValue.Key, DebugModeDefaultValue.Value);
            }

            var pis = this.GetType().GetProperties();
            foreach (var pi in pis)
            {
                try
                {
                    var defaultValue =
                        DefaultValues.ContainsKey(pi.Name) ?
                        DefaultValues[pi.Name] :
                        null;

                    if (defaultValue != null)
                    {
                        pi.SetValue(this, defaultValue);
                    }
                }
                catch
                {
                    Debug.WriteLine($"Error! Config load default values : {pi.Name}");
                }
            }
        }

        #endregion Default Values

        #region INotifyPropertyChanged

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(
            [CallerMemberName]string propertyName = null)
            => this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

        protected virtual bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

            return true;
        }

        #endregion INotifyPropertyChanged
    }

    public static class JsonConfigBaseExtensions
    {
        /// <summary>
        /// 設定内容をファイルに保存する（シリアライズする）
        /// </summary>
        /// <param name="config">
        /// 保存する設定オブジェクト</param>
        /// <param name="fileName">
        /// 保存先のファイルパス</param>
        public static void Save(
            this JsonConfigBase config,
            string fileName)
        {
            fileName = JsonConfigBase.SwitchFileName(fileName);

            lock (JsonConfigBase.Locker)
            {
                if (config == null)
                {
                    return;
                }

                var json = JsonConvert.SerializeObject(
                    config,
                    Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                    });

                var dir = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(
                    fileName,
                    json + Environment.NewLine,
                    new UTF8Encoding(false));
            }
        }
    }

    [JsonObject]
    public class KeyValueContainer :
        INotifyPropertyChanged
    {
        private string name;

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        private object forReleaseValue;

        [JsonProperty(PropertyName = "value")]
        public object ForReleaseValue
        {
            get => this.forReleaseValue;
            set => this.SetProperty(ref this.forReleaseValue, value);
        }

        private object forDebugValue;

        [JsonProperty(PropertyName = "debug")]
        public object ForDebugValue
        {
            get => this.forDebugValue;
            set => this.SetProperty(ref this.forDebugValue, value);
        }

        #region INotifyPropertyChanged

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(
            [CallerMemberName]string propertyName = null)
            => this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

        protected virtual bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

            return true;
        }

        #endregion INotifyPropertyChanged
    }
}
