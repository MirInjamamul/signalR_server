﻿using static chat_server.Utils.Util;

namespace chat_server.Models
{
    public class LiveCall
    {
        public string SenderId { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string RoomId { get; set; }
        public string Message { get; set; }
        public bool IsVideo { get; set; } = false;
    }

    public class Termination
    {
        public SeatType seatType { get; set; } = SeatType.none;
    }
}
