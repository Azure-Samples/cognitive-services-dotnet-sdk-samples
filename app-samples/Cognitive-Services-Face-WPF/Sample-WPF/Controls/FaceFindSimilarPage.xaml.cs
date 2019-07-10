//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
//
// Microsoft Cognitive Services (formerly Project Oxford) GitHub:
// https://github.com/Microsoft/Cognitive-Face-Windows
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Microsoft.ProjectOxford.Face.Controls
{
    /// <summary>
    /// Interaction logic for FaceFindSimilar.xaml
    /// </summary>
    public partial class FaceFindSimilarPage : Page, INotifyPropertyChanged
    {

        #region Fields

        /// <summary>
        /// Description dependency property
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(FaceFindSimilarPage));

        /// <summary>
        /// RecognitionModel for Face detection and LargeFaceList
        /// </summary>
        private static readonly string recognitionModel = RecognitionModel.Recognition02;

        /// <summary>
        /// Faces collection which will be used to find similar from
        /// </summary>
        private ObservableCollection<Face> _facesCollection = new ObservableCollection<Face>();

        /// <summary>
        /// Find personal match mode similar results
        /// </summary>
        private ObservableCollection<FindSimilarResult> _findSimilarMatchPersonCollection = new ObservableCollection<FindSimilarResult>();

        /// <summary>
        /// Find facial match mode similar results  
        /// </summary>
        private ObservableCollection<FindSimilarResult> _findSimilarMatchCollection = new ObservableCollection<FindSimilarResult>();

        /// <summary>
        /// User picked image file
        /// </summary>
        private ImageSource _selectedFile;

        /// <summary>
        /// Query faces
        /// </summary>
        private ObservableCollection<Face> _targetFaces = new ObservableCollection<Face>();

        /// <summary>
        /// max concurrent process number for client query.
        /// </summary>
        private int _maxConcurrentProcesses;

        /// <summary>
        /// Temporary stored large face list name
        /// </summary>
        private string _largeFaceListName = Guid.NewGuid().ToString();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceFindSimilarPage" /> class
        /// </summary>
        public FaceFindSimilarPage()
        {
            InitializeComponent();
            _maxConcurrentProcesses = 4;
        }
        #endregion Constructors

        #region Events

        /// <summary>
        /// Implement INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets description
        /// </summary>
        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }

            set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        /// <summary>
        /// Gets faces collection which will be used to find similar from 
        /// </summary>
        public ObservableCollection<Face> FacesCollection
        {
            get
            {
                return _facesCollection;
            }
        }

        /// <summary>
        /// Gets find "matchFace" mode similar results
        /// </summary>
        public ObservableCollection<FindSimilarResult> FindSimilarMatchFaceCollection
        {
            get
            {
                return _findSimilarMatchCollection;
            }
        }
        
        /// <summary>
        /// Gets find "matchPerson" mode similar results
        /// </summary>
        public ObservableCollection<FindSimilarResult> FindSimilarMatchPersonCollection
        {
            get
            {
                return _findSimilarMatchPersonCollection;
            }
        }

        /// <summary>
        /// Gets constant maximum image size for rendering detection result
        /// </summary>
        public int MaxImageSize
        {
            get
            {
                return 300;
            }
        }

        /// <summary>
        /// Gets or sets user picked image file
        /// </summary>
        public ImageSource SelectedFile
        {
            get
            {
                return _selectedFile;
            }

            set
            {
                _selectedFile = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedFile"));
                }
            }
        }

        /// <summary>
        /// Gets query faces
        /// </summary>
        public ObservableCollection<Face> TargetFaces
        {
            get
            {
                return _targetFaces;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Pick image and call find similar with both two modes for each faces detected
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private async void FindSimilar_Click(object sender, RoutedEventArgs e)
        {
            // Show file picker
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Image files (*.jpg, *.png, *.bmp, *.gif) | *.jpg; *.png; *.bmp; *.gif";
            var filePicker = dlg.ShowDialog();

            if (filePicker.HasValue && filePicker.Value)
            {
                // User picked image
                // Clear previous detection and find similar results
                TargetFaces.Clear();
                FindSimilarMatchPersonCollection.Clear();
                FindSimilarMatchFaceCollection.Clear();
                var sw = Stopwatch.StartNew();

                var pickedImagePath = dlg.FileName;
                var renderingImage = UIHelper.LoadImageAppliedOrientation(pickedImagePath);
                var imageInfo = UIHelper.GetImageInfoForRendering(renderingImage);
                SelectedFile = renderingImage;

                // Detect all faces in the picked image
                using (var fStream = File.OpenRead(pickedImagePath))
                {
                    MainWindow.Log("Request: Detecting faces in {0}", SelectedFile);
                    var faceServiceClient = FaceServiceClientHelper.GetInstance(this);
                    IList<DetectedFace> faces = await faceServiceClient.Face.DetectWithStreamAsync(fStream, recognitionModel: recognitionModel);

                    // Update detected faces on UI
                    foreach (var face in UIHelper.CalculateFaceRectangleForRendering(faces, MaxImageSize, imageInfo))
                    {
                        TargetFaces.Add(face);
                    }

                    MainWindow.Log("Response: Success. Detected {0} face(s) in {1}", faces.Count, SelectedFile);

                    // Find two modes similar faces for each face
                    foreach (var f in faces)
                    {
                        if (f.FaceId == null)
                        {
                            continue;
                        }

                        var faceId = f.FaceId.Value;

                        MainWindow.Log("Request: Finding similar faces in Personal Match Mode for face {0}", faceId);

                        try
                        {
                            // Default mode, call find matchPerson similar REST API, the result contains all the face ids which is personal similar to the query face
                            const int requestCandidatesCount = 4;
                            IList<SimilarFace> result = await faceServiceClient.Face.FindSimilarAsync(
                                faceId,
                                null,
                                _largeFaceListName,
                                maxNumOfCandidatesReturned: requestCandidatesCount);

                            // Update find matchPerson similar results collection for rendering
                            var personSimilarResult = new FindSimilarResult();
                            personSimilarResult.Faces = new ObservableCollection<Face>();
                            personSimilarResult.QueryFace = new Face()
                            {
                                ImageFile = SelectedFile,
                                Top = f.FaceRectangle.Top,
                                Left = f.FaceRectangle.Left,
                                Width = f.FaceRectangle.Width,
                                Height = f.FaceRectangle.Height,
                                FaceId = faceId.ToString(),
                            };
                            foreach (var fr in result)
                            {
                                var candidateFace = FacesCollection.First(ff => ff.FaceId == fr.PersistedFaceId.ToString());
                                Face newFace = new Face();
                                newFace.ImageFile = candidateFace.ImageFile;
                                newFace.Confidence = fr.Confidence;
                                newFace.FaceId = candidateFace.FaceId;
                                personSimilarResult.Faces.Add(newFace);
                            }

                            MainWindow.Log("Response: Found {0} similar faces for face {1}", personSimilarResult.Faces.Count, faceId);

                            FindSimilarMatchPersonCollection.Add(personSimilarResult);
                        }
                        catch (APIErrorException ex)
                        {
                            MainWindow.Log("Response: {0}. {1}", ex.Body.Error.Code, ex.Body.Error.Message);
                        }

                        try
                        {
                            // Call find facial match similar REST API, the result faces the top N with the highest similar confidence 
                            const int requestCandidatesCount = 4;
                            var result = await faceServiceClient.Face.FindSimilarAsync(
                                faceId,
                                null,
                                _largeFaceListName,
                                maxNumOfCandidatesReturned: requestCandidatesCount,
                                mode: FindSimilarMatchMode.MatchFace);

                            // Update "matchFace" similar results collection for rendering
                            var faceSimilarResults = new FindSimilarResult();
                            faceSimilarResults.Faces = new ObservableCollection<Face>();
                            faceSimilarResults.QueryFace = new Face()
                            {
                                ImageFile = SelectedFile,
                                Top = f.FaceRectangle.Top,
                                Left = f.FaceRectangle.Left,
                                Width = f.FaceRectangle.Width,
                                Height = f.FaceRectangle.Height,
                                FaceId = faceId.ToString(),
                            };
                            foreach (var fr in result)
                            {
                                var candidateFace = FacesCollection.First(ff => ff.FaceId == fr.PersistedFaceId.ToString());
                                Face newFace = new Face();
                                newFace.ImageFile = candidateFace.ImageFile;
                                newFace.Confidence = fr.Confidence;
                                newFace.FaceId = candidateFace.FaceId;
                                faceSimilarResults.Faces.Add(newFace);
                            }

                            MainWindow.Log("Response: Found {0} similar faces for face {1}", faceSimilarResults.Faces.Count, faceId);

                            FindSimilarMatchFaceCollection.Add(faceSimilarResults);
                        }
                        catch (APIErrorException ex)
                        {
                            MainWindow.Log("Response: {0}. {1}", ex.Body.Error.Code, ex.Body.Error.Message);
                        }
                    }
                }
            }
            GC.Collect();
        }

        /// <summary>
        /// Pick image folder and detect all faces in these images
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private async void FolderPicker_Click(object sender, RoutedEventArgs e)
        {
            bool groupExists = false;

            var faceServiceClient = FaceServiceClientHelper.GetInstance(this);
            try
            {
                MainWindow.Log("Request: Large Face List {0} will be used to build a person database. Checking whether the large face list exists.", _largeFaceListName);

                await faceServiceClient.LargeFaceList.GetAsync(_largeFaceListName);
                groupExists = true;
                MainWindow.Log("Response: Large Face List {0} exists.", _largeFaceListName);
            }
            catch (APIErrorException ex)
            {
                if (ex.Body.Error.Code != "LargeFaceListNotFound")
                {
                    MainWindow.Log("Response: {0}. {1}", ex.Body.Error.Code, ex.Body.Error.Message);
                    return;
                }
                else
                {
                    MainWindow.Log("Response: Large Face List {0} did not exist previously.", _largeFaceListName);
                }
            }

            if (groupExists)
            {
                var cleanFaceList = System.Windows.MessageBox.Show(string.Format("Requires a clean up for large face list \"{0}\" before setting up a new large face list. Click OK to proceed, large face list \"{0}\" will be cleared.", _largeFaceListName), "Warning", MessageBoxButton.OKCancel);
                if (cleanFaceList == MessageBoxResult.OK)
                {
                    await faceServiceClient.LargeFaceList.DeleteAsync(_largeFaceListName);
                }
                else
                {
                    return;
                }
            }

            OpenFaceButton.IsEnabled = false;
            // Show folder picker
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = dlg.ShowDialog();

            bool forceContinue = false;

            
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Enumerate all ".jpg" files in the folder, call detect
                List<Task> tasks = new List<Task>();

                FacesCollection.Clear();
                TargetFaces.Clear();
                FindSimilarMatchPersonCollection.Clear();
                FindSimilarMatchFaceCollection.Clear();
                SelectedFile = null;

                // Set the suggestion count is intent to minimum the data preparation step only,
                // it's not corresponding to service side constraint
                const int SuggestionCount = 10;
                int processCount = 0;

                MainWindow.Log("Request: Preparing, detecting faces in chosen folder.");

                await faceServiceClient.LargeFaceList.CreateAsync(_largeFaceListName, _largeFaceListName, "large face list for sample", recognitionModel);

                var imageList =
                    new ConcurrentBag<string>(
                        Directory.EnumerateFiles(dlg.SelectedPath, "*.*", SearchOption.AllDirectories)
                            .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".bmp") || s.ToLower().EndsWith(".gif")));
                
                string img;
                int invalidImageCount = 0;
                while (imageList.TryTake(out img))
                {
                    tasks.Add(Task.Factory.StartNew(
                        async (obj) =>
                        {
                            var imgPath = obj as string;                            
                            // Call detection
                            using (var fStream = File.OpenRead(imgPath))
                            {
                                try
                                {
                                    var faces =
                                        await faceServiceClient.LargeFaceList.AddFaceFromStreamAsync(_largeFaceListName, fStream);
                                    return new Tuple<string, PersistedFace>(imgPath, faces);
                                }
                                catch (APIErrorException ex)
                                {
                                    // if operation conflict, retry.
                                    if (ex.Body.Error.Code.Equals("ConcurrentOperationConflict"))
                                    {
                                        imageList.Add(imgPath);
                                        return null;
                                    }
                                    // if operation cause rate limit exceed, retry.
                                    else if (ex.Body.Error.Code.Equals("RateLimitExceeded"))
                                    {
                                        imageList.Add(imgPath);
                                        return null;
                                    }
                                    else if (ex.Body.Error.Message.Contains("more than 1 face in the image."))
                                    {
                                        Interlocked.Increment(ref invalidImageCount);
                                    }
                                    // Here we simply ignore all detection failure in this sample
                                    // You may handle these exceptions by check the Error.Error.Code and Error.Message property for ClientException object
                                    return new Tuple<string, PersistedFace>(imgPath, null);
                                }
                            }
                        },
                        img).Unwrap().ContinueWith((detectTask) =>
                        {
                            var res = detectTask?.Result;
                            if (res?.Item2 == null)
                            {
                                return;
                            }

                            // Update detected faces on UI
                            this.Dispatcher.Invoke(
                            new Action
                                <ObservableCollection<Face>, string, PersistedFace>(
                                UIHelper.UpdateFace),
                            FacesCollection,
                            res.Item1,
                            res.Item2);
                        }));

                    processCount++;

                    if (processCount >= SuggestionCount && !forceContinue)
                    {
                        var continueProcess =
                            System.Windows.Forms.MessageBox.Show(
                                "The images loaded have reached the recommended count, may take long time if proceed. Would you like to continue to load images?",
                                "Warning", System.Windows.Forms.MessageBoxButtons.YesNo);
                        if (continueProcess == System.Windows.Forms.DialogResult.Yes)
                        {
                            forceContinue = true;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (tasks.Count >= _maxConcurrentProcesses || imageList.IsEmpty)
                    {
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                    }

                }
                if (invalidImageCount > 0)
                {
                    MainWindow.Log("Warning: more or less than one face is detected in {0} images, can not add to large face list.", invalidImageCount);
                }
                MainWindow.Log("Response: Success. Total {0} faces are detected.", FacesCollection.Count);

                try
                {
                    // Start to train the large face list.
                    MainWindow.Log("Request: Training large face list \"{0}\"", _largeFaceListName);
                    await faceServiceClient.LargeFaceList.TrainAsync(_largeFaceListName);

                    // Wait until the training is completed.
                    while (true)
                    {
                        await Task.Delay(1000);
                        var trainingStatus = await faceServiceClient.LargeFaceList.GetTrainingStatusAsync(_largeFaceListName);
                        MainWindow.Log("Response: {0}. Large face list \"{1}\" training process is {2}", "Success", _largeFaceListName, trainingStatus.Status);
                        if (trainingStatus.Status != TrainingStatusType.Running)
                        {
                            if (trainingStatus.Status == TrainingStatusType.Failed)
                            {
                                MainWindow.Log("Response: Training failed with message {0}.", trainingStatus.Message);
                            }

                            break;
                        }
                    }
                    OpenFaceButton.IsEnabled = true;
                }
                catch (APIErrorException ex)
                {
                    MainWindow.Log("Response: {0}. {1}", ex.Body.Error.Code, ex.Body.Error.Message);
                }
            }

            GC.Collect();
        }
        
        #endregion Methods

        #region Nested Types

        /// <summary>
        /// Find similar result for UI binding
        /// </summary>
        public class FindSimilarResult : INotifyPropertyChanged
        {
            #region Fields

            /// <summary>
            /// Similar faces collection
            /// </summary>
            private ObservableCollection<Face> _faces;
            
            /// <summary>
            /// Query face
            /// </summary>
            private Face _queryFace;

            #endregion Fields

            #region Events

            /// <summary>
            /// Implement INotifyPropertyChanged interface
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion Events

            #region Properties
            
            /// <summary>
            /// Gets or sets similar faces collection
            /// </summary>
            public ObservableCollection<Face> Faces
            {
                get
                {
                    return _faces;
                }

                set
                {
                    _faces = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Faces"));
                    }
                }
            }
            
            /// <summary>
            /// Gets or sets query face
            /// </summary>
            public Face QueryFace
            {
                get
                {
                    return _queryFace;
                }

                set
                {
                    _queryFace = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("QueryFace"));
                    }
                }
            }

            #endregion Properties
        }

        #endregion Nested Types        
    }
}