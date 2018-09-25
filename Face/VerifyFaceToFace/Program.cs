using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace VerifyFaceToFace
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create a client.
            string apiKey = "ENTER YOUR KEY HERE";
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = "ENTER YOUR ENDPOINT HERE"
            };

            List<string> imageFileNames =
                new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg", "Family1-Son1.jpg" };
            List<Guid> faceIds = new List<Guid>();

            foreach (var imageFileName in imageFileNames)
            {
                // Read image file.
                using (FileStream stream = new FileStream(Path.Combine("Images", imageFileName), FileMode.Open))
                {
                    // Detect faces from image stream.
                    IList<DetectedFace> detectedFaces = client.Face.DetectWithStreamAsync(stream).Result;
                    if (detectedFaces == null || detectedFaces.Count == 0)
                    {
                        Console.WriteLine($"[Error] No face detected from image `{imageFileName}`.");
                        return;
                    }

                    Console.WriteLine($"{detectedFaces.Count} faces detected from image `{imageFileName}`.");
                    if (detectedFaces[0].FaceId == null)
                    {
                        Console.WriteLine("[Error] Parameter `returnFaceId` of `DetectWithStreamAsync` must be set to `true` (by default) for verification purpose.");
                        return;
                    }

                    faceIds.Add(detectedFaces[0].FaceId.Value);
                }
            }

            // Verification example for faces of the same person.
            VerifyResult verifyResult1 = client.Face.VerifyFaceToFaceAsync(faceIds[0], faceIds[1]).Result;
            Console.WriteLine(
                verifyResult1.IsIdentical
                    ? $"Faces from {imageFileNames[0]} & {imageFileNames[1]} are of the same (Positive) person, similarity confidence: {verifyResult1.Confidence}."
                    : $"Faces from {imageFileNames[0]} & {imageFileNames[1]} are of different (Negative) persons, similarity confidence: {verifyResult1.Confidence}.");

            // Verification example for faces of different persons.
            VerifyResult verifyResult2 = client.Face.VerifyFaceToFaceAsync(faceIds[1], faceIds[2]).Result;
            Console.WriteLine(
                verifyResult2.IsIdentical
                    ? $"Faces from {imageFileNames[1]} & {imageFileNames[2]} are of the same (Negative) person, similarity confidence: {verifyResult2.Confidence}."
                    : $"Faces from {imageFileNames[1]} & {imageFileNames[2]} are of different (Positive) persons, similarity confidence: {verifyResult2.Confidence}.");

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }
}
