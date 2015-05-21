using Newtonsoft.Json;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.IO;

namespace FlickrUtilities
{
  public class FlickrHelper
  {
    
    //the flickr api uri
    private FlickrApiLocation FlickrApi = FlickrApiLocation.MAIN;

    public string ApiKey { get; set; }
    public string Secret { get; set; }
    public string AuthToken { get; set; }

    public FlickrHelper(string apiKey, string secret, string authToken = null)
    {
      this.ApiKey = apiKey;
      this.Secret = secret;
      this.AuthToken = authToken;
    }

    public string GenerateApiSignature(IDictionary<string, object> parameters)
    {
      //sort the parameters alphabetically
      SortedList<string, object> sorted = new SortedList<string, object>();
      foreach (KeyValuePair<string, object> pair in parameters)
        sorted.Add(pair.Key, pair.Value);

      //build up the string to hash, with the secret prepended
      StringBuilder sb = new StringBuilder(this.Secret);
      foreach (KeyValuePair<string, object> pair in sorted)
      {
        sb.Append(pair.Key);
        sb.Append(pair.Value);
      }

      //hash it
      string signature = MD5Hash(sb.ToString());

      return signature;
    }

    public static string MD5Hash(string data)
    {
      byte[] hashedBytes;

      using (MD5CryptoServiceProvider csp = new MD5CryptoServiceProvider())
      {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        hashedBytes = csp.ComputeHash(bytes, 0, bytes.Length);
      }

      return BitConverter.ToString(hashedBytes).Replace("-", String.Empty).ToLower(CultureInfo.InvariantCulture);
    }

    public async Task<dynamic> Get(Dictionary<string, object> parameters)
    {
      //add in the generic parameters
      parameters.Add("api_key", this.ApiKey);
      parameters.Add("nojsoncallback", "1");
      parameters.Add("format", "json");
      parameters.Add("api_sig", this.GenerateApiSignature(parameters));

      string url = this.BuildQueryStringUrl(this.FlickrApi.Uri, parameters);

      //make the call, get back a string of json
      HttpClient client = new HttpClient();
      string content = await client.GetStringAsync(url);

      //convert that to a dynamic object
      dynamic jsonObject = await JsonConvert.DeserializeObjectAsync<dynamic>(content);

      return jsonObject;
    }

    public async Task<dynamic> Post(Dictionary<string, object> parameters)
    {
      //add in the generic parameters
      parameters.Add("api_key", this.ApiKey);
      parameters.Add("nojsoncallback", "1");
      parameters.Add("format", "json");
      if (!String.IsNullOrWhiteSpace(this.AuthToken)) parameters.Add("auth_token", this.AuthToken);
      parameters.Add("api_sig", this.GenerateApiSignature(parameters));

      string url = this.BuildQueryStringUrl(this.FlickrApi.Uri, parameters);

      //make the call, get back a string of json
      //the posts on flickr take all the parameters in the query string, so we're passing null as the content
      HttpClient client = new HttpClient();
      HttpResponseMessage response = await client.PostAsync(url, null);

      //convert that to a dynamic object
      dynamic jsonObject = await response.Content.ReadAsAsync<dynamic>();

      return jsonObject;

    }

    public string BuildQueryStringUrl(string url, Dictionary<string, object> dictionary = null)
    {
      //if there's no parameters, return just the input
      if (dictionary == null)
        return url;

      //the result will be the input + the parameters in a query string
      StringBuilder sb = new StringBuilder(url + "?");
      foreach (KeyValuePair<string, object> pair in dictionary)
      {
        sb.Append(pair.Key);
        sb.Append("=");
        sb.Append(pair.Value);
        sb.Append("&");
      }

      //remove trailing '&'
      string newUrl = sb.Remove(sb.Length - 1, 1).ToString();

      return newUrl;
    }

    public async Task<FlickrUploadResult> Upload(string filename, Stream buffer)
    {
      //the non-photo parameters to pass to the flickr upload API
      Dictionary<string, object> parameters = new Dictionary<string, object>();
      parameters.Add("api_key", this.ApiKey);
      if (!String.IsNullOrWhiteSpace(this.AuthToken)) parameters.Add("auth_token", this.AuthToken);
      parameters.Add("api_sig", this.GenerateApiSignature(parameters));

      //now build the form posting based on those parameters
      MultipartFormDataContent content = new MultipartFormDataContent();
      foreach (string key in parameters.Keys)
          content.Add(new StringContent(parameters[key].ToString()), key);
      
      //now add the photo
      content.Add(new StreamContent(buffer), "photo", filename);
        
      //now upload it to the flickr Upload api
      HttpClient uploadClient = new HttpClient();
      HttpResponseMessage uploadResponse = await uploadClient.PostAsync(FlickrApiLocation.UPLOAD.Uri, content);

      //read the response and get the uploaded photo id
      string uploadResponseContent = await uploadResponse.Content.ReadAsStringAsync();
      string photoid = extractXmlTag("photoid", uploadResponseContent);

      return new FlickrUploadResult (){ Name = filename, Id = photoid };
    }

    private string extractXmlTag(String tag, string xml)
    {
      int startTagStart = xml.IndexOf("<" + tag + ">");
      int startTagEnd = startTagStart + tag.Length + 2;
      int endTagStart = xml.IndexOf("</" + tag + ">");
      int length = endTagStart - startTagEnd;

      string tagContents = xml.Substring(startTagEnd, length);
      
      return tagContents;
    }
  }

}