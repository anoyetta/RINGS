using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace aframe
{
    public partial class FontDialogContent : UserControl
    {
        private FontInfo fontInfo = (FontInfo)FontInfo.DefaultFont.Clone();

        public FontDialogContent()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) =>
            {
                this.ShowFontInfo();

                // リストボックスにフォーカスを設定する
                ListBox box;

                box = this.FontStyleListBox;
                if (box.SelectedItem != null)
                {
                    var item =
                        box.ItemContainerGenerator.ContainerFromItem(box.SelectedItem)
                        as ListBoxItem;

                    if (item != null)
                    {
                        item.Focus();
                    }
                }

                box = this.FontFamilyListBox;
                if (box.SelectedItem != null)
                {
                    var item =
                        box.ItemContainerGenerator.ContainerFromItem(box.SelectedItem)
                        as ListBoxItem;

                    if (item != null)
                    {
                        item.Focus();
                    }
                }
            };

            this.FontFamilyListBox.SelectionChanged += this.FontFamilyListBox_SelectionChanged;
        }

        public FontInfo FontInfo
        {
            get
            {
                return this.fontInfo;
            }
            set
            {
                this.fontInfo = value;
                this.ShowFontInfo();
            }
        }

        internal void OKBUtton_Click(object sender, RoutedEventArgs e)
        {
            this.fontInfo = new FontInfo(
                this.PreviewTextBlock.FontFamily,
                this.PreviewTextBlock.FontSize,
                this.PreviewTextBlock.FontStyle,
                this.PreviewTextBlock.FontWeight,
                this.PreviewTextBlock.FontStretch);
        }

        private void FontFamilyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.FontStyleListBox.SelectedIndex = 0;
        }

        private void ShowFontInfo()
        {
            if (this.fontInfo == null)
            {
                return;
            }

            this.FontSizeTextBox.Value = this.fontInfo.Size;

            int i = 0;
            foreach (FontFamily item in this.FontFamilyListBox.Items)
            {
                if (this.fontInfo.FontFamily != null)
                {
                    if (item.Source == this.fontInfo.FontFamily.Source ||
                        item.FamilyNames.Any(x => x.Value == this.fontInfo.FontFamily.Source))
                    {
                        break;
                    }
                }

                i++;
            }

            if (i < this.FontFamilyListBox.Items.Count)
            {
                this.FontFamilyListBox.SelectedIndex = i;
                this.FontFamilyListBox.ScrollIntoView(this.FontFamilyListBox.Items[i]);
            }

            this.FontStyleListBox.SelectedItem = this.fontInfo.Typeface;
        }
    }
}
