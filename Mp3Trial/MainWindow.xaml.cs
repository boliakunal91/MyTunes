using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TagLib.Id3v2;
using MusicPlayer.Data;
using MusicPlayer.Controller;
using MusicPlayer.Utility;
using MusicPlayer;


namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Variables

        private const string PLAYPAUSE_LABEL_PLAY = "Play";
        private const string PLAYPAUSE_LABEL_PAUSE = "Pause";

        #endregion

        #region Public Properties
        
        public double DurationInMilliseconds
        {
            get
            {
                if (this.MusicElement.NaturalDuration.HasTimeSpan)
                    return this.MusicElement.NaturalDuration.TimeSpan.TotalMilliseconds;
                return 0;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                if (this.MusicElement.NaturalDuration.HasTimeSpan)
                    return this.MusicElement.NaturalDuration.TimeSpan;
                return new TimeSpan(0);
            }
        }

        public TimeSpan Position
        {
            get
            {
                return this.MusicElement.Position;
            }
            set
            {
                this.MusicElement.Position = value;
            }
        }

        public WindowType Type { get; set; }

        public int PlaylistId { get; set; }

        public int PlaylistShown = -1;
        public int PlaylistPlayed = -1;

        public MenuItem HeaderMenu;
        public ContextMenu HeaderContextMenu;

        public tblMedia selectedR { get; set; }

        #endregion

        #region Public Enum

        public enum WindowType { Library, Playlist };

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            Setup(WindowType.Library);
            RefreshRecentList();
        }

        
        public MainWindow(WindowType type)
        {
            InitializeComponent();
            Setup(type);
        }

        #endregion

        #region Public Method

        public void UpdateMusicSource(tblMedia media)
        {
            //for log
            LibraryController.updateLog(media);
            MusicElement.Source = new Uri(media.Location);
            TimeSpan sec = TimeSpan.FromMinutes(double.Parse(media.TotalLenghtMins.ToString()));
            BackwardTimer.Text = sec.ToString("c");
            RefreshRecentList();
        }

        public void RefreshMediaGrid()
        {
            switch (Type)
            {
                case WindowType.Library:
                    UpdateGrid(LibraryController.GetAllMedia());

                    break;
                case WindowType.Playlist:
                    UpdateGrid(LibraryController.GetPlaylistMedia(PlaylistId));
                    break;
            }
        }

        public void RefreshRecentList()
        {
            List<tblMedia> recent = LibraryController.getRecentPlayedSongs();
            MnuPlayRecent.Items.Clear();            
            foreach (var item in recent)
            {
                MenuItem ob = new MenuItem();
                ob.Header = item.Title;
                ob.Tag = item;
                MnuPlayRecent.Items.Add(ob);
                ob.Click += ob_Click;
            }
            //MnuPlayRecent
        }

        void ob_Click(object sender, RoutedEventArgs e)
        {
            MusicElement.Stop();            
            MenuItem item = sender as MenuItem;
            LibraryController.SelectedMedia = item.Tag as tblMedia;
            LibraryController.LibraryEvent.Changed(item.Tag as tblMedia);
            MediaController.Play();
        }

        #endregion

        #region Event Handler

        /// <summary>
        /// Play a media file without adding it to the library
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenMedia_Click(object sender, RoutedEventArgs e)
        {
            MediaController.PlayDontSave();
        }

        /// <summary>
        /// A different position on the grid is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tblMediaDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {                
                tblMedia obj = tblMediaDataGrid.SelectedItem as tblMedia;
                LibraryController.LibraryEvent.Changed(obj);
            }
            catch(Exception ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }   

        /// <summary>
        /// A row in the grid was double clicked which should start playing that song.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tblMediaDataGrid_rowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                PlaylistPlayed = PlaylistShown;
                selectedR = tblMediaDataGrid.SelectedItem as tblMedia;
                DataGridRow dgr = sender as DataGridRow;
                tblMedia clickObj = dgr.Item as tblMedia;
                LibraryController.LibraryEvent.Changed(clickObj);
                MediaController.Play();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

        }   

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Updates the MediaController with a reference to the latest window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Activated_1(object sender, EventArgs e)
        {
            MediaController.SetFocusedWindow(this);
        }

        #endregion

        #region Private Methods

        private void Setup(WindowType type)
        {
            Type = type;

            //Hide things that shouldn't be shown on Playlist windows.
            if (Type == WindowType.Playlist)
            {
                this.MainTree.Visibility = Visibility.Collapsed;
                this.Menu.Visibility = System.Windows.Visibility.Collapsed;
                //this.PlaylistMnu.Visibility = System.Windows.Visibility.Collapsed;

                tblMediaDataGrid.SetValue(Grid.ColumnProperty, 0);
                tblMediaDataGrid.SetValue(Grid.ColumnSpanProperty, 3);
                GridSplitter_1.Visibility = System.Windows.Visibility.Collapsed;
                PlaybackControls.SetValue(Grid.ColumnProperty, 0);
                PlaybackControls.SetValue(Grid.ColumnSpanProperty, 7);

                btnAddSong.Visibility = System.Windows.Visibility.Collapsed;
                btnDeleteSong.Visibility = System.Windows.Visibility.Collapsed;                
            }

            //Load Library
            LibraryController.Initalize(this);
            RefreshMediaGrid();
            LibraryController.LibraryEvent.MediaHasBeenModified += LibraryEvent_MediaAdded;
            LibraryController.LibraryEvent.PlaylistHasBeenModified += LibraryEvent_PlaylistHasBeenModified;

            if (Type == WindowType.Library)
                UpdateTreeNMenu();

            //RefreshPlaylistMenu();

            //Load MediaController
            MediaController.Initalize(this);
            MediaController.MediaEvent.MediaPlay += MediaEvent_MediaPlay;
            MediaController.MediaEvent.MediaPause += MediaEvent_MediaPause;
            MediaController.MediaEvent.MediaStop += MediaEvent_MediaStop;
            MediaController.MediaEvent.MediaEnded += MediaEvent_MediaEnded;
            MediaController.MediaEvent.MediaPositionChanged += MediaEvent_MediaPositionChanged;
            this.Closed += MainWindow_Closed;

        }

        
        void MainWindow_Closed(object sender, EventArgs e)
        {
            if (Type == WindowType.Library)
            {
                App.Current.Shutdown();
            }
            //throw new NotImplementedException();
        }

        private void RefreshPlaylistMenu()
        {
            //Load Menu items
            List<tblPlaylist> playlistAll = LibraryController.GetAllPlaylist();
            foreach (tblPlaylist item in playlistAll)
            {
                System.Windows.Controls.MenuItem p = new System.Windows.Controls.MenuItem();
                p.Tag = item;
                p.Header = item.PlaylistName;
                PlaylistMenu.Items.Add(p);
                p.Click += p_Click;
            }
        }

        void p_Click(object sender, RoutedEventArgs e)
        {
            tblMedia selObj = tblMediaDataGrid.SelectedItem as tblMedia;
            System.Windows.Controls.MenuItem p = sender as System.Windows.Controls.MenuItem;
            tblPlaylist ob1 = p.Tag as tblPlaylist;
            LibraryController.AddMediaToPlaylist(selObj, ob1.PId);
        }

        /// <summary>
        /// Updates the seek bar's size and slider position
        /// </summary>
        private void UpdateSeekBarValue()
        {
            slideSeek.Maximum = MediaController.DurationInMilliseconds;
            slideSeek.ValueChanged -= slideSeek_ValueChanged;
            slideSeek.Value = MediaController.Position.TotalMilliseconds;
            slideSeek.ValueChanged += slideSeek_ValueChanged;
        }

        /// <summary>
        /// Refreshes the library list
        /// </summary>
        /// <param name="medias"></param>
        private void UpdateGrid(List<tblMedia> medias)
        {
            tblMediaDataGrid.ItemsSource = null;
            tblMediaDataGrid.ItemsSource = medias;
            BuildHeaderContextMenu();
        }

        private void BuildHeaderContextMenu()
        {
            HeaderContextMenu = new System.Windows.Controls.ContextMenu();
            foreach (DataGridColumn item in tblMediaDataGrid.Columns)
            {
                if (item.Header.ToString()!="Title")
                {
                    HeaderMenu = new MenuItem();
                    HeaderMenu.Header = item.Header;
                    HeaderMenu.IsChecked = LibraryController.getSessionVal(item.Header.ToString());
                    if (!HeaderMenu.IsChecked)
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                    HeaderContextMenu.Items.Add(HeaderMenu);
                    HeaderMenu.Click += new RoutedEventHandler(HeaderMenu_Click);
                    HeaderMenu.Checked += new RoutedEventHandler(HeaderMenu_Checked);
                    HeaderMenu.Unchecked += new RoutedEventHandler(HeaderMenu_Unchecked);
                }
            }
        }

        void HeaderMenu_Unchecked(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            foreach (DataGridColumn col in tblMediaDataGrid.Columns)
            {
                if (col.Header.ToString().Contains(item.Header.ToString()))
                {
                    col.Visibility = Visibility.Collapsed;
                    LibraryController.setSessionVal(item.Header.ToString(),false);
                    break;
                }
            }
        }

        void HeaderMenu_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            foreach (DataGridColumn col in tblMediaDataGrid.Columns)
            {
                if (col.Header.ToString().Contains(item.Header.ToString()))
                {
                    col.Visibility = Visibility.Visible;
                    LibraryController.setSessionVal(item.Header.ToString(), true);
                    break;
                }
            }
        }

        void HeaderMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item.IsChecked)
            {
                item.IsChecked = false;
            }
            else
            {
                item.IsChecked = true;
            }            
        }

        /// <summary>
        /// Updates the selected media in the library grid.
        /// </summary>
        /// <param name="index"></param>
        private void UpdateGridSelection(int index)
        {
            tblMediaDataGrid.SelectionChanged -= tblMediaDataGrid_SelectionChanged;
            tblMediaDataGrid.SelectedIndex = index;
            tblMediaDataGrid.SelectionChanged += tblMediaDataGrid_SelectionChanged;
        }

        #endregion

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UpdateGrid(LibraryController.GetAllMedia());// load library
            PlaylistShown = -1;
        }

        private void tblMediaDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject depObj = (DependencyObject)e.OriginalSource;
            while ((depObj!=null) && !(depObj is DataGridColumnHeader))
            {
                depObj = VisualTreeHelper.GetParent(depObj);
            }
            if (depObj == null)
            {
                return;
            }
            if (depObj is DataGridColumnHeader)
            {
                DataGridColumnHeader dgcolHeader = depObj as DataGridColumnHeader;
                dgcolHeader.ContextMenu = HeaderContextMenu;
            }
        }

        private void tblMediaDataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject depObj = (DependencyObject)e.OriginalSource;
            while ((depObj != null) && !(depObj is DataGridColumnHeader))
            {
                depObj = VisualTreeHelper.GetParent(depObj);
            }
            if (depObj == null)
            {
                return;
            }
            if (depObj is DataGridColumnHeader)
            {
                tblMediaDataGrid.SelectedItem = null;                
            }
        }

        private void mnuShowCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (Type == WindowType.Library )
            {
                if (PlaylistPlayed == -1)
                {
                    UpdateGrid(LibraryController.GetAllMedia());
                    TreeViewLib.Focus();
                }
                else
                {
                    UpdateGrid(LibraryController.GetPlaylistMedia(PlaylistPlayed));
                    PlaylistItem.Focus();
                }
            }           
            tblMediaDataGrid.SelectedItem = selectedR;
            tblMediaDataGrid.Focus();
        }

        private void mnuIncreaseVol_Click(object sender, RoutedEventArgs e)
        {         
            slideVolume.Value += 0.05;            
        }

        private void mnuDecreaseVol_Click(object sender, RoutedEventArgs e)
        {
            slideVolume.Value -= 0.05;
        }

        #region Commands
        
        private void PlayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MediaController.PlayPause();
        }

        private void PrevCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Prev();
        }

        private void NextCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Next();
        }

        #endregion
    }

}
