using FlickrUtilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace SlackIntegrations.Controllers
{
	public class ImageController : ApiController
	{

		#region Config Values
		private string ApiKey { get { return ConfigurationManager.AppSettings["FlickrApiKey"]; } }
		private string Secret { get { return ConfigurationManager.AppSettings["FlickrApiSecret"]; } }
		private string AuthToken { get { return ConfigurationManager.AppSettings["FlickrAuthToken"]; } }
		#endregion

		private FlickrHelper _flickrHelper;
		private FlickrHelper FlickrHelper
		{
			get
			{
				if (_flickrHelper == null)
					_flickrHelper = new FlickrHelper(this.ApiKey, this.Secret, this.AuthToken);
				return _flickrHelper;
			}
		}


		// GET api/values
		public async Task<dynamic> Get(
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
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("text", text);
			dictionary.Add("per_page", 1);
			dictionary.Add("method", "flickr.photos.search");

			dynamic result = await this.FlickrHelper.Get(dictionary);

			String photoId = result.photos.photo[0].id;
			String photoOwner = result.photos.photo[0].owner;
			String photoSecret = result.photos.photo[0].secret;
			String photoServer = result.photos.photo[0].server;
			String photoFarm = result.photos.photo[0].farm;

			String imageUrl = "https://farm" + photoFarm + ".staticflickr.com/" + photoServer + "/" + photoId + "_" + photoSecret + "_m.jpg";
		
			return imageUrl;
		}
	}
}
