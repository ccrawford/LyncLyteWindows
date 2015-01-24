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

namespace LyncWPFApplication3
{
    /// <summary>
    /// Interaction logic for NiceConfig.xaml
    /// </summary>
    public partial class NiceConfig : Window
    {
        LyncVM _vm;


        public NiceConfig()
        {
            InitializeComponent();
         //   _vm = (LyncVM)this.DataContext;

         //   _vm.PropertyChanged += _vm_PropertyChanged;
        }

        void _vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "currentLightColor")        // _vm.currentLightColor
            {
                this.Dispatcher.Invoke((Action)(() =>
                    {
                        this.Icon = BitmapFrame.Create(new Uri("pacK://application:,,,/Icons/" + _vm.currentLightColor.ToLower() + ".png", UriKind.RelativeOrAbsolute));
                    }));
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	DragMove();
        }
		
		private void CommandBinding_CanExecute_Close(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
    }
}
