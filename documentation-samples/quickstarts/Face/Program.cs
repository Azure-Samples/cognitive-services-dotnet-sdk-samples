using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

/**
 * FACE QUICKSTART
 * 
 * This quickstart includes the following examples for Face:
 *  - Detect Faces
 *  - Find Similar
 *  - Identify faces (and person group operations)
 *  - Group Faces
 *  - Snapshot Operations
 * 
 * Prerequisites:
 *  - Visual Studio 2019 (or 2017, but this is app uses .NETCore, not .NET Framework)
 *  - NuGet libraries:
 *    Microsoft.Azure.CognitiveServices.Vision.Face
 *    
 * The Snapshot sample needs a 2nd Face resource in Azure to execute. Create one with a different region than your original Face resource. 
 * For example, Face resource 1: created with the 'westus' region. Face resource 2: created with the 'eastus' region. 
 * For this sample, the 1st region is referred to as the source region, and the 2nd region is referred to as the target region.
 * In the Program.cs file at the top, set your environment variables with your keys, regions, and ID. 
 * Once set, close and reopen the solution file for them to take effect.
 *   
 * How to run:
 *  - Create a new C# Console app in Visual Studio 2019.
 *  - Copy/paste the Program.cs file in the Github quickstart into your own Program.cs file. Make sure to rename the namespace if different.
 *  
 * Dependencies within the samples: 
 *  - Authenticate produces a client that's used by all samples.
 *  - Detect Faces is a helper function that is used by several other samples. 
 *  - Snapshot Operations need a person group ID to be executed, so it uses the one created from Identify Faces. 
 *  - The Delete Person Group uses a person group ID, so it uses the one used in the Snapshot example. 
 *    It will delete the person group from both of your Face resources in their respective regions.
 *   
 * References:
 *  - Face Documentation: https://docs.microsoft.com/en-us/azure/cognitive-services/face/
 *  - .NET SDK: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/cognitiveservices/client/face?view=azure-dotnet
 *  - API Reference: https://docs.microsoft.com/en-us/azure/cognitive-services/face/apireference
 */

namespace FaceQuickstart
{
	class Program
	{
		// Used for the Identify, Snapshot, and Delete examples.
		// The same person group is used for both Person Group Operations and Snapshot Operations.
		static string sourcePersonGroup = null;
		static string targetPersonGroup = null;

		// Used for all examples.
		// URL for the images.
		const string IMAGE_BASE_URL = "https://csdx.blob.core.windows.net/resources/Face/Images/";

