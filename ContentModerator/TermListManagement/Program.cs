using System;
using System.Threading;
using System.IO;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator.Models;
using System.Text;

namespace TermLists
{
    class Program
    {
        // NOTE: Replace this with the appropriate language for your region.
        /// <summary>
        /// The language of the terms in the term lists.
        /// </summary>
        private const string lang = "eng";

        /// <summary>
        /// The minimum amount of time, in milliseconds, to wait between calls
        /// to the Content Moderator APIs.
        /// </summary>
        private const int throttleRate = 3000;

        /// <summary>
        /// The number of minutes to delay after updating the search index before
        /// performing image match operations against the list.
        /// </summary>
        private const double latencyDelay = 0.5;

        /// <summary>
        /// Creates a new term list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <returns>The term list ID.</returns>
        static string CreateTermList (ContentModeratorClient client)
        {
            Console.WriteLine("Creating term list.");

            Body body = new Body("Term list name", "Term list description");
            TermList list = client.ListManagementTermLists.Create("application/json", body);
            if (false == list.Id.HasValue)
            {
                throw new Exception("TermList.Id value missing.");
            }
            else
            {
                string list_id = list.Id.Value.ToString();
                Console.WriteLine("Term list created. ID: {0}.", list_id);
                Thread.Sleep(throttleRate);
                return list_id;
            }
            
        }

        /// <summary>
        /// Update the information for the indicated term list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="list_id">The ID of the term list to update.</param>
        /// <param name="name">The new name for the term list.</param>
        /// <param name="description">The new description for the term list.</param>
        static void UpdateTermList (ContentModeratorClient client, string list_id, string name = null, string description = null)
        {
            Console.WriteLine("Updating information for term list with ID {0}.", list_id);
            Body body = new Body(name, description);
            client.ListManagementTermLists.Update(list_id, "application/json", body);
            Thread.Sleep(throttleRate);
        }

        /// <summary>
        /// Add a term to the indicated term list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="list_id">The ID of the term list to update.</param>
        /// <param name="term">The term to add to the term list.</param>
        static void AddTerm (ContentModeratorClient client, string list_id, string term)
        {
            Console.WriteLine("Adding term \"{0}\" to term list with ID {1}.", term, list_id);
            client.ListManagementTerm.AddTerm(list_id, term, lang);
            Thread.Sleep(throttleRate);
        }

        /// <summary>
        /// Get all terms in the indicated term list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="list_id">The ID of the term list from which to get all terms.</param>
        static void GetAllTerms(ContentModeratorClient client, string list_id)
        {
            Console.WriteLine("Getting terms in term list with ID {0}.", list_id);
            Terms terms = client.ListManagementTerm.GetAllTerms(list_id, lang);
            TermsData data = terms.Data;
            foreach (TermsInList term in data.Terms)
            {
                Console.WriteLine(term.Term);
            }
            Thread.Sleep(throttleRate);
        }

        /// <summary>
        /// Refresh the search index for the indicated term list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="list_id">The ID of the term list to refresh.</param>
        static void RefreshSearchIndex (ContentModeratorClient client, string list_id)
        {
            Console.WriteLine("Refreshing search index for term list with ID {0}.", list_id);
            client.ListManagementTermLists.RefreshIndexMethod(list_id, lang);
            Thread.Sleep((int)(latencyDelay * 60 * 1000));
        }

        /// <summary>
        /// Screen the indicated text for terms in the indicated term list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="list_id">The ID of the term list to use to screen the text.</param>
        /// <param name="text">The text to screen.</param>
        static void ScreenText (ContentModeratorClient client, string list_id, string text)
        {
            Console.WriteLine("Screening text: \"{0}\" using term list with ID {1}.", text, list_id);
            Screen screen = client.TextModeration.ScreenText("text/plain", new MemoryStream(Encoding.UTF8.GetBytes(text)), lang, false, false, list_id);
            if (null == screen.Terms)
            {
                Console.WriteLine("No terms from the term list were detected in the text.");
            }
            else
            {
                foreach (DetectedTerms term in screen.Terms)
                {
                    Console.WriteLine(String.Format("Found term: \"{0}\" from list ID {1} at index {2}.", term.Term, term.ListId, term.Index));
                }
            }
            Thread.Sleep(throttleRate);
        }

