

# The Bing Visual Search SDK Sample

This sample will show you how to get up and running using the Bing Visual Search Nuget package. This example will cover a few usecases and hopefully express best practices for interacting with the data from this API. For more information on the Bing Visual Search API v7, you can navigate to: https://azure.microsoft.com/en-us/services/cognitive-services/bing-visual-search/. Exhaustive reference documentation including description of parameters, their values, and supported markets is [here](https://docs.microsoft.com/en-us/rest/api/cognitiveservices/bingvisualsearch/images/visualsearch).

If you are looking for amending samples to suit your needs, the single, most important file is VisualSearchSamples.cs. See how you can access this file in the "Quickstart" section below.

## Features

This sample references the Bing Visual Search SDK, which is a stand-alone package for the v7 version of this API. All-in-one package including all the Bing Search APIs on Cognitive Services will be available in future.

This example provides sample usecases of the the [Bing Visual Search v7](https://azure.microsoft.com/en-us/services/cognitive-services/bing-visual-search/)

* Using the **Bing Visual Search Nuget Package** at https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.VisualSearch/0.18.0-preview

## Getting Started

### Prerequisites

- Visual Studio 2017. If required, you can download free community version from here: https://www.visualstudio.com/vs/community/.
- A cognitive services API key is required to authenticate SDK calls. You can [sign up here](https://azure.microsoft.com/en-us/try/cognitive-services/?api=search-api-v7) for the **free** trial key. This trial key is good for 7 days with 1 call per second. **Alternately**, for production scenario, you can buy access key from here: https://portal.azure.com/#create/Microsoft.CognitiveServicesBingSearch-v7. While buying access key you may want to consider which tier is appropriate for you. More information is [here](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/search-api/visual/). 
- .NET core SDK (ability to run .netcore 1.1 apps). You can get CORE, Framework, and Runtime from here: https://www.microsoft.com/net/download/. 

### Quickstart

To get the Bing Visual Search sample running locally, follow these steps:

1. git clone https://github.com/Azure-Samples/cognitive-services-dotnet-sdk-samples.git
2. Open cognitive-services-dotnet-sdk-samples\BingSearchv7\BingVisualSearch\bing-search-dotnet.sln from Visual Studio 2017
3. npm install https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.VisualSearch/0.18.0-preview from Tools > Nuget Package Manager > Package Manager Console. **Alternately**, you can go to Project > Manage Nuget Packages and search for "Microsoft.Azure.CognitiveServices.Search.VisualSearch" in the "Browse" tab, and click on "Install". 
4. Click on "bing-search-dotnet" for debug/release version from the top of Visual Studio. This will run examples from the **BingVisualSearch\VisualSearchSamples.cs** file. **Alternately** you can build and run solution in separate steps.

### Note: 
Change TargetFramework in bing-search-dotnet.csproj to “netcoreapp1.1” if you have .NET Framework version as 2.1.2. [ Older ] as follows:

**Current**
````  
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>
  </PropertyGroup>
````
**Revision**
````
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp1.1</TargetFramework>
  </PropertyGroup>
````
## Resources
- [Bing Visual Search API Demo & capabilities](https://azure.microsoft.com/en-us/services/cognitive-services/bing-visual-search/)
- [Bing Visual Search Reference Document](https://docs.microsoft.com/en-us/rest/api/cognitiveservices/bingvisualsearch/images/visualsearch)
- [Bing Visual Search Nuget Package](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.VisualSearch/0.18.0-preview)
- [Bing Visual Search Dotnet SDK (source code)](https://github.com/Azure/azure-sdk-for-net/tree/psSdkJson6/src/SDKs/CognitiveServices/dataPlane/Search/BingVisualSearch) 
- Support channels: [Stack Overflow](https://stackoverflow.com/questions/tagged/bing-search) or [Azure Support](https://azure.microsoft.com/en-us/support/options/)
