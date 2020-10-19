using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace WolfLive.Api.Commands.Common
{
	public interface IAuthService
	{
		bool ToggleUser(string userid);
		bool ToggleUser(string userid, string groupid);

		bool Validate(IWolfClient client, CommandMessage message);
		bool Validate(string userid, string groupid = null);
	}

	public class AuthService : IAuthService
	{
		private AuthModel _authUsers = null;
		private readonly ILogger _logger;

		public string AuthUsersFilePath { get; set; } = "authusers.json";

		public AuthService(ILogger<AuthService> logger)
		{
			_logger = logger;
		}

		public AuthModel LoadAuthUsers()
		{
			try
			{
				if (!File.Exists(AuthUsersFilePath))
					return new AuthModel();

				var data = File.ReadAllText(AuthUsersFilePath);
				return JsonConvert.DeserializeObject<AuthModel>(data);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while fetching authorized users list");
				return null;
			}
		}

		public bool ToggleUser(string userid)
		{
			throw new NotImplementedException();
		}

		public bool ToggleUser(string userid, string groupid)
		{
			throw new NotImplementedException();
		}

		public bool Validate(IWolfClient client, CommandMessage message)
		{
			throw new NotImplementedException();
		}

		public bool Validate(string userid, string groupid = null)
		{
			throw new NotImplementedException();
		}
	}
}
