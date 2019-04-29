# Quickstart for Text Analytics API with C# 
<a name="HOLTop"></a>

This article shows you how to detect language, analyze sentiment, extract key phrases and entities using the [Text Analytics APIs](//go.microsoft.com/fwlink/?LinkID=759711) with C#. The code was written to work on a .Net Core application, with minimal references to external libraries, so you could also run it on Linux or MacOS.

Refer to the [API definitions](//go.microsoft.com/fwlink/?LinkID=759346) for technical documentation for the APIs.

## Prerequisites

You must have a [Cognitive Services API account](https://docs.microsoft.com/azure/cognitive-services/cognitive-services-apis-create-account) with **Text Analytics API**. You can use the **free tier for 5,000 transactions/month** to complete this quickstart.

You must also have the [endpoint and access key](../How-tos/text-analytics-how-to-access-key.md) that was generated for you during sign up. 


## Running the project
1. Open the TextAnalyticsSample.sln file in Visual Studio 2017.
1. Replace the subscription key stub with the key you obtain from your free account.
1. Ensure that region is set to the Azure region associated with the free account.
1. Run the solution.

> [!Tip]
>  While you could call the [HTTP endpoints](https://westus.dev.cognitive.microsoft.com/docs/services/TextAnalytics-v2-1/operations/56f30ceeeda5650db055a3c9) directly from C#, the Microsoft.Azure.CognitiveServices.Language SDK makes it much easier to call the service without having to worry about serializing and deserializing JSON.
>
> A few useful links:
> - [SDK Nuget page](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Language.TextAnalytics)
> - [SDK code ](https://github.com/Azure/azure-sdk-for-net/tree/master/src/SDKs/CognitiveServices/dataPlane/Language/TextAnalytics)


