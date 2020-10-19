using System;

namespace WolfLive.Api.Models
{
    [Flags]
	public enum PrivilegeType : long
	{
        User = 0,
        SelectClubOne = 1 << 4,
        EliteClubOne = 1 << 6,
        Volunteer = 1 << 9,
        SelectClubTwo = 1 << 10,
        ClientTester = 1<< 11,
        Staff = 1 << 12,
        EliteClubTwo = 1 << 17,
        Pest = 1 << 18,
        ValidEmail = 1<< 19,
        PremiumAccountHolder = 1 << 20,
        VIP = 1 << 21,
        EliteClubThree = 1 << 22,
        UserAdmin = 1 << 24,
        GroupAdmin = 1 << 25,
        Bot = 1 << 26,
        Agent = 1 << 28,
        Entertainer = 1 << 29,
        ShadowBanned = 1 << 30,
	}
}
