## Face Quickstart

The FaceQuickstart backs the code snippets represented in the [Face API Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/face/). 

The samples included in this C# solution are:

* **Authenticate**: authenticates using your Azure portal subscription key and endpoint to authorize a client.
* **DetectFaceExtract**: detects faces from input images using the recognition model 01 for the purpose of extracting facial features. 
* **FindSimilar**: using a query image, it finds a similar face in a list of detected faces from other very similar face images.
* **IdentifyInPersonGroup**: Creates a person group from similar face images, then uses a group image for identifying who in the image is from an established person group.
* **Group**: groups together images that are similar. This is used if you do not need to create/define a person group.
* **Snapshot**: takes a snapshot of an existing person group in one region and copies it to another region.
* **Delete**: deletes a person group from a specific region.

### Prerequisites
* Visual Studio 2017+
* NuGet libraries: <br>
      - Microsoft.Azure.CognitiveServices.Vision.Face <br>
      - Microsoft.Azure.CognitiveServices.Vision.Face.Models

### How to run
* In Visual Studio, create a .NET Framework C# console app and replace the default Program.cs file with the Program.cs file provided here.
* Dependencies: 
    - The **Authenticate** function is used by all samples.
    - The **DetectFaceRecognize** function (using recognitional model 02) is a helper function that is used by several other samples.
    - The **Snapshot** function needs a person group ID to be executed, so it uses the one created from the **IdentifyInPersonGroup**. 
    - The **DeletePersonGroup** function uses a person group ID, so it uses the one used in the **Snapshot** function.

### References
* [Face recognition concepts](https://docs.microsoft.com/en-us/azure/cognitive-services/face/concepts/face-recognition)
* [Face detection and attributes](https://docs.microsoft.com/en-us/azure/cognitive-services/face/concepts/face-detection)
* [API Reference](https://docs.microsoft.com/en-us/azure/cognitive-services/face/apireference)
* [.NET SDK](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/cognitiveservices/client/face?view=azure-dotnet)
