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

namespace WczytywaczObrazow
{
    /// <summary>
    /// Logika interakcji dla klasy ChooseGrayScale.xaml
    /// </summary>
    public partial class ChooseGrayScale : Window
    {
        private bool whichOne;
        public ChooseGrayScale()
        {
            InitializeComponent();
        }
        public bool Response
        {
            get { return whichOne; }
            set { whichOne = value; }
        }

        private void NumberOne(object sender, System.Windows.RoutedEventArgs e)
        {
            whichOne = true;
            DialogResult = true;
        }
        private void NumberTwo(object sender, System.Windows.RoutedEventArgs e)
        {
            whichOne = false;
            DialogResult = true;
        }
    }
}
