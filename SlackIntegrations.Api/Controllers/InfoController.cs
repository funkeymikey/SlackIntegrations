using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SlackIntegration.Controllers
{
	public class InfoController : ApiController
	{
		// GET api/values
		public string Get(
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

			dynamic obj = new { Token = token, TeamId = team_id, TeamDomain = team_domain, ChannelId = channel_id, ChannelName = channel_name, UserId = user_id, Username = user_name, Command = command, Text = text };

			return "got: " + obj.ToString();
		}
	}
}
