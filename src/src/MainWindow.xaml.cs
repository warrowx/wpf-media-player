using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace WpfMediaPlayer
{
    public partial class MainWindow : Window
    {
        // Поля
        private bool isPlaying = false;
        private bool isDragging = false;
        private System.Windows.Threading.DispatcherTimer timer;
        private List<string> playlist = new List<string>();
        private int currentTrackIndex = -1;
        private bool isPlayingFromPlaylist = false;
        private bool isShuffleEnabled = false;
        private List<int> shuffledIndicies = new List<int>();
        private int shufflePointer = 0;
        Random random = new Random();
        // Конструктор
        public MainWindow()
        {
            InitializeComponent();
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += Timer_Tick;
        }

        // Вспомогательные методы
        private void ResetUI()
        {
            isPlaying = false;
            PlayPauseButton.Content = "Play";
            ProgressBar.Value = 0;
            CurrentTimeText.Text = "00:00";
        }
        private void ResetAllTimes()
        {
            CurrentTimeText.Text = "00:00";
            TotalTimeText.Text = "00:00";
            ProgressBar.Value = 0;
        }
        private void PlayFile(string filePath)
        {
            mediaPlayer.Source = new Uri(filePath);
            TrackNameText.Text = System.IO.Path.GetFileName(filePath);
            mediaPlayer.Play();
            isPlaying = true;
            PlayPauseButton.Content = "Pause";
            timer.Start();
        }

        private void PlayNextTrack()
        {
            if (!isPlayingFromPlaylist) return;
            if (playlist.Count == 0) return;

            if (isShuffleEnabled)
            {
                if (shufflePointer >= shuffledIndicies.Count)
                {
                    GenerateShuffledIndicies();
                    shufflePointer = 0;
                }
                int nextIdx = shuffledIndicies[shufflePointer];
                shufflePointer++;
                currentTrackIndex = nextIdx;
            }
            else
            {
                int nextIndex = currentTrackIndex + 1;
                if (nextIndex >= playlist.Count) return;
                currentTrackIndex = nextIndex;
            }

            PlayFile(playlist[currentTrackIndex]);
            PlaylistBox.SelectedIndex = currentTrackIndex;
        }

        private void GenerateShuffledIndicies()
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < playlist.Count; i++)
            {
                indices.Add(i);
            }

            
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                int temp = indices[i];
                indices[i] = indices[j];
                indices[j] = temp;
            }

            shuffledIndicies = indices;
            shufflePointer = 0;

            if (shuffledIndicies.Count > 1 && shuffledIndicies[0] == currentTrackIndex)
            {
                int swapIndex = random.Next(1, shuffledIndicies.Count);
                int temp = shuffledIndicies[0];
                shuffledIndicies[0] = shuffledIndicies[swapIndex];
                shuffledIndicies[swapIndex] = temp;
                
            }
        }
        // Методы

        // Кнопки
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            var dialog = new OpenFileDialog();
            dialog.Filter = "Аудио файлы|*.mp3;*.wav;*.wma|Все файлы|*.*";

            if (dialog.ShowDialog() == true)
            {
                mediaPlayer.Source = new Uri(dialog.FileName);
                TrackNameText.Text = System.IO.Path.GetFileName(dialog.FileName);
                ResetUI();
                ResetAllTimes();
                isPlayingFromPlaylist = false;
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {

            if (mediaPlayer.Source == null) return;

            // Если трек доигран - возвращаем обратно на старт
            if (mediaPlayer.Position.TotalSeconds >= ProgressBar.Maximum && ProgressBar.Maximum > 0)
            {
                mediaPlayer.Position = TimeSpan.Zero;
                ProgressBar.Value = 0;
                CurrentTimeText.Text = "00:00";
            }
                if (isPlaying)
                {
                    mediaPlayer.Pause();
                    PlayPauseButton.Content = "Play";
                    isPlaying = false;
                }
                else
                {
                    mediaPlayer.Play();
                    PlayPauseButton.Content = "Pause";
                    isPlaying = true;

                    if (!timer.IsEnabled) 
                    {
                        timer.Start(); 
                    }

                }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer?.Stop();
            ResetUI();
            timer.Stop();
            
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            PlayNextTrack();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            int previousIndex = currentTrackIndex - 1;
            if (previousIndex >= 0)
            {
                currentTrackIndex = previousIndex;
                PlayFile(playlist[currentTrackIndex]);
                PlaylistBox.SelectedIndex = currentTrackIndex;
            }
            else if (previousIndex < 0) return;
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            isShuffleEnabled = !isShuffleEnabled;
            if (isShuffleEnabled)
            {
                if (playlist.Count > 0)
                {
                    GenerateShuffledIndicies();
                    ShuffleButton.Background = System.Windows.Media.Brushes.LightBlue;
                }
                
            }
            else
            {
                ShuffleButton.Background = System.Windows.Media.Brushes.Transparent;
            }
        }
        // Громкость
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer != null && VolumeValueText != null)
            {
                mediaPlayer.Volume = VolumeSlider.Value;
                VolumeValueText.Text = (VolumeSlider.Value * 100).ToString("F0") + "%";
            }
        }
        // Состояния плеера
        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                TimeSpan duration = mediaPlayer.NaturalDuration.TimeSpan;
                TotalTimeText.Text = duration.ToString(@"mm\:ss");
                ProgressBar.Maximum = duration.TotalSeconds;
                timer.Start();
            }
        }
        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (playlist.Count > 0 && currentTrackIndex >= 0)
            {
                PlayNextTrack();
            }
            else
            {
                ResetUI();
                timer.Stop();
            }
            
        }
        // Таймер
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer != null && mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                double currentSeconds = mediaPlayer.Position.TotalSeconds;
                
                CurrentTimeText.Text = mediaPlayer.Position.ToString(@"mm\:ss");

                if (!isDragging)
                {
                    ProgressBar.Value = currentSeconds;
                }
            }
        }
        // Перемотка
        private void ProgressBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
            if (mediaPlayer != null && mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(ProgressBar.Value);
                
            }
            isDragging = false;
        }
        private void ProgressBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;  
        }

        private void AddFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Аудио файлы|*.mp3;*.wav;*.wma;*.m4a|Все файлы|*.*";
            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                {
                    playlist.Add(file);
                    PlaylistBox.Items.Add(System.IO.Path.GetFileName(file));
                }
            }
        }

        private void PlaylistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (PlaylistBox.SelectedIndex >= 0)
            {
                currentTrackIndex = PlaylistBox.SelectedIndex;
                PlayFile(playlist[currentTrackIndex]);
            }
            isPlayingFromPlaylist = true;
        }

        private void PlaylistBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PlaylistBox.SelectedIndex >= 0)
            {
                currentTrackIndex = PlaylistBox.SelectedIndex;
                PlayFile(playlist[currentTrackIndex]);
            }
            isPlayingFromPlaylist = true;
        }

        private void RemoveSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistBox.SelectedIndex >= 0)
            {
                int idx = PlaylistBox.SelectedIndex;
                playlist.RemoveAt(idx);
                PlaylistBox.Items.RemoveAt(idx);

                if (playlist.Count == 0)
                {
                    mediaPlayer.Stop();
                    ResetUI();
                    timer.Stop();
                    TrackNameText.Text = "Трек не выбран";
                    currentTrackIndex = -1;
                }
                else if (idx == currentTrackIndex)
                {
                    mediaPlayer.Stop();
                    ResetUI();
                    currentTrackIndex = -1;
                }
                else if (idx < currentTrackIndex)
                {
                    currentTrackIndex--;
                }
            }
        }

        private void ClearPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlayingFromPlaylist)
            {
                mediaPlayer.Stop();
                mediaPlayer.Source = null;
                ResetUI();
                timer.Stop();
                TrackNameText.Text = "Трек не выбран";
            }
            playlist.Clear();
            PlaylistBox.Items.Clear();
            currentTrackIndex = -1;
        }


    }
}