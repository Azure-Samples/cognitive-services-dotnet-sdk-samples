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

        private static FrameGrabber<LiveCameraResult> _grabber = null;
        private static readonly ImageEncodingParam[] s_jpegParams = {
            new ImageEncodingParam(ImwriteFlags.JpegQuality, 60)
        };

        private readonly string _isolatedStorageSubscriptionKeyFileName = "Subscription.txt";
        private readonly string _isolatedStorageSubscriptionEndpointFileName = "SubscriptionEndpoint.txt";

        private readonly double _HeadPitchMaxThreshold = 30;
        private readonly double _HeadPitchMinThreshold = -15;
        private readonly double _HeadYawMaxThreshold = 20;
        private readonly double _HeadYawMinThreshold = -20;
        private readonly double _HeadRollMaxThreshold = 20;
        private readonly double _HeadRollMinThreshold = -20;

        private readonly string _IndicateDefaultMsg = "Head pose test finished!";

        static IFaceClient client;

        static bool StartHeadPoseProcess = false;
        static bool ProcessIdel = true;
        static bool FirstInProcess = true;

        static int ProcessStep = 1;
        static List<double> buff = new List<double>();

        static int activeFrames = 14;

        #endregion

        #region Initialization

        private void Initialization()
        {
            SubscriptionKey = GetSubscriptionKeyFromIsolatedStorage();
            SubscriptionEndpoint = GetSubscriptionEndpointFromIsolatedStorage();

            try
            {
                client = new FaceClient(new ApiKeyServiceClientCredentials(SubscriptionKey)) { Endpoint = SubscriptionEndpoint };
                Log("FaceClient Initialization successed.");
            }
            catch(Exception e)
            {
                Log($"Exception:{e.Message}");
            }

            _grabber = new FrameGrabber<LiveCameraResult>();

            _grabber.NewFrameProvided += (s, e) =>
            {

                // The callback may occur on a different thread, so we must use the
                // MainWindow.Dispatcher when manipulating the UI. 
                ImageDisplay.Dispatcher.Invoke((Action)(() =>
                {
                    // Display the image in the left pane.
                    ImageDisplay.Source = e.Frame.Image.ToBitmapSource();
                }));
            };

            // Set up a listener for when the client receives a new result from an API call. 
            _grabber.NewResultAvailable += (s, e) =>
            {
                if (ProcessIdel)
                {
                    if (e == null)
                    {
                        return;
                    }

                    this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (e.Exception == null && !e.TimedOut)
                        {
                            if (e.Analysis.Faces != null && e.Analysis.Faces.Count() != 0)
                            {
                                var headpost = e.Analysis.Faces.FirstOrDefault().FaceAttributes.HeadPose;

                                if (StartHeadPoseProcess)
                                {
                                    doProcess(headpost);
                                }
                                else if(_IndicateDefaultMsg != IndicateMsg)
                                {
                                    ResetProgressBarString();
                                }

                                D_Yaw = headpost.Yaw;
                                D_Pitch = -headpost.Pitch;
                                D_Roll = -headpost.Roll;
                            }
                        }
                        else if (e.Exception != null)
                        {
                            Log($"Exception:{e.Exception.Message}");
                        }
                        else
                        {
                            Log($"Exception:{nameof(e.TimedOut)} is {e.TimedOut}");
                        }
                    }));
                }
            };

            _grabber.TriggerAnalysisOnInterval(TimeSpan.FromSeconds(0.3));
            int numCameras = _grabber.GetNumCameras();
            _grabber.StartProcessingCameraAsync(0).Wait();
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
            var attrs = new List<FaceAttributeType> {
                FaceAttributeType.HeadPose
            };

            var faces = await client.Face.DetectWithStreamAsync(jpg, returnFaceId: false, returnFaceAttributes: attrs);

            // Output. 
            return new LiveCameraResult { Faces = faces.ToArray() };
        }

        public void Log(string logMessage)
        {
            if (String.IsNullOrEmpty(logMessage) || logMessage == "\n")
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

        private string _SubscriptionKey = "Paste your subscription key";
        public string SubscriptionKey
        {
            get
            {
                return _SubscriptionKey;
            }
            set
            {
                _SubscriptionKey = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SubscriptionKey"));
            }
        }

        private string _SubscriptionEndpoint = "Paste your endpoint";
        public string SubscriptionEndpoint
        {
            get
            {
                return _SubscriptionEndpoint;
            }
            set
            {
                _SubscriptionEndpoint = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SubscriptionEndpoint"));
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
                PropertyChanged(this, new PropertyChangedEventArgs("IndicateMsg"));
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
                PropertyChanged(this, new PropertyChangedEventArgs("MsgProcessVerticalTop"));
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
                PropertyChanged(this, new PropertyChangedEventArgs("MsgProcessVerticalDown"));
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
                PropertyChanged(this, new PropertyChangedEventArgs("MsgProcessHorizontalLeft"));
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
                PropertyChanged(this, new PropertyChangedEventArgs("MsgProcessHorizontalRight"));
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
                PropertyChanged(this, new PropertyChangedEventArgs("D_Yaw"));
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
                PropertyChanged(this, new PropertyChangedEventArgs("D_Roll"));
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
                PropertyChanged(this, new PropertyChangedEventArgs("D_Pitch"));
            }
        }

        #endregion

        #region Main process

        private void CleanBuffAndSetToStep(int step)
        {
            buff = new List<double>();
            FirstInProcess = true;
            ProcessStep = step;
        }

        private void Wait2SecondsToReleaseProcess()
        {
            new Task(() =>
            {
                Thread.Sleep(2000);
                ProcessIdel = true;
            }).Start();
        }

        private void ResetProgressBarString()
        {
            MsgProcessHorizontalLeft = GetHorizontalLeftProgressBarString();
            MsgProcessHorizontalRight = GetHorizontalRightProgressBarString();
            MsgProcessVerticalTop = GetVerticalTopProgressBarString();
            MsgProcessVerticalDown = GetVerticalDownProgressBarString();

            IndicateMsg = _IndicateDefaultMsg;
        }

        private void doProcess(HeadPose headPose)
        {
            ProcessIdel = false;

            var pitch = headPose.Pitch;
            var roll = headPose.Roll;
            var yaw = headPose.Yaw;

            switch (ProcessStep)
            {
                case 1:
                    if (FirstInProcess)
                    {
                        FirstInProcess = false;
                        Log("Step1: detect headpose up and down.");
                        IndicateMsg = "Please look Up and Down!";
                    }
                    StepOne(pitch);
                    break;
                case 2:
                    if (FirstInProcess)
                    {
                        FirstInProcess = false;
                        Log("Step2: detect headpose Left and Right.");
                        IndicateMsg = "Please look Left and Right!";
                    }
                    StepTwo(yaw);
                    break;
                case 3:
                    if (FirstInProcess)
                    {
                        FirstInProcess = false;
                        Log("Step3: detect headpose roll left and Right.");
                        IndicateMsg = "Please roll you face Left and Right!";
                    }
                    StepThree(roll);
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
        }

        private void StepOne(double pitch)
        {
            buff.Add(pitch);
            if (buff.Count > activeFrames)
                buff.RemoveAt(0);

            var max = buff.Max();
            var min = buff.Min();
            var avg = buff.Average();

            var maxCorrection = max < 0 ? 0 : Convert.ToInt32(max);
            var minCorrection = min > 0 ? 0 : Convert.ToInt32(Math.Abs(min));

            MsgProcessVerticalTop = GetVerticalTopProgressBarString(maxCorrection);
            MsgProcessVerticalDown = GetVerticalDownProgressBarString(minCorrection);

            if (max > _HeadPitchMaxThreshold && min < _HeadPitchMinThreshold)
            {
                IndicateMsg = "Noding Detected!";
                Log("Noding Detected success.");
                CleanBuffAndSetToStep(2);
                Wait2SecondsToReleaseProcess();
            }
            else
            {
                ProcessIdel = true;
            }
        }

        private void StepTwo(double Yaw)
        {
            buff.Add(Yaw);
            if (buff.Count > activeFrames)
                buff.RemoveAt(0);

            var max = buff.Max();
            var min = buff.Min();
            var avg = buff.Average();
            
            var maxCorrection = max < 0 ? 0 : Convert.ToInt32(max * 2);
            var minCorrection = min > 0 ? 0 : Convert.ToInt32(Math.Abs(min * 2));

            MsgProcessHorizontalLeft = GetHorizontalLeftProgressBarString(maxCorrection);
            MsgProcessHorizontalRight = GetHorizontalRightProgressBarString(minCorrection);

            if (min < _HeadYawMinThreshold && max > _HeadYawMaxThreshold)
            {
                CleanBuffAndSetToStep(3);
                IndicateMsg = "Shaking Detected!";
                Log("Shaking Detected success.");
                Wait2SecondsToReleaseProcess();
            }
            else
            {
                ProcessIdel = true;
            }
        }

        private void StepThree(double Roll)
        {
            buff.Add(Roll);
            if (buff.Count > activeFrames)
                buff.RemoveAt(0);

            var max = buff.Max();
            var min = buff.Min();
            var avg = buff.Average();

            var maxCorrection = max < 0 ? 0 : Convert.ToInt32(max * 2);
            var minCorrection = min > 0 ? 0 : Convert.ToInt32(Math.Abs(min * 2));

            MsgProcessHorizontalLeft = GetHorizontalLeftProgressBarString(maxCorrection);
            MsgProcessHorizontalRight = GetHorizontalRightProgressBarString(minCorrection);

            if (min < _HeadRollMinThreshold && max > _HeadRollMaxThreshold)
            {
                StopProcess();
                IndicateMsg = "Rolling Detected!";
                Log("Rolling Detected success.");
                Log("All headpose detection finished.");
                Wait2SecondsToReleaseProcess();
            }
            else
            {
                ProcessIdel = true;
            }
        }

        private void StartProcess()
        {
            StartHeadPoseProcess = true;
        }

        private void StopProcess()
        {
            StartHeadPoseProcess = false;
            CleanBuffAndSetToStep(1);
        }

        #endregion

        #region Draw ProgressBarString

        private static string GetHorizontalLeftProgressBarString(int leftFinish = 0, int totalCount = 40)
        {
            leftFinish = leftFinish < 0 ? 0 : leftFinish;
            leftFinish = leftFinish > totalCount ? totalCount : leftFinish;

            return new String('.', totalCount - leftFinish) + new String('|', leftFinish);
        }

        private static string GetHorizontalRightProgressBarString(int rightFinish = 0, int totalCount = 40)
        {
            rightFinish = rightFinish < 0 ? 0 : rightFinish;
            rightFinish = rightFinish > totalCount ? totalCount : rightFinish;

            return new String('|', rightFinish) + new String('.', totalCount - rightFinish);
        }

        private static string GetVerticalTopProgressBarString(int topFinish = 0, int topCount = 30)
        {
            topFinish = topFinish < 0 ? 0 : topFinish;
            topFinish = topFinish > topCount ? topCount : topFinish;

            return new String('.', topCount - topFinish) + new String('|', topFinish);
        }

        private static string GetVerticalDownProgressBarString(int downFinish = 0, int downCount = 15)
        {
            downFinish = downFinish < 0 ? 0 : downFinish;
            downFinish = downFinish > downCount ? downCount : downFinish;

            return new String('|', downFinish) + new String('.', downCount - downFinish);
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

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                try
                {
                    using (var iStream = new IsolatedStorageFileStream(_isolatedStorageSubscriptionKeyFileName, FileMode.Open, isoStore))
                    {
                        using (var reader = new StreamReader(iStream))
                        {
                            subscriptionKey = reader.ReadLine();
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    subscriptionKey = null;
                }
            }
            if (string.IsNullOrEmpty(subscriptionKey))
            {
                subscriptionKey = _SubscriptionKey;
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

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                try
                {
                    using (var iStreamForEndpoint = new IsolatedStorageFileStream(_isolatedStorageSubscriptionEndpointFileName, FileMode.Open, isoStore))
                    {
                        using (var readerForEndpoint = new StreamReader(iStreamForEndpoint))
                        {
                            subscriptionEndpoint = readerForEndpoint.ReadLine();
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    subscriptionEndpoint = null;
                }
            }
            if (string.IsNullOrEmpty(subscriptionEndpoint))
            {
                subscriptionEndpoint = _SubscriptionEndpoint;
            }
            return subscriptionEndpoint;
        }


        /// <summary>
        /// Saves the subscription key to isolated storage.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        private void SaveSubscriptionKeyToIsolatedStorage(string subscriptionKey)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(_isolatedStorageSubscriptionKeyFileName, FileMode.Create, isoStore))
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
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(_isolatedStorageSubscriptionEndpointFileName, FileMode.Create, isoStore))
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
            if (StartHeadPoseProcess)
            {
                Log("Headpose detection is running, please stop it and save the subscriptions again.");
                return;
            }

            SaveSubscriptionKeyToIsolatedStorage(SubscriptionKey);
            SaveSubscriptionEndpointToIsolatedStorage(SubscriptionEndpoint);

            try
            {
                client = new FaceClient(new ApiKeyServiceClientCredentials(SubscriptionKey)) { Endpoint = SubscriptionEndpoint };
                MessageBox.Show("Subscription key and endpoint are saved in your disk.\nYou do not need to paste the key next time.", "Subscription Setting");
            }
            catch
            {
                MessageBox.Show("SubscriptionKey or SubscriptionEndpoint invalid.");
            }
        }

        private void Button_StopHeadPoseTest(object sender, RoutedEventArgs e)
        {
            Log("Stop headpose detection.");
            StopProcess();
        }

        private void Button_StartHeadPoseTest(object sender, RoutedEventArgs e)
        {
            if (StartHeadPoseProcess)
            {
                Log("Headpose detection is running.");
                return;
            }

            Log("Start headpose detection.");

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
