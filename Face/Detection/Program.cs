using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Detection
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Create a client.
            string apiKey = "ENTER YOUR KEY HERE";
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = "ENTER YOUR ENDPOINT HERE"
            };

            List<string> imageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Son1.jpg" };

            foreach (var imageFileName in imageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", imageFileName), FileMode.Open))
                {
                    // Detect faces with all attributes from image stream.
                    IList<DetectedFace> detectedFaces = client.Face.DetectWithStreamAsync(
                        stream,
                        false,
                        true,
                        new List<FaceAttributeType>()
                        {
                            FaceAttributeType.Accessories,
                            FaceAttributeType.Age,
                            FaceAttributeType.Blur,
                            FaceAttributeType.Emotion,
                            FaceAttributeType.Exposure,
                            FaceAttributeType.FacialHair,
                            FaceAttributeType.Gender,
                            FaceAttributeType.Glasses,
                            FaceAttributeType.Hair,
                            FaceAttributeType.HeadPose,
                            FaceAttributeType.Makeup,
                            FaceAttributeType.Noise,
                            FaceAttributeType.Occlusion,
                            FaceAttributeType.Smile
                        }
                    ).Result;

                    if (detectedFaces == null || detectedFaces.Count == 0)
                    {
                        Console.WriteLine($"[Error] No face detected from image `{imageFileName}`.");
                        return;
                    }

                    Console.WriteLine($"{detectedFaces.Count} faces detected from image `{imageFileName}`.");
                    if (detectedFaces[0].FaceAttributes == null)
                    {
                        Console.WriteLine("[Error] Parameter `returnFaceAttributes` of `DetectWithStreamAsync` must be set to get FaceAttributes purpose.");
                        return;
                    }

                    // all attributes of faces 
                    foreach (var face in detectedFaces)
                    {
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Rectangle(Left/Top/Width/Height) : {face.FaceRectangle.Left} {face.FaceRectangle.Top} {face.FaceRectangle.Width} {face.FaceRectangle.Height}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Accessories : {GetAccessories(face.FaceAttributes.Accessories)}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Age : {face.FaceAttributes.Age}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Blur : {face.FaceAttributes.Blur.BlurLevel}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Emotion : {face.FaceAttributes.Emotion}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Exposure : {face.FaceAttributes.Exposure.ExposureLevel}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   FacialHair : {string.Format("{0}", face.FaceAttributes.FacialHair.Moustache + face.FaceAttributes.FacialHair.Beard + face.FaceAttributes.FacialHair.Sideburns > 0 ? "Yes" : "No")}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Gender : {face.FaceAttributes.Gender}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Glasses : {face.FaceAttributes.Glasses}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Hair : {GetHair(face.FaceAttributes.Hair)}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   HeadPose : {string.Format("Pitch: {0}, Roll: {1}, Yaw: {2}", Math.Round(face.FaceAttributes.HeadPose.Pitch, 2), Math.Round(face.FaceAttributes.HeadPose.Roll, 2), Math.Round(face.FaceAttributes.HeadPose.Yaw, 2))}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Makeup : {string.Format("{0}", ((face.FaceAttributes.Makeup.EyeMakeup || face.FaceAttributes.Makeup.LipMakeup) ? "Yes" : "No"))}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Noise : {face.FaceAttributes.Noise.NoiseLevel}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Occlusion : {string.Format("EyeOccluded: {0}", ((face.FaceAttributes.Occlusion.EyeOccluded) ? "Yes" : "No"))}   {string.Format("ForeheadOccluded: {0}", ((face.FaceAttributes.Occlusion.ForeheadOccluded) ? "Yes" : "No"))}   {string.Format("MouthOccluded: {0}", ((face.FaceAttributes.Occlusion.MouthOccluded) ? "Yes" : "No"))}");
                        Console.WriteLine($"FaceAttribute from {imageFileName}   Smile : {face.FaceAttributes.Smile}");
                        Console.WriteLine();
                    }
                }
            }
            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }

        private static string GetAccessories(IList<Accessory> accessories)
        {
            if (accessories.Count == 0)
            {
                return "NoAccessories";
            }

            string[] accessoryArray = new string[accessories.Count];

            for (int i = 0; i < accessories.Count; ++i)
            {
                accessoryArray[i] = accessories[i].Type.ToString();
            }

            return "Accessories: " + String.Join(",", accessoryArray);
        }
        private static string GetHair(Hair hair)
        {
            if (hair.HairColor.Count == 0)
            {
                if (hair.Invisible)
                    return "Invisible";
                else
                    return "Bald";
            }
            else
            {
                HairColorType returnColor = HairColorType.Unknown;
                double maxConfidence = 0.0f;

                for (int i = 0; i < hair.HairColor.Count; ++i)
                {
                    if (hair.HairColor[i].Confidence > maxConfidence)
                    {
                        maxConfidence = hair.HairColor[i].Confidence;
                        returnColor = hair.HairColor[i].Color;
                    }
                }

                return returnColor.ToString();
            }
        }
    }
}
