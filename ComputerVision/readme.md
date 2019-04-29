---
topic: sample
languages:
  - C# 
products:
  - azure
---

# Quickstart samples for Computer Vision Services SDK and Rest APIs

These samples show you how to use the Azure Cognitive Services Computer Vision SDKs and APIs quickly within minutes without having to write any code.

## Contents

| File/folder | Description |
|-------------|-------------|
| Images       | Folder with sample images |
| SDK | The Visual Studio solution with samples on how to use the SDK |
| Rest | The Visual Studio solution with samples on how to use the Rest API|
| `README.md` | This README file. |


## Prerequisites

- [Visual Studio 2015](https://visualstudio.microsoft.com/) or higher
- [An Azure Subscription Key](https://azure.microsoft.com/en-us/try/cognitive-services/?api=computer-vision) 

## Setup

1. Clone or download this sample repository
2. Open the SDK or the Rest Visual Studio Solutions in Visual Studio

## Running the samples

1. Right-click on the relavant project (detectObjects, extractObjects, etc) and click on 'Set as StartUp Project'
1. In 'Program.cs' , update the line 
```c#
private const string subscriptionKey = "<your training key here>";
```
with your subscription key. For example, if your subscription key is `0123456789abcdef0123456789ABCDEF`, then the line should look like
```c#
private const string subscriptionKey = "0123456789abcdef0123456789ABCDEF";
```
 1. Update the endpoint with the region you generated your endpoint for. For example, if you are using the westcentralus endpoint, you should change the line ``` computerVision.Endpoint = "https://westus.api.cognitive.microsoft.com"; ``` to 
```c#
computerVision.Endpoint = "https://westcentralus.api.cognitive.microsoft.com";
```
1. (Optional) Change the remote image URL and location of local image
1. (Optional) Comment out visual parameters not needed for AnalyzeImage 
1. Hit F5 or build the solution

