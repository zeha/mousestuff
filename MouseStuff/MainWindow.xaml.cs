using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Win32;

namespace MouseStuff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GhostMouseScript gms;
        ObservableCollection<MouseEvent> mouseEventsObservable;
        MouseEventsTarget met;
        DateTime lastUiUpdateTime;

        public MainWindow()
        {
            InitializeComponent();
            met = new MouseEventsTarget();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
        //    OpenFile("C:\\Users\\ch\\Documents\\ghostmouses\\allesklicken-teil1.gms", false);
#endif
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (met != null) met.Close();
        }

        private void OpenFile(String filename, bool Append) {
            textFilename.Text = filename;
            if (gms == null)
                gms = new GhostMouseScript();
            if (!Append)
                gms = new GhostMouseScript();
            gms.appendFromFile(filename);
            labelEntryCount.Content = gms.MouseEvents.Count();

            mouseEventsObservable = new ObservableCollection<MouseEvent>(gms.MouseEvents);
            mouseEventsObservable.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(mouseEventsObservable_CollectionChanged);
            datagridMouseEvents.DataContext = null;
            datagridMouseEvents.DataContext = mouseEventsObservable;
            StartDrawMouseEventsPath();
        }

        void mouseEventsObservable_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            gms.MouseEvents = new List<MouseEvent>(mouseEventsObservable);
            StartDrawMouseEventsPath();
        }

        public bool myUpdateMouseEventsTargetStatus(GhostMouseScript gms, int currentId)
        {
            labelEntryCount.Content = String.Format("{0}/{1}", currentId, gms.MouseEvents.Count());
            TimeSpan ts = DateTime.Now.Subtract(lastUiUpdateTime);
            if (ts.TotalMilliseconds > 250)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                lastUiUpdateTime = DateTime.Now;
            }
            return false;
        }

        private void StartDrawMouseEventsPath()
        {
            lastUiUpdateTime = DateTime.Now;
            met.Show();
            met.DrawMouseEventsPath(this.gms, myUpdateMouseEventsTargetStatus);
        }

        private void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void buttonSimulate_Click(object sender, RoutedEventArgs e)
        {
            StartDrawMouseEventsPath();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = textFilename.Text;
            sfd.DefaultExt = ".gms";
            sfd.Filter = "GMS (*.gms)|*.gms";
            sfd.ShowDialog();
            if (sfd.FileName.Length == 0) return;
            gms.saveToFile(sfd.FileName);
            textFilename.Text = sfd.FileName;
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open script";
            ofd.CheckFileExists = true;
            ofd.Filter = "GMS (*.gms)|*.gms";
            ofd.ShowDialog();
            if (ofd.FileName.Length == 0) return;
            OpenFile(ofd.FileName, false);
        }

        private void buttonAppend_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select file to append to current script";
            ofd.CheckFileExists = true;
            ofd.Filter = "GMS (*.gms)|*.gms";
            ofd.ShowDialog();
            if (ofd.FileName.Length == 0) return;
            OpenFile(ofd.FileName, true);
        }

        private void buttonNew_Click(object sender, RoutedEventArgs e)
        {
            gms = new GhostMouseScript();
            labelEntryCount.Content = gms.MouseEvents.Count();
            datagridMouseEvents.DataContext = gms.MouseEvents;
        }

        private void ctxMenu_OffsetByTime_Click(object sender, RoutedEventArgs e)
        {
            if (datagridMouseEvents.SelectedItem == null) return;
            
        }
    }
}
