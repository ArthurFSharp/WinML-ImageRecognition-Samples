using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using WinMLHelloWorld.Models;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace WinMLHelloWorld.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private FoodModel learningModel;
        private FoodModelInput modelInput = new FoodModelInput();

        private static readonly int IMAGE_SIZE = 227;

        public MainPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
            }
            else
            {
                // Load existings images in local app data
                // step
                // get all files in app data
                // add them in the collection
                GetFilesFromAppDataAsync();

                // Initialize cameras
                this.CameraNames = Task.Run(async () => await Utilities.DeviceUtility.GetAvailableCameraNamesAsync()).Result.ToList();
            }
        }
        
        #region Propriétés

        string _burger = "Non";
        public string Burger { get { return _burger; } set { Set(ref _burger, value); } }

        string _hotdog = "Non";
        public string HotDog { get { return _hotdog; } set { Set(ref _hotdog, value); } }

        string _tacos = "Non";
        public string Tacos { get { return _tacos; } set { Set(ref _tacos, value); } }

        string _category = "";
        public string Category { get { return _category; } set { Set(ref _category, value); } }

        BitmapImage _imageLocation = new BitmapImage();
        public BitmapImage ImageLocation { get { return _imageLocation; } set { Set(ref _imageLocation, value); } }

        private ObservableCollection<BitmapImage> _inputFiles = new ObservableCollection<BitmapImage>();
        public ObservableCollection<BitmapImage> InputFiles
        {
            get
            {
                return _inputFiles;
            }
            set
            {
                Set(ref _inputFiles, value);
            }
        }

        #endregion

        public async Task InitialiseVideoFrame()
        {
            await LoadModelAsync();
        }

        public async Task RecognizeImageBitmap()
        {
            ImageViewerVisibilityState = Visibility.Visible;
            CameraViewerVisibilityState = Visibility.Collapsed;

            var softwareBitmap = await GetSoftwareBitmapAsync(ImageLocation);
            if (softwareBitmap != null)
            {
                VideoFrame vf = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
                //croppedImage = await CropAndDisplayInputImageAsync(vf);

                modelInput.data = vf;

                var evalOutput = await learningModel.EvaluateAsync(modelInput);

                ProcessOutput(evalOutput);
            }
        }

        public async Task EvaluateVideoFrameAsync(VideoFrame videoFrame)
        {
            modelInput.data = videoFrame;

            var evalOutput = await learningModel.EvaluateAsync(modelInput);

            ProcessOutput(evalOutput);
        }

        public async Task OpenImagePicker()
        {
            var picker = new FileOpenPicker();

            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            picker.ViewMode = PickerViewMode.Thumbnail;

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                var path = await CopyImageAsync(file);
                var image = new BitmapImage(new Uri($"ms-appdata:///local/{path}"));
                InputFiles.Add(image);

                ImageLocation = image;
            }
        }

        #region File manipulation

        private async Task<BitmapImage> LoadImageAsync(StorageFile file)
        {
            BitmapImage bitmapImage = new BitmapImage();
            FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);
            bitmapImage.SetSource(stream);
            return bitmapImage;
        }

        /// <summary>
        /// Store image in app data
        /// </summary>
        /// <param name="file">The file</param>
        /// <returns>Path of the stored image</returns>
        private async Task<string> CopyImageAsync(StorageFile file)
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var newCopy = await file.CopyAsync(localFolder, file.Name,
                NameCollisionOption.GenerateUniqueName);
            return newCopy.Name;
        }

        private async Task GetFilesFromAppDataAsync()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var files = await localFolder.GetFilesAsync();

            foreach (var file in files)
            {
                var image = await LoadImageAsync(file);
                InputFiles.Add(image);
            }
        }

        private async Task<SoftwareBitmap> GetSoftwareBitmapAsync(BitmapImage image)
        {
            var randomAccess = RandomAccessStreamReference.CreateFromUri(image.UriSour‌​ce);
            using (var stream = await randomAccess.OpenReadAsync())
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                return softwareBitmap;
            }
        }

        #endregion

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                Burger = suspensionState[nameof(Burger)]?.ToString();
                HotDog = suspensionState[nameof(HotDog)]?.ToString();
                Tacos = suspensionState[nameof(Tacos)]?.ToString();
                Category = suspensionState[nameof(Category)]?.ToString();
                ImageLocation = suspensionState[nameof(ImageLocation)] as BitmapImage;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(Burger)] = Burger;
                suspensionState[nameof(HotDog)] = HotDog;
                suspensionState[nameof(Tacos)] = Tacos;
                suspensionState[nameof(Category)] = Category;
                suspensionState[nameof(ImageLocation)] = Category;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public void GotoDetailsPage() =>
            NavigationService.Navigate(typeof(Views.DetailPage), "");

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);

        #endregion

        #region Load Model

        private async Task LoadModelAsync()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(
                new Uri("ms-appx:///Assets/food.onnx"));
            learningModel = await FoodModel.CreateFoodModel(file);
        }

        #endregion

        #region Crop image

        private async Task<VideoFrame> CropAndDisplayInputImageAsync(VideoFrame inputVideoFrame)
        {
            bool useDX = inputVideoFrame.SoftwareBitmap == null;

            BitmapBounds cropBounds = new BitmapBounds();
            uint h = (uint)IMAGE_SIZE;
            uint w = (uint)IMAGE_SIZE;
            var frameHeight = useDX ? inputVideoFrame.Direct3DSurface.Description.Height : inputVideoFrame.SoftwareBitmap.PixelHeight;
            var frameWidth = useDX ? inputVideoFrame.Direct3DSurface.Description.Width : inputVideoFrame.SoftwareBitmap.PixelWidth;

            var requiredAR = ((float)IMAGE_SIZE / IMAGE_SIZE);
            w = Math.Min((uint)(requiredAR * frameHeight), (uint)frameWidth);
            h = Math.Min((uint)(frameWidth / requiredAR), (uint)frameHeight);
            cropBounds.X = (uint)((frameWidth - w) / 2);
            cropBounds.Y = 0;
            cropBounds.Width = w;
            cropBounds.Height = h;

            var croppedVf = new VideoFrame(BitmapPixelFormat.Bgra8, IMAGE_SIZE, IMAGE_SIZE, BitmapAlphaMode.Ignore);

            await inputVideoFrame.CopyToAsync(croppedVf, cropBounds, null);

            return croppedVf;
        }

        #endregion

        #region Display output

        private void ProcessOutput(FoodModelOutput evalOutput)
        {
            Burger = $"{BuildOutputString(evalOutput, "burger")}";
            HotDog = $"{BuildOutputString(evalOutput, "hot dog")}";
            Tacos = $"{BuildOutputString(evalOutput, "taco")}";
            if (!string.IsNullOrEmpty(evalOutput.classLabel.FirstOrDefault()) &&
                (Burger != "Non" || HotDog != "Non" || Tacos != "Non"))
            {
                Category = evalOutput.classLabel.FirstOrDefault();
            }
            else
            {
                Category = "aucune";
            }
        }

        private string BuildOutputString(FoodModelOutput evalOutput, string key)
        {
            var result = "Non";

            if (evalOutput.loss[key] > 0.25f)
            {
                result = $"{evalOutput.loss[key]:N2}";
            }
            return (result);
        }

        #endregion

        /// <summary>
        /// List of camera names
        /// </summary>
        public List<string> CameraNames { get; private set; }

        #region Visibility section

        /// <summary>
        /// Web camera visibility state
        /// </summary>
        public Visibility CameraOptionVisibilityState => this.CameraNames?.Count() > 0 == true ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ImageViewerVisibilityState { get; set; } = Visibility.Visible;

        public Visibility CameraViewerVisibilityState { get; set; } = Visibility.Collapsed;

        #endregion
    }
}
