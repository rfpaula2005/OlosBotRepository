using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net;
using System.Configuration;
using System.Diagnostics;
using Microsoft.Azure.WebJobs.Host;


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
                WebRequest request = WebRequest.Create("https://olosrepeaterfunction.azurewebsites.net/api/HttpTriggerCSharp1?code=ylw6l1SXaU6SqAae/4ee/Vq6fjNU6lYBXMdWTWeWPL8gznaLgHgaMA==&message=" + message.Text);
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                Debug.WriteLine(((HttpWebResponse)response).StatusDescription);

                switch (((HttpWebResponse)response).StatusCode)
                {
                    // OK
                    case HttpStatusCode.NoContent:
                    case HttpStatusCode.OK:
                        Debug.WriteLine($"Resquest Status: CODE: {((HttpWebResponse)response).StatusCode} \n\n {((HttpWebResponse)response).StatusDescription}");
                        // Get the stream containing content returned by the server.
                        Stream dataStream = response.GetResponseStream();
                        // Open the stream using a StreamReader for easy access.
                        StreamReader reader = new StreamReader(dataStream);
                        // Read the content.
                        string responseFromServer = reader.ReadToEnd();
                        // Display the content.
                        Debug.WriteLine(responseFromServer);
                        // Clean up the streams and the response.
                        reader.Close();
                        response.Close();
                        dt_messageReceivedFim = DateTime.Now;
                        await context.PostAsync($"Message Count: {this.count++} \n\n Bot ID: {appBotId} \n\n appId: [{appId}] \n\n Duração: {(dt_messageReceivedInicio - dt_messageReceivedInicio).TotalSeconds} segundos \n\n {responseFromServer} ");
                        context.Wait(MessageReceivedAsync);
                        break;
                    // Server Problems
                    case HttpStatusCode.ServiceUnavailable:
                    case HttpStatusCode.RequestTimeout:
                    case HttpStatusCode.RequestEntityTooLarge:
                    case HttpStatusCode.NotImplemented:
                    case HttpStatusCode.NotAcceptable:
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.GatewayTimeout:
                    case HttpStatusCode.Conflict:
                    case HttpStatusCode.InternalServerError:
                        Debug.WriteLine($"Resquest Status: CODE: {((HttpWebResponse)response).StatusCode} \n\n {((HttpWebResponse)response).StatusDescription}");
                        await context.PostAsync($"Por favor me desculpe, no momento estamos passando por algumas dificuldades técnicas.\n\nPor favor, retorne mais tarde e teremos o maior prazer em ajudá-lo com a sua solicitação.");
                        context.Wait(MessageReceivedAsync);
                        break;
                    // Request Problems
                    case HttpStatusCode.Ambiguous:
                    case HttpStatusCode.LengthRequired:
                    case HttpStatusCode.Gone:
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.MethodNotAllowed:
                    case HttpStatusCode.ProxyAuthenticationRequired:
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.ResetContent:
                    // Network Problems
                    case HttpStatusCode.BadGateway:
                        Debug.WriteLine($"Resquest Status: CODE: {((HttpWebResponse)response).StatusCode} \n\n {((HttpWebResponse)response).StatusDescription}");
                        await context.PostAsync($"Por favor me desculpe, no momento estamos passando por algumas dificuldades técnicas.\n\nPor favor, retorne mais tarde e teremos o maior prazer em ajudá-lo com a sua solicitação.");
                        context.Wait(MessageReceivedAsync);
                        break;
                    // Network Redirects
                    case HttpStatusCode.Redirect:
                    case HttpStatusCode.RedirectMethod:
                    case HttpStatusCode.TemporaryRedirect:
                    case HttpStatusCode.MovedPermanently:
                        Debug.WriteLine($"Resquest Status: CODE: {((HttpWebResponse)response).StatusCode} \n\n {((HttpWebResponse)response).StatusDescription}");
                        await context.PostAsync($"Verificar o comportamento em caso de redirect.");
                        context.Wait(MessageReceivedAsync);
                        break;
                    default:
                        Debug.WriteLine($"Resquest Status: CODE: {((HttpWebResponse)response).StatusCode} \n\n {((HttpWebResponse)response).StatusDescription}");
                        await context.PostAsync($"Por favor me desculpe, no momento estamos passando por algumas dificuldades técnicas.\n\nPor favor, retorne mais tarde e teremos o maior prazer em ajudá-lo com a sua solicitação.");
                        context.Wait(MessageReceivedAsync);
                        break;
                }
            }
            catch
            {

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
