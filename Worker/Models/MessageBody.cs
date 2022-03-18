using Newtonsoft.Json;

namespace Worker.Models
{
    public class MessageBody
    {
        public MessageBody()
        {
            Sections = new List<Section>();
            Sections.Add(new Section());
            Actions = new List<Action>();
        }

        [JsonProperty(PropertyName = "@context")]
        public string Context { get; set; } = "https://schema.org/extensions";
        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; } = "MessageCard";
        [JsonProperty(PropertyName = "themeColor")]
        public string ThemeColor { get; set; } = CardColours.DANGER;
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; } = "";
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; } = "";
        [JsonProperty(PropertyName = "sections")]
        public List<Section> Sections { get; set; }
        [JsonProperty(PropertyName = "potentialAction")]
        public List<Action> Actions { get; set; }


        public class Section
        {
            public Section()
            {
                Facts = new List<Fact>();
            }

            [JsonProperty(PropertyName = "facts")]
            public List<Fact> Facts { get; set; }
        }

        public class Fact
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; } = "";
            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; } = "";
        }

        public class Action
        {
            public Action()
            {
                Targets = new List<ActionTarget>();
            }

            [JsonProperty(PropertyName = "@type")]
            public string Type { get; set; } = "OpenUri";
            [JsonProperty(PropertyName = "name")]
            public string DisplayName { get; set; } = "";
            [JsonProperty(PropertyName = "targets")]
            public List<ActionTarget> Targets { get; set; }
        }

        public class ActionTarget
        {
            [JsonProperty(PropertyName = "os")]
            public string OS { get; set; } = "default";
            [JsonProperty(PropertyName = "uri")]
            public string UriLink { get; set; } = "";
        }
    }



}