		static void Main(string[] args)
		{
			// Used in Authenticate and Snapshot examples. The client they help create is used by all examples.
			// From your Face subscription in the Azure portal, get your subscription key and location/region (for example, 'westus').
			// Set your environment variables with these with the names below. Close and reopen your project for changes to take effect.
			string SUBSCRIPTION_KEY = Environment.GetEnvironmentVariable("FACE_SUBSCRIPTION_KEY");
			string ENDPOINT = Environment.GetEnvironmentVariable("FACE_ENDPOINT");
			// The Snapshot example needs its own 2nd client, since it uses two different regions.
			string TARGET_SUBSCRIPTION_KEY = Environment.GetEnvironmentVariable("FACE_SUBSCRIPTION_KEY2");
			string TARGET_ENDPOINT = Environment.GetEnvironmentVariable("FACE_ENDPOINT2");
			// Grab your subscription ID, from any resource in Azure, from the Overview page (all resources have the same subscription ID). 
			Guid AZURE_SUBSCRIPTION_ID = new Guid(Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID"));

			// Used in the Detect Faces and Verify examples.
			// Recognition model 2 is used for feature extraction, use 1 to simply recognize/detect a face. 
			// However, the API calls to Detection that are used with Verify, Find Similar, or Identify must share the same recognition model.
			const string RECOGNITION_MODEL2 = RecognitionModel.Recognition02;
			const string RECOGNITION_MODEL1 = RecognitionModel.Recognition01;

			// Authenticate.
			IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
			// Authenticate for another region (used in Snapshot only).
			IFaceClient clientTarget = Authenticate(TARGET_ENDPOINT, TARGET_SUBSCRIPTION_KEY);
			// Detect features from faces.
			DetectFaceExtract(client, IMAGE_BASE_URL, RECOGNITION_MODEL2).Wait();
			// Find a similar face from a list of faces.
			FindSimilar(client, IMAGE_BASE_URL, RECOGNITION_MODEL1).Wait();
			// Compare two images if the same person or not.
			Verify(client, IMAGE_BASE_URL, RECOGNITION_MODEL2).Wait();
			// Identify a face(s) in a person group (person group is created in this example).
			IdentifyInPersonGroup(client, IMAGE_BASE_URL, RECOGNITION_MODEL1).Wait();
			// Automatically group similar faces.
			Group(client, IMAGE_BASE_URL, RECOGNITION_MODEL1).Wait();
			// Take a snapshot of a person group in one region, move it to the next region.
			// Can also be used for moving a person group from one Azure subscription to the next.
			Snapshot(client, clientTarget, sourcePersonGroup, AZURE_SUBSCRIPTION_ID).Wait();

			// At end, delete person groups in both regions (since testing only)
			Console.WriteLine("========DELETE PERSON GROUP========");
			DeletePersonGroup(client, sourcePersonGroup).Wait();
			DeletePersonGroup(clientTarget, targetPersonGroup).Wait();

			Console.WriteLine("End of quickstart.");
		}

		/*
		 *	AUTHENTICATE
		 *	Uses subscription key and region to create a client.
		 */
		public static IFaceClient Authenticate(string endpoint, string key)
		{
			return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
		}
		/*
		 * END - Authenticate
		 */

		/* 
		 * DETECT FACES
		 * Detects features from faces and IDs them.
		 */
		public static async Task DetectFaceExtract(IFaceClient client, string url, string recognitionModel)
		{
			Console.WriteLine("========DETECT FACES========");

			// Create a list of images
			List<string> imageFileNames = new List<string>
							{
								"detection1.jpg",    // single female with glasses
								// "detection2.jpg", // (optional: single man)
								// "detection3.jpg", // (optional: single male construction worker)
								// "detection4.jpg", // (optional: 3 people at cafe, 1 is blurred)
								"detection5.jpg",    // family, woman child man
								"detection6.jpg"     // elderly couple, male female
							};

			foreach (var imageFileName in imageFileNames)
			{
				IList<DetectedFace> detectedFaces;

				// Detect faces with all attributes from image url.
				detectedFaces = await client.Face.DetectWithUrlAsync($"{url}{imageFileName}",
						returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.Accessories, FaceAttributeType.Age,
						FaceAttributeType.Blur, FaceAttributeType.Emotion, FaceAttributeType.Exposure, FaceAttributeType.FacialHair,
						FaceAttributeType.Gender, FaceAttributeType.Glasses, FaceAttributeType.Hair, FaceAttributeType.HeadPose,
						FaceAttributeType.Makeup, FaceAttributeType.Noise, FaceAttributeType.Occlusion, FaceAttributeType.Smile },
						recognitionModel: recognitionModel);

				Console.WriteLine($"{detectedFaces.Count} face(s) detected from image `{imageFileName}`.");

				// Parse and print all attributes of each detected face.
				foreach (var face in detectedFaces)
				{
					Console.WriteLine($"Face attributes for {imageFileName}:");

					// Get bounding box of the faces
					Console.WriteLine($"Rectangle(Left/Top/Width/Height) : {face.FaceRectangle.Left} {face.FaceRectangle.Top} {face.FaceRectangle.Width} {face.FaceRectangle.Height}");

					// Get accessories of the faces
					List<Accessory> accessoriesList = (List<Accessory>)face.FaceAttributes.Accessories;
					int count = face.FaceAttributes.Accessories.Count;
					string accessory; string[] accessoryArray = new string[count];
					if (count == 0) { accessory = "NoAccessories"; }
					else
					{
						for (int i = 0; i < count; ++i) { accessoryArray[i] = accessoriesList[i].Type.ToString(); }
						accessory = string.Join(",", accessoryArray);
					}
					Console.WriteLine($"Accessories : {accessory}");

					// Get face other attributes
					Console.WriteLine($"Age : {face.FaceAttributes.Age}");
					Console.WriteLine($"Blur : {face.FaceAttributes.Blur.BlurLevel}");

					// Get emotion on the face
					string emotionType = string.Empty;
					double emotionValue = 0.0;
					Emotion emotion = face.FaceAttributes.Emotion;
					if (emotion.Anger > emotionValue) { emotionValue = emotion.Anger; emotionType = "Anger"; }
					if (emotion.Contempt > emotionValue) { emotionValue = emotion.Contempt; emotionType = "Contempt"; }
					if (emotion.Disgust > emotionValue) { emotionValue = emotion.Disgust; emotionType = "Disgust"; }
					if (emotion.Fear > emotionValue) { emotionValue = emotion.Fear; emotionType = "Fear"; }
					if (emotion.Happiness > emotionValue) { emotionValue = emotion.Happiness; emotionType = "Happiness"; }
					if (emotion.Neutral > emotionValue) { emotionValue = emotion.Neutral; emotionType = "Neutral"; }
					if (emotion.Sadness > emotionValue) { emotionValue = emotion.Sadness; emotionType = "Sadness"; }
					if (emotion.Surprise > emotionValue) { emotionType = "Surprise"; }
					Console.WriteLine($"Emotion : {emotionType}");

					// Get more face attributes
					Console.WriteLine($"Exposure : {face.FaceAttributes.Exposure.ExposureLevel}");
					Console.WriteLine($"FacialHair : {string.Format("{0}", face.FaceAttributes.FacialHair.Moustache + face.FaceAttributes.FacialHair.Beard + face.FaceAttributes.FacialHair.Sideburns > 0 ? "Yes" : "No")}");
					Console.WriteLine($"Gender : {face.FaceAttributes.Gender}");
					Console.WriteLine($"Glasses : {face.FaceAttributes.Glasses}");

					// Get hair color
					Hair hair = face.FaceAttributes.Hair;
					string color = null;
					if (hair.HairColor.Count == 0) { if (hair.Invisible) { color = "Invisible"; } else { color = "Bald"; } }
					HairColorType returnColor = HairColorType.Unknown;
					double maxConfidence = 0.0f;
					foreach (HairColor hairColor in hair.HairColor)
					{
						if (hairColor.Confidence <= maxConfidence) { continue; }
						maxConfidence = hairColor.Confidence; returnColor = hairColor.Color; color = returnColor.ToString();
					}
					Console.WriteLine($"Hair : {color}");

					// Get more attributes
					Console.WriteLine($"HeadPose : {string.Format("Pitch: {0}, Roll: {1}, Yaw: {2}", Math.Round(face.FaceAttributes.HeadPose.Pitch, 2), Math.Round(face.FaceAttributes.HeadPose.Roll, 2), Math.Round(face.FaceAttributes.HeadPose.Yaw, 2))}");
					Console.WriteLine($"Makeup : {string.Format("{0}", (face.FaceAttributes.Makeup.EyeMakeup || face.FaceAttributes.Makeup.LipMakeup) ? "Yes" : "No")}");
					Console.WriteLine($"Noise : {face.FaceAttributes.Noise.NoiseLevel}");
					Console.WriteLine($"Occlusion : {string.Format("EyeOccluded: {0}", face.FaceAttributes.Occlusion.EyeOccluded ? "Yes" : "No")} " +
						$" {string.Format("ForeheadOccluded: {0}", face.FaceAttributes.Occlusion.ForeheadOccluded ? "Yes" : "No")}   {string.Format("MouthOccluded: {0}", face.FaceAttributes.Occlusion.MouthOccluded ? "Yes" : "No")}");
					Console.WriteLine($"Smile : {face.FaceAttributes.Smile}");
					Console.WriteLine();
				}
			}
		}

