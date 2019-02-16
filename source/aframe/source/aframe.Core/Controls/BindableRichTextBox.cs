using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace aframe
{
    public class BindableRichTextBox : RichTextBox
    {
        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(
            nameof(Document),
            typeof(FlowDocument),
            typeof(BindableRichTextBox),
            new UIPropertyMetadata(
                null,
                OnRichTextItemsChanged));

        public new FlowDocument Document
        {
            get => (FlowDocument)this.GetValue(DocumentProperty);
            set => this.SetValue(DocumentProperty, value);
        }

        private static void OnRichTextItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as RichTextBox;
            if (control != null)
            {
                control.Document = e.NewValue as FlowDocument;
            }
        }
    }
}
