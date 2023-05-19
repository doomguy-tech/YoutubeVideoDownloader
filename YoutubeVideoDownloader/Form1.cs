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
using System.Diagnostics;
using System.Configuration;
using YoutubeExplode.Playlists;

namespace YoutubeVideoDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool YoutubeValidURLCheck(string url) 
        {
            var regex = new Regex(@"^(https?\:\/\/)?(www\.)?(youtube\.com|youtu\.?be)\/.+$");

            if (regex.IsMatch(url))
            {
                return true;
            }
            else
            {
                return false;   
            }
        }

        public void DisableAllButtons() 
        {
            buttonDownload.Enabled = false;
            buttonDownloadVideo.Enabled = false;
            buttonChangeFolder.Enabled = false; 
            buttonClear.Enabled = false;
        }
        public void EnableAllButtons()
        {
            buttonDownload.Enabled = true;
            buttonDownloadVideo.Enabled = true;
            buttonChangeFolder.Enabled = true;
            buttonClear.Enabled = true;
        }

        public string RemoveIllegalFileNameCharacters(string fileName)
        {
            string illegalChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            string regexPattern = string.Format("[{0}]", Regex.Escape(illegalChars));
            return Regex.Replace(fileName, regexPattern, "");
        }

        private async void buttonDownload_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox1.Text))
                return;
            if (!YoutubeValidURLCheck(textBox1.Text))
            {
                label2.Text = "Not a valid URL";
                return;
            }


           
            DisableAllButtons();
            label2.Text = string.Empty;
            string url = textBox1.Text;
            var youtube = new YoutubeClient();

            if (url.Contains("list="))
            {
                var playlist = await youtube.Playlists.GetAsync(url);
                //var playlist = await youtube.Playlists.GetAsync("https://www.youtube.com/watch?v=4X2jQXcF0S8&list=PLfou2nEbJ6Qs738XcjmsIdLtB-5SRsj8V&pp=iAQB");
                var videos = await youtube.Playlists.GetVideosAsync(playlist.Id);
                string savedPath = labelOutputFolder.Text;
                var currentPath = Path.Combine(savedPath, playlist.Title);
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
                        label2.Text = Path.Combine(currentPath, cleanedName + ".mp3") + " already exists";
                    }
                    else
                        try
                        {
                            await youtube.Videos.Streams.DownloadAsync(streamInfo, Path.Combine(currentPath, cleanedName + ".mp3"));
                        }
                        catch(Exception ex) {
                            var filePath = Path.Combine(Environment.CurrentDirectory, playlist.Title+"_Exception.txt"); 

                            using (var writer = new StreamWriter(filePath))
                            {
                                writer.WriteLine(cleanedName);
                                writer.WriteLine(ex.ToString());
                            }

                        }
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
                string savedPath = labelOutputFolder.Text;
                var fileName = RemoveIllegalFileNameCharacters($"{video.Title}.mp3");
                var filePath = Path.Combine(savedPath, fileName);
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    label2.Text = fileName + " already exists";
                }
                else
                {
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
                    label2.Text = fileName + " has been saved successfully";
                }
            }
            EnableAllButtons();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBox1.Text= string.Empty;
            label2.Text= string.Empty;  
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label2.Text= string.Empty;

           
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings.Settings;
            string path = appSettings["OutputPath"].Value;
            if(String.IsNullOrEmpty(path))
                labelOutputFolder.Text= Environment.CurrentDirectory;
            else
            {
                labelOutputFolder.Text= path;
            }
        }

        private async void buttonDownloadVideo_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox1.Text))
                return;
            if (!YoutubeValidURLCheck(textBox1.Text))
            {
                label2.Text = "Not a valid URL";
                return;
            }
            DisableAllButtons();
            label2.Text = string.Empty;
            string url = textBox1.Text;
            var youtube = new YoutubeClient();

            if (url.Contains("list="))
            {
                var playlist = await youtube.Playlists.GetAsync(url);
                //var playlist = await youtube.Playlists.GetAsync("https://www.youtube.com/watch?v=4X2jQXcF0S8&list=PLfou2nEbJ6Qs738XcjmsIdLtB-5SRsj8V&pp=iAQB");
                var videos = await youtube.Playlists.GetVideosAsync(playlist.Id);
                string savedPath=labelOutputFolder.Text;
                var currentPath = Path.Combine(savedPath, playlist.Title);
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
                        label2.Text = Path.Combine(currentPath, cleanedName + ".mp4") + " already exists";
                    }
                    else
                        try
                        {
                            await youtube.Videos.Streams.DownloadAsync(streamInfo, Path.Combine(currentPath, cleanedName + ".mp4"));
                        }
                        catch(Exception ex)
                        {
                            var filePath = Path.Combine(Environment.CurrentDirectory, playlist.Title + "_Exception.txt");

                            using (var writer = new StreamWriter(filePath))
                            {
                                writer.WriteLine(cleanedName);
                                writer.WriteLine(ex.ToString());
                            }
                        }
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
                string savedPath = labelOutputFolder.Text;
                var fileName = RemoveIllegalFileNameCharacters($"{video.Title}.mp4");
                var filePath = Path.Combine(savedPath, fileName);
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    label2.Text = fileName + " already exists";
                }
                else
                {
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
                    label2.Text = fileName + " has been saved successfully";
                }
            }
            EnableAllButtons();
        }

        private void buttonOpenFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", labelOutputFolder.Text);
        }

        private void buttonChangeFolder_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            string path=string.Empty;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                path = dialog.SelectedPath;
            }
            if (!String.IsNullOrEmpty(path))
            {
                labelOutputFolder.Text = path;
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var appSettings = config.AppSettings.Settings;
                // Set the value for a key
                appSettings["OutputPath"].Value = path;

                // Save the changes to the app.config file
                config.Save(ConfigurationSaveMode.Modified);

                // Refresh the appSettings section
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
    }
}