		// Detect faces from image url for recognition purpose. This is a helper method for other functions in this quickstart.
		// Parameter `returnFaceId` of `DetectWithUrlAsync` must be set to `true` (by default) for recognition purpose.
		// The field `faceId` in returned `DetectedFace`s will be used in Face - Find Similar, Face - Verify. and Face - Identify.
		// It will expire 24 hours after the detection call.
		private static async Task<List<DetectedFace>> DetectFaceRecognize(IFaceClient faceClient, string url, string RECOGNITION_MODEL1)
		{
			// Detect faces from image URL. Since only recognizing, use the recognition model 1.
			IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithUrlAsync(url, recognitionModel: RECOGNITION_MODEL1);
			Console.WriteLine($"{detectedFaces.Count} face(s) detected from image `{Path.GetFileName(url)}`");
			return detectedFaces.ToList();
		}
		/*
		 * END - DETECT FACES 
		 */

		/*
		 * FIND SIMILAR
		 * This example will take an image and find a similar one to it in another image.
		 */
		public static async Task FindSimilar(IFaceClient client, string url, string RECOGNITION_MODEL1)
		{
			Console.WriteLine("========FIND SIMILAR========");

			List<string> targetImageFileNames = new List<string>
								{
									"Family1-Dad1.jpg",
									"Family1-Daughter1.jpg",
									"Family1-Mom1.jpg",
									"Family1-Son1.jpg",
									"Family2-Lady1.jpg",
									"Family2-Man1.jpg",
									"Family3-Lady1.jpg",
									"Family3-Man1.jpg"
								};

			string sourceImageFileName = "findsimilar.jpg";
			IList<Guid?> targetFaceIds = new List<Guid?>();
			foreach (var targetImageFileName in targetImageFileNames)
			{
				// Detect faces from target image url.
				var faces = await DetectFaceRecognize(client, $"{url}{targetImageFileName}", RECOGNITION_MODEL1);
				// Add detected faceId to list of GUIDs.
				targetFaceIds.Add(faces[0].FaceId.Value);
			}

			// Detect faces from source image url.
			IList<DetectedFace> detectedFaces = await DetectFaceRecognize(client, $"{url}{sourceImageFileName}", RECOGNITION_MODEL1);
			Console.WriteLine();

			// Find a similar face(s) in the list of IDs. Comapring only the first in list for testing purposes.
			IList<SimilarFace> similarResults = await client.Face.FindSimilarAsync(detectedFaces[0].FaceId.Value, null, null, targetFaceIds);
			foreach (var similarResult in similarResults)
			{
				Console.WriteLine($"Faces from {sourceImageFileName} & ID:{similarResult.FaceId} are similar with confidence: {similarResult.Confidence}.");
			}
			Console.WriteLine();
		}
		/*
		 * END - FIND SIMILAR 
		 */

