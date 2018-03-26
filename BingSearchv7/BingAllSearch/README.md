

# The Bing All Search SDK Samples

This sample will show you how to get up and running using the Bing Search SDKs. This example will cover some usecases and hopefully express best practices for interacting with the data from the Bing Search APIs. Examples are included for these offerings: Bing Web Search, Bing Image Search, Bing Video Search, Bing News Search, Bing Custom Search, Bing Entity Search, and Bing Spell Check. For more information on the Bing Search APIs, you can navigate to: https://azure.microsoft.com/en-us/services/cognitive-services/directory/search/. 

If you are interested in a specific Bing Search API, please check out the corresponding subfolder. This sample is meant for checking multiple samples at the same time and references packages for all the APIs.

## Features

This sample references the all Bing Search SDKs, which is the complete list of packages for the v7 version of Bing APIs. All-in-one package - including all the Bing Search APIs on Cognitive Services - will be available in future.

This example provides sample usecases of the the [Bing Search APIs v7](https://azure.microsoft.com/en-us/services/cognitive-services/directory/search/) using the below packages:

* **Bing Web Search Nuget Package** at https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.WebSearch/1.2.0
* **Bing Image Search Nuget Package** at https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.ImageSearch/1.2.0
* **Bing News Search Nuget Package** at https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.NewsSearch/1.2.0
* **Bing Video Search Nuget Package** at https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.VideoSearch/1.2.0
* **Bing Custom Search Nuget Package** at https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.CustomSearch/1.2.0
* **Bing Entity Search Nuget Package** at https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.EntitySearch/1.2.0
* **Bing Spell Check Nuget Package** at https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.SpellCheck/1.2.0

For ease, these packages are comntained in the TempPackage folder. 

## Getting Started

### Prerequisites

- Visual Studio 2017. If required, you can download free community version from here: https://www.visualstudio.com/vs/community/.
- A cognitive services API key is required to authenticate SDK calls. You can [sign up here](https://azure.microsoft.com/en-us/try/cognitive-services/?api=bing-web-search-api) for the **free** trial key. This trial key is good for 30 days with 3 calls per second. **Alternately**, for production scenario, you can buy access key from here: https://portal.azure.com/#create/Microsoft.CognitiveServicesBingSearch-v7. While buying access key you may want to consider which tier is appropriate for you. More information is [here](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/search-api/web/). 
- .NET core SDK (ability to run .netcore 1.1 apps). You can get CORE, Framework, and Runtime from here: https://www.microsoft.com/net/download/. 
- You will need Custom Config ID, if you are running the Bing Custom Search sample. You can get this from customsearch.ai by creating your own custom search instance. Here is a step-by-step guide to create a custom search instance: https://blogs.bing.com/search-quality-insights/2017-12/build-your-ads-free-search-engine-with-bing-custom-search.

### Quickstart

To get the Bing Search sample running locally, follow these steps:

1. git clone https://github.com/Azure-Samples/cognitive-services-dotnet-sdk-samples.git
2. Open cognitive-services-dotnet-sdk-samples\BingSearchv7\BingAllSearch\bing-search-dotnet.sln from Visual Studio 2017
3. You need not install individual packages as they are contained in the tempPackage folder. If these don't work you can get the latest by: npm install <packages as mentioned above in Features section> from Tools > Nuget Package Manager > Package Manager Console. **Alternately**, you can go to Project > Manage Nuget Packages and search for individual packages in the "Browse" tab, and click on "Install". 
4. Click on "bing-search-dotnet" for debug/release version from the top of Visual Studio. **Alternately** you can build and run solution in separate steps.

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
- [Bing Search APIs Demo & capabilities](https://azure.microsoft.com/en-us/services/cognitive-services/directory/search/)
- Bing Search Reference Documents: Please check documentation of the corresponding API
- Nuget Packages: As mentioned in "Features" section
- Bing Search Dotnet SDKs (source code): Please check corresponding entry in https://github.com/Azure/azure-sdk-for-net/tree/psSdkJson6/src/SDKs/CognitiveServices/dataPlane/ 
- Support channels: [Stack Overflow](https://stackoverflow.com/questions/tagged/bing-search) or [Azure Support](https://azure.microsoft.com/en-us/support/options/)
