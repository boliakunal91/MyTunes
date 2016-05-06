using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Data;

namespace MusicPlayer.Event
{
    public class LibraryEvent
    {
        public LibraryEvent()
        {

        }

        public void PlaylistModified()
        {
            OnPlaylistModified(EventArgs.Empty);
        }

        public void MediaModified()
        {
            OnMediaModified(EventArgs.Empty);
        }

        public void Changed(tblMedia media)
        {
            var args = new MediaChangedEventArgs();
            args.Media = media;
            OnMediaChanged(args);
        }

        protected virtual void OnPlaylistModified(EventArgs e)
        {
            EventHandler handler = PlaylistHasBeenModified;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMediaModified(EventArgs e)
        {
            EventHandler handler = MediaHasBeenModified;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMediaChanged(MediaChangedEventArgs e)
        {
            EventHandler<MediaChangedEventArgs> handler = MediaChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler PlaylistHasBeenModified;
        public event EventHandler MediaHasBeenModified;
        public event EventHandler<MediaChangedEventArgs> MediaChanged;
    }

    public class MediaChangedEventArgs : EventArgs
    {
        public tblMedia Media { get; set; }
    }
}
