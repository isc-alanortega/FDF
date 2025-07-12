using Azure.AI.OpenAI;
using Newtonsoft.Json;
using PCG_ENTITIES.Resultsets;
using PCG_FDF.Data.DataAccess;
using PCG_FDF.Data.Entities;

namespace PCG_FDF.Data.ComponentDI
{
    public class CIMAChatService
    {
        private readonly PCG_FDF_DB DATA_ACCESS;

        private bool Chat_Initialized { get; set; } = false;
        private bool Loading { get; set; } = false;
        private bool Error { get; set; } = false;
        private IList<OAIChatMessage> Messages { get; set; } = new List<OAIChatMessage>();

        /// <summary>
        /// Action that triggers a StateHasChanged event to rerender the shared components
        /// </summary>
        public event Action OnChange;

        public CIMAChatService(PCG_FDF_DB data_access)
        {
            DATA_ACCESS = data_access;
        }

        /// <summary>
        /// Triggers the StateHasChanged event on shared components to rerender themselves
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        public bool GetChatInitialized()
        {
            return Chat_Initialized;
        }

        public bool GetIsLoading()
        {
            return Loading;
        }

        public IList<OAIChatMessage> GetMessages()
        {
            return Messages;
        }

        public async Task Initialize()
        {
            if (!Chat_Initialized)
            {
                Loading = true;
                NotifyStateChanged();
                var response = await DATA_ACCESS.SendUnauthTAsync<APIResult<string?>>("/PCG_FDFChat/PostGetChatResponse", HttpMethod.Post, null, JsonConvert.SerializeObject(Messages));

                if (response == default)
                {
                    Error = true;
                }
                else
                {
                    if (response.Operation_Succeeded && response.Result is not null)
                    {
                        Messages.Add(new OAIChatMessage(new ChatRequestAssistantMessage(response.Result)));
                        Chat_Initialized = true;
                    }
                    else
                    {
                        Error = true;
                    }
                }
                Loading = false;
                NotifyStateChanged();
            }
        }

        public async Task SendMessage(string Message)
        {
            Messages.Add(new OAIChatMessage(new ChatRequestUserMessage(Message)));
            Loading = true;
            NotifyStateChanged();
            var response = await DATA_ACCESS.SendUnauthTAsync<APIResult<string?>>("/PCG_FDFChat/PostGetChatResponse", HttpMethod.Post, null, JsonConvert.SerializeObject(Messages));

            if (response == default)
            {
                Error = true;
            }
            else
            {
                if (response.Operation_Succeeded && response.Result is not null)
                {
                    Messages.Add(new OAIChatMessage(new ChatRequestAssistantMessage(response.Result)));
                }
                else
                {
                    Error = true;
                }
            }
            Loading = false;
            NotifyStateChanged();
        }
    }
}
