﻿using Discord;
using Discord.WebSocket;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        readonly EditCommand[] InputCommands = new EditCommand[] {
            new EditCommand("lastT", "Gets the last messages text", (SocketMessage m, string a, object o) => {
                return m.Channel.GetMessagesAsync(2).FlattenAsync().Result.Last().Content;
            }, null, typeof(string)),
            new EditCommand("lastP", "Gets the last messages picture", (SocketMessage m, string a, object o) => {
                IMessage lm = m.Channel.GetMessagesAsync(2).FlattenAsync().Result.Last();
                string pic = null;
                if (lm.Attachments.Count > 0 && lm.Attachments.ElementAt(0).Size > 0)
                {
                    if (lm.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                        pic = lm.Attachments.ElementAt(0).Url;
                    else if (lm.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                        pic = lm.Attachments.ElementAt(0).Url;
                }
                string picLink = lm.Content.GetPictureLink();
                if (string.IsNullOrWhiteSpace(pic) && picLink != null)
                    pic = picLink;
                return pic.GetBitmapFromURL();
            }, null, typeof(Bitmap)),
            new EditCommand("thisT", "Outputs the given arguments", (SocketMessage m, string a, object o) => {
                return a;
            }, null, typeof(string)),
            new EditCommand("thisP", "Gets this messages picture / picture link in the arguments", (SocketMessage m, string a, object o) => {
                string pic = null;
                if (m.Attachments.Count > 0 && m.Attachments.ElementAt(0).Size > 0)
                {
                    if (m.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                        pic = m.Attachments.ElementAt(0).Url;
                    else if (m.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                        pic = m.Attachments.ElementAt(0).Url;
                }
                string picLink = a.GetPictureLink();
                if (string.IsNullOrWhiteSpace(pic) && picLink != null)
                    pic = picLink;
                return pic.GetBitmapFromURL();
            }, null, typeof(Bitmap)),
            new EditCommand("thisA", "Gets mp3 or wav audio files attached to this message", (SocketMessage m, string a, object o) => {
                string url = m.Attachments.FirstOrDefault(x => x.Url.EndsWith(".mp3")).Url;
                if (!string.IsNullOrWhiteSpace(url))
                    return url.Getmp3AudioFromURL();

                url = m.Attachments.FirstOrDefault(x => x.Url.EndsWith(".wav")).Url;
                if (!string.IsNullOrWhiteSpace(url))
                    return url.GetwavAudioFromURL();

                url = m.Attachments.FirstOrDefault(x => x.Url.EndsWith(".ogg")).Url;
                if (!string.IsNullOrWhiteSpace(url))
                    return url.GetoggAudioFromURL();

                throw new Exception("No audio file found!");
            }, null, typeof(WaveStream)),
            new EditCommand("profilePicture", "Gets a profile picture", (SocketMessage m, string a, object o) => {
                return Program.GetUserFromId(Convert.ToUInt64((a as string).Trim(new char[] { '<', '>', '@' }))).GetAvatarUrl(ImageFormat.Png, 512).GetBitmapFromURL();
            }, null, typeof(Bitmap)),
            new EditCommand("mp3FromYT", "Gets the mp3 of an youtube video, takes the video url as argument", 
                (SocketMessage m, string a, object o) => {
                    MemoryStream mem = new MemoryStream();
                    using (Process P = Program.GetAudioStreamFromYouTubeVideo(a, "mp3"))
                    {
                        while (true)
                        {
                            Task.Delay(1001).Wait();
                            if (string.IsNullOrWhiteSpace(P.StandardError.ReadLine()))
                                break;
                        }
                        P.StandardOutput.BaseStream.CopyTo(mem);
                        return WaveFormatConversionStream.CreatePcmStream(new StreamMediaFoundationReader(mem));
                    }
            }, null, typeof(WaveStream)),
        };
    }
}
