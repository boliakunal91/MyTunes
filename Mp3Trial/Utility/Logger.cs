using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MusicPlayer.Utility
{
    public static class Logger
    {
        #region Public Variables

        public enum LogLevel
        {
            Error,
            Information,
            Warning
        }

        public enum LogType
        {
            EventLog,
            MessageBox,
            Both
        }

        #endregion

        #region Private Variables

        private const string MESSAGEBOX_TEXT = "Music Player Message";
        private const string EVENTLOG_SOURCE = "MediaPlayer";

        #endregion

        #region Public Methods

        public static void Write(Exception e, string method, LogType type = LogType.EventLog, LogLevel level = LogLevel.Error)
        {
            var message = string.Format("{0} caused in method {1}", e.Message, method);

            if (!(type == LogType.MessageBox))
            {

            }

        }

        #endregion

        #region Private Methods

        private static void WriteEventLog(string message, LogLevel level)
        {
            try
            {
                using (var el = new EventLog())
                {
                    el.Source = EVENTLOG_SOURCE;

                    switch (level)
                    {
                        case LogLevel.Information:
                            el.WriteEntry(message, EventLogEntryType.Information);
                            break;
                        case LogLevel.Error:
                            el.WriteEntry(message, EventLogEntryType.Error);
                            break;
                        case LogLevel.Warning:
                            el.WriteEntry(message, EventLogEntryType.Warning);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                var newmessage = string.Format("{0} encountered when trying to write error {1}", e.Message, message);
                WriteUI(newmessage, level);
            }
        }

        private static void WriteUI(string message, LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Information:
                    MessageBox.Show(MESSAGEBOX_TEXT, message, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case LogLevel.Error:
                    MessageBox.Show(MESSAGEBOX_TEXT, message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case LogLevel.Warning:
                    MessageBox.Show(MESSAGEBOX_TEXT, message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }   
        }

        #endregion
    }
}
