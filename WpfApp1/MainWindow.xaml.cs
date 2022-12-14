using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            process.Text = "Остановлено";
            process.FontSize = 17;
            process.Foreground = Brushes.DarkRed;
            process.FontWeight = FontWeights.Bold;

        }
            
        
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => startAction( sender,  e));
        }

        private void startAction(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource(); int index = 0;

            this.Dispatcher.Invoke(new Action(delegate ()
            {
                btnOpen.IsEnabled = false;
                DataGridXAML.Items.Clear();
                process.Text = "Запущено";
                process.FontSize = 17;
                process.Foreground = Brushes.DarkGreen;
                process.FontWeight = FontWeights.Bold;
            }));

            List<Urls> count = new List<Urls>();
            var ws = new webSite();
            List<Urls> allRoutes = new List<Urls>();
            allRoutes.AddRange(Urls);

            foreach (var item in allRoutes)
            {
                if (cts.IsCancellationRequested)
                {
                    Counter(count);
                    break;
                }

                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(item.URL);

                ws = new Urls
                {
                    Status = "Загрузка...",
                    Method = Method.Send,
                    URL = item.URL,
                };
                
                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    start.IsEnabled = false;
                    cancel.IsEnabled = true;
                    DataGridXAML.Items.Add(ws);

                }));

                Thread.Sleep(500);

                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    DataGridXAML.Items.RemoveAt(index);
                    index++;
                }));

                var amount = doc.DocumentNode.Descendants("a").Count();
                ws = new Urls
                {
                    Amount = amount,
                    Status = "Успешно!",
                    Method = Method.Update,
                    URL = item.URL
                };
                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    DataGridXAML.Items.Add(ws);
                    count.Add(new Urls
                    {
                        Amount = amount,
                        URL = item.URL
                    });
                }));
            }
            if (!cts.IsCancellationRequested)
                Counter(count);

            this.Dispatcher.Invoke(new Action(delegate ()
            {
                start.IsEnabled = true;
                cancel.IsEnabled = false;
                process.Text = "Остановлено";
                process.FontSize = 17;
                process.Foreground = Brushes.DarkRed;
                process.FontWeight = FontWeights.Bold;
                btnOpen.IsEnabled = true;
                cts.Cancel();
            }));
        }

        private void Counter(List<Urls> count)
        {
            var res = count.FirstOrDefault(s => s.Amount == count.Max(x => x.Amount));

            this.Dispatcher.Invoke(new Action(delegate ()
            {
                DataGridXAML.Items.Add(new Urls
                {
                    Amount = res.Amount,
                    Status = "MAXIMUM",
                    URL = res.URL
                });
            }));

        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                cancel.IsEnabled = false;
            }));
            cancel.IsEnabled = false;
        }
        

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {}

        private void DataGridXAML_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {}

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Urls.Clear();
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "Textfiles|*.txt|All Files|*.*";
            fileDialog.DefaultExt = ".log";

            Nullable<bool> dialogOk = fileDialog.ShowDialog();
            foreach (var fileName in fileDialog.FileNames)
            {
                using (var reader = new StreamReader(fileName))
                {
                    string line; var Url = String.Empty; string Name;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var data = line.Split("|").ToList();
                        Url = data.First();
                        //Name = data.Last();
        
                        var check = Urls.FirstOrDefault(x=>x.URL == Url);
                        if (check == null)
                        {
                            Urls.Add(new WpfApp1.Urls
                            {
                                URL = Url,
                                //Name = Name
                            });
                        }
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
                    this.Dispatcher.Invoke(new Action(delegate ()
                    {
                        start.IsEnabled = true;
                    }));
                }
            }
            
        }
    }
}
    