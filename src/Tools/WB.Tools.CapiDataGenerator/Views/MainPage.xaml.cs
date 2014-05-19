using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using Cirrious.MvvmCross.Wpf.Views;

namespace WB.Tools.CapiDataGenerator.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : MvxWpfView
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public void Connect(int connectionId, object target)
        {
            throw new NotImplementedException();
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)sender;
            string arg = string.Format(@"/select,""{0}""", link.NavigateUri.AbsolutePath.Replace('/','\\'));

        

            Process.Start("explorer", arg);
        }
    }
}
