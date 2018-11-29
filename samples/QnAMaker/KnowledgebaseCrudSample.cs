using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.QnAMaker
{
    public static class KnowledgebaseCrudSample
    {
        public static async Task Run(string key, string endpoint)
        {
            var client = new QnAMakerClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
            // Create a KB
            Console.WriteLine("Creating KB...");
            var kbId = await CreateSampleKb(client);
            Console.WriteLine("Created KB with ID : {0}", kbId);

            // Update the KB
            Console.WriteLine("Updating KB...");
            await UpdateKB(client, kbId);
            Console.WriteLine("KB Updated.");

            // Publish the KB
            Console.Write("Publishing KB...");
            await client.Knowledgebase.PublishAsync(kbId);
            Console.WriteLine("KB Published.");


            // Download the KB
            Console.Write("Downloading KB...");
            var kbData = await client.Knowledgebase.DownloadAsync(kbId, EnvironmentType.Prod);
            Console.WriteLine("KB Downloaded. It has {0} QnAs.", kbData.QnaDocuments.Count);

            // Delete the KB
            Console.Write("Deleting KB...");
            await client.Knowledgebase.DeleteAsync(kbId);
            Console.WriteLine("KB Deleted.");
        }

        private static async Task UpdateKB(IQnAMakerClient client, string kbId)
        {
            var updateOp = await client.Knowledgebase.UpdateAsync(kbId, new UpdateKbOperationDTO
            {
                Add = new UpdateKbOperationDTOAdd { QnaList = new List<QnADTO> { new QnADTO { Questions = new List<string> { "bye" }, Answer = "goodbye" } } }
            });

            // Loop while operation is success
            updateOp = await MonitorOperation(client, updateOp);
        }

        private static async Task<string> CreateSampleKb(IQnAMakerClient client)
        {
            var qna = new QnADTO
            {
                Answer = "You can use our REST APIs to manage your knowledge base.",
                Questions = new List<string> { "How do I manage my knowledgebase?" },
                Metadata = new List<MetadataDTO> { new MetadataDTO { Name = "Category", Value = "api" } }
            };

            var urls = new List<string> { "https://docs.microsoft.com/en-in/azure/cognitive-services/qnamaker/faqs" };
            var createKbDto = new CreateKbDTO
            {
                Name = "QnA Maker FAQ from quickstart",
                QnaList = new List<QnADTO> { qna },
                Urls = urls
            };

            var createOp = await client.Knowledgebase.CreateAsync(createKbDto);
            createOp = await MonitorOperation(client, createOp);

            return createOp.ResourceLocation.Replace("/knowledgebases/", string.Empty);
        }

        private static async Task<Operation> MonitorOperation(IQnAMakerClient client, Operation operation)
        {
            // Loop while operation is success
            for (int i = 0;
                i < 20 && (operation.OperationState == OperationStateType.NotStarted || operation.OperationState == OperationStateType.Running);
                i++)
            {
                Console.WriteLine("Waiting for operation: {0} to complete.", operation.OperationId);
                await Task.Delay(5000);
                operation = await client.Operations.GetDetailsAsync(operation.OperationId);
            }

            if (operation.OperationState != OperationStateType.Succeeded)
            {
                throw new Exception($"Operation {operation.OperationId} failed to completed.");
            }
            return operation;
        }

    }
}
