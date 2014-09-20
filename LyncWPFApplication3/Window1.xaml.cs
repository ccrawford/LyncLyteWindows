using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Lync.Model;
using System.Diagnostics;
using System.Windows.Threading;
using LyncLights;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;

namespace LyncWPFApplication3
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        LinkStatusVM vm;


        public Window1()
        {
            InitializeComponent();
            vm = (LinkStatusVM)base.DataContext;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm.CleanUp();
        }

    }

}
