using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bomberman.View
{
    /// <summary>
    /// Interaction logic for SavedGamesWindow.xaml
    /// </summary>
    public partial class SavedGamesWindow : Window
    {
        public SavedGamesWindow()
        {
            InitializeComponent();
            lb_Saves.SelectionChanged += lb_Saves_Selected;
            tb_Save.GotFocus += tb_Save_Selected;
        }

        private void lb_Saves_Selected(object sender, RoutedEventArgs e)
        {
            if (lb_Saves.SelectedIndex != -1)
            {
                btn_Load.Visibility = Visibility.Visible;
                btn_Delete.Visibility = Visibility.Visible;
            }
            else
            {
                btn_Delete.Visibility = Visibility.Hidden;
            }
        }
        private void tb_Save_Selected(object sender, RoutedEventArgs e)
        {
            tb_Save.Text = "";
            
        }
        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            btn_Delete.Visibility = Visibility.Hidden;
        }
    }
}
