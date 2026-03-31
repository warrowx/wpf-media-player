using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace WpfMediaPlayer
{
    public partial class MainWindow : Window
    {
        // Поля
        private bool isPlaying = false;
        private bool isDragging = false;
        private System.Windows.Threading.DispatcherTimer timer;

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

        // Методы

        // Кнопки
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Аудио файлы|*.mp3;*.wav;*.wma|Все файлы|*.*";

            if (dialog.ShowDialog() == true)
            {
                mediaPlayer.Source = new Uri(dialog.FileName);
                TrackNameText.Text = System.IO.Path.GetFileName(dialog.FileName);
                ResetUI();
                ResetAllTimes();
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
            ResetUI();
            timer.Stop();
            
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

        
    }
}