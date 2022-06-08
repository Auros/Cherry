﻿using Cherry.Models;
using System;

namespace Cherry.Interfaces
{
    public interface IRequestManager
    {
        event EventHandler<RequestEventArgs> SongSkipped;
        event EventHandler<RequestEventArgs> SongAccepted;
        event EventHandler<RequestEventArgs> SongRequested;

        public bool HasNewRequests { get; set; }

        void Remove(RequestEventArgs request);
        void MarkAsRead(RequestEventArgs request);
    }
}