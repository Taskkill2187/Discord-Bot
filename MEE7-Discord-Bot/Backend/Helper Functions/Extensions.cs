﻿using Discord;
using Discord.WebSocket;
using NAudio.Wave;
using NVorbis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.WinForms;
using Color = System.Drawing.Color;

namespace MEE7.Backend
{
    public static class Extensions
    {
        private static int RunAsConsoleCommandThreadIndex = 0;

        // String
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
        public static bool ContainsOneOf(this string str, string[] tests)
        {
            foreach (string s in tests)
                if (str.Contains(s))
                    return true;
            return false;
        }
        public static bool ContainsAllOf(this string str, string[] tests)
        {
            foreach (string s in tests)
                if (!str.Contains(s))
                    return false;
            return true;
        }
        public static string GetEverythingBetween(this string str, string left, string right)
        {
            int leftIndex = str.IndexOf(left);
            int rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex + 1);

            if (leftIndex == -1 || rightIndex == -1 || leftIndex > rightIndex)
            {
                //throw new Exception("String doesnt contain left or right borders!");
                return "";
            }

            try
            {
                string re = str.Remove(0, leftIndex + left.Length);
                re = re.Remove(rightIndex - leftIndex - left.Length);
                return re;
            }
            catch
            {
                return "";
            }
        }
        public static List<string> GetEverythingBetweenAll(this string str, string left, string right)
        {
            List<string> re = new List<string>();

            int leftIndex = str.IndexOf(left);
            int rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex + 1);

            if (leftIndex == -1 || rightIndex == -1 || leftIndex > rightIndex)
            {
                return re;
            }

