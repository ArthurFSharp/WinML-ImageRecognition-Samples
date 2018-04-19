﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.Effects;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinMLHelloWorld.Utilities;
using WinMLHelloWorld.ViewModels;

// Pour en savoir plus sur le modèle d'élément Contrôle utilisateur, consultez la page https://go.microsoft.com/fwlink/?LinkId=234236

namespace WinMLHelloWorld.Controls
{
    public sealed partial class CameraControl : UserControl
    {
        public double CameraAspectRatio { get; set; }
        public int CameraResolutionWidth { get; private set; }
        public int CameraResolutionHeight { get; private set; }

        public CameraStreamState CameraStreamState { get { return this.CaptureManager != null ? this.CaptureManager.CameraStreamState : CameraStreamState.NotStreaming; } }

        private MediaCapture CaptureManager;
        private VideoEncodingProperties VideoProperties;

        private ThreadPoolTimer FrameProcessingTimer;
        private SemaphoreSlim FrameProcessingSemaphore = new SemaphoreSlim(1);

        private MainPageViewModel ViewModel;

        public CameraControl()
        {
            this.InitializeComponent();
        }

        private async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
        {
            if (this.CaptureManager.CameraStreamState != Windows.Media.Devices.CameraStreamState.Streaming
                || !this.FrameProcessingSemaphore.Wait(0))
            {
                return;
            }

            try
            {
                const BitmapPixelFormat InputPixelFormat = BitmapPixelFormat.Bgra8;
                using (VideoFrame previewFrame = new VideoFrame(InputPixelFormat, (int)this.VideoProperties.Width, (int)this.VideoProperties.Height))
                {
                    await this.CaptureManager.GetPreviewFrameAsync(previewFrame);

                    await this.ViewModel.EvaluateVideoFrameAsync(previewFrame);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception with ProcessCurrentVideoFrame: " + ex);
            }
            finally
            {
                this.FrameProcessingSemaphore.Release();
            }
        }

        public async Task StartStreamAsync(MainPageViewModel viewModel, string cameraName)
        {
            try
            {
                if (string.IsNullOrEmpty(cameraName) == true || viewModel == null)
                {
                    return;
                }

                this.ViewModel = viewModel;

                if (this.CaptureManager == null ||
                    this.CaptureManager.CameraStreamState == CameraStreamState.Shutdown ||
                    this.CaptureManager.CameraStreamState == CameraStreamState.NotStreaming)
                {
                    if (this.CaptureManager != null)
                    {
                        this.CaptureManager.Dispose();
                    }

                    this.CaptureManager = new MediaCapture();

                    MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
                    var allCameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                    var selectedCamera = allCameras.FirstOrDefault(c => DeviceUtility.GetCameraName(c, allCameras) == cameraName) ?? allCameras.FirstOrDefault();

                    if (selectedCamera != null)
                    {
                        settings.VideoDeviceId = selectedCamera.Id;
                    }

                    await this.CaptureManager.InitializeAsync(settings);

                    this.webCamCaptureElement.Source = this.CaptureManager;
                }

                if (this.CaptureManager.CameraStreamState == CameraStreamState.NotStreaming)
                {
                    if (this.FrameProcessingTimer != null)
                    {
                        this.FrameProcessingTimer.Cancel();
                        this.FrameProcessingSemaphore.Release();
                    }

                    TimeSpan timerInterval = TimeSpan.FromMilliseconds(66); //15fps
                    this.FrameProcessingTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(ProcessCurrentVideoFrame), timerInterval);

                    this.VideoProperties = this.CaptureManager.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                    await this.CaptureManager.StartPreviewAsync();

                    this.webCamCaptureElement.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception with StartStreamAsync: " + ex);
            }
        }

        public async Task StopStreamAsync()
        {
            try
            {
                if (this.FrameProcessingTimer != null)
                {
                    this.FrameProcessingTimer.Cancel();
                }

                if (this.CaptureManager != null && this.CaptureManager.CameraStreamState != Windows.Media.Devices.CameraStreamState.Shutdown)
                {
                    await this.CaptureManager.StopPreviewAsync();

                    this.webCamCaptureElement.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception with StopStreamAsync: " + ex);
            }
        }
    }
}
