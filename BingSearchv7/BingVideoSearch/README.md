# Bing Video Search SDK Samples

These samples will show you how to get up and running using the SDKs for various Bing Search services. They'll cover a few rudimentary use cases and hopefully express best practices for interacting with the data from these APIs.

## Features

Please note that this samples package references an all-in-one SDK which includes all Bing Search services. Individual packages exist for each service if you would prefer working with smaller assembly sizes. Both individual service packages as well as the all-in-one will have feature parity for a particular service.

This project framework provides examples for the Web Search service:

* Using the **Bing Web Search SDK** \[[individual package](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.VideoSearch/1.1.0-preview) for the [Video Search API](https://azure.microsoft.com/en-us/services/cognitive-services/bing-web-search-api/)

## Getting Started

### Prerequisites

- A cognitive services API key with which to authenticate the SDK's calls. [Sign up here](https://azure.microsoft.com/en-us/services/cognitive-services/directory/) by navigating to the **Search** services and acquiring an API key. You can get a trial key for **free** which will expire after 30 days.
- .NET core SDK (ability to run .netcore 1.1 apps)

### Quickstart

To get these samples running locally, simply get the pre-requisites above, then:

1. git clone https://github.com/Azure-Samples/cognitive-services-dotnet-sdk-samples.git
2. cd cognitive-services-dotnet-sdk-samples\SearchV7\VideoSearch
3. open bing-search-dotnet.sln
4. npm install [VideoSearch](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Search.VideoSearch/1.1.0-preview)
5. navigate through the console app to play with examples