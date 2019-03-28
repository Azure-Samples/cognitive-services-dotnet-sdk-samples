namespace FaceApiSnapshotSample
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Vision.Face;
    using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
    using Newtonsoft.Json;

    public class Program
    {
        public static void Main(string[] args)
        {
            new Sample().Run().GetAwaiter().GetResult();
            Console.ReadKey();
        }
    }

    /// <summary>
    /// A sample of migrating a person group from source subscription in East Asia region to target subscription in West US Region.
    /// You can specify the source and target subscription with keys and Azure face subscription ids by your actual needs,
    /// and the source and target subscription can also be the same subscription.
    /// </summary>
    public class Sample
    {
        /// <summary>
        /// RecognitionModel for Face detection and PersonGroup.
        /// </summary>
        private static readonly string recognitionModel = RecognitionModel.Recognition02;

        /// <summary>
        /// Source endpoint in East Asia Region.
        /// </summary>
        private const string SourceEastAsiaEndpoint = "https://southeastasia.api.cognitive.microsoft.com/";

        /// <summary>
        /// Source subscription key in East Asia Region.
        /// </summary>
        private const string SourceEastAsiaKey = "<East Asia Subscription Key>";

        /// <summary>
        /// Target endpoint in West US Region.
        /// </summary>
        private const string TargetWestUSEndpoint = "https://westus.api.cognitive.microsoft.com/";

        /// <summary>
        /// Target subscription key in West US Region.
        /// </summary>
        private const string TargetWestUSKey = "<West US Subscription Key>";

        /// <summary>
        /// Target subscription key related azure subscription ID in West US Region.
        /// </summary>
        private static readonly Guid TargetWestUSAzureSubscriptionId = new Guid("<Azure West US Subscription ID>");

        /// <summary>
        /// Initializes a new instance of the <see cref="Sample"/> class.
        /// </summary>
        public Sample()
        {
            FaceClientEastAsia = new FaceClient(new ApiKeyServiceClientCredentials(SourceEastAsiaKey))
            {
                Endpoint = SourceEastAsiaEndpoint
            };

            FaceClientWestUS = new FaceClient(new ApiKeyServiceClientCredentials(TargetWestUSKey))
            {
                Endpoint = TargetWestUSEndpoint
            };
        }

        private FaceClient FaceClientEastAsia { get; }

        private FaceClient FaceClientWestUS { get; }

        public async Task Run()
        {
            // Step 1, create a person group in source East Asia region.
            var personGroupId = await CreatePersonGroup(FaceClientEastAsia);

            await DisplayPersonGroup(FaceClientEastAsia, personGroupId);
            await IdentifyInPersonGroup(FaceClientEastAsia, personGroupId);

            // Step 2, take a snapshot for the created person group.
            var takeSnapshotResult = await FaceClientEastAsia.Snapshot.TakeAsync(
                SnapshotObjectType.PersonGroup,
                personGroupId,
                new[] { TargetWestUSAzureSubscriptionId });

            // Get operation id from response for tracking the progress of snapshot taking.
            var takeOperationId = Guid.Parse(takeSnapshotResult.OperationLocation.Split('/')[2]);
            Console.WriteLine($"Taking snapshot(operation ID: {takeOperationId})... Started");

            // Step 3, wait for taking snapshot to complete.
            var operationStatus = await WaitForOperation(FaceClientEastAsia, takeOperationId);

            if (operationStatus.Status == OperationStatusType.Succeeded)
            {
                Console.WriteLine($"Resource Location: {operationStatus.ResourceLocation}");
            }
            else
            {
                Console.WriteLine($"Failed message: {operationStatus.Message}");
                return;
            }

            // Get snapshot id from response.
            var snapshotId = Guid.Parse(operationStatus.ResourceLocation.Split('/')[2]);
            Console.WriteLine($"Snapshot ID: {snapshotId}");
            Console.WriteLine("Taking snapshot... Done\n");

            // Step 4, apply the snapshot in target West US region.
            var newPersonGroupId = Guid.NewGuid().ToString();
            var applySnapshotResult = await FaceClientWestUS.Snapshot.ApplyAsync(snapshotId, newPersonGroupId);

            // Get operation id from response for tracking the progress of snapshot applying.
            var applyOperationId = Guid.Parse(applySnapshotResult.OperationLocation.Split('/')[2]);
            Console.WriteLine($"Applying snapshot(operation ID: {applyOperationId})... Started");

            // Step 5, wait for applying snapshot to complete.
            operationStatus = await WaitForOperation(FaceClientWestUS, applyOperationId);

            if (operationStatus.Status == OperationStatusType.Succeeded)
            {
                Console.WriteLine($"Resource Location: {operationStatus.ResourceLocation}");
            }
            else
            {
                Console.WriteLine($"Failed message: {operationStatus.Message}");
                return;
            }

            Console.WriteLine("Applying snapshot... Done\n");

            await DisplayPersonGroup(FaceClientWestUS, newPersonGroupId);

            // No need to retrain the person group before identification, 
            // training results are copied by snapshot as well.
            await IdentifyInPersonGroup(FaceClientWestUS, newPersonGroupId);

            // Step 6, delete test resources.
            await FaceClientEastAsia.PersonGroup.DeleteAsync(personGroupId);
            await FaceClientEastAsia.Snapshot.DeleteAsync(snapshotId);

            await FaceClientWestUS.PersonGroup.DeleteAsync(newPersonGroupId);

            Console.WriteLine("Delete test resource... Done");
        }

        /// <summary>
        /// Creates a PersonGroup.
        /// </summary>
        /// <returns>The PersonGroup ID.</returns>
        private static async Task<string> CreatePersonGroup(IFaceClient client)
        {
            // Create a PersonGroup.
            var personGroupId = Guid.NewGuid().ToString();
            await client.PersonGroup.CreateAsync(personGroupId, "test", recognitionModel: recognitionModel);
            Console.WriteLine("Creating person group... Done");
            Console.WriteLine($"Person group ID: {personGroupId}\n");

            // Add persons.
            var personFolders = Directory.EnumerateDirectories("data\\PersonGroup");
            foreach (var personFolder in personFolders)
            {
                var person = await client.PersonGroupPerson.CreateAsync(
                                 personGroupId,
                                 Path.GetFileNameWithoutExtension(personFolder));

                var faceFiles = Directory.EnumerateFiles(personFolder, "*.*", SearchOption.AllDirectories).Where(
                    filename => string.Compare(".jpg", Path.GetExtension(filename), StringComparison.OrdinalIgnoreCase)
                                == 0).ToList();

                // Add faces.
                foreach (var faceFile in faceFiles)
                {
                    using (var fileStream = new FileStream(faceFile, FileMode.Open, FileAccess.Read))
                    {
                        await client.PersonGroupPerson.AddFaceFromStreamAsync(
                            personGroupId,
                            person.PersonId,
                            fileStream);
                    }
                }
            }

            // Train the PersonGroup for identification usage.
            await client.PersonGroup.TrainAsync(personGroupId);
            Console.WriteLine("Training PersonGroup... Started");

            // Wait for training to complete.
            TrainingStatus trainingStatus = null;
            do
            {
                if (trainingStatus != null)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                }

                trainingStatus = await client.PersonGroup.GetTrainingStatusAsync(personGroupId);
                Console.WriteLine($"Training Status: {trainingStatus.Status}");
            }
            while (trainingStatus.Status != TrainingStatusType.Failed
                   && trainingStatus.Status != TrainingStatusType.Succeeded);

            Console.WriteLine("Training PersonGroup... Done\n");

            return personGroupId;
        }

        private static async Task DisplayPersonGroup(IFaceClient client, string personGroupId)
        {
            var personGroup = await client.PersonGroup.GetAsync(personGroupId);
            Console.WriteLine("Person Group:");
            Console.WriteLine(JsonConvert.SerializeObject(personGroup));

            // List persons.
            var persons = await client.PersonGroupPerson.ListAsync(personGroupId);

            foreach (var person in persons)
            {
                Console.WriteLine(JsonConvert.SerializeObject(person));
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Identification against the person group.
        /// </summary>
        private static async Task IdentifyInPersonGroup(IFaceClient client, string personGroupId)
        {
            using (var fileStream = new FileStream("data\\PersonGroup\\Daughter\\Daughter1.jpg", FileMode.Open, FileAccess.Read))
            {
                var detectedFaces = await client.Face.DetectWithStreamAsync(fileStream, recognitionModel: recognitionModel);

                var result = await client.Face.IdentifyAsync(detectedFaces.Select(face => face.FaceId).Where(faceId => faceId != null).Select(faceId => faceId.Value).ToList(), personGroupId);
                Console.WriteLine("Test identify against PersonGroup");
                Console.WriteLine(JsonConvert.SerializeObject(result));
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Waits for the take/apply operation to complete and returns the final operation status.
        /// </summary>
        /// <returns>The final operation status.</returns>
        private static async Task<OperationStatus> WaitForOperation(IFaceClient client, Guid operationId)
        {
            OperationStatus operationStatus = null;
            do
            {
                if (operationStatus != null)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                }

                // Get the status of the operation.
                operationStatus = await client.Snapshot.GetOperationStatusAsync(operationId);

                Console.WriteLine($"Operation Status: {operationStatus.Status}");
            }
            while (operationStatus.Status != OperationStatusType.Succeeded
                   && operationStatus.Status != OperationStatusType.Failed);

            return operationStatus;
        }
    }
}
