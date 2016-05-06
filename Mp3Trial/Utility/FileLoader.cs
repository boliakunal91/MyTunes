using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Data;
using System.Windows.Forms;
using MusicPlayer.Controller;

namespace MusicPlayer.Utility
{
    public static class FileLoader
    {
        public static tblMedia Load()
        {
            try
            {
                var openFD = new OpenFileDialog();
                openFD.AddExtension = true;
                openFD.DefaultExt = "*.*";
                openFD.Filter = "Media Files (*.*)|*.*";
                openFD.ShowDialog();

                var mediaList = Load(openFD.FileName);
                if(mediaList.Count > 0)
                    return mediaList[0];
            }
            catch (Exception ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }

            return null;
        }

        private static List<tblMedia> Load(string filename)
        {
            return Load(new string[] { filename });
        }

        public static List<tblMedia> Load(string[] filenames)
        {
            var mediaList = new List<tblMedia>();

            foreach (var filename in filenames)
            {
                try
                {
                    if (!String.IsNullOrEmpty(filename))
                    {
                        var media = new tblMedia();
                        var music = TagLib.File.Create(filename); // imp!
                        if (music.Tag.Title != " " || music.Tag.Title != null)
                        {
                            media.Title = music.Tag.Title;                            
                        }
                        else
                        {
                            media.Title = filename;
                        }
                        media.Album = music.Tag.Album;
                        media.FirstArtist = music.Tag.FirstAlbumArtist;
                        media.FirstComposer = music.Tag.FirstComposer;
                        media.FirstGenere = music.Tag.FirstGenre;
                        media.Comment = music.Tag.Comment;
                        media.TotalLenghtMins = (decimal)music.Properties.Duration.TotalMinutes;
                        media.Location = filename;
                        media.Year = music.Tag.Year.ToString();
                        media.TotalLenghtMins = (decimal) music.Properties.Duration.TotalSeconds;
                        //media.Picture = music.Tag.Pictures[0].Data.Data;                        
                        mediaList.Add(media);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }

            return mediaList;
        }
    }
}
