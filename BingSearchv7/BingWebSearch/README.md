# The Bing Web Search SDK Sample

This sample will show you how to get up and running using the Bing Web Search SDK. This example will cover a few usecases and hopefully express best practices for interacting with the data from this API. For more information on the Bing Web Search API v7, you can go [here](https://azure.microsoft.com/en-us/services/cognitive-services/bing-web-search-api/). Exhaustive reference documentation including description of parameters and their values is [here](https://docs.microsoft.com/en-us/rest/api/cognitiveservices/bing-web-api-v7-reference).

If you are looking for amending samples to suit your needs, the single, most important file is WebSearchSamples.cs. See how you can access it in the "Quickstart" section."

## Features

Please note that this sample references the Bing Web Search SDK, which is a stand-alone package for the v7 version of the corresponding API. All-in-one package including all Bing Search APIs on Cognitive Services will be available in future.

This example provides sample usecases of the the [Bing Web Search v7](https://azure.microsoft.com/en-us/services/cognitive-services/bing-web-search-api/)

* Using the **Bing Web Search Nuget Package** at https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.WebSearch/1.1.0-preview

## Getting Started

### Prerequisites

- Visual Studio 2017. If required, you can download free community version of VS from here: https://www.visualstudio.com/vs/community/.
- A cognitive services API key is required to authenticate the SDK calls. You can [sign up here](https://azure.microsoft.com/en-us/try/cognitive-services/?api=bing-web-search-api) for the **free** trial key. This trial key is good for 30 days with 3 calls per second. **Alternately**, for production scenario, you can buy access key from here: https://portal.azure.com/#create/Microsoft.CognitiveServicesBingSearch-v7. While buying access key you may want to consider which tier is appropriate for you. More information is [here](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/search-api/web/). 
- .NET core SDK (ability to run .netcore 1.1 apps)

### Quickstart

To get the Bing Web Search sample running locally, follow these steps:

1. git clone https://github.com/Azure-Samples/cognitive-services-dotnet-sdk-samples.git
2. Open cognitive-services-dotnet-sdk-samples\BingSearchv7\BingWebSearch\bing-search-dotnet.sln from Visual Studio 2017
3. npm install https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.WebSearch/1.1.0-preview from Tools > Nuget Package Manager > Package Manager Console. **Alternately**, you can go to Project > Manage Nuget Packages and search for "Microsoft.Azure.CognitiveServices.Search.WebSearch" in the "Browse" tab, and click on "Install". 
4. Click on "bing-search-dotnet" for debug/release version from the top of Visual Studio. This will run examples from the **BingWebSearch\WebSearchSamples.cs** file.

## Resources
- [Bing Web Search API Demo & capabilities](https://azure.microsoft.com/en-us/services/cognitive-services/bing-web-search-api/)
- [Bing Web Search Reference Document](https://docs.microsoft.com/en-us/rest/api/cognitiveservices/bing-web-api-v7-reference)
- [Bing Web Search Nuget Package](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.WebSearch/1.1.0-preview)
- [Bing Web Search Dotnet SDK (source code)](https://github.com/Azure/azure-sdk-for-net/tree/psSdkJson6/src/SDKs/CognitiveServices/dataPlane/Search/BingWebSearch) 
- Support channels: [Stack Overflow](https://stackoverflow.com/questions/tagged/bing-search) or [Azure Support](https://azure.microsoft.com/en-us/support/options/)