		/*
		 * VERIFY
		 * The Verify operation takes a face ID from DetectedFace or PersistedFace and either another face ID 
		 * or a Person object and determines whether they belong to the same person. If you pass in a Person object, 
		 * you can optionally pass in a PersonGroup to which that Person belongs to improve performance.
		 */
		public static async Task Verify(IFaceClient client, string url, string recognitionModel02)
		{
			Console.WriteLine("========VERIFY========");

			List<string> targetImageFileNames = new List<string> { "Family1-Dad1.jpg", "Family1-Dad2.jpg" };
			string sourceImageFileName1 = "Family1-Dad3.jpg";
			string sourceImageFileName2 = "Family1-Son1.jpg";


			List<Guid> targetFaceIds = new List<Guid>();
			foreach (var imageFileName in targetImageFileNames)
			{
				// Detect faces from target image url.
				List<DetectedFace> detectedFaces = await DetectFaceRecognize(client, $"{url}{imageFileName} ", recognitionModel02);
				targetFaceIds.Add(detectedFaces[0].FaceId.Value);
				Console.WriteLine($"{detectedFaces.Count} faces detected from image `{imageFileName}`.");
			}

			// Detect faces from source image file 1.
			List<DetectedFace> detectedFaces1 = await DetectFaceRecognize(client, $"{url}{sourceImageFileName1} ", recognitionModel02);
			Console.WriteLine($"{detectedFaces1.Count} faces detected from image `{sourceImageFileName1}`.");
			Guid sourceFaceId1 = detectedFaces1[0].FaceId.Value;

			// Detect faces from source image file 2.
			List<DetectedFace> detectedFaces2 = await DetectFaceRecognize(client, $"{url}{sourceImageFileName2} ", recognitionModel02);
			Console.WriteLine($"{detectedFaces2.Count} faces detected from image `{sourceImageFileName2}`.");
			Guid sourceFaceId2 = detectedFaces2[0].FaceId.Value;

			// Verification example for faces of the same person.
			VerifyResult verifyResult1 = await client.Face.VerifyFaceToFaceAsync(sourceFaceId1, targetFaceIds[0]);
			Console.WriteLine(
				verifyResult1.IsIdentical
					? $"Faces from {sourceImageFileName1} & {targetImageFileNames[0]} are of the same (Positive) person, similarity confidence: {verifyResult1.Confidence}."
					: $"Faces from {sourceImageFileName1} & {targetImageFileNames[0]} are of different (Negative) persons, similarity confidence: {verifyResult1.Confidence}.");

			// Verification example for faces of different persons.
			VerifyResult verifyResult2 = await client.Face.VerifyFaceToFaceAsync(sourceFaceId2, targetFaceIds[0]);
			Console.WriteLine(
				verifyResult2.IsIdentical
					? $"Faces from {sourceImageFileName2} & {targetImageFileNames[0]} are of the same (Negative) person, similarity confidence: {verifyResult2.Confidence}."
					: $"Faces from {sourceImageFileName2} & {targetImageFileNames[0]} are of different (Positive) persons, similarity confidence: {verifyResult2.Confidence}.");

			Console.WriteLine();
		}
		/*
		 * END - VERIFY 
		 */

