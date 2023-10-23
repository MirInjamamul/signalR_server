﻿namespace chat_server.Models
{
    public class MessageModel
    {
        public string SenderId { get; set; } = string.Empty;
        public string SenderUserName { get; set; } =string.Empty;
        public string To { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

    }

    public class FileMessageModel
    {
        public MessageModel Message { get; set; } = new MessageModel();
        public string Url { get; set; } = string.Empty;

    }
}
