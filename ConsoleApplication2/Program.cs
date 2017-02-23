using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading;
using Microsoft.Speech.Synthesis;


namespace ConsoleApplication2
{

    static class Program
    {
        private class Message
        {
            public int id;
            public string from;
            public string message;
            public DateTime timestamp;
            public bool isRead;
            public override int GetHashCode()
            {
                return (id.ToString() + from.ToString() + message.ToString()).GetHashCode();
            }

            public override bool Equals(object obj)
            {
                Message M = obj as Message;
                return M.GetHashCode() == this.GetHashCode() ;
            }
        }


        private static void ReadMessage(this HashSet<Message> messages, SpeechSynthesizer synth)
        {
            foreach (var message in messages.Where(m => !m.isRead).OrderBy(m => m.timestamp))
            {
                synth.Speak(message.message);
                message.isRead = true;
            }
        }
        static void Main(string[] args)
        {
            HashSet<Message> messages = new HashSet<Message>();
            SpeechSynthesizer synth = new SpeechSynthesizer();
            var temp = synth.GetInstalledVoices();
            synth.SelectVoice("Microsoft Server Speech Text to Speech Voice (ru-RU, Elena)");
            synth.SelectVoice(synth.GetInstalledVoices().Where(v => v.VoiceInfo.Culture == System.Globalization.CultureInfo.GetCultureInfo("ru-RU") ).First().ToString());

            string dbPath = @"DataSource=C:\Users\Nixon\AppData\Roaming\Skype\ivan_gevchuk\main.db";
            var db = new SQLiteConnection(dbPath);
            SQLiteCommand cmd = new SQLiteCommand(
                @"select 
                  m.id, m.from_dispname, m.body_xml, datetime(m.timestamp, 'unixepoch')
                  from messages m
                  where m.convo_id = 128300
                  and m.timestamp > strftime('%s', 'now', '-6000 second')", db);
            SQLiteDataReader reader = null;
            try
            {
                db.Open();
                Console.WriteLine("Press ESC to stop");
                do
                {
                    while (!Console.KeyAvailable)
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            // get the results of each column
                            var message = new Message();
                            message.id = reader.GetInt32(0);
                            message.from = reader.GetString(1);
                            message.message = reader.GetString(2);
                            message.timestamp = reader.GetDateTime(3);
                            message.isRead = false;
                            messages.Add(message);
                        }
                        reader.Close();
                        messages.ReadMessage(synth);
                        Thread.Sleep(500);
                    }
                }
                while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }
            finally
            {
                // 3. close the reader
                if (reader != null)
                {
                    reader.Close();
                }

                // close the connection
                if (db != null)
                {
                    db.Close();
                }
            }
        }


    }
}
