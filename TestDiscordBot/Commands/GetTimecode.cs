﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class GetTimecode : Command
    {
        public GetTimecode() : base("getTimestamp", "Gets the exact timestamp of a message", false)
        {

        }

        public override async Task execute(SocketMessage message)
        {
            string[] split = message.Content.Split(' ');
            if (split.Length == 1)
                await Global.SendText("I need a messageID!", message.Channel);
            else if (split.Length == 2)
            {
                string messageID = split[1].Split('/').Last();

                ulong id = 0;
                try { id = Convert.ToUInt64(messageID); }
                catch { await Global.SendText("The messageID needs to be a number!", message.Channel); return; }

                if (id == 0) { await Global.SendText("The messageID can't be 0!", message.Channel); return; }

                IMessage m = await message.Channel.GetMessageAsync(id);
                if (m == null)
                    await Global.SendText("I can't find that message!", message.Channel);
                else
                    await Global.SendText("Posted: " + m.Timestamp + "\nEdited: " + m.EditedTimestamp, message.Channel);
            }
            else
                await Global.SendText("Thats too many parameters, I only need 2!", message.Channel);
        }
    }
}