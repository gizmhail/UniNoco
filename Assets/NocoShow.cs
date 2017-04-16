using System;
using UnityEngine;

namespace Noco
{

	public class NocoShowsRequest:NocoRequest
	{
		public NocoShowsSet showSet;
		public NocoShowsRequest(NocoAPI api) : base(api){
			this.queryStr = "/shows?page=0&elements_per_page=40";
		}

		override protected void Parse(){
			this.showSet = NocoShowsSet.FromJSONArray (this.result);
		}
	}

	[Serializable]
	public class NocoShowsSet
	{
		public NocoShow[] shows;

		public static NocoShowsSet FromJSONArray(string json){
			string fixedJson = "{\"shows\":"+json+"}";
			return JsonUtility.FromJson <NocoShowsSet>(fixedJson);
		}
	}

	[Serializable]
	public class NocoShow
	{
		public int id_show;
		public string show_resume;
		public long duration_ms;
		public int season_number;
		public int episode_number;
		public string broadcast_date_utc;
		// Screenshot url
		public string screenshot_1024x576;
		public string show_TT;
		public string show_OT_lang;
		public string show_OT;

		public int id_family;
		public int id_partner;
		// Image url for mosaique of the show
		public string mosaique;
		// Image url
		public string banner_family;
		public string partner_key;
		public string partner_name;
		public int id_type;
		public int guest_free;
		public int mark_read;
		public string progress;
		public int id_theme;
		public string theme_key;
		public string theme_name;
		public string type_key;
		public string type_name;
		public int user_free;
		public int resume_play;
		public string link_comment;
		public string family_resume;
		public string family_OT;
		public string family_TT;
		public string family_OT_lang;
		public string rating_fr;
		public string show_key;

		public string template_1l;
		public string template_2l;

		public string Description1L(){
			// TODO: Really use template_1l (currently gives ugly results ...)
			/*
			using System.Reflection;
			using System.ComponentModel.Design;

			if (template_1l != null) {
				string desc = template_1l;	
				foreach (FieldInfo field in this.GetType().GetFields()) {
					string name = field.Name;
					if (name == "episode_number") {
						name = "number";
					}
					desc = desc.Replace("{"+ name +"}", field.GetValue(this).ToString() );
				}
				return desc;
			}
			*/
			return "[" + this.family_TT+ " - "+this.episode_number+"] "+this.show_TT;

		}
	}
}

