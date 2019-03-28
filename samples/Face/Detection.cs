namespace Microsoft.Azure.CognitiveServices.Samples.Face
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

    public static class Detection
    {
        public static async Task Run(string endpoint, string key)
        {
            Console.WriteLine("Sample of face detection.");

            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
            string recognitionModel = RecognitionModel.Recognition02;

            const string ImageUrlPrefix = "https://csdx.blob.core.windows.net/resources/Face/Images/";
            List<string> imageFileNames = new List<string>
                                              {
                                                  "detection1.jpg",
                                                  "detection2.jpg",
                                                  "detection3.jpg",
                                                  "detection4.jpg",
                                                  "detection5.jpg",
                                                  "detection6.jpg"
                                              };

            foreach (var imageFileName in imageFileNames)
            {
                // Detect faces with all attributes from image url.
                IList<DetectedFace> detectedFaces = await client.Face.DetectWithUrlAsync(
                                                        $"{ImageUrlPrefix}{imageFileName}",
                                                        false,
                                                        true,
                                                        new List<FaceAttributeType>
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
                                                            },
                                                        recognitionModel: recognitionModel);

                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    throw new Exception($"No face detected from image `{imageFileName}`.");
                }

                Console.WriteLine($"{detectedFaces.Count} faces detected from image `{imageFileName}`.");
                if (detectedFaces[0].FaceAttributes == null)
                {
                    throw new Exception(
                        "Parameter `returnFaceAttributes` of `DetectWithStreamAsync` must be set to get face attributes.");
                }

                // Parse and print all attributes of each detected face.
                foreach (var face in detectedFaces)
                {
                    Console.WriteLine(
                        $"Face attributes of {imageFileName}   Rectangle(Left/Top/Width/Height) : {face.FaceRectangle.Left} {face.FaceRectangle.Top} {face.FaceRectangle.Width} {face.FaceRectangle.Height}");
                    Console.WriteLine(
                        $"Face attributes of {imageFileName}   Accessories : {GetAccessories(face.FaceAttributes.Accessories)}");
                    Console.WriteLine($"Face attributes of {imageFileName}   Age : {face.FaceAttributes.Age}");
                    Console.WriteLine(
                        $"Face attributes of {imageFileName}   Blur : {face.FaceAttributes.Blur.BlurLevel}");
                    Console.WriteLine(
                        $"Face attributes of {imageFileName}   Emotion : {GetEmotion(face.FaceAttributes.Emotion)}");
                    Console.WriteLine(
                        $"Face attributes of {imageFileName}   Exposure : {face.FaceAttributes.Exposure.ExposureLevel}");
                    Console.WriteLine(
                        $"Face attributes of {imageFileName}   FacialHair : {string.Format("{0}", face.FaceAttributes.FacialHair.Moustache + face.FaceAttributes.FacialHair.Beard + face.FaceAttributes.FacialHair.Sideburns > 0 ? "Yes" : "No")}");
                    Console.WriteLine($"Face attributes of {imageFileName}   Gender : {face.FaceAttributes.Gender}");
                    Console.WriteLine($"Face attributes of {imageFileName}   Glasses : {face.FaceAttributes.Glasses}");
                    Console.WriteLine(
                        $"Face attributes of {imageFileName}   Hair : {GetHair(face.FaceAttributes.Hair)}");
                    Console.WriteLine(
                        $"Face attributes of {imageFileName}   HeadPose : {string.Format("Pitch: {0}, Roll: {1}, Yaw: {2}", Math.Round(face.FaceAttributes.HeadPose.Pitch, 2), Math.Round(face.FaceAttributes.HeadPose.Roll, 2), Math.Round(face.FaceAttributes.HeadPose.Yaw, 2))}");
                    Console.WriteLine(
                        $"Face attributes of {imageFileName}   Makeup : {string.Format("{0}", (face.FaceAttributes.Makeup.EyeMakeup || face.FaceAttributes.Makeup.LipMakeup) ? "Yes" : "No")}");
                    Console.WriteLine(
                        $"Face attributes of {imageFileName}   Noise : {face.FaceAttributes.Noise.NoiseLevel}");
                    Console.WriteLine(
                        $"Face attributes of {imageFileName}   Occlusion : {string.Format("EyeOccluded: {0}", face.FaceAttributes.Occlusion.EyeOccluded ? "Yes" : "No")}   {string.Format("ForeheadOccluded: {0}", face.FaceAttributes.Occlusion.ForeheadOccluded ? "Yes" : "No")}   {string.Format("MouthOccluded: {0}", face.FaceAttributes.Occlusion.MouthOccluded ? "Yes" : "No")}");
                    Console.WriteLine($"Face attributes of {imageFileName}   Smile : {face.FaceAttributes.Smile}");
                    Console.WriteLine();
                }
            }
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

            return string.Join(",", accessoryArray);
        }

        private static string GetEmotion(Emotion emotion)
        {
            string emotionType = string.Empty;
            double emotionValue = 0.0;
            if (emotion.Anger > emotionValue)
            {
                emotionValue = emotion.Anger;
                emotionType = "Anger";
            }

            if (emotion.Contempt > emotionValue)
            {
                emotionValue = emotion.Contempt;
                emotionType = "Contempt";
            }

            if (emotion.Disgust > emotionValue)
            {
                emotionValue = emotion.Disgust;
                emotionType = "Disgust";
            }

            if (emotion.Fear > emotionValue)
            {
                emotionValue = emotion.Fear;
                emotionType = "Fear";
            }

            if (emotion.Happiness > emotionValue)
            {
                emotionValue = emotion.Happiness;
                emotionType = "Happiness";
            }

            if (emotion.Neutral > emotionValue)
            {
                emotionValue = emotion.Neutral;
                emotionType = "Neutral";
            }

            if (emotion.Sadness > emotionValue)
            {
                emotionValue = emotion.Sadness;
                emotionType = "Sadness";
            }

            if (emotion.Surprise > emotionValue)
            {
                emotionType = "Surprise";
            }

            return $"{emotionType}";
        }

        private static string GetHair(Hair hair)
        {
            if (hair.HairColor.Count == 0)
            {
                return hair.Invisible ? "Invisible" : "Bald";
            }

            HairColorType returnColor = HairColorType.Unknown;
            double maxConfidence = 0.0f;

            foreach (HairColor hairColor in hair.HairColor)
            {
                if (hairColor.Confidence <= maxConfidence)
                {
                    continue;
                }

                maxConfidence = hairColor.Confidence;
                returnColor = hairColor.Color;
            }

            return returnColor.ToString();
        }
    }
}
