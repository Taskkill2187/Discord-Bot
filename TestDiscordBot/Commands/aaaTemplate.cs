﻿using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class aaaTemplate : Command
    {
        public aaaTemplate() : base("", "", true, true)
        {

        }

        public override Task Execute(SocketMessage message)
        {
            // <Insert command code here>
            return Task.FromResult(default(object));
        }
    }
}
