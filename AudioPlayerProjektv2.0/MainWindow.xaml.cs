using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AudioPlayerProjektv2._0
{
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private ObservableCollection<string> playlist = new ObservableCollection<string>();
        private DispatcherTimer timer;
        private int currentIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            Playlist.ItemsSource = playlist;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            VolumeLabel.Content = ((int)VolumeSlider.Value).ToString();
            TrackTimeLabel.Content = "0:00 / 0:00";
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
            timer.Start();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            timer.Stop();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                Playlist.SelectedIndex = currentIndex;
                PlaySelectedSong();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex < playlist.Count - 1)
            {
                currentIndex++;
                Playlist.SelectedIndex = currentIndex;
                PlaySelectedSong();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                SeekBar.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                SeekBar.Value = mediaPlayer.Position.TotalSeconds;
                UpdateTrackTimeLabel();
            }
        }

        private void LoadPlaylist()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    playlist.Add(filename);
                }
            }
        }

        private void SavePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Playlist files (*.playlist)|*.playlist|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllLines(saveFileDialog.FileName, playlist);
            }
        }

        private void LoadPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Playlist files (*.playlist)|*.playlist|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string[] lines = File.ReadAllLines(openFileDialog.FileName);
                playlist.Clear();
                foreach (string line in lines)
                {
                    playlist.Add(line);
                }
            }
        }

        private void PlaySelectedSong()
        {
            if (Playlist.SelectedItem != null)
            {
                currentIndex = Playlist.SelectedIndex;
                mediaPlayer.Open(new Uri(Playlist.SelectedItem.ToString()));
                mediaPlayer.Play();
                timer.Start();
                UpdateSongInfo();
            }
        }

        private void UpdateSongInfo()
        {
            if (Playlist.SelectedItem != null)
            {
                TrackInfo.Content = $"Now Playing: {System.IO.Path.GetFileName(Playlist.SelectedItem.ToString())}";
            }
        }

        private void Playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlaySelectedSong();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeLabel != null)
            {
                mediaPlayer.Volume = VolumeSlider.Value / 100;
                VolumeLabel.Content = ((int)VolumeSlider.Value).ToString();
            }
        }

        private void LoadFilesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPlaylist();
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SeekBar.IsMouseCaptureWithin && mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(SeekBar.Value);
                UpdateTrackTimeLabel();
            }
        }

        private void SeekBar_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(SeekBar.Value);
                UpdateTrackTimeLabel();
            }
        }

        private void UpdateTrackTimeLabel()
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                var currentPosition = mediaPlayer.Position;
                var totalDuration = mediaPlayer.NaturalDuration.TimeSpan;

                string currentPositionString = $"{(int)currentPosition.TotalMinutes}:{currentPosition.Seconds:D2}";
                string totalDurationString = $"{(int)totalDuration.TotalMinutes}:{totalDuration.Seconds:D2}";

                TrackTimeLabel.Content = $"{currentPositionString} / {totalDurationString}";
            }
        }
    }
}