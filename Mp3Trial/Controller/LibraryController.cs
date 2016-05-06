using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Event;
using MusicPlayer.Data;
using MusicPlayer.Utility;
using System.Windows;

namespace MusicPlayer.Controller
{
    public static class LibraryController
    {
        #region Public Variables

        public static LibraryEvent LibraryEvent = new LibraryEvent();

        #endregion

        #region Private Variables

        private static MainWindow _Player;
        private static MusicPlayerEntities dbObj = new MusicPlayerEntities();    // for singleton

        #endregion

        #region Public Properties

        public static tblMedia SelectedMedia { get; set; }

        #endregion

        #region Public Methods

        public static void Initalize(MainWindow player)
        {
            _Player = player;
        }

        /// <summary>
        /// Displays the File Dialog box to load the media file and adds
        /// it to the database. Raises the Library modified event.
        /// </summary>
        /// 
        public static void AddMedia()
        {
            try
            {
                var objAdd = FileLoader.Load();

                if (objAdd == null)
                    throw new ArgumentNullException("File was not loaded.");

                AddMedia(objAdd);
            }
            catch (ArgumentNullException ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, Logger.LogType.MessageBox, Logger.LogLevel.Warning);
            }
            catch (Exception ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public static void AddMedia(tblMedia mediaFile)
        {
            AddMedia(new List<tblMedia>() { mediaFile });
        }

        public static void AddMedia(List<tblMedia> mediaFiles)
        {
            try
            {
                foreach (var file in mediaFiles)
                {
                    if(!dbObj.tblMedias.Any(x=>x.Location == file.Location)) // checking for duplicates
                        dbObj.tblMedias.Add(file);  //adding a song to the library
                }

                dbObj.SaveChanges();            // saving schanges in the db

                LibraryEvent.MediaModified();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public static bool CheckMedia(tblMedia obj)
        {
            if (dbObj.tblMedias.Any(x => x.Location == obj.Location))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Deletes the media object from the database and raises the 
        /// Library modified event.
        /// </summary>
        /// <param name="media"></param>
        public static void DeleteMedia(tblMedia media)
        {
            try
            {
                IEnumerable<tblPlaylistMapping> li = dbObj.tblPlaylistMappings.Where(x => x.MId == media.MId);
                dbObj.tblPlaylistMappings.RemoveRange(li);
                dbObj.tblMedias.Remove(media);
                dbObj.SaveChanges();

                LibraryEvent.MediaModified();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public static void DeleteMediaFrmPlaylist(tblMedia media, int pid)
        {
            try
            {
                tblPlaylistMapping obj = dbObj.tblPlaylistMappings.Where(x => x.MId == media.MId && x.PId == pid).First();
                dbObj.tblPlaylistMappings.Remove(obj);
                dbObj.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public static List<tblMedia> GetAllMedia()
        {
            return dbObj.tblMedias.Select(x => x).OrderBy(x => x.Title).ToList();
        }

        public static tblMedia GetMedia()
        {
            if (SelectedMedia == null)
                MessageBox.Show("Select a song!!!");
            //throw new ArgumentNullException("No media is selected in the library.");
            return SelectedMedia;
        }

        public static tblMedia GetMedia(int MID)
        {
            var media = (tblMedia)dbObj.tblMedias.Select(o => o.MId == MID);

            if (media == null)
                throw new ArgumentNullException(string.Format("No media found in the library with MID {0}.", MID));

            return media;
        }

        public static List<tblMedia> GetPlaylistMedia(int PID)
        {
            return (from p in dbObj.tblPlaylistMappings
                          join m in dbObj.tblMedias on p.MId equals m.MId
                           where p.PId == PID select m).OrderBy(x=>x.Title).ToList();
        }

        /// <summary>
        /// Adds a new playlist to the list
        /// </summary>
        public static int AddPlaylist(string pName)
        {
            try
            {
                if (!dbObj.tblPlaylists.Any(x => x.PlaylistName == pName))
                {
                    var Addobj = new tblPlaylist();
                    Addobj.PlaylistName = pName;
                    dbObj.tblPlaylists.Add(Addobj);
                    dbObj.SaveChanges();
                    LibraryEvent.PlaylistModified();
                    return Addobj.PId;
                }
                else
                {
                    MessageBox.Show("Enter a different Playlist Name!!!");
                    return -1;
                }
            }
            catch (ArgumentNullException ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, Logger.LogType.MessageBox, Logger.LogLevel.Warning);
                return -1;
            }
            catch (Exception ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return -1;
            }
        }

        public static void DeletePlaylist(tblPlaylist delObj)
        {
            try
            {
                IEnumerable<tblPlaylistMapping> lis = dbObj.tblPlaylistMappings.Where(x=>x.PId == delObj.PId);
                dbObj.tblPlaylistMappings.RemoveRange(lis);
                dbObj.tblPlaylists.Remove(delObj);
                dbObj.SaveChanges();
                LibraryEvent.PlaylistModified();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public static List<tblPlaylist> GetAllPlaylist()
        {
            return dbObj.tblPlaylists.Select(x => x).ToList();
        }

        public static void AddMediaToPlaylist(tblMedia mediaFile, int PID)
        {
            AddMediaToPlaylist(new List<tblMedia>() { mediaFile }, PID);
        }

        public static void AddMediaToPlaylist(List<tblMedia> mediaFiles, int PID)
        {
            try
            {
                var playlist = dbObj.tblPlaylists.First(o => o.PId == PID);

                foreach (var media in mediaFiles)
                {
                    if (CheckMedia(media))
                    {
                        var map = new tblPlaylistMapping();
                        map.MId = media.MId;
                        map.PId = playlist.PId;
                        map.tblMedia = media;
                        map.tblPlaylist = playlist;
                        dbObj.tblPlaylistMappings.Add(map);  //adding a song to the library
                    }
                    else
                    {
                        tblMedia obj = dbObj.tblMedias.Where(x => x.Location == media.Location).First();
                        var map = new tblPlaylistMapping();
                        map.MId = obj.MId;
                        map.PId = playlist.PId;
                        map.tblMedia = obj;
                        map.tblPlaylist = playlist;
                        dbObj.tblPlaylistMappings.Add(map);  //adding a song to the library
                    }
                }

                dbObj.SaveChanges();            // saving schanges in the db
                LibraryEvent.MediaModified();                
            }
            catch (Exception ex)
            {
                Logger.Write(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion

        #region Session

        public static bool getSessionVal(string colN)
        {
            try
            {
                tblHeaderSession ob = dbObj.tblHeaderSessions.Where(x => x.ColName == colN).FirstOrDefault();
                return ob.IsShown;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void setSessionVal(string colN, bool val)
        {
            tblHeaderSession ob = dbObj.tblHeaderSessions.Where(x => x.ColName == colN).FirstOrDefault();
            ob.IsShown = val;
            dbObj.SaveChanges();
        }

        #endregion

        #region Log

        public static void updateLog(tblMedia obj)
        {
            try
            {
                tblPlayLog newLog = new tblPlayLog();
                newLog.MId = obj.MId;
                newLog.TimeStamp = DateTime.Now;
                dbObj.tblPlayLogs.Add(newLog);
                dbObj.SaveChanges();
            }
            catch (Exception)
            {
            }
        }

        public static List<tblMedia> getRecentPlayedSongs()
        {
            try
            {
                List<tblPlayLog> lis = dbObj.tblPlayLogs.OrderByDescending(x => x.TimeStamp).ToList();
                var l1 = lis.Select(x => x.tblMedia).Distinct().Take(10).ToList();
                List<tblMedia> ret = new List<tblMedia>();
                foreach (tblMedia item in l1)
                {
                    tblMedia song = item;
                    ret.Add(song);
                }
                return ret;
            }
            catch (Exception)
            {
                return new List<tblMedia>();
            }
        }

        #endregion
    }
}
