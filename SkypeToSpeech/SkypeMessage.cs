using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypeToSpeech
{
    public class SkypeMessage
    {
        private int id;
        private string from;
        private string messagebody;
        private DateTime timestamp;
        private bool isRead;

        private string From
        {
            get { return from; }
            set { from = value; }
        }
        public string Messagebody { get { return messagebody; } set { messagebody = value; } }
        public DateTime Timestamp { get { return timestamp; } set { timestamp = value; } }
        public bool IsRead { get { return isRead; } set { isRead = value; } }

        internal string GetSender()
        {
            var senderNameSeparate = from.Split('"');
            if (senderNameSeparate.Count() > 2)
                return from.Split('"')[1];
            else return From; 
        }


        internal string GetMessage()
        {
            return messagebody;
        }

        public SkypeMessage(int id, string from, string message, DateTime timestamp)
        {
            this.id = id;
            From = from;
            Messagebody = message;
            Timestamp = timestamp;
            IsRead = false;
        }

        public override int GetHashCode()
        {
            return (id.ToString() + From.ToString() + Messagebody.ToString()).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            SkypeMessage M = obj as SkypeMessage;
            return M.GetHashCode() == this.GetHashCode();
        }
    }
}
