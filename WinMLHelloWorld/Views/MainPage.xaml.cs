using System;
using WinMLHelloWorld.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace WinMLHelloWorld.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            
            ViewModel = new MainPageViewModel();
        }

        public MainPageViewModel ViewModel { get; set; }

        private async void OnWebCameraButtonClicked(object sender, RoutedEventArgs e)
        {
            await StartWebCameraAsync();
        }

        private async void OnCameraSourceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await RestartWebCameraAsync();
        }

        /// <summary>
        /// Event handler for camera source changes
        /// </summary>
        private async Task StartWebCameraAsync()
        {
            ViewModel.CameraViewerVisibilityState = Visibility.Visible;
            ViewModel.ImageViewerVisibilityState = Visibility.Collapsed;
            if (this.cameraSourceComboBox.SelectedItem == null)
            {
                return;
            }

            // Start camera
            await this.cameraControl.StartStreamAsync(this.ViewModel, this.cameraSourceComboBox.SelectedItem.ToString());
            await Task.Delay(250);
        }

        /// <summary>
        /// Restart the web camera
        /// </summary>
        private async Task RestartWebCameraAsync()
        {
            if (this.cameraSourceComboBox.SelectedItem == null)
            {
                return;
            }

            if (this.cameraControl.CameraStreamState == Windows.Media.Devices.CameraStreamState.Streaming)
            {
                await this.cameraControl.StopStreamAsync();
                await Task.Delay(1000);
                await this.cameraControl.StartStreamAsync(this.ViewModel, this.cameraSourceComboBox.SelectedItem.ToString());
            }
        }
    }
}