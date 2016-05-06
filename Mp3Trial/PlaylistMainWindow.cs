using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
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

namespace MusicPlayer
{
    public partial class MainWindow
    {
        #region Event Handler

        void LibraryEvent_PlaylistHasBeenModified(object sender, EventArgs e)
        {
            UpdateTreeNMenu();            
            //RefreshPlaylistMenu();
        }

        private void cmAddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            AddNewPlaylist();            
        }

        private void cmDeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (MainTree.SelectedItem != null)
            {
                DeletePlaylist((tblPlaylist)(MainTree.SelectedItem));
                UpdateTreeNMenu();
                //RefreshPlaylistMenu();
            }
            else
                System.Windows.MessageBox.Show("Select a Playlist first!!!");

        }

        private void cmNewWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MainTree.SelectedItem != null)
                {
                    tblPlaylist win = (tblPlaylist)(MainTree.SelectedItem);
                    MainWindow ob = new MainWindow(WindowType.Playlist);
                    ob.Title = string.Format("Music Player Playlist - {0}", win.PlaylistName);
                    ob.PlaylistId = win.PId;
                    //remove components                  
                    ob.Activate();
                    ob.RefreshMediaGrid();
                    ob.Show();
                    UpdateGrid(LibraryController.GetAllMedia());
                    TreeViewLib.Focus();
                    PlaylistShown = -1;
                    ob.Focus();
                }
            }
            catch (InvalidCastException)
            {
                // do nothing
            }
        }

        private void PlaylistItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (MainTree.SelectedItem != null)
                {
                    tblPlaylist win = (tblPlaylist)(MainTree.SelectedItem);
                    if (win != null)
                    {
                        UpdateGrid(LibraryController.GetPlaylistMedia(win.PId));
                        PlaylistShown = win.PId;
                    }
                }
            }
            catch(InvalidCastException)
            {
                // do nothing
            }
        }

        private void MenuCreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            AddNewPlaylist();            
        }

        #endregion

        #region Private Methods

        private void AddNewPlaylist()
        {
            var dialog = new AddPlaylistDialog();
            dialog.Owner = this;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();

            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value && !String.IsNullOrWhiteSpace(dialog.PlaylistName))
            {
                int pi = LibraryController.AddPlaylist(dialog.PlaylistName);
                if (pi > 0)
                {
                    UpdateGrid(LibraryController.GetPlaylistMedia(pi));
                    PlaylistShown = pi;
                }
            }
        }

        private void DeletePlaylist(tblPlaylist obj)
        {
            var delDialog = new ConfirmDeletionDialog();
            delDialog.Owner = this;
            delDialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            delDialog.ShowDialog();
            if (delDialog.DialogResult.HasValue && delDialog.DialogResult.Value)
            {
                tblPlaylist del = obj;
                LibraryController.DeletePlaylist(del);
            }
            UpdateGrid(LibraryController.GetAllMedia());// load library
            PlaylistShown = -1;
            TreeViewLib.Focus();
        }

        private void UpdateTreeNMenu()
        {
            PlaylistItem.ItemsSource = null;
            PlaylistItem.ItemsSource = LibraryController.GetAllPlaylist();            
            PlaylistItem.IsExpanded = true;

            //Load Menu items
            PlaylistMenu.Items.Clear();
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

        #endregion
    }
}
