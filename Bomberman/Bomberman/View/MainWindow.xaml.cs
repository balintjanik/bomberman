using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bomberman
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var listViewItemsSource = (INotifyCollectionChanged)MessageListBox.Items.SourceCollection;
            listViewItemsSource.CollectionChanged += MessageListBoxViewCollectionChanged;
        }

        private void MessageListBoxViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var border = (Decorator)VisualTreeHelper.GetChild(MessageListBox, 0);
            var scrollViewer = (ScrollViewer)border.Child;
            scrollViewer.ScrollToEnd();
        }
    }
}