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
        public static Random rand = new Random();

        #region Event Handler

        private void MediaEvent_MediaPositionChanged(object sender, EventArgs e)
        {
            if (!this.IsActive)
                return;

            UpdateSeekBarValue();
        }

        private void MediaEvent_MediaPause(object sender, EventArgs e)
        {
            if (!this.IsActive)
                return;

            var img = new System.Windows.Controls.Image();
            img.Source = (System.Windows.Media.ImageSource)FindResource("PlayImg");
            btnPlayPause.Content = img;

            MusicElement.Pause();
        }

        private void MediaEvent_MediaStop(object sender, EventArgs e)
        {
            if (!this.IsActive)
                return;

            var img = new System.Windows.Controls.Image();
            img.Source = (System.Windows.Media.ImageSource)FindResource("PlayImg");
            btnPlayPause.Content = img;

            MusicElement.Stop();
            UpdateSeekBarValue();
        }

        private void MediaEvent_MediaPlay(object sender, EventArgs e)
        {
            if (!this.IsActive)
                return;

            var img = new System.Windows.Controls.Image();
            img.Source = (System.Windows.Media.ImageSource)FindResource("PauseImg");
            btnPlayPause.Content = img;
            MusicElement.Play();            
        }

        private void MediaEvent_MediaEnded(object sender, EventArgs e)
        {
            if (!this.IsActive)
                return;

            Next();
        }

        /// <summary>
        /// Updates the position of the Media file is someone drags the slider position manually
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slideSeek_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaController.Position = new TimeSpan(0, 0, 0, 0, (int)slideSeek.Value);
        }

        private void slideVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MusicElement.Volume = (double)slideVolume.Value;
        }

        private void btnAddSong_Click(object sender, RoutedEventArgs e)
        {
            LibraryController.AddMedia();
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            PlaylistPlayed = PlaylistShown;
            selectedR = tblMediaDataGrid.SelectedItem as tblMedia;
            MediaController.PlayPause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            MediaController.Stop();
        }

        private void btnNxt_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }
        
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            Prev();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            MediaController.Play();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            MediaController.Stop();
        }

        private void ShuffleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MediaController.IsShuffle = true;
        }

        private void ShuffleCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            MediaController.IsShuffle = false;
        }

        private void RepeatCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MediaController.IsRepeat = true;
        }

        private void RepeatCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            MediaController.IsRepeat = false;
        }

        #endregion

        


        #region Private Methods

        /// <summary>
        /// If you've reached the end of the library, wrap around to the start of the library.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Next()
        {
            try
            {
                if (MediaController.IsRepeat)
                {
                    var current = tblMediaDataGrid.SelectedIndex;
                    MediaController.Next(tblMediaDataGrid.Items[current] as tblMedia);
                    UpdateGridSelection(current);
                }
                else if (MediaController.IsShuffle)
                {
                    var next = -1;
                    do
                    {
                        next = rand.Next(1, tblMediaDataGrid.Items.Count) - 1;
                    } while (next == tblMediaDataGrid.SelectedIndex);

                    MediaController.Next(tblMediaDataGrid.Items[next] as tblMedia);
                    UpdateGridSelection(next);
                }
                else if (tblMediaDataGrid.SelectedIndex + 1 >= tblMediaDataGrid.Items.Count)
                {
                    var media = tblMediaDataGrid.Items[0] as tblMedia;
                    MediaController.Next(media);
                    UpdateGridSelection(0);
                }
                else
                {
                    var media = tblMediaDataGrid.Items[tblMediaDataGrid.SelectedIndex + 1] as tblMedia;
                    MediaController.Next(media);

                    UpdateGridSelection(tblMediaDataGrid.SelectedIndex + 1);
                }
            }
            catch (Exception)
            {
                MediaController.Stop();
            }
        }

        /// <summary>
        /// If you've reach the first media file in the library, wrap around to the last media file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Prev()
        {
            try
            {
                if (tblMediaDataGrid.SelectedIndex == 0)
                {
                    var media = tblMediaDataGrid.Items[tblMediaDataGrid.Items.Count - 1] as tblMedia;
                    MediaController.Prev(media);
                    UpdateGridSelection(tblMediaDataGrid.Items.Count - 1);
                }
                else
                {
                    var media = tblMediaDataGrid.Items[tblMediaDataGrid.SelectedIndex - 1] as tblMedia;
                    MediaController.Prev(media);
                    UpdateGridSelection(tblMediaDataGrid.SelectedIndex - 1);
                }
            }
            catch (Exception)
            {
                MediaController.Stop();
            }
        }

        #endregion

    }
}