		/*
		 * IDENTIFY FACES
		 * To identify faces, you need to create and define a person group.
		 * The Identify operation takes one or several face IDs from DetectedFace or PersistedFace and a PersonGroup and returns 
		 * a list of Person objects that each face might belong to. Returned Person objects are wrapped as Candidate objects, 
		 * which have a prediction confidence value.
		 */
		public static async Task IdentifyInPersonGroup(IFaceClient client, string url, string RECOGNITION_MODEL1)
		{
			Console.WriteLine("========IDENTIFY FACES========");

			// Create a dictionary for all your images, grouping similar ones under the same key.
			Dictionary<string, string[]> targetImageFileDictionary =
				new Dictionary<string, string[]>
					{ { "Family1-Dad", new[] { "Family1-Dad1.jpg", "Family1-Dad2.jpg" } },
					  { "Family1-Mom", new[] { "Family1-Mom1.jpg", "Family1-Mom2.jpg" } },
					  { "Family1-Son", new[] { "Family1-Son1.jpg", "Family1-Son2.jpg" } },
					  { "Family1-Daughter", new[] { "Family1-Daughter1.jpg", "Family1-Daughter2.jpg" } },
					  { "Family2-Lady", new[] { "Family2-Lady1.jpg", "Family2-Lady2.jpg" } },
					  { "Family2-Man", new[] { "Family2-Man1.jpg", "Family2-Man2.jpg" } }
					};
			// A group photo that includes some of the persons you seek to identify from your dictionary.
			string sourceImageFileName = "identification1.jpg";

			// Create a person group. 
			string personGroupId = Guid.NewGuid().ToString();
			sourcePersonGroup = personGroupId;
			Console.WriteLine($"Create a person group ({personGroupId}).");
			await client.PersonGroup.CreateAsync(personGroupId, personGroupId, recognitionModel: RECOGNITION_MODEL1);
			// The similar faces will be grouped into a single person group person.
			foreach (var targetImageFileDictionaryName in targetImageFileDictionary.Keys)
			{
				// Create a person group person.
				Person person = new Person { Name = targetImageFileDictionaryName, UserData = "Person for example" };
				// Limit TPS
				await Task.Delay(250);
				person.PersonId = (await client.PersonGroupPerson.CreateAsync(personGroupId, person.Name)).PersonId;
				Console.WriteLine($"Create a person group person '{person.Name}'.");

				// Add face to the person group person.
				foreach (var targetImageFileName in targetImageFileDictionary[targetImageFileDictionaryName])
				{
					Console.WriteLine($"Add face to the person group person({targetImageFileDictionaryName}) from image `{targetImageFileName}`");
					PersistedFace face = await client.PersonGroupPerson.AddFaceFromUrlAsync(personGroupId, person.PersonId,
						$"{url}{targetImageFileName}", targetImageFileName);
				}
			}

			// Start to train the person group.
			Console.WriteLine();
			Console.WriteLine($"Train person group {personGroupId}.");
			await client.PersonGroup.TrainAsync(personGroupId);

			// Wait until the training is completed.
			while (true)
			{
				await Task.Delay(1000);
				var trainingStatus = await client.PersonGroup.GetTrainingStatusAsync(personGroupId);
				Console.WriteLine($"Training status: {trainingStatus.Status}.");
				if (trainingStatus.Status == TrainingStatusType.Succeeded) { break; }
			}
			Console.WriteLine();
			List<Guid> sourceFaceIds = new List<Guid>();
			// Detect faces from source image url.
			List<DetectedFace> detectedFaces = await DetectFaceRecognize(client, $"{url}{sourceImageFileName}", RECOGNITION_MODEL1);

			// Add detected faceId to sourceFaceIds.
			foreach (var detectedFace in detectedFaces) { sourceFaceIds.Add(detectedFace.FaceId.Value); }

			// Identify the faces in a person group. 
			var identifyResults = await client.Face.IdentifyAsync(sourceFaceIds, personGroupId);

			foreach (var identifyResult in identifyResults)
			{
				Person person = await client.PersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);
				Console.WriteLine($"Person '{person.Name}' is identified for face in: {sourceImageFileName} - {identifyResult.FaceId}," +
					$" confidence: {identifyResult.Candidates[0].Confidence}.");
			}
			Console.WriteLine();
		}
		/*
		 * END - IDENTIFY FACES
		 */

