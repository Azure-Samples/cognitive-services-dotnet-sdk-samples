using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageAnalyze
{
    class Program
    {
        // subscriptionKey = "0123456789abcdef0123456789ABCDEF"
        private const string subscriptionKey = "0123456789abcdef0123456789ABCDEF";

        
        private const string remoteImageUrl =
            "https://github.com/harishkrishnav/cognitive-services-dotnet-sdk-samples/raw/master/ComputerVision/Images/sample3.png";

        // Specify the features to return
        private static readonly List<VisualFeatureTypes> features =
            new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
            VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
            VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
            VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
            VisualFeatureTypes.Objects
        };

        static void Main(string[] args)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(subscriptionKey),
                new System.Net.Http.DelegatingHandler[] { });
            
            // You must use the same region as you used to get your subscription
            // keys. For example, if you got your subscription keys from westus,
            // replace "westcentralus" with "westus".
            //
            // Free trial subscription keys are generated in the westcentralus
            // region. If you use a free trial subscription key, you shouldn't
            // need to change the region.

            // Specify the Azure region
            computerVision.Endpoint = "https://westus.api.cognitive.microsoft.com";



            // localImagePath = @"C:\Documents\LocalImage.jpg"
            string localImagePath = Directory.GetCurrentDirectory() + @"../../../../../../Images\sample5.png";
            
            Console.WriteLine("Images being analyzed ...");
            var t1 = AnalyzeRemoteAsync(computerVision, remoteImageUrl);
            var t2 = AnalyzeLocalAsync(computerVision, localImagePath);

            Task.WhenAll(t1, t2).Wait(5000);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        // Analyze a remote image
        private static async Task AnalyzeRemoteAsync(
            ComputerVisionClient computerVision, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine(
                    "\nInvalid remoteImageUrl:\n{0} \n", imageUrl);
                return;
            }

            ImageAnalysis analysis =
                await computerVision.AnalyzeImageAsync(imageUrl, features);
            Console.WriteLine(imageUrl);
            DisplayImageDescription(analysis);
            DisplayImageCategoryResults(analysis);
            DisplayTagResults(analysis);
            DisplayObjectDetectionResults(analysis);
            DisplayFaceResults(analysis);
            DisplayBrandDetectionResults(analysis);
            DisplayAdultResults(analysis);
            DisplayColorSchemeResults(analysis);
            DisplayDomainSpecificResults(analysis);
            DisplayImageTypeResults(analysis);
        }

        // Analyze a local image
        private static async Task AnalyzeLocalAsync(
            ComputerVisionClient computerVision, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine(
                    "\nUnable to open or read localImagePath:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                ImageAnalysis analysis = await computerVision.AnalyzeImageInStreamAsync(
                    imageStream, features);

                Console.WriteLine(imagePath);
                DisplayImageDescription(analysis);
                DisplayImageCategoryResults(analysis);
                DisplayTagResults(analysis);
                DisplayObjectDetectionResults(analysis);
                DisplayFaceResults(analysis);
                DisplayBrandDetectionResults(analysis);
                DisplayAdultResults(analysis);
                DisplayColorSchemeResults(analysis);
                DisplayDomainSpecificResults(analysis);
                DisplayImageTypeResults(analysis);

            }
        }

        private static void DisplayFaceResults(ImageAnalysis analysis)
        {
            //faces
            Console.WriteLine("faces:");
            foreach (var face in analysis.Faces)
            {
                Console.WriteLine("A {0} of age {1} at location {2},{3},{4},{5}", face.Gender, face.Age,
                    face.FaceRectangle.Left, face.FaceRectangle.Top,
                    face.FaceRectangle.Left + face.FaceRectangle.Width,
                    face.FaceRectangle.Top + face.FaceRectangle.Height);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayAdultResults(ImageAnalysis analysis)
        {
            //racy content
            Console.WriteLine("racy content:");
            Console.WriteLine("Adult score = {0} ({1}), racy score = {2} ({3})",
                analysis.Adult.AdultScore, analysis.Adult.IsAdultContent,
                analysis.Adult.RacyScore, analysis.Adult.IsRacyContent);
            Console.WriteLine("\n");
        }

        private static void DisplayTagResults(ImageAnalysis analysis)
        {
            //image tags
            Console.WriteLine("tags, confidence:");
            foreach (var tag in analysis.Tags)
            {
                Console.WriteLine("{0} ({1})", tag.Name, tag.Confidence);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayImageDescription(ImageAnalysis analysis)
        {
            //captioning
            Console.WriteLine("captions:");
            foreach (var caption in analysis.Description.Captions)
            {
                Console.WriteLine("{0} (confidence = {1})",
                    caption.Text, caption.Confidence);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayObjectDetectionResults(ImageAnalysis analysis)
        {
            //objects
            Console.WriteLine("objects:");
            foreach (var obj in analysis.Objects)
            {
                Console.WriteLine("{0} with confidence {1} at location {2},{3},{4},{5}",
                    obj.ObjectProperty, obj.Confidence,
                    obj.Rectangle.X, obj.Rectangle.X + obj.Rectangle.W,
                    obj.Rectangle.Y, obj.Rectangle.Y + obj.Rectangle.H);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayBrandDetectionResults(ImageAnalysis analysis)
        {
            //brands
            Console.WriteLine("brands:");
            foreach (var brand in analysis.Brands)
            {
                Console.WriteLine("Logo of {0} with confidence {1} at location {2},{3},{4},{5}",
                    brand.Name, brand.Confidence,
                    brand.Rectangle.X, brand.Rectangle.X + brand.Rectangle.W,
                    brand.Rectangle.Y, brand.Rectangle.Y + brand.Rectangle.H);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayDomainSpecificResults(ImageAnalysis analysis)
        {
            //celebrities
            Console.WriteLine("Celebrities:");
            foreach (var category in analysis.Categories)
            {
                if (category.Detail != null)
                {
                    if (category.Detail.Celebrities != null)
                    {
                        foreach (var celeb in category.Detail.Celebrities)
                        {
                            Console.WriteLine("Name: " + celeb.Name);
                            Console.WriteLine("Bounding box: {0}, {1}, {2}, {3}",
                                celeb.FaceRectangle.Left, celeb.FaceRectangle.Top,
                                celeb.FaceRectangle.Height, celeb.FaceRectangle.Width);
                            Console.WriteLine("Confidence:" + celeb.Confidence);
                        }
                    }
                }
            }

            //landmarks
            Console.WriteLine("Landmarks:");
            foreach (var category in analysis.Categories)
            {
                if (category.Detail != null)
                {
                    if (category.Detail.Landmarks != null)
                    {
                        foreach (var landmark in category.Detail.Landmarks)
                        {
                            Console.WriteLine("Name: " + landmark.Name);
                            Console.WriteLine("Confidence:" + landmark.Confidence);
                        }
                    }
                }
            }

            Console.WriteLine("\n");
        }

        private static void DisplayColorSchemeResults(ImageAnalysis analysis)
        {
            //color scheme
            Console.WriteLine("color:\n");
            Console.WriteLine("Is black and white?:" + analysis.Color.IsBWImg);
            Console.WriteLine("Accent color:" + analysis.Color.AccentColor);
            Console.WriteLine("Dominant background color:" + analysis.Color.DominantColorBackground);
            Console.WriteLine("Dominant foreground color:" + analysis.Color.DominantColorForeground);
            Console.WriteLine("dominant colors:");
            foreach (var color in analysis.Color.DominantColors)
            { Console.WriteLine(color); }
            Console.WriteLine("\n");
        }

        private static void DisplayImageCategoryResults(ImageAnalysis analysis)
        {
            //categorize
            Console.WriteLine("categories:\n");
            foreach (var category in analysis.Categories)
            {
                Console.WriteLine("{0} with confidence {1}",
                    category.Name, category.Score);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayImageTypeResults(ImageAnalysis analysis)
        {
            //image types
            Console.WriteLine("image type:\nClip art score:");

            if (analysis.ImageType.ClipArtType == 0)
            {
                Console.WriteLine("non clip-art");
            }
            else if (analysis.ImageType.ClipArtType == 1)
            {
                Console.WriteLine("ambiguous");
            }
            else if (analysis.ImageType.ClipArtType == 2)
            {
                Console.WriteLine("normal clip-art");
            }
            else if (analysis.ImageType.ClipArtType == 3)
            {
                Console.WriteLine("good clip-art");
            }

            if (analysis.ImageType.LineDrawingType == 0)
            {
                Console.WriteLine("Is not line drawing");
            }
            else if (analysis.ImageType.LineDrawingType == 0)
            {
                Console.WriteLine("Is line drawing");
            }
            Console.WriteLine("\n");

        }
    }
}