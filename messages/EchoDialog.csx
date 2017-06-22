using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net;
using System.Configuration;
using System.Text.RegularExpressions;



// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : IDialog<object>
{
    protected int count = 1;

    public Task StartAsync(IDialogContext context)
    {
        try
        {
            context.Wait(MessageReceivedAsync);
        }
        catch (OperationCanceledException error)
        {
            return Task.FromCanceled(error.CancellationToken);
        }
        catch (Exception error)
        {
            return Task.FromException(error);
        }

        return Task.CompletedTask;
    }

    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        var appId = ConfigurationManager.AppSettings["MicrosoftAppId"];
        var appPass = ConfigurationManager.AppSettings["MicrosoftAppPassword"];
        var appBotId = ConfigurationManager.AppSettings["BotId"];
        DateTime dt_messageReceivedInicio;
        DateTime dt_messageReceivedFim;
        string http_code;

        if (message.Text == "reset")
        {
            PromptDialog.Confirm(
                context,
                AfterResetAsync,
                "Are you sure you want to reset the count?",
                "Didn't get that!",
                promptStyle: PromptStyle.Auto);
        }
        else
        {
            try
            {
                dt_messageReceivedInicio = DateTime.Now;
                string pattern = "(http_code=)([0-9][0-9][0-9])";
                http_code = (Regex.Match(message.Text, pattern, RegexOptions.IgnoreCase)).Value;
                string uri = "https://olosrepeaterfunction.azurewebsites.net/api/HttpTriggerCSharp1?code=ylw6l1SXaU6SqAae/4ee/Vq6fjNU6lYBXMdWTWeWPL8gznaLgHgaMA==&message=" + message.Text + "&" + http_code;

                /*
                WebRequest request = WebRequest.Create("https://olosrepeaterfunction.azurewebsites.net/api/HttpTriggerCSharp1?code=ylw6l1SXaU6SqAae/4ee/Vq6fjNU6lYBXMdWTWeWPL8gznaLgHgaMA==&message=" + message.Text + "&" + http_code);
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                // Clean up the streams and the response.
                reader.Close();
                response.Close();
                */
                Task<string> getStringTask = Utils.AccessTheWebAsync(uri);
                string responseFromServer = await getStringTask;
                dt_messageReceivedFim = DateTime.Now;
                await context.PostAsync($"Message Count: {this.count++} \n\n Bot ID: {appBotId} \n\n appId: [{appId}] \n\n Duração: {(dt_messageReceivedInicio - dt_messageReceivedInicio).TotalSeconds} segundos \n\n {responseFromServer} ");
                context.Wait(MessageReceivedAsync);
            }
            catch (WebException wex)
            {
                await context.PostAsync($"Por favor me desculpe, no momento estamos passando por algumas dificuldades técnicas.Retorne mais tarde e teremos o maior prazer em ajudá-lo com a sua solicitação.\n\n Execption:\n\n" + wex.Message);
                context.Wait(MessageReceivedAsync);
            }

        }
    }

    public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            this.count = 1;
            await context.PostAsync("Reset count.");
        }
        else
        {
            await context.PostAsync("Did not reset count.");
        }
        context.Wait(MessageReceivedAsync);
    }

}

/*
public class Utils
{
    public static async Task<string> AccessTheWebAsync(string uri)
    {
        HttpClient client = new HttpClient();

        Task<string> getStringTask = client.GetStringAsync(uri);
        string urlContents = await getStringTask;

        return urlContents;
    }
}
*/
