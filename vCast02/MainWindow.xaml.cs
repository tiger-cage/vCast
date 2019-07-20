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
using MessageBox = System.Windows.Forms.MessageBox;
using StarDust.CasparCG.net.AmcpProtocol;
using StarDust.CasparCG.net.Connection;
using StarDust.CasparCG.net.Device;
using StarDust.CasparCG.net.Models;
using StarDust.CasparCG.net.Models.Media;
using StarDust.CasparCG.net.OSC;
using Unity;
using Unity.Lifetime;

namespace vCast02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static IUnityContainer _container;

        private static readonly bool quit;

        ICasparDevice casparCGServer;

        public MainWindow()
        {
            InitializeComponent();
            ConfigureIOC(inputCasServer.Text, Int32.Parse(inputCasPort.Text));

            //Get casparCG device instance
            casparCGServer = _container.Resolve<ICasparDevice>();

        }

        static void ConfigureIOC(String server, int port = 5250)
        {
            _container = new UnityContainer();

            _container.RegisterInstance<IServerConnection>(new ServerConnection(new CasparCGConnectionSettings(server, port)));
            _container.RegisterType(typeof(IAMCPTcpParser), typeof(AmcpTCPParser));
            _container.RegisterSingleton<IDataParser, CasparCGDatasParser>();
            _container.RegisterType(typeof(IAMCPProtocolParser), typeof(AMCPProtocolParser));
            _container.RegisterType<ICasparDevice, CasparDevice>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IOscListener, OscListener>(); //OSC
        }

        private void OscStop()
        {
            var oscListener = _container.Resolve<OscListener>();
            oscListener.StopListening();


        }

        private void OscStart()
        {
            var oscListener = _container.Resolve<IOscListener>();
            //oscListener.RegisterMethod("/channel/1/stage/layer/1/file/time");
            oscListener.AddToAddressBlackList("/channel/[0-9]/output/consume_time");
            oscListener.AddToAddressBlackList("/channel/1/stage/layer/1/profiler/time");

            oscListener.OscMessageReceived += OscListener_OscMessageReceived;
            oscListener.StartListening("127.0.0.1", 6250);
            //Console.WriteLine("Osc listener strarted");
        }

        public void OscListener_OscMessageReceived(object sender, OscMessageEventArgs e)
        {
            lstbxOSC.Items.Add(e.OscPacket.ToString());
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {

            //Handler to be notify if the Server is connected or disconnected
            //casparCGServer.ConnectionStatusChanged += CasparDevice_ConnectionStatusChanged;

            //MessageBox.Show(casparCGServer.ConnectionStatusChanged);

           
            //if (casparCGServer.IsConnected)
            //{
            //    //Disconnect the connection
            //    casparCGServer.Disconnect();
            //    //lblConnectStatus.Foreground = (Brush) "Red";
            //    lblConnectStatus.Content = "Not Connected.";
            //    btnConnect.Content = "Connect";
            //} else
            //{
                //Initialize the connection
                casparCGServer.Connect();
                //lblConnectStatus.Foreground = "Green";
                lblConnectStatus.Content = "Connected.";
                //btnConnect.Content = "Disconnect";
            //}
        }

        private void BtnSendCommand_Click(object sender, RoutedEventArgs e)
        {
            var input = inptCmd.Text;
            if (input.Equals("play", StringComparison.InvariantCultureIgnoreCase))
                Play();
            if (input.Equals("stop", StringComparison.InvariantCultureIgnoreCase))
                Stop();
            if (input.Equals("load", StringComparison.InvariantCultureIgnoreCase))
                Load();
            if (input.Equals("loadbg", StringComparison.InvariantCultureIgnoreCase))
                LoadBg();
            if (input.Equals("cls", StringComparison.InvariantCultureIgnoreCase))
                Cls();
            //if (input.Equals("channelgrid", StringComparison.InvariantCultureIgnoreCase))
            //    ChannelGrid();
            //if (input.Equals("template", StringComparison.InvariantCultureIgnoreCase))
            //    PlayTemplate();
            //if (input.Equals("mixer", StringComparison.InvariantCultureIgnoreCase))
            //    PlayMixer();
            if (input.Equals("channelinfo", StringComparison.InvariantCultureIgnoreCase))
                ChannelInfo();
            //if (input.Equals("templateinfo", StringComparison.InvariantCultureIgnoreCase))
            //    TemplateInfo();
            //if (input.Equals("systeminfo", StringComparison.InvariantCultureIgnoreCase))
            //    SystemInfo();
            if (input.Equals("pathsinfo", StringComparison.InvariantCultureIgnoreCase))
                PathsInfo();
            //if (input.Equals("threadsinfo", StringComparison.InvariantCultureIgnoreCase))
            //    ThreadsInfo();
            //if (input.Equals("glinfo", StringComparison.InvariantCultureIgnoreCase))
            //    GlInfo();
            if (input.Equals("call", StringComparison.InvariantCultureIgnoreCase))
                Call();
            if (input.Equals("add", StringComparison.InvariantCultureIgnoreCase))
                Add();
            if (input.Equals("remove", StringComparison.InvariantCultureIgnoreCase))
                Remove();
        }

        private void Cls()
        {
            var clips = casparCGServer.GetMediafiles();
            foreach(var clip in clips)
            {
                lstBoxFiles.Items.Add(clip.FullName);
            }
        }

        private void Play(String clip = "AMB")
        {
            var channel = casparCGServer.Channels.First(x => x.ID == 1);
            channel.LoadBG(new CasparPlayingInfoItem { VideoLayer = 1, Clipname = clip });
            channel.Play(1);
        }

        private void Stop()
        {
            var channel = casparCGServer.Channels.First(x => x.ID == 1);
            channel.Stop();
            channel.Clear();
        }

        private void Load(String clip = "AMB")
        {
            casparCGServer.Channels.First()?.LoadBG(new CasparPlayingInfoItem(clip), false);
            casparCGServer.Channels.First()?.Play();
        }

        private void LoadBg(String clip = "AMB")
        {
            casparCGServer.Channels.First()?.LoadBG(new CasparPlayingInfoItem(clip, new Transition(TransitionType.SLIDE, 5000)));
            casparCGServer.Channels.First()?.Play();
        }
        private void ChannelInfo()
        {
            Console.WriteLine(casparCGServer.Channels.FirstOrDefault()?.GetInfo());
        }

        private void PathsInfo()
        {
            var casparCGServer = _container.Resolve<ICasparDevice>();
            var info = casparCGServer.GetInfoPaths();
            Console.WriteLine($"Media Path: {info?.Mediapath}");
        }

        private void Call()
        {
            Play();
            casparCGServer.Channels.FirstOrDefault()?.Call(1, false, 50);
        }

        private void Add()
        {
            casparCGServer.Channels.FirstOrDefault()?.Add(ConsumerType.File, 700, "\"test.mp4\" -vcodec libx264 -acodec acc");
        }

        private void Remove()
        {
            casparCGServer.Channels.FirstOrDefault()?.Remove(700);
        }

        private void BtnOSC_Click(object sender, RoutedEventArgs e)
        {
            OscStart();

            //if (btnOSC.Content.ToString() == "Stop OSC")
            //{
            //    OscStart();
            //    btnOSC.Content = "Stop OSC";
            //}
            //else
            //{
            //    OscStop();
            //    btnOSC.Content = "OSC Start";
            //}
        }
    }
}