		/*
		 * GROUP FACES
		 * This method of grouping is useful if you don't need to create a person group. It will automatically group similar
		 * images, whereas the person group method allows you to define the grouping.
		 * A single "messyGroup" array contains face IDs for which no similarities were found.
		 */
		public static async Task Group(IFaceClient client, string url, string RECOGNITION_MODEL1)
		{
			Console.WriteLine("========GROUP FACES========");
			// Create list of image names
			List<string> imageFileNames = new List<string>
							  {
								  "Family1-Dad1.jpg",
								  "Family1-Dad2.jpg",
								  "Family3-Lady1.jpg",
								  "Family1-Daughter1.jpg",
								  "Family1-Daughter2.jpg",
								  "Family1-Daughter3.jpg"
							  };
			// Create empty dictionary to store the groups
			Dictionary<string, string> faces = new Dictionary<string, string>();
			List<Guid> faceIds = new List<Guid>();

			// First, detect the faces in your images
			foreach (var imageFileName in imageFileNames)
			{
				// Detect faces from image url.
				IList<DetectedFace> detectedFaces = await DetectFaceRecognize(client, $"{url}{imageFileName}", RECOGNITION_MODEL1);
				// Add detected faceId to faceIds and faces.
				faceIds.Add(detectedFaces[0].FaceId.Value);
				faces.Add(detectedFaces[0].FaceId.ToString(), imageFileName);
			}
			Console.WriteLine();
			// Group the faces. Grouping result is a group collection, each group contains similar faces.
			var groupResult = await client.Face.GroupAsync(faceIds);

			// Face groups contain faces that are similar to all members of its group.
			for (int i = 0; i < groupResult.Groups.Count; i++)
			{
				Console.Write($"Found face group {i + 1}: ");
				foreach (var faceId in groupResult.Groups[i]) { Console.Write($"{faces[faceId.ToString()]} "); }
				Console.WriteLine(".");
			}

			// MessyGroup contains all faces which are not similar to any other faces. The faces that cannot be grouped.
			if (groupResult.MessyGroup.Count > 0)
			{
				Console.Write("Found messy face group: ");
				foreach (var faceId in groupResult.MessyGroup) { Console.Write($"{faces[faceId.ToString()]} "); }
				Console.WriteLine(".");
			}
			Console.WriteLine();
		}
		/*
		 * END - GROUP FACES
		 */

