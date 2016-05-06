using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
using TagLib.Id3v2;
using MusicPlayer.Data;
using MusicPlayer.Controller;
using MusicPlayer.Utility;
using System.Windows.Controls.Primitives;

namespace MusicPlayer
{
    public partial class MainWindow
    {
        #region Event Handler

        private void LibraryEvent_MediaAdded(object sender, EventArgs e)
        {
            RefreshMediaGrid();
        }

        /// <summary>
        /// Remove the media from the library if it is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteSong_Click(object sender, RoutedEventArgs e)
        {
            if (tblMediaDataGrid.SelectedItem != null)
            {
                tblMedia delObj = tblMediaDataGrid.SelectedItem as tblMedia;
                if (PlaylistShown == -1)
                {
                    LibraryController.DeleteMedia(delObj);
                    UpdateGrid(LibraryController.GetAllMedia());
                }
                else
                {
                    LibraryController.DeleteMediaFrmPlaylist(delObj, PlaylistShown);
                    UpdateGrid(LibraryController.GetPlaylistMedia(PlaylistShown));
                }
            }
        }

        private void AddMedia_Click(object sender, RoutedEventArgs e)
        {
            LibraryController.AddMedia();
        }

        private void DelMedia_Click(object sender, RoutedEventArgs e)
        {
            if (tblMediaDataGrid.SelectedItem != null)
            {
                tblMedia delObj = tblMediaDataGrid.SelectedItem as tblMedia;
                if (PlaylistShown == -1)
                {
                    LibraryController.DeleteMedia(delObj);
                    UpdateGrid(LibraryController.GetAllMedia());
                }
                else
                {
                    LibraryController.DeleteMediaFrmPlaylist(delObj, PlaylistShown);
                    UpdateGrid(LibraryController.GetPlaylistMedia(PlaylistShown));
                }
            }
        }

        //drag action
        private void tblMediaDataGrid_PreviewDragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true) == true) //Check if a File
                e.Effects = System.Windows.DragDropEffects.All;
            else
                e.Effects = System.Windows.DragDropEffects.None;

            e.Handled = true;
        }

        //drop action
        private void tblMediaDataGrid_PreviewDrop(object sender, System.Windows.DragEventArgs e)
        {
            var filesPaths = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);

            if (filesPaths != null)
            {
                //drag to a laylist or library
                var files = FileLoader.Load(filesPaths);
                if (PlaylistShown == -1)
                    LibraryController.AddMedia(files);
                else
                {
                    LibraryController.AddMediaToPlaylist(files, PlaylistShown);
                    UpdateGrid(LibraryController.GetPlaylistMedia(PlaylistShown));
                }

                if (Type == WindowType.Playlist)
                    LibraryController.AddMediaToPlaylist(files, PlaylistId);
            }


            if (Type == WindowType.Playlist)
            {
                var draggedMedia = (List<tblMedia>)e.Data.GetData("Songs", true);
                if(draggedMedia != null)
                    LibraryController.AddMediaToPlaylist(draggedMedia, PlaylistId);
            }

        }

        private void CM_AddMedia_Click(object sender, RoutedEventArgs e)
        {
            LibraryController.AddMedia();
        }

        private void CM_RemoveMedia_Click(object sender, RoutedEventArgs e)
        {
            if (tblMediaDataGrid.SelectedItem != null)
            {
                tblMedia delObj = tblMediaDataGrid.SelectedItem as tblMedia;
                if (PlaylistShown == -1)
                {
                    LibraryController.DeleteMedia(delObj);
                    UpdateGrid(LibraryController.GetAllMedia());
                }
                else
                {
                    LibraryController.DeleteMediaFrmPlaylist(delObj, PlaylistShown);
                    UpdateGrid(LibraryController.GetPlaylistMedia(PlaylistShown));
                }
            }
        }

        private void tblMediaDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (tblMediaDataGrid.SelectedItem != null)
            {
                var data = new DataObject();
                //tblMediaDataGrid.SelectedItems.Cast<tblMedia>().ToList();
                data.SetData("Songs", (tblMediaDataGrid.SelectedItems.Cast<tblMedia>().ToList()));
                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            }
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

        private void tblMediaDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            //Hide the context menu - sridhar to do
            if (Type == WindowType.Playlist)
                e.Handled = true;
            else
            {
                if (PlaylistShown > -1)
                {
                    ContextMenuAdd.Visibility = Visibility.Collapsed;
                    PlaylistMenu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ContextMenuAdd.Visibility = Visibility.Visible;
                    PlaylistMenu.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

    }
}
