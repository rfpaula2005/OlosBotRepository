using System;
using System.Threading.Tasks;
using System.Net;


// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]

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
