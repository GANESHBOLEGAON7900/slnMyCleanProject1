﻿using System;

namespace MyCleanProject1.Application.Features.Events.Queries.GetEventsList
{
    public class EventListVm
    {
        public string EventId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string ImageUrl { get; set; }
    }
}
