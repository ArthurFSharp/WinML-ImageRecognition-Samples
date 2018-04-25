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

        private async void OnBrowseButtonClicked(object sender, RoutedEventArgs e)
        {
            await StopWebCameraAsync();
            ShowImageView();
            await ViewModel.OpenImagePicker();
        }

        private async void OnCameraSourceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await RestartWebCameraAsync();
        }

        private async void imagePickerGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await StopWebCameraAsync();
            ShowImageView();
        }

        /// <summary>
        /// Event handler for camera source changes
        /// </summary>
        private async Task StartWebCameraAsync()
        {
            ShowCameraView();
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

        /// <summary>
        /// Stop the web camera
        /// </summary>
        /// <returns></returns>
        private async Task StopWebCameraAsync()
        {
            if (this.cameraControl.CameraStreamState == Windows.Media.Devices.CameraStreamState.Streaming)
            {
                await this.cameraControl.StopStreamAsync();
            }
        }

        private void ShowCameraView()
        {
            ViewModel.CameraViewerVisibilityState = Visibility.Visible;
            ViewModel.ImageViewerVisibilityState = Visibility.Collapsed;
        }

        private void ShowImageView()
        {
            ViewModel.CameraViewerVisibilityState = Visibility.Collapsed;
            ViewModel.ImageViewerVisibilityState = Visibility.Visible;
        }
    }
}