            while (leftIndex != -1 && rightIndex != -1)
            {
                try
                {
                    str = str.Remove(0, leftIndex + left.Length);
                    re.Add(str.Remove(rightIndex - leftIndex - left.Length));
                }
                catch { break; }

                leftIndex = str.IndexOf(left);
                rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex + 1);
            }

            return re;
        }
        public static bool StartsWith(this string str, string[] values)
        {
            foreach (string s in values)
                if (str.StartsWith(s))
                    return true;
            return false;
        }
        public static string GetDiscordPictureLink(this string str)
        {
            string[] split = str.Split(' ');
            foreach (string s in split)
                if (s.StartsWith("https://cdn.discordapp.com/") && s.Contains(".png") ||
                    s.StartsWith("https://cdn.discordapp.com/") && s.Contains(".jpg"))
                    return s;
            return null;
        }
        public static string GetPictureLink(this string str)
        {
            string[] split = str.Split(' ');
            foreach (string s in split)
            {
                Uri.TryCreate(s, UriKind.Absolute, out Uri uriResult);
                if (uriResult != null && uriResult.Scheme == Uri.UriSchemeHttps ||
                    uriResult != null && uriResult.Scheme == Uri.UriSchemeHttp)
                {
                    var req = (HttpWebRequest)HttpWebRequest.Create(s);
                    req.Method = "HEAD";
                    using (var resp = req.GetResponse())
                        if (resp.ContentType.ToLower(CultureInfo.InvariantCulture)
                                .StartsWith("image/"))
                            return s;
                }
            }
            return null;
        }
        public static double ConvertToDouble(this string s)
        {
            return Convert.ToDouble(s.Replace('.', ','));
        }
        public static string ToCapital(this string s)
        {
            string o = "";
            for (int i = 0; i < s.Length; i++)
                if (i == 0)
                    o += char.ToUpper(s[i]);
                else
                    o += char.ToLower(s[i]);
            return o;
        }
        public static Bitmap GetBitmapFromURL(this string url) => new Bitmap(WebRequest.Create(url).GetResponse().GetResponseStream());
        public static Mp3FileReader Getmp3AudioFromURL(this string url)
        {
            Stream ms = new MemoryStream();
            using (Stream stream = WebRequest.Create(url).GetResponse().GetResponseStream())
                stream.CopyTo(ms);

            ms.Position = 0;
            return new Mp3FileReader(ms);
        }
        public static WaveFileReader GetwavAudioFromURL(this string url) => new WaveFileReader(WebRequest.Create(url).GetResponse().GetResponseStream());
        public static VorbisReader GetoggAudioFromURL(this string url) => new VorbisReader(WebRequest.Create(url).GetResponse().GetResponseStream(), true);
        public static int LevenshteinDistance(this string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
        public static float ModifiedLevenshteinDistance(this string smaller, string longer)
        {
            if (string.IsNullOrEmpty(smaller))
            {
                if (string.IsNullOrEmpty(longer))
                    return 0;
                return longer.Length;
            }

            if (string.IsNullOrEmpty(longer))
                return smaller.Length;

            // initialize the top and right of the table to 0, 1, 2, ...
            float[,] d = new float[smaller.Length + 1, longer.Length + 1];
            for (int i = 0; i <= smaller.Length; d[i, 0] = i++) ;
            for (int j = 1; j <= longer.Length; d[0, j] = j++) ;

            for (int i = 1; i <= smaller.Length; i++)
                for (int j = 1; j <= longer.Length; j++)
                {
                    float delete = d[i - 1, j] + 1;
                    float insert = d[i, j - 1] + 0.5f;
                    float replace = d[i - 1, j - 1] + ((longer[j - 1] == smaller[i - 1]) ? 0 : 1);
                    d[i, j] = Math.Min(Math.Min(delete, insert), replace);
                }
            return d[smaller.Length, longer.Length];
        }
        public static string Combine(this IEnumerable<string> s, string combinator = "")
        {
            return s.Foldl("", (x, y) => x + combinator + y).Remove(0, combinator.Length);
        }
        public static void RunAsConsoleCommand(this string command, int TimeLimitInSeconds, Action TimeoutEvent, Action<string, string> ExecutedEvent,
            Action<StreamWriter> RunEvent = null)
        {
            bool exited = false;
            string[] split = command.Split(' ');

            if (split.Length == 0)
                return;

            Process compiler = new Process();
            compiler.StartInfo.FileName = split.First();
            compiler.StartInfo.Arguments = split.Skip(1).Foldl("", (x, y) => x + " " + y);
            compiler.StartInfo.CreateNoWindow = true;
            compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            compiler.StartInfo.RedirectStandardInput = true;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.RedirectStandardError = true;
            compiler.Start();

            Task.Run(() => { RunEvent?.Invoke(compiler.StandardInput); });

            DateTime start = DateTime.Now;

            Task.Run(() => {
                Thread.CurrentThread.Name = $"RunAsConsoleCommand Thread {RunAsConsoleCommandThreadIndex++}";
                compiler.WaitForExit();

                string o = compiler.StandardOutput.ReadToEnd();
                string e = compiler.StandardError.ReadToEnd();

                exited = true;
                ExecutedEvent(o, e);
            });

            while (!exited && (DateTime.Now - start).TotalSeconds < TimeLimitInSeconds)
                Thread.Sleep(100);
            if (!exited)
            {
                exited = true;
                try
                {
                    compiler.Close();
                }
                catch { }
                TimeoutEvent();
            }
        }
        public static string GetHTMLfromURL(this string URL)
        {
            try {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
                req.KeepAlive = false;
                req.Timeout = 3000;
                req.AllowAutoRedirect = true;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
                using (WebResponse w = req.GetResponse())
                using (Stream s = w.GetResponseStream())
                using (StreamReader sr = new StreamReader(s))
                    return sr.ReadToEnd();
            } catch (Exception e) { return $"Exception: {e}"; }
        }
        public static Bitmap ConvertHtmlToImage(this string HTML, int width = 600, int height = 800)
        {
            Bitmap b = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(b))
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, width, height));
            HtmlRender.Render(Graphics.FromImage(b), HTML, new PointF(0, 0), new SizeF(500, 500));
            return b;
        }

        // Discord
        public static EmbedBuilder ToEmbed(this IMessage m)
        {
            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithColor(0, 128, 255);
            Embed.WithAuthor(m.Author);
            Embed.WithTitle(string.IsNullOrWhiteSpace(m.Content) ?
                m.Attachments.Select(x => x.Url).
                Where(x => !x.EndsWith(".png") && !x.EndsWith(".jpg")).
                Union(new string[] { "-" }).
                Aggregate((x, y) => y == "-" ? x : x + " " + y) : 
                (m.Content.Length > 256 ? m.Content.Substring(0, 253) + "..." : m.Content));
            try
            {
                if (m.Attachments.Count > 0)
                    Embed.WithImageUrl(m.Attachments.ElementAt(0).Url);
            }
            catch { }
            return Embed;
        }
        public static ulong GetServerID(this IMessage m)
        {
            return Program.GetGuildFromChannel(m.Channel).Id;
        }
        public static string GetDisplayName(this SocketGuildUser u)
        {
            if (u.Nickname != null)
                return u.Nickname;
            return u.Username;
        }
        public static Discord.Color? GetDisplayColor(this SocketGuildUser u)
        {
            SocketRole[] r = u.Roles.Where(x => x.Color.RawValue != 0).ToArray();
            if (r.Length > 0)
                return new Discord.Color(r.MaxElement(x => x.Position).Color.RawValue);
            else
                return null;
        }
        public static void AddFieldDirectly(this EmbedBuilder e, string Name, object Value, bool IsInline = false)
        {
            string text = Value.ToString();
            if (text.Length < 1024)
                e.Fields.Add(new EmbedFieldBuilder() { Name = Name, Value = Value, IsInline = IsInline });
            else
            {
                int i;
                for (i = 1; text.Length >= 1015; i++)
                {
                    int cutIndex = text.AllIndexesOf("\n").Where(x => x <= 1020).Max();
                    e.Fields.Add(new EmbedFieldBuilder() { Name = $"{Name} {i}", Value = text.Substring(0, cutIndex), IsInline = IsInline });
                    text = text.Remove(0, cutIndex);
                }
                e.Fields.Add(new EmbedFieldBuilder() { Name = $"{Name} {i}", Value = text, IsInline = IsInline });
            }
        }
        public static SelfmadeMessage EditContent(this SelfmadeMessage m, string newContent)
        {
            m.Content = newContent;
            return m;
        }

        // Drawing
        public static Bitmap CropImage(this Bitmap source, Rectangle section)
        {
            // An empty bitmap which will hold the cropped image
            Bitmap bmp = new Bitmap(section.Width, section.Height);

            using (Graphics g = Graphics.FromImage(bmp))

                // Draw the given area (section) of the source image
                // at location 0,0 on the empty bitmap (bmp)
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);

            return bmp;
        }
        public static int GetGrayScale(this Color c) => (c.R + c.G + c.B) / 3;
        public static int GetColorDiff(this Color c, Color C) => c.R - C.R + c.G - C.G + c.B - C.B;
        public static Color HsvToRgb(this Vector3 HSV) // from https://stackoverflow.com/questions/1335426/is-there-a-built-in-c-net-system-api-for-hsv-to-rgb
        {
            double h = HSV.X;
            double S = HSV.Y;
            double V = HSV.Z;

            int Clamp(int i)
            {
                if (i < 0) return 0;
                if (i > 255) return 255;
                return i;
            }

            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            return Color.FromArgb(
                Clamp((int)(R * 255.0)), 
                Clamp((int)(G * 255.0)), 
                Clamp((int)(B * 255.0)));
        }
        public static float GetValue(this Color c)
        {
            return new float[] { c.R / 255f, c.G / 255f, c.B / 255f }.Max();
        }
        public static Bitmap RotateImage(this Bitmap b, float AngleInDegrees, Vector2 RotationOrigin)
        {
            Bitmap re = new Bitmap(b.Width, b.Height);
            using (Graphics g = Graphics.FromImage(re))
            {
                g.TranslateTransform(RotationOrigin.X, RotationOrigin.Y);
                g.RotateTransform(AngleInDegrees);
                g.TranslateTransform(-RotationOrigin.X, -RotationOrigin.Y);
                g.DrawImage(b, Point.Empty);
            }
            return re;
        }
        public static Point RotatePointAroundPoint(this Point P, Point RotationOrigin, double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new Point((int)(cos * (P.X - RotationOrigin.X) - sin * (P.Y - RotationOrigin.Y) + RotationOrigin.X),
                             (int)(sin * (P.X - RotationOrigin.X) + cos * (P.Y - RotationOrigin.Y) + RotationOrigin.Y));
        }

        // Linq Extensions
        public static b Foldl<a, b>(this IEnumerable<a> xs, b y, Func<b, a, b> f)
        {
            foreach (a x in xs)
                y = f(y, x);
            return y;
        }
        public static b Foldl<a, b>(this IEnumerable<a> xs, Func<b, a, b> f)
        {
            return xs.Foldl(default, f);
        }
        public static a MaxElement<a>(this IEnumerable<a> xs, Func<a, double> f) { return xs.MaxElement(f, out double max); }
        public static a MaxElement<a>(this IEnumerable<a> xs, Func<a, double> f, out double max)
        {
            max = 0; a maxE = default;
            foreach (a x in xs)
            {
                double res = f(x);
                if (res > max)
                {
                    max = res;
                    maxE = x;
                }
            }
            return maxE;
        }
        public static a MinElement<a>(this IEnumerable<a> xs, Func<a, double> f) { return xs.MinElement(f, out double min); }
        public static a MinElement<a>(this IEnumerable<a> xs, Func<a, double> f, out double min)
        {
            min = 0; a minE = default;
            foreach (a x in xs)
            {
                double res = f(x);
                if (res < min)
                {
                    min = res;
                    minE = x;
                }
            }
            return minE;
        }
        public static bool ContainsAny<a>(this IEnumerable<a> xs, IEnumerable<a> ys)
        {
            foreach (a y in ys)
                if (xs.Contains(y))
                    return true;
            return false;
        }
        public static a GetRandomValue<a>(this IEnumerable<a> xs)
        {
            a[] arr = xs.ToArray();
            return arr[Program.RDM.Next(arr.Length)];
        }
        public static string RemoveLastGroup(this string s, char seperator)
        {
            string[] split = s.Split(seperator);
            return split.Take(split.Length - 1).Foldl("", (a, b) => a + seperator + b).Remove(0, 1);
        }

        // ???
        public static bool IsFileLocked(this FileInfo file) // from https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public static void InvokeParallel(this Delegate del, params object[] args)
        {
            foreach (var d in del.GetInvocationList())
                Task.Run(() => { try { d.DynamicInvoke(args); }
                                 catch { } });
        }
        /// <summary>
        /// For the given type, returns its representation in C# code.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="genericArgs">Used internally, ignore.</param>
        /// <param name="arrayBrackets">Used internally, ignore.</param>
        /// <returns>The representation of the type in C# code.</returns>
        public static string ToReadableString(this Type type)
        {
            return type.ToString().
                Replace("`System.Object`System.Linq.Enumerable+RepeatIterator`1[System.Char]", "string").
                Replace("`System.Object`System.Linq.Enumerable+RepeatIterator`1[", "").
                Replace("]", "[]").
                Split('.').Last().Replace("`", "'").Replace("´", "'");
        }
        public static byte[] ToArray(this Stream stream)
        {
            byte[] buffer = new byte[4096];
            int reader = 0;
            MemoryStream memoryStream = new MemoryStream();
            while ((reader = stream.Read(buffer, 0, buffer.Length)) != 0)
                memoryStream.Write(buffer, 0, reader);
            return memoryStream.ToArray();
        }
        public static Vector2 Normalize(this Vector2 v)
        {
            float l = v.Length();
            v.X = v.X / l;
            v.Y = v.Y / l;
            return v;
        }
    }
}