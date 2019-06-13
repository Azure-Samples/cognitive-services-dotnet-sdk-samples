## Face Quickstart

The FaceQuickstart backs the code snippets represented in the [Face API Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/face/). 

The samples included in this C# solution are:

* **Authenticate**: authenticates using your Azure portal subscription key and endpoint to authorize a client.
* **DetectFaceExtract**: detects faces from input images (using the [recognition model](https://docs.microsoft.com/en-us/azure/cognitive-services/face/face-api-how-to-topics/specify-recognition-model) 01) for the purpose of extracting facial features. 
* **FindSimilar**: using a query image, it finds a similar face in a list of detected faces from other very similar face images.
* **IdentifyInPersonGroup**: Creates a person group from similar face images, then uses a group image for identifying who in the image is from an established person group.
* **Group**: groups together images that are similar. This is used if you do not need to create/define a person group.
* **Snapshot**: takes a snapshot of an existing person group in one region and copies it to another region.
* **Delete**: deletes a person group from a specific region.

### Prerequisites
* Visual Studio 2017+
* NuGet libraries: <br>
      - Microsoft.Azure.CognitiveServices.Vision.Face
* The Snapshot sample needs a 2nd Face resource in Azure to execute. Create one with a different region than your original Face resource. For example, Face resource 1: created with the 'westus' region. Face resource 2: created with the 'eastus' region. For this sample, the 1st region is referred to as the source region, and the 2nd region is referred to as the target region.
* In the Program.cs file at the top, set your environment variables with your keys, regions, and ID. Once set, close and reopen the solution file for them to take effect.

### How to run
* This .NET console app can be downloaded and run as a solution in Visual Studio.
      * Download the repo as a zip file or clone it.
      * Double-click the solution file in the FaceQuickstart folder.
      * Select 'Start' in Visual Studio. 
* Another way to use this sample is to view the Program.cs file and find all of the above samples as functions. 
* Dependencies within the samples: 
    - **Authenticate** produces a client that's used by all samples.
    - **DetectFaceRecognize** (using [recognition model](https://docs.microsoft.com/en-us/azure/cognitive-services/face/face-api-how-to-topics/specify-recognition-model) 02) is a helper function that is used by several other samples.
    - **Snapshot** needs a person group ID to be executed, so it uses the one created from **IdentifyInPersonGroup**. 
    - The **DeletePersonGroup** uses a person group ID, so it uses the one used in the **Snapshot**. It will delete the person group from both of your Face resources in their respective regions.

### References
* [Face recognition concepts](https://docs.microsoft.com/en-us/azure/cognitive-services/face/concepts/face-recognition)
* [Face detection and attributes](https://docs.microsoft.com/en-us/azure/cognitive-services/face/concepts/face-detection)
* [API Reference](https://docs.microsoft.com/en-us/azure/cognitive-services/face/apireference)
* [.NET SDK](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/cognitiveservices/client/face?view=azure-dotnet)