		/*
		 * SNAPSHOT OPERATIONS
		 * Copies a person group from one Azure region (or subscription) to another. For example: from the EastUS region to the WestUS.
		 * The same process can be used for face lists. 
		 * NOTE: the person group in the target region has a new person group ID, so it no longer associates with the source person group.
		 */
		public static async Task Snapshot(IFaceClient clientSource, IFaceClient clientTarget, string personGroupId, Guid azureId)
		{
			Console.WriteLine("========SNAPSHOT OPERATIONS========");
			// Take a snapshot for the person group that was previously created in your source region.
			var takeSnapshotResult = await clientSource.Snapshot.TakeAsync(SnapshotObjectType.PersonGroup, personGroupId, new[] { azureId });
			// Get operation id from response for tracking the progress of snapshot taking.
			var operationId = Guid.Parse(takeSnapshotResult.OperationLocation.Split('/')[2]);
			Console.WriteLine($"Taking snapshot(operation ID: {operationId})... Started");

			// Wait for taking the snapshot to complete.
			OperationStatus operationStatus = null;
			do
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(1000));
				// Get the status of the operation.
				operationStatus = await clientSource.Snapshot.GetOperationStatusAsync(operationId);
				Console.WriteLine($"Operation Status: {operationStatus.Status}");
			}
			while (operationStatus.Status != OperationStatusType.Succeeded && operationStatus.Status != OperationStatusType.Failed);
			// Confirm the location of the resource where the snapshot is taken and its snapshot ID
			var snapshotId = Guid.Parse(operationStatus.ResourceLocation.Split('/')[2]);
			Console.WriteLine($"Source region snapshot ID: {snapshotId}");
			Console.WriteLine($"Taking snapshot of person group: {personGroupId}... Done\n");

			// Apply the snapshot in target region, with a new ID.
			var newPersonGroupId = Guid.NewGuid().ToString();
			targetPersonGroup = newPersonGroupId;

			try
			{
				var applySnapshotResult = await clientTarget.Snapshot.ApplyAsync(snapshotId, newPersonGroupId);

				// Get operation id from response for tracking the progress of snapshot applying.
				var applyOperationId = Guid.Parse(applySnapshotResult.OperationLocation.Split('/')[2]);
				Console.WriteLine($"Applying snapshot(operation ID: {applyOperationId})... Started");
				// Wait for applying operation to complete
				do
				{
					Thread.Sleep(TimeSpan.FromMilliseconds(1000));
					// Get the status of the operation.
					operationStatus = await clientSource.Snapshot.GetOperationStatusAsync(applyOperationId);
					Console.WriteLine($"Operation Status: {operationStatus.Status}");
				}
				while (operationStatus.Status != OperationStatusType.Succeeded && operationStatus.Status != OperationStatusType.Failed);
				// Confirm location of the target resource location, with its ID.
				Console.WriteLine($"Person group in new region: {newPersonGroupId}");
				Console.WriteLine("Applying snapshot... Done\n");
			}
			catch (Exception e)
			{
				throw new ApplicationException("Do you have a second Face resource in Azure? " +
					"It's needed to transfer the person group to it for the Snapshot example.", e);
			}
		}
		/*
		 * END - SNAPSHOT OPERATIONS 
		 */

		/*
		 * DELETE PERSON GROUP
		 * After this entire example is executed, delete the person group in your Azure account,
		 * otherwise you cannot recreate one with the same name (if running example repeatedly).
		 */
		public static async Task DeletePersonGroup(IFaceClient client, String personGroupId)
		{
			Console.WriteLine("Delete started... ");

			await client.PersonGroup.DeleteAsync(personGroupId);
			Console.WriteLine($"Deleted the person group {personGroupId}.");
			Console.WriteLine();
		}
		/*
		 * END - DELETE PERSON GROUP
		 */
	}
}
