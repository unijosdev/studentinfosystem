using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.Events
{
    public class Event
    {
        public int EventId { get; set; }
        public string UserId { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public string Location { get; set; }
        public bool IsPublished { get; set; }
        public string EventType { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EventDateTime { get; set; }

        public int GroupId { get; set; }
    }
}
