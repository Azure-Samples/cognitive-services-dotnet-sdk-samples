using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace FaceAPIHeadPoseSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        #region Fields

        private static FrameGrabber<LiveCameraResult> _grabber;

        private static readonly ImageEncodingParam[] s_jpegParams = { new ImageEncodingParam(ImwriteFlags.JpegQuality, 60) };

        private readonly string _isolatedStorageSubscriptionKeyFileName = "Subscription.txt";

        private readonly string _isolatedStorageSubscriptionEndpointFileName = "SubscriptionEndpoint.txt";

        private readonly double _headPitchMaxThreshold = 30;

        private readonly double _headPitchMinThreshold = -15;

        private readonly double _headYawMaxThreshold = 20;

        private readonly double _headYawMinThreshold = -20;

        private readonly double _headRollMaxThreshold = 20;

        private readonly double _headRollMinThreshold = -20;

        private readonly string _indicateDefaultMsg = "Head pose test finished!";

        private static IFaceClient client;

        private static bool startHeadPoseProcess = false;

        private static bool processIdel = true;

        private static bool firstInProcess = true;

        private static int processStep = 1;

        private static List<double> buff = new List<double>();

        private static int activeFrames = 14;

        #endregion

        #region Initialization

        private void Initialization()
        {
            SubscriptionKey = GetSubscriptionKeyFromIsolatedStorage();
            SubscriptionEndpoint = GetSubscriptionEndpointFromIsolatedStorage();

            try
            {
                client =
                    new FaceClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
                    {
                        Endpoint =
                                SubscriptionEndpoint
                    };
                Log("FaceClient Initialization succeeded.");
            }
            catch (Exception e)
            {
                Log($"Exception:{e.Message}");
            }

            _grabber = new FrameGrabber<LiveCameraResult>();

            _grabber.NewFrameProvided += (s, e) =>
            {
                // The callback may occur on a different thread, so we must use the
                // MainWindow.Dispatcher when manipulating the UI. 
                ImageDisplay.Dispatcher.Invoke(
                    () =>
                    {
                            // Display the image in the left pane.
                            ImageDisplay.Source = e.Frame.Image.ToBitmapSource();
                    });
            };

            // Set up a listener for when the client receives a new result from an API call. 
            _grabber.NewResultAvailable += (s, e) =>
            {
                if (processIdel)
                {
                    if (e == null)
                    {
                        return;
                    }

                    this.Dispatcher.BeginInvoke(
                        (Action)(() =>
                        {
                            if (e.Exception == null && !e.TimedOut)
                            {
                                if (e.Analysis.Faces != null && e.Analysis.Faces.Length != 0)
                                {
                                    var headpose = e.Analysis.Faces.FirstOrDefault()?.FaceAttributes?.HeadPose;
                                    if (headpose == null)
                                    {
                                        return;
                                    }

                                    if (startHeadPoseProcess)
                                    {
                                        Doprocess(headpose);
                                    }
                                    else if (_indicateDefaultMsg != IndicateMsg)
                                    {
                                        ResetProgressBarString();
                                    }

                                    D_Yaw = headpose.Yaw;
                                    D_Pitch = -headpose.Pitch;
                                    D_Roll = -headpose.Roll;
                                }
                            }
                            else if (e.Exception != null)
                            {
                                Log($"Exception:{e.Exception.Message}");
                            }
                        }));
                }
            };

            if (_grabber.GetNumCameras() != 0)
            {
                _grabber.TriggerAnalysisOnInterval(TimeSpan.FromSeconds(0.3));
                _grabber.StartProcessingCameraAsync().Wait();
            }
            else
            {
                Log("Exception: No camera detected, please connect a camera and restart.");
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Initialization();
        }

        #region Events

        /// <summary>
        /// Implement INotifyPropertyChanged event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task<LiveCameraResult> HeadPoseDemoFunction(VideoFrame frame)
        {
            // Encode image. 
            var jpg = frame.Image.ToMemoryStream(".jpg", s_jpegParams);

            // Submit image to API. 
            var attrs = new List<FaceAttributeType> { FaceAttributeType.HeadPose };

            var faces = await client.Face.DetectWithStreamAsync(jpg, returnFaceId: false, returnFaceAttributes: attrs);

            // Output. 
            return new LiveCameraResult { Faces = faces.ToArray() };
        }

        public void Log(string logMessage)
        {
            if (string.IsNullOrEmpty(logMessage) || logMessage == "\n")
            {
                _logTextBox.Text += "\n";
            }
            else
            {
                string timeStr = DateTime.Now.ToString("HH:mm:ss.ffffff");
                string messaage = "[" + timeStr + "]: " + logMessage + "\n";
                _logTextBox.Text += messaage;
            }

            _logTextBox.ScrollToEnd();
        }

        #endregion

        #region Properties

        private string _subscriptionKey = "Paste your subscription key";

        public string SubscriptionKey
        {
            get
            {
                return _subscriptionKey;
            }

            set
            {
                _subscriptionKey = value;
                OnPropertyChanged("SubscriptionKey");
            }
        }

        private string _subscriptionEndpoint = "Paste your endpoint";

        public string SubscriptionEndpoint
        {
            get
            {
                return _subscriptionEndpoint;
            }

            set
            {
                _subscriptionEndpoint = value;
                OnPropertyChanged("SubscriptionEndpoint");
            }
        }

        private string _indicateMsg = "Head pose test";

        public string IndicateMsg
        {
            get
            {
                return _indicateMsg;
            }

            set
            {
                _indicateMsg = value;
                OnPropertyChanged("IndicateMsg");
            }
        }

        private string _msgProcessVerticalTop = GetVerticalTopProgressBarString();

        public string MsgProcessVerticalTop
        {
            get
            {
                return _msgProcessVerticalTop;
            }

            set
            {
                _msgProcessVerticalTop = value;
                OnPropertyChanged("MsgProcessVerticalTop");
            }
        }

        private string _msgProcessVerticalDown = GetVerticalDownProgressBarString();

        public string MsgProcessVerticalDown
        {
            get
            {
                return _msgProcessVerticalDown;
            }

            set
            {
                _msgProcessVerticalDown = value;
                OnPropertyChanged("MsgProcessVerticalDown");
            }
        }

        private string _msgProcessHorizontalLeft = GetHorizontalLeftProgressBarString();

        public string MsgProcessHorizontalLeft
        {
            get
            {
                return _msgProcessHorizontalLeft;
            }

            set
            {
                _msgProcessHorizontalLeft = value;
                OnPropertyChanged("MsgProcessHorizontalLeft");
            }
        }

        private string _msgProcessHorizontalRight = GetHorizontalRightProgressBarString();

        public string MsgProcessHorizontalRight
        {
            get
            {
                return _msgProcessHorizontalRight;
            }

            set
            {
                _msgProcessHorizontalRight = value;
                OnPropertyChanged("MsgProcessHorizontalRight");
            }
        }

        private double _d_Yaw = 0;

        public double D_Yaw
        {
            get
            {
                return _d_Yaw;
            }

            set
            {
                _d_Yaw = value;
                OnPropertyChanged("D_Yaw");
            }
        }

        private double _d_Roll = 0;

        public double D_Roll
        {
            get
            {
                return _d_Roll;
            }

            set
            {
                _d_Roll = value;
                OnPropertyChanged("D_Roll");
            }
        }

        private double _d_Pitch = 0;

        public double D_Pitch
        {
            get
            {
                return _d_Pitch;
            }

            set
            {
                _d_Pitch = value;
                OnPropertyChanged("D_Pitch");
            }
        }

        #endregion

        #region Main process

        private void CleanBuffAndSetToStep(int step)
        {
            buff = new List<double>();
            firstInProcess = true;
            processStep = step;
        }

        private void Wait2SecondsToReleaseProcess()
        {
            new Task(
                () =>
                {
                    Thread.Sleep(2000);
                    processIdel = true;
                }).Start();
        }

        private void ResetProgressBarString()
        {
            MsgProcessHorizontalLeft = GetHorizontalLeftProgressBarString();
            MsgProcessHorizontalRight = GetHorizontalRightProgressBarString();
            MsgProcessVerticalTop = GetVerticalTopProgressBarString();
            MsgProcessVerticalDown = GetVerticalDownProgressBarString();

            IndicateMsg = _indicateDefaultMsg;
        }

        private void Doprocess(HeadPose headPose)
        {
            processIdel = false;

            var pitch = headPose.Pitch;
            var roll = headPose.Roll;
            var yaw = headPose.Yaw;

            switch (processStep)
            {
                case 1:
                    if (firstInProcess)
                    {
                        firstInProcess = false;
                        Log("Step1: detect head pose up and down.");
                        IndicateMsg = "Please look Up and Down!";
                    }

                    StepOne(pitch);
                    break;
                case 2:
                    if (firstInProcess)
                    {
                        firstInProcess = false;
                        Log("Step2: detect head pose Left and Right.");
                        IndicateMsg = "Please look Left and Right!";
                    }

                    StepTwo(yaw);
                    break;
                case 3:
                    if (firstInProcess)
                    {
                        firstInProcess = false;
                        Log("Step3: detect head pose roll left and Right.");
                        IndicateMsg = "Please roll you face Left and Right!";
                    }

                    StepThree(roll);
                    break;
                default:
                    break;
            }
        }

        private void StepOne(double pitch)
        {
            buff.Add(pitch);
            if (buff.Count > activeFrames)
            {
                buff.RemoveAt(0);
            }

            var max = buff.Max();
            var min = buff.Min();

            var maxCorrection = max < 0 ? 0 : Convert.ToInt32(max);
            var minCorrection = min > 0 ? 0 : Convert.ToInt32(Math.Abs(min));

            MsgProcessVerticalTop = GetVerticalTopProgressBarString(maxCorrection);
            MsgProcessVerticalDown = GetVerticalDownProgressBarString(minCorrection);

            if (max > _headPitchMaxThreshold && min < _headPitchMinThreshold)
            {
                IndicateMsg = "Nodding Detected!";
                Log("Nodding Detected success.");
                CleanBuffAndSetToStep(2);
                Wait2SecondsToReleaseProcess();
            }
            else
            {
                processIdel = true;
            }
        }

        private void StepTwo(double yaw)
        {
            buff.Add(yaw);
            if (buff.Count > activeFrames)
            {
                buff.RemoveAt(0);
            }

            var max = buff.Max();
            var min = buff.Min();

            var maxCorrection = max < 0 ? 0 : Convert.ToInt32(max * 2);
            var minCorrection = min > 0 ? 0 : Convert.ToInt32(Math.Abs(min * 2));

            MsgProcessHorizontalLeft = GetHorizontalLeftProgressBarString(maxCorrection);
            MsgProcessHorizontalRight = GetHorizontalRightProgressBarString(minCorrection);

            if (min < _headYawMinThreshold && max > _headYawMaxThreshold)
            {
                CleanBuffAndSetToStep(3);
                IndicateMsg = "Shaking Detected!";
                Log("Shaking Detected success.");
                Wait2SecondsToReleaseProcess();
            }
            else
            {
                processIdel = true;
            }
        }

        private void StepThree(double roll)
        {
            buff.Add(roll);
            if (buff.Count > activeFrames)
            {
                buff.RemoveAt(0);
            }

            var max = buff.Max();
            var min = buff.Min();

            var maxCorrection = max < 0 ? 0 : Convert.ToInt32(max * 2);
            var minCorrection = min > 0 ? 0 : Convert.ToInt32(Math.Abs(min * 2));

            MsgProcessHorizontalLeft = GetHorizontalLeftProgressBarString(maxCorrection);
            MsgProcessHorizontalRight = GetHorizontalRightProgressBarString(minCorrection);

            if (min < _headRollMinThreshold && max > _headRollMaxThreshold)
            {
                StopProcess();
                IndicateMsg = "Rolling Detected!";
                Log("Rolling Detected success.");
                Log("All head pose detection finished.");
                Wait2SecondsToReleaseProcess();
            }
            else
            {
                processIdel = true;
            }
        }

        private void StartProcess()
        {
            startHeadPoseProcess = true;
        }

        private void StopProcess()
        {
            startHeadPoseProcess = false;
            CleanBuffAndSetToStep(1);
        }

        #endregion

        #region Draw ProgressBarString

        private static string GetHorizontalLeftProgressBarString(int leftFinish = 0, int totalCount = 40)
        {
            leftFinish = leftFinish < 0 ? 0 : leftFinish;
            leftFinish = leftFinish > totalCount ? totalCount : leftFinish;

            return new string('.', totalCount - leftFinish) + new string('|', leftFinish);
        }

        private static string GetHorizontalRightProgressBarString(int rightFinish = 0, int totalCount = 40)
        {
            rightFinish = rightFinish < 0 ? 0 : rightFinish;
            rightFinish = rightFinish > totalCount ? totalCount : rightFinish;

            return new string('|', rightFinish) + new string('.', totalCount - rightFinish);
        }

        private static string GetVerticalTopProgressBarString(int topFinish = 0, int topCount = 30)
        {
            topFinish = topFinish < 0 ? 0 : topFinish;
            topFinish = topFinish > topCount ? topCount : topFinish;

            return new string('.', topCount - topFinish) + new string('|', topFinish);
        }

        private static string GetVerticalDownProgressBarString(int downFinish = 0, int downCount = 15)
        {
            downFinish = downFinish < 0 ? 0 : downFinish;
            downFinish = downFinish > downCount ? downCount : downFinish;

            return new string('|', downFinish) + new string('.', downCount - downFinish);
        }

        #endregion

        #region Subscriptions IsolatedStorage methods

        /// <summary>
        /// Gets the subscription key from isolated storage.
        /// </summary>
        /// <returns></returns>
        private string GetSubscriptionKeyFromIsolatedStorage()
        {
            string subscriptionKey = null;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(
                IsolatedStorageScope.User | IsolatedStorageScope.Assembly,
                null,
                null))
            {
                try
                {
                    using (var iStream = new IsolatedStorageFileStream(
                        _isolatedStorageSubscriptionKeyFileName,
                        FileMode.Open,
                        isoStore))
                    {
                        using (var reader = new StreamReader(iStream))
                        {
                            subscriptionKey = reader.ReadLine();
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }

            if (string.IsNullOrEmpty(subscriptionKey))
            {
                subscriptionKey = _subscriptionKey;
            }

            return subscriptionKey;
        }

        /// <summary>
        /// Gets the subscription endpoint from isolated storage.
        /// </summary>
        /// <returns></returns>
        private string GetSubscriptionEndpointFromIsolatedStorage()
        {
            string subscriptionEndpoint = null;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(
                IsolatedStorageScope.User | IsolatedStorageScope.Assembly,
                null,
                null))
            {
                try
                {
                    using (var iStreamForEndpoint = new IsolatedStorageFileStream(
                        _isolatedStorageSubscriptionEndpointFileName,
                        FileMode.Open,
                        isoStore))
                    {
                        using (var readerForEndpoint = new StreamReader(iStreamForEndpoint))
                        {
                            subscriptionEndpoint = readerForEndpoint.ReadLine();
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }

            if (string.IsNullOrEmpty(subscriptionEndpoint))
            {
                subscriptionEndpoint = _subscriptionEndpoint;
            }

            return subscriptionEndpoint;
        }

        /// <summary>
        /// Saves the subscription key to isolated storage.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        private void SaveSubscriptionKeyToIsolatedStorage(string subscriptionKey)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(
                IsolatedStorageScope.User | IsolatedStorageScope.Assembly,
                null,
                null))
            {
                using (var oStream = new IsolatedStorageFileStream(
                    _isolatedStorageSubscriptionKeyFileName,
                    FileMode.Create,
                    isoStore))
                {
                    using (var writer = new StreamWriter(oStream))
                    {
                        writer.WriteLine(subscriptionKey);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the subscription endpoint to isolated storage.
        /// </summary>
        /// <param name="subscriptionEndpoint">The subscription endpoint.</param>
        private void SaveSubscriptionEndpointToIsolatedStorage(string subscriptionEndpoint)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(
                IsolatedStorageScope.User | IsolatedStorageScope.Assembly,
                null,
                null))
            {
                using (var oStream = new IsolatedStorageFileStream(
                    _isolatedStorageSubscriptionEndpointFileName,
                    FileMode.Create,
                    isoStore))
                {
                    using (var writer = new StreamWriter(oStream))
                    {
                        writer.WriteLine(subscriptionEndpoint);
                    }
                }
            }
        }

        #endregion

        #region UI button click methods

        private void Button_SaveSubscriptions(object sender, RoutedEventArgs e)
        {
            if (startHeadPoseProcess)
            {
                Log("Head pose detection is running, please stop it and save the subscriptions again.");
                return;
            }

            SaveSubscriptionKeyToIsolatedStorage(SubscriptionKey);
            SaveSubscriptionEndpointToIsolatedStorage(SubscriptionEndpoint);

            try
            {
                client =
                    new FaceClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
                    {
                        Endpoint =
                                SubscriptionEndpoint
                    };
                MessageBox.Show(
                    "Subscription key and endpoint are saved in your disk.\nYou do not need to paste the key next time.",
                    "Subscription Setting");
            }
            catch
            {
                MessageBox.Show("SubscriptionKey or SubscriptionEndpoint invalid.");
            }
        }

        private void Button_StopHeadPoseTest(object sender, RoutedEventArgs e)
        {
            Log("Stop head pose detection.");
            _grabber.AnalysisFunction = null;
            StopProcess();
        }

        private void Button_StartHeadPoseTest(object sender, RoutedEventArgs e)
        {
            if (startHeadPoseProcess)
            {
                Log("Head pose detection is running.");
                return;
            }

            Log("Start head pose detection.");

            CleanBuffAndSetToStep(1);
            if (_grabber.AnalysisFunction == null)
            {
                _grabber.AnalysisFunction = HeadPoseDemoFunction;
            }

            StartProcess();
        }

        #endregion
    }
}
