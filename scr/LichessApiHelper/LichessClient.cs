using LichessApiHelper.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LichessApiHelper
{
    public class LichessClient
    {
        private readonly string PersonalAccessToken;

        #region [Constructor]

        public LichessClient(string personalAccessToken)
        {
            PersonalAccessToken = personalAccessToken;
        }

        #endregion [Constructor]

        #region [Public Api Methods]

        /// <summary>
        /// Gets the members of a team.
        /// </summary>
        /// <param name="teamId">The team's id.</param>
        /// <returns>A list with all team members.</returns>
        public async Task<List<TeamMember>> GetTeamMembersAsync(string teamId)
        {
            string response = 
                await SendGetRequest($"https://lichess.org/api/team/{teamId}/users");

            return DeserializeNDJsonList<TeamMember>(response);
        }

        #endregion [Public Api Methods]

        #region [Rest Api Methods]

        private async Task<string> SendGetRequest(string path)
        {
            string response;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization
                         = new AuthenticationHeaderValue("Bearer", PersonalAccessToken);
                HttpResponseMessage requestResponse =
                    await client.GetAsync(path);
                response = await requestResponse.Content.ReadAsStringAsync();
            }

            return response;
        }

        #endregion [Rest Api Methods]

        #region [Helper Methods]

        private static List<T> DeserializeNDJsonList<T>(string content)
        {
            List<T> items = new List<T>();

            JsonTextReader jsonReader = new JsonTextReader(new StringReader(content))
            {
                SupportMultipleContent = true
            };

            JsonSerializer jsonSerializer = new JsonSerializer();
            while (jsonReader.Read())
            {
                items.Add(jsonSerializer.Deserialize<T>(jsonReader));
            }
            return items;
        }

        #endregion [Helper Methods]
    }
}
