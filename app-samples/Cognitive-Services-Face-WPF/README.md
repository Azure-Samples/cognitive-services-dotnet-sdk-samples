# Microsoft Cognitive Service Face API: WPF Sample Application
This repo contains the Windows WPF sample for the Microsoft Face API, an offering within [Microsoft Cognitive Services](https://www.microsoft.com/cognitive-services), formerly known as Project Oxford.

This is to deprecate the [Microsoft.ProjectOxford.Face](https://www.nuget.org/packages/Microsoft.ProjectOxford.Face/) SDK and its sample at [Microsoft/Cognitive-Face-Windows](https://github.com/Microsoft/Cognitive-Face-Windows). The sample in this repo is using latest [Microsoft.Azure.CognitiveServices.Vision.Face SDK](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Vision.Face/) while keeping the same sample logic.
* [Learn about the Face API](https://www.microsoft.com/cognitive-services/en-us/face-api)
* [Read the documentation](https://www.microsoft.com/cognitive-services/en-us/face-api/documentation/overview)
* [Find more SDKs & Samples](https://www.microsoft.com/cognitive-services/en-us/SDK-Sample?api=face)

## The Sample
This sample is a Windows WPF application to demonstrate the use of Face API. It demonstrates face detection, face verification, face grouping, finding similar faces, and face identification.

### Build the Sample
 1. Starting in the folder where you clone the repository (this folder)
 2. Start Microsoft Visual Studio 2015 and select `File > Open > Project/Solution`.
 3. Go to `Sample-WPF Folder`.
 4. Double-click the Visual Studio 2015 Solution (.sln) file FaceAPI-WPF-Samples.
 5. Press Ctrl+Shift+B, or select `Build > Build Solution`.

### Run the Sample
After the build is complete, press F5 to run the sample.

First, you must obtain a Face API subscription key by [following instructions on our website](<https://www.microsoft.com/cognitive-services/en-us/sign-up>).

Locate the text edit box saying "Paste your subscription key here to start" on
the top right corner. Paste your subscription key. You can choose to persist
your subscription key in your machine by clicking "Save Key" button. When you
want to delete the subscription key from the machine, click "Delete Key" to
remove it from your machine.

Click on "Select Scenario" to use samples of different scenarios, and
follow the instructions on screen.

Microsoft will receive the images you upload and may use them to improve Face
API and related services. By submitting an image, you confirm you have consent
from everyone in it.

There are sample images to be used with this sample application. You can find these images under Data folder. Please note the use of these images is licensed under [LICENSE-IMAGE](</LICENSE-IMAGE.md>).

<img src="SampleScreenshots/SampleRunning.png" width="100%"/>

## Contributing
We welcome contributions. Feel free to file issues and pull requests on the repo and we'll address them as we can. Learn more about how you can help on our [Contribution Rules & Guidelines](</CONTRIBUTING.md>). 

You can reach out to us anytime with questions and suggestions using our communities below:
 - **Support questions:** [StackOverflow](<https://stackoverflow.com/questions/tagged/microsoft-cognitive>)
 - **Feedback & feature requests:** [Cognitive Services UserVoice Forum](<https://cognitive.uservoice.com>)

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Updates
* [Face API Release Notes](https://www.microsoft.com/cognitive-services/en-us/face-api/documentation/ReleaseNotes)

## License
All Microsoft Cognitive Services SDKs and samples are licensed with the MIT License. For more details, see
[LICENSE](</LICENSE.md>).

Sample images are licensed separately, please refer to [LICENSE-IMAGE](</LICENSE-IMAGE.md>)


## Developer Code of Conduct
Developers using Cognitive Services, including this client library & sample, are expected to follow the “Developer Code of Conduct for Microsoft Cognitive Services”, found at [http://go.microsoft.com/fwlink/?LinkId=698895](http://go.microsoft.com/fwlink/?LinkId=698895).
