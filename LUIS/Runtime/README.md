---
services: cognitive-services, luis, language-understanding
platforms: dotnet
author: cahann
---

# Cognitive Services: LUIS Console Application Sample

A simple console demostrating how to consume the LUIS Runtime SDK to predict user utterances.

### Prerequisites

The minimum prerequisites to run this sample are:
* The latest update of Visual Studio 2015 or 2017. You can download the community version [here](http://www.visualstudio.com) for free.
* A [LUIS.ai account](https://www.luis.ai/) where to upload the sample's LUIS model.

#### LUIS Application

The first step to using LUIS is to create or import an application. Go to the home page, [www.luis.ai](https://www.luis.ai/), and log in. After creating your LUIS account you'll be able to Import an Existing Application where can you can select a local copy of the LuisApp.json file an import it.

![Import an Existing Application](images/prereqs-import.png)

If you want to test this sample, you have to import the pre-build [LuisApp.json](LuisApp.json) file to your LUIS account.

Once you imported the application you'll need to "train" the model ([Training](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/train-test)) before you can "Publish" the model in an HTTP endpoint. For more information, take a look at [Publishing a Model](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/publishapp).

Finally, edit the [appsettings.json](Microsoft.Azure.CognitiveServices.LUIS.Sample/appsettings.json) file and update the attribute placeholders with the values corresponding to your Subscription, Application and endpoint URL.

#### Where to find the Application ID and Subscription Key

You'll need these two values to configure the LuisDialog through the LuisModel attribute:

1. Application ID

    You can find the App ID in the LUIS application's settings.

    ![App Settings](images/prereqs-appid.png)

2. Subscription Key and Endpoint URL

    Click on the Publish App link from the top of the LUIS application dashboard. Once your app is published, copy the endpoint URL (https://westus.api.cognitive.microsoft.com) and Key String from *Starter_Key* from the Endpoints table on the Publish App page.

    ![Programmatic API Key](images/prereqs-apikey.png)


### Code Highlights

One of the key problems in human-computer interactions is the ability of the computer to understand what a person wants, and to find the pieces of information that are relevant to their intent. In the LUIS application, you will bundle together the intents and entities that are important to your task. Read more about [Planning an Application](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/plan-your-app) in the [LUIS Docs](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/).

Once your model is set, you can invoke de LUIS Runtime API to analize user input and obtain its intent and possible entities.

From .NET you can use the *Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime* NuGet package. Once you have reference the library, you can start making call to the API.

````C#
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;

// Create client with SuscriptionKey and AzureRegion
var client = new LUISRuntimeClient(new Uri([EndPointURL]),
new ApiKeyServiceClientCredentials([LUIS_SUBSCRIPTION_KEY]));

// Predict
var result = await client.Prediction.ResolveAsync([LUIS_APPLICATION_ID], "Text to Predict or User input");
````

The [LuisResult](https://github.com/Azure/azure-sdk-for-net/blob/psSdkJson6/src/SDKs/CognitiveServices/dataPlane/Language/LUIS-Runtime/Generated/Models/LuisResult.cs) object contains the possible detected intents and entities that could be extracted from the input.

### Outcome

You will see the following when running the application:

![Sample Outcome](images/outcome.png)

### More Information

To get more information about how to get started in Bot Builder for .NET and Conversations please review the following resources:
* [Language Understanding Intelligent Service](https://azure.microsoft.com/en-us/services/cognitive-services/language-understanding-intelligent-service/)
* [LUIS.ai](https://www.luis.ai)
* [LUIS Docs](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/home)
* [LUIS Runtime API v2 - Specification](https://github.com/Azure/azure-rest-api-specs/tree/current/specification/cognitiveservices/data-plane/LUIS/Runtime)
