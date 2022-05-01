using System;

namespace PaylineDirectScale.Payline.Model
{
    public class PaylineLogMessage
    {
        public PaylineLogMessage(DateTime timeStamp, string msg, string level, string invoked)
        {
            DateTimeStamp = timeStamp;
            Message = msg;
            Level = level;
            Invoked = invoked;
        }

        public DateTime DateTimeStamp { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
        public string LevelString { get { return Level.ToString(); } }
        public string Invoked { get; set; }
    }
}
