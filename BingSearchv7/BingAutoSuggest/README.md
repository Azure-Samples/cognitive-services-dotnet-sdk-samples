

# The Bing AutoSuggest Search SDK Sample

This sample will show you how to get up and running using the Bing AutoSuggest Search Nuget package. This example will cover a few usecases and hopefully express best practices for interacting with the data from this API. For more information on the Bing AutoSuggest Search API v7, you can navigate to: https://azure.microsoft.com/en-us/services/cognitive-services/autosuggest/. Exhaustive reference documentation including description of parameters, their values, and supported markets is [here](https://docs.microsoft.com/en-us/rest/api/cognitiveservices/bing-autosuggest-api-v7-reference).

If you are looking for amending samples to suit your needs, the single, most important file is AutoSuggestSearchSamples.cs. See how you can access this file in the "Quickstart" section below.

## Features

This sample references the Bing AutoSuggest Search SDK, which is a stand-alone package for the v7 version of this API. All-in-one package including all the Bing AutoSuggest APIs on Cognitive Services will be available in future.

This example provides sample usecases of the the [Bing AutoSuggest Search v7](https://azure.microsoft.com/en-us/services/cognitive-services/autosuggest)

* Using the **Bing AutoSuggest Search Nuget Package** at https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.AutoSuggest/

## Getting Started

### Prerequisites

- Visual Studio 2017. If required, you can download free community version from here: https://www.visualstudio.com/vs/community/.
- A cognitive services API key is required to authenticate SDK calls. [Create a new Azure account, and try Cognitive Services for free.](https://azure.microsoft.com/free/cognitive-services/) **Alternately**, for a production scenario, you can buy an access key from here: https://portal.azure.com/#create/Microsoft.CognitiveServicesBingSearch-v7. While buying an access key, you may want to consider which tier is appropriate for you. More information is [here](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/search-api/web/). 
- .NET core SDK (ability to run .netcore 1.1 apps). You can get CORE, Framework, and Runtime from here: https://www.microsoft.com/net/download/. 

### Quickstart

To get the Bing AutoSuggest Search sample running locally, follow these steps:

1. git clone https://github.com/Azure-Samples/cognitive-services-dotnet-sdk-samples.git
2. Open cognitive-services-dotnet-sdk-samples\BingSearchv7\BingAutoSuggest\bing-search-dotnet.sln from Visual Studio 2017
3. npm install https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.AutoSuggest/1.0.0 from Tools > Nuget Package Manager > Package Manager Console. **Alternately**, you can go to Project > Manage Nuget Packages and search for "Microsoft.Azure.CognitiveServices.Search.AutoSuggest" in the "Browse" tab, and click on "Install". 
4. Click on "bing-search-dotnet" for debug/release version from the top of Visual Studio. This will run examples from the **BingAutoSuggest\AutoSuggestSearchSamples.cs** file. **Alternately** you can build and run solution in separate steps.

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
- [Bing AutoSuggest Search API Demo & capabilities](https://azure.microsoft.com/en-us/services/cognitive-services/autosuggest)
- [Bing AutoSuggest Search Reference Document](https://docs.microsoft.com/en-us/rest/api/cognitiveservices/bing-autosuggest-api-v7-reference)
- [Bing AutoSuggest Search Nuget Package](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.AutoSuggest/1.0.0)
- [Bing AutoSuggest Search Dotnet SDK (source code)](https://github.com/Azure/azure-sdk-for-net/tree/psSdkJson6/src/SDKs/CognitiveServices/dataPlane/Search/BingAutoSuggest) 
- Support channels: [Stack Overflow](https://stackoverflow.com/questions/tagged/bing-search) or [Azure Support](https://azure.microsoft.com/en-us/support/options/)
