using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace MusicPlayer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        List<MusicClass> Musics;

        public class MusicClass
        {
            public String Title { get; set; }
            public String Artist { get; set; }
            public String MusicURI { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();

            isPlaying(false);
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();            
        }        

        private void isPlaying(bool flag)
        {
            Play.IsEnabled = flag;
            Stop.IsEnabled = flag;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if(mediaElement.Source != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                lblStatus.Content = String.Format("{0}                                            {1}", mediaElement.Position.ToString(@"mm\:ss"), mediaElement.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                Progress.Minimum = 0;
                Progress.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                Progress.Value = mediaElement.Position.TotalSeconds;
            }            
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            isPlaying(true);
            var curVolume = mediaElement.Volume;

            if(VolumeButton.Content == FindResource("VolumeButton"))
            {
                mediaElement.IsMuted = true;
                VolumeButton.Content = FindResource("MuteButton");
            }
            else
            {
                mediaElement.IsMuted = false;
                mediaElement.Volume = curVolume;
                VolumeButton.Content = FindResource("VolumeButton");
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            isPlaying(true);            
            if(Play.Content == FindResource("PlayButton"))
            {
                mediaElement.Play();
                Play.Content = FindResource("PauseButton");
            }
            else
            {
                mediaElement.Pause();
                Play.Content = FindResource("PlayButton");
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            Play.Content = FindResource("PlayButton");
            isPlaying(false);
            Play.IsEnabled = true;
        }

        private void PlaylistAdd_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|(*.*)";
            openDialog.Multiselect = true;

            Nullable<bool> result = openDialog.ShowDialog();
            
            if(result == true)
            {
                var file = openDialog.FileName;

                mediaElement.Source = new Uri(file);
                TagLib.File tagFile = TagLib.File.Create(file);
                
                Musics = new List<MusicClass> { new MusicClass { Title = tagFile.Tag.Title, Artist = tagFile.Tag.FirstAlbumArtist, MusicURI = file } };
                playlist.Items.Add(Musics);
                playlist.SelectedIndex += 1;

                mediaElement.Play();
                Play.Content = FindResource("PauseButton");
                Play.IsEnabled = true;
                Stop.IsEnabled = true;

                MusicName.Content = tagFile.Tag.Title;
                ArtistName.Content = tagFile.Tag.FirstAlbumArtist;
                if (tagFile.Tag.Pictures != null && tagFile.Tag.Pictures.Length > 0)
                {
                    TagLib.IPicture picture = tagFile.Tag.Pictures[0];
                    var sys = new System.IO.MemoryStream(picture.Data.Data);
                    sys.Seek(0, System.IO.SeekOrigin.Begin);

                    BitmapImage albumArt = new BitmapImage();

                    albumArt.BeginInit();
                    albumArt.StreamSource = sys;
                    albumArt.EndInit();

                    Cover.Source = albumArt;
                }
            }
        }

        private double SetProgressBarValue(double MousePosition)
        {
            double ratio = MousePosition / Progress.ActualWidth;
            double progressBarValue = ratio * Progress.Maximum;
            mediaElement.Position = TimeSpan.FromSeconds(progressBarValue);
            return progressBarValue;
        }

        private void Progress_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            double mousePosition = e.GetPosition(Progress).X;
            Progress.Value = SetProgressBarValue(mousePosition);
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if(playlist.SelectedIndex == 0 && playlist.Items.Count - 1 == 1)
            {
                mediaElement.Stop();
                mediaElement.Position = TimeSpan.FromSeconds(0);
                mediaElement.Play();
            }

            else if(playlist.SelectedIndex == playlist.Items.Count - 1)
            {
                playlist.SelectedIndex = 0;
                mediaElement.Stop();
                mediaElement.Position = TimeSpan.FromSeconds(0);
                mediaElement.Source = new Uri(playlist.SelectedValue as string);
                mediaElement.Play();

                TagLib.File tagFile = TagLib.File.Create(playlist.SelectedValue as string);
                MusicName.Content = tagFile.Tag.Title;
                ArtistName.Content = tagFile.Tag.FirstAlbumArtist;
                if (tagFile.Tag.Pictures != null && tagFile.Tag.Pictures.Length > 0)
                {
                    TagLib.IPicture picture = tagFile.Tag.Pictures[0];
                    var sys = new System.IO.MemoryStream(picture.Data.Data);
                    sys.Seek(0, System.IO.SeekOrigin.Begin);

                    BitmapImage albumArt = new BitmapImage();

                    albumArt.BeginInit();
                    albumArt.StreamSource = sys;
                    albumArt.EndInit();

                    Cover.Source = albumArt;
                }
            }

            else
            {
                playlist.SelectedIndex += 1;
                mediaElement.Stop();
                mediaElement.Position = TimeSpan.FromSeconds(0);
                mediaElement.Source = new Uri(playlist.SelectedValue as string);
                mediaElement.Play();

                TagLib.File tagFile = TagLib.File.Create(playlist.SelectedValue as string);
                MusicName.Content = tagFile.Tag.Title;
                ArtistName.Content = tagFile.Tag.FirstAlbumArtist;
                if (tagFile.Tag.Pictures != null && tagFile.Tag.Pictures.Length > 0)
                {
                    TagLib.IPicture picture = tagFile.Tag.Pictures[0];
                    var sys = new System.IO.MemoryStream(picture.Data.Data);
                    sys.Seek(0, System.IO.SeekOrigin.Begin);

                    BitmapImage albumArt = new BitmapImage();

                    albumArt.BeginInit();
                    albumArt.StreamSource = sys;
                    albumArt.EndInit();

                    Cover.Source = albumArt;
                }
            }
        }

        private void playlist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Stop();
            mediaElement.Position = TimeSpan.FromSeconds(0);            
            mediaElement.Source = new Uri(playlist.SelectedValue as string);
            mediaElement.Play();

            TagLib.File tagFile = TagLib.File.Create(playlist.SelectedValue as string);
            MusicName.Content = tagFile.Tag.Title;
            ArtistName.Content = tagFile.Tag.FirstAlbumArtist;
            if (tagFile.Tag.Pictures != null && tagFile.Tag.Pictures.Length > 0)
            {
                TagLib.IPicture picture = tagFile.Tag.Pictures[0];
                var sys = new System.IO.MemoryStream(picture.Data.Data);
                sys.Seek(0, System.IO.SeekOrigin.Begin);

                BitmapImage albumArt = new BitmapImage();

                albumArt.BeginInit();
                albumArt.StreamSource = sys;
                albumArt.EndInit();

                Cover.Source = albumArt;
            }
        }

        private void Backward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mediaElement.Position >= TimeSpan.FromSeconds(5))
                {
                    mediaElement.Stop();
                    mediaElement.Position = TimeSpan.FromSeconds(0);
                    mediaElement.Play();
                }
                else if (playlist.SelectedValue != null || playlist.SelectedIndex > 0)
                {
                    playlist.SelectedIndex -= 1;
                    mediaElement.Stop();
                    mediaElement.Position = TimeSpan.FromSeconds(0);
                    mediaElement.Source = new Uri(playlist.SelectedValue as string);
                    mediaElement.Play();

                    TagLib.File tagFile = TagLib.File.Create(playlist.SelectedValue as string);
                    MusicName.Content = tagFile.Tag.Title;
                    ArtistName.Content = tagFile.Tag.FirstAlbumArtist;
                    if (tagFile.Tag.Pictures != null && tagFile.Tag.Pictures.Length > 0)
                    {
                        TagLib.IPicture picture = tagFile.Tag.Pictures[0];
                        var sys = new System.IO.MemoryStream(picture.Data.Data);
                        sys.Seek(0, System.IO.SeekOrigin.Begin);

                        BitmapImage albumArt = new BitmapImage();

                        albumArt.BeginInit();
                        albumArt.StreamSource = sys;
                        albumArt.EndInit();

                        Cover.Source = albumArt;
                    }
                }
                else
                {
                    mediaElement.Stop();
                    mediaElement.Position = TimeSpan.FromSeconds(0);
                    mediaElement.Play();
                }
            }
            catch(System.ArgumentNullException)
            {
                mediaElement.Stop();
                mediaElement.Position = TimeSpan.FromSeconds(0);
                mediaElement.Play();
            }
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            playlist.SelectedIndex += 1;
            mediaElement.Stop();
            mediaElement.Position = TimeSpan.FromSeconds(0);
            mediaElement.Source = new Uri(playlist.SelectedValue as string);
            mediaElement.Play();

            TagLib.File tagFile = TagLib.File.Create(playlist.SelectedValue as string);
            MusicName.Content = tagFile.Tag.Title;
            ArtistName.Content = tagFile.Tag.FirstAlbumArtist;
            if (tagFile.Tag.Pictures != null && tagFile.Tag.Pictures.Length > 0)
            {
                TagLib.IPicture picture = tagFile.Tag.Pictures[0];
                var sys = new System.IO.MemoryStream(picture.Data.Data);
                sys.Seek(0, System.IO.SeekOrigin.Begin);

                BitmapImage albumArt = new BitmapImage();

                albumArt.BeginInit();
                albumArt.StreamSource = sys;
                albumArt.EndInit();

                Cover.Source = albumArt;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = (double)VolumeSlider.Value;
        }        
    }
}
