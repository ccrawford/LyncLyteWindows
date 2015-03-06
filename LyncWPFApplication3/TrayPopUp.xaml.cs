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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LyncWPFApplication3
{
    /// <summary>
    /// Interaction logic for TrayPopUp.xaml
    /// </summary>
    public partial class TrayPopUp : UserControl
    {

        public static readonly RoutedEvent PopupRestoreEvent = EventManager.RegisterRoutedEvent(
            "PopupRestore", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TrayPopUp));

        public event RoutedEventHandler PopupRestore
        {
            add { AddHandler(PopupRestoreEvent, value); }
            remove { RemoveHandler(PopupRestoreEvent, value); }
        }

        public TrayPopUp()
        {
            InitializeComponent();

        }

        private void PopupRestoreCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void PopupRestoreExecute(object sender, ExecutedRoutedEventArgs e)
        {
            // Send the message up the chain.
            
        }

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(TrayPopUp));

        public string StatusText
        {
            get { return GetValue(StatusTextProperty).ToString(); }
            set { SetValue(StatusTextProperty, value.ToString()); }
        }


        public string UsbImage
        {
            get { return (string)GetValue(UsbImageProperty); }
            set { SetValue(UsbImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UsbImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsbImageProperty =
            DependencyProperty.Register("UsbImage", typeof(string), typeof(TrayPopUp), new PropertyMetadata("Icons/usb icon.png"));

        public string LightImage
        {
            get { return (string)GetValue(LightImageProperty); }
            set { SetValue(LightImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LightImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LightImageProperty =
            DependencyProperty.Register("LightImage", typeof(string), typeof(TrayPopUp), new PropertyMetadata("Icons/ll tube unknown.PNG"));

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            var newEventArgs = new RoutedEventArgs(PopupRestoreEvent);
            RaiseEvent(newEventArgs);
            
        }

    }
}
