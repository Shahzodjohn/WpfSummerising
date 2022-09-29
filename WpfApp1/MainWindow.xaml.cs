using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cts;
        private List<Urls> Urls;
        public MainWindow()
        {
            InitializeComponent();
            cts = new CancellationTokenSource();
            Urls = new List<Urls>();
        }
            int index = 0;
        
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => startAction( sender,  e));
        }

        private void startAction(object sender, RoutedEventArgs e)
        {
            var ws = new webSite();
            foreach (var item in Urls)
            {
                if (cts.IsCancellationRequested)
                    break;
                
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(item.URL);

                ws = new webSite
                {
                    Name = item.Name,
                    Amount = "", //
                    Status = "Загрузка...",
                    Method = Method.Send
                };

  
                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    DataGridXAML.Items.Add(ws);
                }));

                   Thread.Sleep(500);
      
                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    DataGridXAML.Items.RemoveAt(index);
                    index++;
                }));

                var amount = doc.DocumentNode.Descendants("a").Count().ToString();
                ws = new webSite
                {
                    Name = item.Name,
                    Amount = amount,
                    Status = "Успешно!",
                    Method = Method.Update
                };
                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    DataGridXAML.Items.Add(ws);
                }));
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }


        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {}

        private void DataGridXAML_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {}

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "Textfiles|*.txt|All Files|*.*";
            fileDialog.DefaultExt = ".log";
            Nullable<bool> dialogOk = fileDialog.ShowDialog();
            using (var reader = new StreamReader(fileDialog.FileName))
            {
                string line; var Url = String.Empty; string Name;
                
                while ((line = reader.ReadLine()) != null)
                {
                    var data = line.Split("|").ToList();
                    Url = data.First();
                    Name = data.Last();
                    Urls.Add(new WpfApp1.Urls
                    {
                          URL = Url,
                          Name = Name
                    });
                }
            }
            if (dialogOk == true)
            {
                string sFileNames = "";
                foreach (var sFileName in fileDialog.FileNames)
                {
                    sFileNames += ";" + sFileName;
                }
                sFileNames = sFileNames.Substring(1);
                tbxFiles.Text = sFileNames;
            }
        }
    }
}
    