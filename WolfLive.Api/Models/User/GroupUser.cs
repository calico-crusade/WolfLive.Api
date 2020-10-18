namespace WolfLive.Api.Models
{
	public class GroupUser
	{
		public User User { get; set; }
		public Group Group { get; set; }
		public GroupUserType Capabilities { get; set; }
	}
}
