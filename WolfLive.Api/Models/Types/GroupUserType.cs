namespace WolfLive.Api.Models
{
	public enum GroupUserType
	{
		User = 0,
		Admin = 1,
		Mod = 2,
		Banned = 4,
		Silenced = 8,
		Kicked = 16,
		Owner = 32
	}
}
