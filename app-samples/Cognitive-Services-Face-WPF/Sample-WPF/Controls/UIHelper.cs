﻿//
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace Microsoft.ProjectOxford.Face.Controls
{
    /// <summary>
    /// UI helper functions
    /// </summary>
    internal static class UIHelper
    {
        #region Methods

        /// <summary>
        /// Rotate image by its orientation
        /// </summary>
        /// <param name="imagePath">image path</param>
        /// <returns>image for rendering</returns>
        public static BitmapImage LoadImageAppliedOrientation(string imagePath)
        {
            var im = new BitmapImage();
            im.BeginInit();
            im.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            im.Rotation = GetImageOrientation(imagePath);
            im.EndInit();
            return im;
        }

        /// <summary>
        /// Get image orientation flag.
        /// </summary>
        /// <param name="imagePath">image path</param>
        /// <returns></returns>
        public static Rotation GetImageOrientation(string imagePath)
        {
            using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                // See WIC Photo metadata policies for orientation query 
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ee872007(v=vs.85).aspx
                const string query = "System.Photo.Orientation";
                var metadata = (BitmapMetadata)(BitmapFrame.Create(fs).Metadata);

                if (metadata != null && metadata.ContainsQuery(query))
                {
                    var orientationFlag = metadata.GetQuery(query);
                    if (orientationFlag != null)
                    {
                        switch ((ushort)orientationFlag)
                        {
                            case 6:
                                return Rotation.Rotate90;
                            case 3:
                                return Rotation.Rotate180;
                            case 8:
                                return Rotation.Rotate270;
                        }
                    }
                }
            }
            return Rotation.Rotate0;
        }

        /// <summary>
        /// Calculate the rendering face rectangle
        /// </summary>
        /// <param name="faces">Detected face from service</param>
        /// <param name="maxSize">Image rendering size</param>
        /// <param name="imageInfo">Image width and height</param>
        /// <returns>Face structure for rendering</returns>
        public static IEnumerable<Face> CalculateFaceRectangleForRendering(IList<DetectedFace> faces, int maxSize, Tuple<int, int> imageInfo)
        {
            var imageWidth = imageInfo.Item1;
            var imageHeight = imageInfo.Item2;
            var ratio = (float)imageWidth / imageHeight;
            int uiWidth = 0;
            int uiHeight = 0;
            if (ratio > 1.0)
            {
                uiWidth = maxSize;
                uiHeight = (int)(maxSize / ratio);
            }
            else
            {
                uiHeight = maxSize;
                uiWidth = (int)(ratio * uiHeight);
            }

            var uiXOffset = (maxSize - uiWidth) / 2;
            var uiYOffset = (maxSize - uiHeight) / 2;
            var scale = (float)uiWidth / imageWidth;

            foreach (var face in faces)
            {
                var left = (int)(face.FaceRectangle.Left * scale + uiXOffset);
                var top = (int)(face.FaceRectangle.Top * scale + uiYOffset);

                // Angle of face rectangles, default value is 0 (not rotated).
                double faceAngle = 0;

                // If head pose attributes have been obtained, re-calculate the left & top (X & Y) positions.
                if (face.FaceAttributes?.HeadPose != null)
                {
                    // Head pose's roll value acts directly as the face angle.
                    faceAngle = face.FaceAttributes.HeadPose.Roll;
                    var angleToPi = Math.Abs((faceAngle / 180) * Math.PI);

                    // _____       | / \ |
                    // |____|  =>  |/   /|
                    //             | \ / |
                    // Re-calculate the face rectangle's left & top (X & Y) positions.
                    var newLeft = face.FaceRectangle.Left +
                        face.FaceRectangle.Width / 2 -
                        (face.FaceRectangle.Width * Math.Sin(angleToPi) + face.FaceRectangle.Height * Math.Cos(angleToPi)) / 2;

                    var newTop = face.FaceRectangle.Top +
                        face.FaceRectangle.Height / 2 -
                        (face.FaceRectangle.Height * Math.Sin(angleToPi) + face.FaceRectangle.Width * Math.Cos(angleToPi)) / 2;

                    left = (int)(newLeft * scale + uiXOffset);
                    top = (int)(newTop * scale + uiYOffset);
                }

                yield return new Face()
                {
                    FaceId = face.FaceId?.ToString(),
                    Left = left,
                    Top = top,
                    OriginalLeft = (int)(face.FaceRectangle.Left * scale + uiXOffset),
                    OriginalTop = (int)(face.FaceRectangle.Top * scale + uiYOffset),
                    Height = (int)(face.FaceRectangle.Height * scale),
                    Width = (int)(face.FaceRectangle.Width * scale),
                    FaceAngle = faceAngle,
                };
            }
        }

        /// <summary>
        /// Get image basic information for further rendering usage
        /// </summary>
        /// <param name="imageFile">image file</param>
        /// <returns>Image width and height</returns>
        public static Tuple<int, int> GetImageInfoForRendering(BitmapImage imageFile)
        {
            try
            {
                return new Tuple<int, int>(imageFile.PixelWidth, imageFile.PixelHeight);
            }
            catch
            {
                return new Tuple<int, int>(0, 0);
            }
        }

        /// <summary>
        /// Append detected face to UI binding collection
        /// </summary>
        /// <param name="collections">UI binding collection</param>
        /// <param name="imagePath">Original image path, used for rendering face region</param>
        /// <param name="face">Face structure returned from service</param>
        public static void UpdateFace(ObservableCollection<Face> collections, string imagePath, PersistedFace face)
        {
            var renderingImage = LoadImageAppliedOrientation(imagePath);
            collections.Add(new Face()
            {
                ImageFile = renderingImage,
                FaceId = face.PersistedFaceId.ToString(),
            });
        }

        /// <summary>
        /// Append detected face to UI binding collection
        /// </summary>
        /// <param name="collections">UI binding collection</param>
        /// <param name="imagePath">Original image path, used for rendering face region</param>
        /// <param name="face">Face structure returned from service</param>
        public static void UpdateFace(ObservableCollection<Face> collections, string imagePath, DetectedFace face)
        {
            var renderingImage = LoadImageAppliedOrientation(imagePath);
            collections.Add(new Face()
            {
                ImageFile = renderingImage,
                Left = face.FaceRectangle.Left,
                Top = face.FaceRectangle.Top,
                Width = face.FaceRectangle.Width,
                Height = face.FaceRectangle.Height,
                FaceId = face.FaceId.ToString(),
            });
        }

        /// <summary>
        /// Logging helper function
        /// </summary>
        /// <param name="log">log output instance</param>
        /// <param name="newMessage">message to append</param>
        /// <returns>log string</returns>
        public static string AppendLine(this string log, string newMessage)
        {
            return string.Format("{0}[{3}]: {2}{1}", log, Environment.NewLine, newMessage, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
        }

        #endregion Methods
    }

    public static class ControlHelper
    {
        public static readonly DependencyProperty PassMouseWheelToParentProperty =
         DependencyProperty.RegisterAttached("PassMouseWheelToParent", typeof(bool), typeof(UIElement),
         new PropertyMetadata((bool)false));

        public static void SetPassMouseWheelToParent(UIElement obj, bool value)
        {
            obj.SetValue(PassMouseWheelToParentProperty, value);
            if (value)
            {
                obj.PreviewMouseWheel += Obj_PreviewMouseWheel;
            }
            else
            {
                obj.PreviewMouseWheel -= Obj_PreviewMouseWheel;
            }
        }

        public static bool GetPassMouseWheelToParent(UIElement obj)
        {
            return (bool)obj.GetValue(PassMouseWheelToParentProperty);
        }

        private static void Obj_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = FindVisualParent<UIElement>((DependencyObject)sender);
                if (parent != null)
                    parent.RaiseEvent(eventArg);
            }
        }

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null reference is being returned.</returns>
        public static T FindVisualParent<T>(DependencyObject child)
          where T : DependencyObject
        {
            // get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                // use recursion to proceed with next level
                return FindVisualParent<T>(parentObject);
            }
        }
    }
}