using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using Microsoft.Speech.Synthesis;


namespace SkypeToSpeech
{

    static class Program
    {
        private static void ReadMessage(this HashSet<SkypeMessage> messages, SpeechSynthesizer synth)
        {
            string previousSender = string.Empty;
            foreach (var message in messages.Where(m => !m.IsRead).OrderBy(m => m.Timestamp))
            {
                if (previousSender != message.GetSender())
                {
                    synth.Speak(message.GetSender()+ " " + message.GetMessage());
                    Console.WriteLine(message.GetSender() + message.GetMessage());
                }
                else
                {
                    synth.Speak(message.GetMessage());
                    Console.WriteLine(message.GetMessage());
                }
                message.IsRead = true;
                previousSender = message.GetSender();
            }
        }
        static void Main(string[] args)
        {
            HashSet<SkypeMessage> messages = new HashSet<SkypeMessage>();
            SpeechSynthesizer synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();
            var temp = synth.GetInstalledVoices();
            synth.SelectVoice("Microsoft Server Speech Text to Speech Voice (ru-RU, Elena)");
            //synth.SelectVoice(synth.GetInstalledVoices().Where(v => v.VoiceInfo.Culture == System.Globalization.CultureInfo.GetCultureInfo("ru-RU") ).First().ToString());

            string dbPath = @"DataSource=C:\Users\Nixon\AppData\Roaming\Skype\ivan_gevchuk\main.db";
            var db = new SQLiteConnection(dbPath);
            SQLiteCommand cmd = new SQLiteCommand(
                @"select 
                  m.id, m.from_dispname, m.body_xml, datetime(m.timestamp, 'unixepoch') as messageDate
                  from messages m
                  where m.convo_id = 128300
                  and m.timestamp > strftime('%s', 'now', '-10000 second')", db);
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
                            var message = new SkypeMessage(id: reader.GetInt32(0),
                                                           from: reader.GetString(1),
                                                           message: reader.GetString(2),
                                                           timestamp: reader.GetDateTime(3));
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
