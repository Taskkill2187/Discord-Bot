﻿using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class AnimeQuote : Command
    {
        public AnimeQuote() : base("animeQuote", "Posts a random anime quote.", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                string[] Files = Directory.GetFiles(@"D:\Eigene Dateien\Medien\Bilder\Reactions\Anime Quote");
                List<string> SendableFiles = new List<string>();
                foreach (string s in Files)
                {
                    if (Path.GetExtension(s) == ".jpg" || Path.GetExtension(s) == ".png" || Path.GetExtension(s) == ".jpeg" ||
                        Path.GetExtension(s) == ".gif" || Path.GetExtension(s) == ".mp4")
                        SendableFiles.Add(s);
                }
                string filepath = SendableFiles[Global.RDM.Next(SendableFiles.Count)];
                await Global.SendFile(filepath, commandmessage.Channel);
                Console.CursorLeft = 0;
                Console.WriteLine("Send anime quote in " + commandmessage.Channel.Name + " for " + commandmessage.Author.Username + " from " + filepath);
                Console.Write("$");
            }
            catch (Exception e)
            {
                await Global.SendText("Uwu We made a fucky wucky!! A wittle fucko boingo! The code monkeys at our headquarters are working VEWY HAWD to fix this!", commandmessage.Channel);

                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
        }
    }
}
