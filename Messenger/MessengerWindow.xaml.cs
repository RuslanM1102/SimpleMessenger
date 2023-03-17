using System.IO;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Text;

namespace Messenger
{
    /// http://127.0.0.2:8889/connection/
    /// http://127.0.0.1:8888/connection/?qweBFYUWEFGUYSDADSDFW
    public partial class MessengerWindow : Window
    {
        private const string protocol = @"http://";
        private const string connection = @"/connection/";
        private const string magicThing = @"?qweBFYUWEFGUYSDADSDFW";
        private HttpListener _httpListener;
        private string _myIP, _otherIP;


        
        public MessengerWindow(string myIP, string otherIP)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(protocol + myIP + connection);
            _myIP = myIP;
            _otherIP = otherIP;

            InitializeComponent();
            this.Title = myIP;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => Listen());
        }

        private void Listen()
        {
            _httpListener.Start();

            while (true)
            {
                HttpListenerContext context = _httpListener.GetContextAsync().GetAwaiter().GetResult();
                using (var streamReader = new StreamReader(context.Request.InputStream, System.Text.Encoding.UTF8))
                {
                    string message = streamReader.ReadToEnd();
                    Dispatcher.Invoke(() => {
                        Chat.Items.Add($"Собеседник: {message}");
                    });
                }
                context.Response.StatusCode = 200;
                context.Response.Close();
            }
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string url = protocol + _otherIP + connection + magicThing;
            await Dispatcher.Invoke(async () =>
            {
                using (HttpClient client = new HttpClient())
                {
                    StringContent content = new StringContent(MessageInput.Text, Encoding.UTF8, "text/plain");
                    await client.PostAsync(url, content);
                }
                Chat.Items.Add($"Вы: {MessageInput.Text}");
                MessageInput.Text = string.Empty;
            });
        }
    }
}