        /// <summary>
        /// Delete a term from the indicated term list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="list_id">The ID of the term list from which to delete the term.</param>
        /// <param name="term">The term to delete.</param>
        static void DeleteTerm (ContentModeratorClient client, string list_id, string term)
        {
            Console.WriteLine("Removed term \"{0}\" from term list with ID {1}.", term, list_id);
            client.ListManagementTerm.DeleteTerm(list_id, term, lang);
            Thread.Sleep(throttleRate);
        }

        /// <summary>
        /// Delete all terms from the indicated term list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="list_id">The ID of the term list from which to delete all terms.</param>
        static void DeleteAllTerms (ContentModeratorClient client, string list_id)
        {
            Console.WriteLine("Removing all terms from term list with ID {0}.", list_id);
            client.ListManagementTerm.DeleteAllTerms(list_id, lang);
            Thread.Sleep(throttleRate);
        }

        /// <summary>
        /// Delete the indicated term list.
        /// </summary>
        /// <param name="client">The Content Moderator client.</param>
        /// <param name="list_id">The ID of the term list to delete.</param>
        static void DeleteTermList (ContentModeratorClient client, string list_id)
        {
            Console.WriteLine("Deleting term list with ID {0}.", list_id);
            client.ListManagementTermLists.Delete(list_id);
            Thread.Sleep(throttleRate);
        }

        static void Main(string[] args)
        {
            using (var client = Clients.NewClient())
            {
                string list_id = CreateTermList(client);

                UpdateTermList(client, list_id, "name", "description");
                AddTerm(client, list_id, "term1");
                AddTerm(client, list_id, "term2");

                GetAllTerms(client, list_id);

                // Always remember to refresh the search index of your list
                RefreshSearchIndex(client, list_id);

                string text = "This text contains the terms \"term1\" and \"term2\".";
                ScreenText(client, list_id, text);

                DeleteTerm(client, list_id, "term1");

                // Always remember to refresh the search index of your list
                RefreshSearchIndex(client, list_id);

                text = "This text contains the terms \"term1\" and \"term2\".";
                ScreenText(client, list_id, text);

                DeleteAllTerms(client, list_id);
                DeleteTermList(client, list_id);

                Console.WriteLine("Press ENTER to close the application.");
                Console.ReadLine();
            }
        }
    }

    /// <summary>
    /// Wraps the creation and configuration of a Content Moderator client.
    /// </summary>
    /// <remarks>This class library contains insecure code. If you adapt this 
    /// code for use in production, use a secure method of storing and using
    /// your Content Moderator subscription key.</remarks>
    public static class Clients
    {
        // TODO We could make team name a static property on this class, to move all of the subscription information into one project.

        /// <summary>
        /// The base URL fragment for Content Moderator calls.
        /// Add your Azure Content Moderator endpoint to your environment variables.
        /// </summary>
        private static readonly string AzureBaseURL =
            Environment.GetEnvironmentVariable("CONTENT_MODERATOR_ENDPOINT");

        /// <summary>
        /// Your Content Moderator subscription key.
        /// Add your Azure Content Moderator subscription key to your environment variables.
        /// </summary>
        private static readonly string CMSubscriptionKey = 
            Environment.GetEnvironmentVariable("CONTENT_MODERATOR_SUBSCRIPTION_KEY");

        /// <summary>
        /// Returns a new Content Moderator client for your subscription.
        /// </summary>
        /// <returns>The new client.</returns>
        /// <remarks>The <see cref="ContentModeratorClient"/> is disposable.
        /// When you have finished using the client,
        /// you should dispose of it either directly or indirectly. </remarks>
        public static ContentModeratorClient NewClient()
        {
            // Create and initialize an instance of the Content Moderator API wrapper.
            ContentModeratorClient client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(CMSubscriptionKey));

            client.Endpoint = AzureBaseURL;
            return client;
        }
    }
}
