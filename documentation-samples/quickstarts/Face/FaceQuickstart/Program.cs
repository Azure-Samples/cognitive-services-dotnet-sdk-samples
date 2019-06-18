using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace FaceQuickstart
{
	class Program
	{
		// The single person group for entire sample
		static string sourcePersonGroup = null;
		static string targetPersonGroup = null;
		// URL to get images fom
		const string IMAGE_BASE_URL = "https://csdx.blob.core.windows.net/resources/Face/Images/";

		static void Main(string[] args)
		{
			// After creating a Face resource in the Azure portal, get a subscription key (on the Keys page) and location (on the Overview page).
			// Add environment variables to your local machine with your key and location as values. Close and reopen this project for changes to take effect.
			string subscriptionKey = Environment.GetEnvironmentVariable("FACE_SUBSCRIPTION_KEY");
			string endpoint = $"https://{Environment.GetEnvironmentVariable("FACE_REGION")}.api.cognitive.microsoft.com";
			// Create another Face resource in Azure (used for Snapshot sample) with a different region than your main one.
			// Name the key FACE_SUBSCRIPTION_KEY2 and name the endpoint FACE_REGION2 in your environment variables,
			// with your 2nd subscription key and 2nd endpoint region as values.
			string targetSubscriptionKey = Environment.GetEnvironmentVariable("FACE_SUBSCRIPTION_KEY2");
			string targetEndpoint = $"https://{Environment.GetEnvironmentVariable("FACE_REGION2")}.api.cognitive.microsoft.com";
			// Grab your subscription ID, from any resource in Azure, from the Overview page (all resources have the same subscription ID). 
			// Add the ID to your environment variables as a value.
			Guid azureSubscriptionId = new Guid(Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID"));

			// Recognition model 2 is used for feature extraction, use 1 to simply recognize a face. 
			// However, the API calls to Detection that are used with Verify, Find Simlar, or Identify must share the same recognition model.
			const string recognitionModel2 = RecognitionModel.Recognition02;
			const string recognitionModel1 = RecognitionModel.Recognition01;

			// Authenticate.
			IFaceClient client = Authenticate(endpoint, subscriptionKey);
			// Authenticate for another region
			IFaceClient clientTarget = Authenticate(targetEndpoint, targetSubscriptionKey);
			// Extract features from faces.
			DetectFaceExtract(client, recognitionModel2).Wait();
			// Find a similar face from a list of faces.
			FindSimilar(client, recognitionModel1).Wait();
			// Identify a face(s) in a person group (created in this sample).
			IdentifyInPersonGroup(client, recognitionModel1).Wait();
			// Automatically group similar faces
			Group(client, IMAGE_BASE_URL, recognitionModel1).Wait();
			// Take a snapshot of a person group in one region, move it to the next
			//Snapshot(client2, recognitionModel1, samplePersonGroup, azureSubscriptionId).Wait();
			Snapshot(client, clientTarget, sourcePersonGroup, azureSubscriptionId).Wait();

			// At end, delete person groups in both regions (since testing only)
			Console.WriteLine("========Sample of deleting a person group========");
			DeletePersonGroup(client, sourcePersonGroup).Wait();
			DeletePersonGroup(clientTarget, targetPersonGroup).Wait();

			Console.WriteLine("End of sample.");
			Console.WriteLine("Select ENTER key to exit...");
			Console.ReadLine();
		}

		/*
		 * Authenticate with Face subscription key and endpoint
		 */
		public static IFaceClient Authenticate(string endpoint, string key)
		{
			return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint }; 
		}

		/* 
		 * Detect faces from URL images
		 */
		public static async Task DetectFaceExtract(IFaceClient client, string recognitionModel)
		{
			Console.WriteLine("========Sample of face detection========");

			// Create a list of images
			const string IMAGE_BASE_URL = "https://csdx.blob.core.windows.net/resources/Face/Images/";
			List<string> imageFileNames = new List<string>
										{
											"detection1.jpg",    // single female with glasses
											// "detection2.jpg", // (optional: single man)
											// "detection3.jpg", // (optional: single male construction worker)
											// "detection4.jpg", // (optional: 3 people at cafe, 1 is blurred)
											"detection5.jpg",    // black family, woman child man
											"detection6.jpg"     // elderly couple, male female
										};

			foreach (var imageFileName in imageFileNames)
			{
				IList<DetectedFace> detectedFaces;

				// Detect faces with all attributes from image url.
				detectedFaces = await client.Face.DetectWithUrlAsync($"{IMAGE_BASE_URL}{imageFileName}",
											returnFaceAttributes: new List <FaceAttributeType> { FaceAttributeType.Accessories, FaceAttributeType.Age,
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
					else { for (int i = 0; i < count; ++i) { accessoryArray[i] = accessoriesList[i].Type.ToString(); }
						accessory = string.Join(",", accessoryArray); }
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
					foreach (HairColor hairColor in hair.HairColor) { if (hairColor.Confidence <= maxConfidence) { continue; }
						maxConfidence = hairColor.Confidence; returnColor = hairColor.Color; color = returnColor.ToString(); }
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

		// Detect faces from image url for recognition purpose.
		// Parameter `returnFaceId` of `DetectWithUrlAsync` must be set to `true` (by default) for recognition purpose.
		// The field `faceId` in returned `DetectedFace`s will be used in Face - Identify, Face - Verify, and Face - Find Similar.
		// It will expire 24 hours after the detection call.
		private static async Task<List<DetectedFace>> DetectFaceRecognize(IFaceClient faceClient, string imageUrl, string recognitionModel1)
		{
			// Detect faces from image URL. Since only recognizing, use the recognition model 1.
			IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithUrlAsync(imageUrl, recognitionModel: recognitionModel1);
			Console.WriteLine($"{detectedFaces.Count} face(s) detected from image `{imageUrl}`.");
			return detectedFaces.ToList();
		}

		/*
		 * Find similar face
		 */
		public static async Task FindSimilar(IFaceClient client, string recognitionModel1)
		{
			Console.WriteLine("========Sample of finding similar faces in another photo========");

			const string IMAGE_BASE_URL = "https://csdx.blob.core.windows.net/resources/Face/Images/";
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
				var faces = await DetectFaceRecognize(client, $"{IMAGE_BASE_URL}{targetImageFileName}", recognitionModel1);
				// Add detected faceId to list of GUIDs.
				targetFaceIds.Add(faces[0].FaceId.Value);
			}

			// Detect faces from source image url.
			IList<DetectedFace> detectedFaces = await DetectFaceRecognize(client, $"{IMAGE_BASE_URL}{sourceImageFileName}", recognitionModel1);
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
		 * Idenitfy faces
		 * To identify faces, you need to create and define a person group.
		 */
		public static async Task IdentifyInPersonGroup(IFaceClient client, string recognitionModel1)
		{
			Console.WriteLine("========Sample of identifing faces in a person group========");

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
			await client.PersonGroup.CreateAsync(personGroupId, personGroupId, recognitionModel: recognitionModel1);
			// The similar faces will be grouped into a single person group person.
			foreach (var targetImageFileDictionaryName in targetImageFileDictionary.Keys)
			{
				// Create a person group person.
				Person person = new Person { Name = targetImageFileDictionaryName, UserData = "Person for sample" };
				// Limit TPS
				await Task.Delay(250);
				person.PersonId = (await client.PersonGroupPerson.CreateAsync(personGroupId, person.Name)).PersonId;
				Console.WriteLine($"Create a person group person '{person.Name}'.");

				// Add face to the person group person.
				foreach (var targetImageFileName in targetImageFileDictionary[targetImageFileDictionaryName])
				{   Console.WriteLine($"Add face to the person group person({targetImageFileDictionaryName}) from image `{targetImageFileName}`.");
					PersistedFace face = await client.PersonGroupPerson.AddFaceFromUrlAsync(personGroupId, person.PersonId, 
						$"{IMAGE_BASE_URL}{targetImageFileName}", targetImageFileName);
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
			List<DetectedFace> detectedFaces = await DetectFaceRecognize(client, $"{IMAGE_BASE_URL}{sourceImageFileName}", recognitionModel1);

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
		 * Group images
		 * This method of grouping is useful if you don't need to create a person group. It will automatically group similar
		 * images based on IDs, whereas the person group method allows you to define the groups.
		 */
		public static async Task Group(IFaceClient client, string imageUrlBase, string recognitionModel1)
		{
			Console.WriteLine("========Sample of grouping faces========");
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
				IList<DetectedFace> detectedFaces = await DetectFaceRecognize(client, $"{imageUrlBase}{imageFileName}", recognitionModel1);
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
		 * Take a snapshot of a person group
		 * This sample uses a pre-existing person group and copies it from one Azure region to another. For example: from the EastUS region to the WestUS region
		 * The same process can be used for face lists. 
		 * NOTE: a copy of the person group in the target region has a new person group ID, so it no longer associates with the source person group.
		 */
		public static async Task Snapshot(IFaceClient clientSource, IFaceClient clientTarget, string personGroupId, Guid azureSubscriptionId)
		{
			Console.WriteLine("========Sample of creating a snapshot========");
			// Take a snapshot for the person group that was previously created in your source region.
			var takeSnapshotResult = await clientSource.Snapshot.TakeAsync(SnapshotObjectType.PersonGroup, personGroupId, new[] { azureSubscriptionId });
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

		/*
		 * Delete person group 
		 * After this entire sample is executed, delete the person group in your Azure account,
		 * otherwise you cannot recreate one with the same name (if running sample repeatedly).
		 */
		public static async Task DeletePersonGroup(IFaceClient client, String personGroupId)
		{
			// First, list the person groups in each region
			Console.WriteLine("Delete started... ");
			IList<PersonGroup> list = await client.PersonGroup.ListAsync();

			await client.PersonGroup.DeleteAsync(personGroupId);
			Console.WriteLine($"Deleted the person group {personGroupId}.");
			Console.WriteLine();
		}
	}
}
