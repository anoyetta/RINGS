using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace aframe
{
    public partial class ColorDialogContent : UserControl
    {
        public ColorDialogContent()
        {
            this.InitializeComponent();

            this.Color = Colors.White;

            this.Loaded += this.ColorDialogContent_Loaded;
            this.PredefinedColorsListBox.SelectionChanged += this.PredefinedColorsListBox_SelectionChanged;
            this.RTextBox.ValueChanged += (s, e) => this.ToHex();
            this.GTextBox.ValueChanged += (s, e) => this.ToHex();
            this.BTextBox.ValueChanged += (s, e) => this.ToHex();
            this.ATextBox.ValueChanged += (s, e) => this.ToHex();
            this.HexTextBox.LostFocus += (s, e) =>
            {
                var color = Colors.White;

                try
                {
                    color = (Color)ColorConverter.ConvertFromString(this.HexTextBox.Text);
                }
                catch
                {
                }

                this.RTextBox.Value = color.R;
                this.GTextBox.Value = color.G;
                this.BTextBox.Value = color.B;
                this.ATextBox.Value = !this.IgnoreAlpha ? color.A : 255;

                this.ToPreview();
            };
        }

        public Color Color { get; set; }

        private bool ignoreAlpha;

        public bool IgnoreAlpha
        {
            get => this.ignoreAlpha;
            set
            {
                if (this.ignoreAlpha != value)
                {
                    this.ignoreAlpha = value;

                    if (this.ignoreAlpha)
                    {
                        this.ATextBox.Value = 255;
                        this.APanel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        this.APanel.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public void Apply()
        {
            var color = Colors.White;

            try
            {
                color = (Color)ColorConverter.ConvertFromString(this.HexTextBox.Text);
            }
            catch
            {
            }

            this.Color = color;
        }

        private async void ColorDialogContent_Loaded(object sender, RoutedEventArgs e)
        {
            var item = await Task.Run(() =>
                this.PredefinedColorsListBox.Items.Cast<PredefinedColor>()
                .FirstOrDefault(x => x.Color == this.Color));

            if (item != null)
            {
                this.PredefinedColorsListBox.SelectedItem = item;
                (this.PredefinedColorsListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem)?.Focus();
            }

            if (this.PredefinedColorsListBox.SelectedItem == null &&
                this.Color != null)
            {
                this.RTextBox.Value = this.Color.R;
                this.GTextBox.Value = this.Color.G;
                this.BTextBox.Value = this.Color.B;
                this.ATextBox.Value = !this.IgnoreAlpha ? this.Color.A : 255;
            }
        }

        private void PredefinedColorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.PredefinedColorsListBox.SelectedItem != null)
            {
                var color = ((PredefinedColor)this.PredefinedColorsListBox.SelectedItem).Color;

                this.RTextBox.Value = color.R;
                this.GTextBox.Value = color.G;
                this.BTextBox.Value = color.B;
                this.ATextBox.Value = !this.IgnoreAlpha ? color.A : 255;
            }
        }

        private void ToHex()
        {
            var color = Color.FromArgb(
                (byte)(this.ATextBox.Value ?? 0),
                (byte)(this.RTextBox.Value ?? 0),
                (byte)(this.GTextBox.Value ?? 0),
                (byte)(this.BTextBox.Value ?? 0));

            this.HexTextBox.Text = color.ToString();

            this.ToPreview();
        }

        private void ToPreview()
        {
            var color = Colors.White;

            try
            {
                color = (Color)ColorConverter.ConvertFromString(this.HexTextBox.Text);
            }
            catch
            {
            }

            this.PreviewRectangle.Fill = new SolidColorBrush(color);
        }
    }
}
