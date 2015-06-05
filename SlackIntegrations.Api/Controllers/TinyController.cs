using FlickrUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SlackIntegrations.Objects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace SlackIntegrations.Controllers
{
	public class TinyController : ApiController
	{

		#region Config Values
		private string ApiToken { get { return ConfigurationManager.AppSettings["TinyApiToken"]; } }
		private string ProjectId { get { return ConfigurationManager.AppSettings["TinyProjectId"]; } }
		#endregion

		public JsonSerializerSettings SerializerSettings
		{
			get
			{
				JsonSerializerSettings serializer = new JsonSerializerSettings();
				serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
				return serializer;
			}
		}

		// GET api/values
		public async Task<string> Get(
			string token,
			string team_id,
			string team_domain,
			string channel_id,
			string channel_name,
			string user_id,
			string user_name,
			string command,
			string text)
		{
			TinyStoryRequest request = this.parseText(text);
			

			//where should the story go? assume backlog
			string postUrl = "https://doolli.tinypm.com/api/project/" + this.ProjectId + "/userstories";
			if(request.StoryLocation == TinyStoryLocation.Sprint)
				postUrl = "https://doolli.tinypm.com/api/project/" + this.ProjectId + "/iteration/current/userstories";
			
			string postContent = JsonConvert.SerializeObject(request, SerializerSettings);

			//do the update
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("apiToken", this.ApiToken);
			HttpResponseMessage response = await client.PostAsync(postUrl, new StringContent(postContent));
			response.EnsureSuccessStatusCode();

			TinyId resultingId = await response.Content.ReadAsAsync<TinyId>();

			return "https://doolli.tinypm.com/userStory/edit/" + resultingId.Id + "?projectCode=Doolli";
		}

		private TinyStoryRequest parseText(string text)
		{
			TinyStoryRequest storyRequest = new TinyStoryRequest();

			if (String.IsNullOrWhiteSpace(text))
				throw new Exception("usage: create [red|blue|green|white|orange|yellow] story in <sprint|backlog> with title \"<title>\" [and with description \"<description>\"]");

			String lower = text.ToLower();

			if (!lower.StartsWith("create"))
				throw new Exception("usage: create [red|blue|green|white|orange|yellow] story in <sprint|backlog> with title \"<title>\" [and with description \"<description>\"]");
			if (!lower.Contains("story"))
				throw new Exception("usage: create [red|blue|green|white|orange|yellow] story in <sprint|backlog> with title \"<title>\" [and with description \"<description>\"]");

			//find the first occurrence of the word "story"
			int storyStartIndex = lower.IndexOf("story");
			if (storyStartIndex < 6)
				throw new Exception("usage: create [red|blue|green|white|orange|yellow] story in <sprint|backlog> with title \"<title>\" [and with description \"<description>\"]");


			//find the color
			String upToStory = text.Substring(0, storyStartIndex - 1);
			String[] parts = upToStory.Split(' ');
			string specifiedColor;
			if (parts.Length == 1)
				specifiedColor = null;
			else if (parts.Length == 2)
				specifiedColor = parts[1];
			else
				throw new Exception("usage: create [red|blue|green|white|orange|yellow] story in <sprint|backlog> with title \"<title>\" [and with description \"<description>\"]");
			storyRequest.Color = parseColor(specifiedColor);

			//find backlog or sprint
			String startingAtStory = text.Substring(storyStartIndex, text.Length - storyStartIndex);
			String startingAtStoryLower = startingAtStory.ToLower();
			TinyStoryLocation storyLocation;
			if (startingAtStoryLower.StartsWith("story in sprint"))
				storyLocation = TinyStoryLocation.Sprint;
			else if (startingAtStoryLower.StartsWith("story in backlog"))
				storyLocation = TinyStoryLocation.Backlog;
			else
				throw new Exception("usage: create [red|blue|green|white|orange|yellow] story in <sprint|backlog> with title \"<title>\" [and with description \"<description>\"]");
			storyRequest.StoryLocation = storyLocation;

			//find the title
			if (lower.IndexOf("with title \"") < 1)
				throw new Exception("usage: create [red|blue|green|white|orange|yellow] story in <sprint|backlog> with title \"<title>\" [and with description \"<description>\"]");
			int titleStart = lower.IndexOf("with title \"") + 12;
			int titleEnd = lower.IndexOf("\"", titleStart);
			String title = text.Substring(titleStart, (titleEnd - titleStart));
			storyRequest.Name = title;

			//find the description, if present
			if (lower.IndexOf("with description \"") > 1)
			{
				int descriptionStart = lower.IndexOf("with description \"") + 18;
				int descriptionEnd = lower.IndexOf("\"", descriptionStart);
				String description = text.Substring(descriptionStart, (descriptionEnd - descriptionStart));
				storyRequest.Description = description;
			}

			return storyRequest;
		}

		private string parseColor(string text)
		{
			if (String.IsNullOrWhiteSpace(text))
				return null;

			text = text.ToLower().Trim();
			switch (text)
			{
				case "blue": return Enum.GetName(typeof(TinyColor), TinyColor.Blue).ToLower();
				case "red": return Enum.GetName(typeof(TinyColor), TinyColor.Red).ToLower();
				case "green": return Enum.GetName(typeof(TinyColor), TinyColor.Green).ToLower();
				case "yellow": return Enum.GetName(typeof(TinyColor), TinyColor.Yellow).ToLower();
				case "orange": return Enum.GetName(typeof(TinyColor), TinyColor.Orange).ToLower();
				case "grey": return Enum.GetName(typeof(TinyColor), TinyColor.White).ToLower();
				case "gray": return Enum.GetName(typeof(TinyColor), TinyColor.White).ToLower();
				case "white": return Enum.GetName(typeof(TinyColor), TinyColor.White).ToLower();
			}
			return null;

		}

	}


}
