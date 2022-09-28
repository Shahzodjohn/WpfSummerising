using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BufferBlock<webSite> _subscribebufferBlock { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _subscribebufferBlock = new BufferBlock<webSite>();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => startAction( sender,  e));
            int index = 0;
            var trueSubscribe = new ActionBlock<webSite>(value =>
            {
                if(value.Method != Method.Send)
                {
                    this.Dispatcher.Invoke(new Action(delegate ()
                    {
                         DataGridXAML.Items.RemoveAt(index);
                         index++;
                         Thread.Sleep(2500);
                    }));
                }

                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    if (!isWorking)
                    {
                        value.Status = "Отменено";
                        value.Amount = "";
                    }
                     DataGridXAML.Items.Add(value);
                }));
            });
            
            TransformBlock<webSite, webSite> _transformblock = new TransformBlock<webSite, webSite>(data =>
            {   
                return data;
            });

            _transformblock.LinkTo(trueSubscribe);
            _subscribebufferBlock.LinkTo(_transformblock);
        }

        private bool isWorking = true;
        private void startAction(object sender, RoutedEventArgs e)
        {
            var lst = new List<Urls> { new Urls { URL = "https://ru.wikipedia.org/wiki/%D0%97%D0%B0%D0%B3%D0%BB%D0%B0%D0%B2%D0%BD%D0%B0%D1%8F_%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0",
               Amount = "",
               Name = "Wikipedia",
               Status = ""
            },
            new Urls { URL = "https://stackoverflow.com", Name = "Stackoverflow" } ,
            new Urls { URL = "https://www.microsoft.com/ru-ru/microsoft-teams/log-in", Name = "Microsoft Teams"} ,
            new Urls { URL = "https://market.yandex.ru", Name = "Yandex market"} ,
            new Urls { URL = "https://www.booking.com", Name = "Booking" } ,
            new Urls { URL = "https://www.aliexpress.com", Name = "AliExpress"} ,
            new Urls { URL = "https://www.cyberforum.ru", Name = "Cyberforum" },
            new Urls { URL = "https://travel.yandex.ru/avia/?utm_source=distribution&utm_medium=bookmark&utm_campaign=ru",  Name = "Travel Yandex" },
            new Urls { URL = "https://metanit.com/", Name = "Metanit" }};


            var ws = new webSite();
            foreach (var item in lst)
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(item.URL);

                ws = new webSite
                {
                    Name = item.Name,
                    Amount = "", //
                    Status = "Загрузка...",
                    Method = Method.Send
                };

                _subscribebufferBlock.Post(ws);

                var amount = doc.DocumentNode.Descendants("a").Count().ToString();
                ws = new webSite
                {
                    Name = item.Name,
                    Amount = amount,
                    Status = "Успешно!",
                    Method = Method.Update
                };
               
                _subscribebufferBlock.Post(ws);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            isWorking = false;
        }


        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {}

        private void DataGridXAML_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {}

        
    }
}
    