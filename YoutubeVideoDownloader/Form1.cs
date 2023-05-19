using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode.Videos;
using YoutubeExplode;
using System.IO;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace YoutubeVideoDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string RemoveIllegalFileNameCharacters(string fileName)
        {
            string illegalChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            string regexPattern = string.Format("[{0}]", Regex.Escape(illegalChars));
            return Regex.Replace(fileName, regexPattern, "");
        }

        private async void buttonDownload_Click(object sender, EventArgs e)
        {
            label2.Text = string.Empty;
            string url = textBox1.Text;
            var youtube = new YoutubeClient();

            if (url.Contains("list="))
            {
                var playlist = await youtube.Playlists.GetAsync(url);
                //var playlist = await youtube.Playlists.GetAsync("https://www.youtube.com/watch?v=4X2jQXcF0S8&list=PLfou2nEbJ6Qs738XcjmsIdLtB-5SRsj8V&pp=iAQB");
                var videos = await youtube.Playlists.GetVideosAsync(playlist.Id);
                var currentPath = Path.Combine(Environment.CurrentDirectory,playlist.Title);
                if (!Directory.Exists(currentPath))
                {
                    Directory.CreateDirectory(currentPath);
                }
                int count = 0;
                label2.Text = "Downloading reproduction list...";
                foreach (var video in videos)
                {
                    count++;
                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                    var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                    var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                    string cleanedName= RemoveIllegalFileNameCharacters( video.Title);
                    label2.Text = $"Downloading {video.Title}, {count} of {videos.Count}";
                    if (File.Exists(Path.Combine(currentPath, cleanedName + ".mp3")) && new FileInfo(Path.Combine(currentPath, cleanedName + ".mp3")).Length > 0)
                    {
                        Console.WriteLine("File exists and is not empty.");
                    }
                    else
                        await youtube.Videos.Streams.DownloadAsync(streamInfo, Path.Combine(currentPath, cleanedName + ".mp3"));

                }
                label2.Text = playlist.Title+" Has been saved successfully";
            }
            else
            {
                

                label2.Text = "Downloading single file...";
                var video = await youtube.Videos.GetAsync(url);
                //var video = await youtube.Videos.GetAsync("https://www.youtube.com/watch?v=tVUYyVfydqY");
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                var fileName = RemoveIllegalFileNameCharacters($"{video.Title}.mp3");
                var filePath = Path.Combine(Environment.CurrentDirectory, fileName);
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    Console.WriteLine("File exists and is not empty.");
                }
                else
                {
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
                    label2.Text = fileName + " has been saved successfully";
                }
            }

        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBox1.Text= string.Empty;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label2.Text= string.Empty;
        }

        private async void buttonDownloadVideo_Click(object sender, EventArgs e)
        {
            label2.Text = string.Empty;
            string url = textBox1.Text;
            var youtube = new YoutubeClient();

            if (url.Contains("list="))
            {
                var playlist = await youtube.Playlists.GetAsync(url);
                //var playlist = await youtube.Playlists.GetAsync("https://www.youtube.com/watch?v=4X2jQXcF0S8&list=PLfou2nEbJ6Qs738XcjmsIdLtB-5SRsj8V&pp=iAQB");
                var videos = await youtube.Playlists.GetVideosAsync(playlist.Id);
                var currentPath = Path.Combine(Environment.CurrentDirectory, playlist.Title);
                if (!Directory.Exists(currentPath))
                {
                    Directory.CreateDirectory(currentPath);
                }
                int count = 0;
                label2.Text = "Downloading reproduction list...";
                foreach (var video in videos)
                {
                    count++;
                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                    var streamInfo = streamManifest.GetVideoOnlyStreams().GetWithHighestBitrate();
                    var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                    string cleanedName = RemoveIllegalFileNameCharacters(video.Title);
                    label2.Text = $"Downloading {video.Title}, {count} of {videos.Count}";
                    if (File.Exists(Path.Combine(currentPath, cleanedName + ".mp4")) && new FileInfo(Path.Combine(currentPath, cleanedName + ".mp4")).Length > 0)
                    {
                        Console.WriteLine("File exists and is not empty.");
                    }
                    else
                        await youtube.Videos.Streams.DownloadAsync(streamInfo, Path.Combine(currentPath, cleanedName + ".mp4"));

                }
                label2.Text = playlist.Title + " Has been saved successfully";
            }
            else
            {
                label2.Text = "Downloading single file...";
                var video = await youtube.Videos.GetAsync(url);
                //var video = await youtube.Videos.GetAsync("https://www.youtube.com/watch?v=tVUYyVfydqY");
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                var streamInfo = streamManifest.GetVideoOnlyStreams().GetWithHighestBitrate();
                var fileName = RemoveIllegalFileNameCharacters($"{video.Title}.mp4");
                var filePath = Path.Combine(Environment.CurrentDirectory, fileName);
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    Console.WriteLine("File exists and is not empty.");
                }
                else
                {
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
                    label2.Text = fileName + " has been saved successfully";
                }
            }
        }
    }
}
