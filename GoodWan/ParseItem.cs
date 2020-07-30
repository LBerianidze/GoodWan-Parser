using System.Collections.Generic;

namespace GoodWan
{
    public class ParseItem
    {
        public int Event_ID { get; }
        public int Device_ID { get; }
        public string TimeStamp { get;}
        public Dictionary<int, string> Components { get; }
        public Dictionary<string, string> Data { get; }

        public ParseItem(int event_id, int device_id, string timestamp_utc, Dictionary<int, string> components, Dictionary<string, string> data)
        {
            this.Event_ID = event_id;
            this.Device_ID = device_id;
            this.TimeStamp = timestamp_utc;
            this.Components = components;
            this.Data = data;
        }
    }
}