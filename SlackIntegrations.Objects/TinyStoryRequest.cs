using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackIntegrations.Objects
{
	public class TinyStoryRequest
	{
		public string Name { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Description { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<String> Tags { get; set; }

		public TinyId Priority { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? EstimatedEffort { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Color { get; set; }

		[JsonIgnore]
		public TinyStoryLocation StoryLocation { get; set; }

		public TinyStoryRequest()
		{
			Priority = new TinyId() { Id = 1 };
		}
	}
}