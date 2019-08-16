## Face Quickstart

The FaceQuickstart backs the code snippets represented in the [Face API Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/face/). 

The samples included in this C# solution are:

* **Authenticate**: authenticates using your Azure portal subscription key and endpoint to authorize a client.
* **Detect Faces**: detects faces from input images (using the [recognition model](https://docs.microsoft.com/en-us/azure/cognitive-services/face/face-api-how-to-topics/specify-recognition-model) 01) for the purpose of extracting facial features. 
* **Find Similar**: using a query image, it finds a similar face in a list of detected faces from other very similar face images.
* **Identify Faces**: Creates a person group from similar face images, then uses a group image for identifying who in the image is from an established person group.
* **Group Faces**: groups together images that are similar. This is used if you do not need to create/define a person group.
* **Snapshot Operations**: takes a snapshot of an existing person group in one region and copies it to another region.
* **Delete Person Group**: deletes a person group from a specific region.

### Prerequisites
* Visual Studio 2019 (if 2017, use the .NET Core console app, not the .NET Framerwork one)
* NuGet libraries: <br>
      - Microsoft.Azure.CognitiveServices.Vision.Face
* The Snapshot sample needs a 2nd Face resource in Azure to execute. Create one with a different region than your original Face resource. For example, Face resource 1: created with the 'westus' region. Face resource 2: created with the 'eastus' region. For this sample, the 1st region is referred to as the source region, and the 2nd region is referred to as the target region.
* In the Program.cs file at the top, set your environment variables with your keys, regions, and ID. Once set, close and reopen the solution file for them to take effect.

### How to run
* Create a new C# Console app in Visual Studio 2019.
* Copy/paste the Program.cs file in the Github quickstart into your own Program.cs file. Make sure to rename the namespace if different.
Dependencies within the samples: 
* Authenticate produces a client that's used by all samples.
* Detect Faces is a helper function that is used by several other samples. 
* Snapshot Operations need a person group ID to be executed, so it uses the one created from Identify Faces. 
* The Delete Person Group uses a person group ID, so it uses the one used in the Snapshot example. 
* It will delete the person group from both of your Face resources in their respective regions.

### References
 * Face Documentation: https://docs.microsoft.com/en-us/azure/cognitive-services/face/
 * .NET SDK: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/cognitiveservices/client/face?view=azure-dotnet
 * API Reference: https://docs.microsoft.com/en-us/azure/cognitive-services/face/apireference
