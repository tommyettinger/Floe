using System;
using System.Collections.Generic;
using System.Text;

namespace Floe.Net
{
	public enum IrcCommand
	{
		PING,
		NICK,
		PRIVMSG,
		NOTICE,
		QUIT,
		JOIN,
		PART,
		TOPIC,
		INVITE,
		KICK,
		MODE,
		CAP,
		MOTD,
		WHO,
		WHOIS,
		WHOWAS,
		USERHOST,
		LIST,
		AWAY,
		USER,
		PASS,
		NoCommand
	}

	/// <summary>
	/// Represents a raw IRC message received from or sent to the IRC server, in accordance with RFC 2812.
	/// </summary>
	public sealed class IrcMessage
	{
		/// <summary>
		/// Gets the prefix that indicates the source of the message.
		/// </summary>
		public readonly IrcPrefix From;

		/// <summary>
		/// command class
		/// </summary>
		public readonly IrcCommand Command;

		public readonly IrcCode Code;

		/// <summary>
		/// Gets the list of parameters.
		/// </summary>
		public readonly IList<string> Parameters;

        /// <summary>
        /// Gets the list of tags.
        /// </summary>
		public readonly IrcTags Tags;

		public readonly DateTime Received;

		public readonly DateTime Time;

		/// <summary>
		/// Create an outgoing message.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="parameters"></param>
		internal IrcMessage(IrcCommand command, params string[] parameters)
			: this(new IrcTags(), null, command, IrcCode.None, parameters)
		{
			if (command == IrcCommand.NoCommand)
				throw new ArgumentException("command required");
		}

		private IrcMessage(IrcTags tags, IrcPrefix prefix, IrcCommand command, IrcCode code,
            string[] parameters)
		{
            this.Tags = tags;
			this.From = prefix;
            this.Command = command;
            this.Code = code;
			this.Parameters = parameters;
            this.Received = DateTime.Now;

            DateTime t;
            if (Tags.ContainsKey("time") && (DateTime.TryParse(Tags["time"], System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal, out t)))
                this.Time = t;
            else
                this.Time = this.Received;
		}

		private IrcMessage(IrcCode code, IrcTags tags, IrcPrefix prefix, string[] parameters)
			: this(tags, prefix, IrcCommand.NoCommand, code, parameters)
		{
		}

		/// <summary>
		/// Convert the message into a string that can be sent to an IRC server or printed to a debug window.
		/// </summary>
		/// <returns>Returns the IRC message formatted in accordance with RFC 2812.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (this.From != null)
				sb.Append(':').Append(this.From.ToString()).Append(' ');
			sb.Append(this.Command);
			for (int i = 0; i < this.Parameters.Count; i++)
			{
				if (string.IsNullOrEmpty(this.Parameters[i]))
				{
					continue;
				}

				sb.Append(' ');
				if (i == this.Parameters.Count - 1)
					sb.Append(':');
				sb.Append(this.Parameters[i]);
			}

			return sb.ToString();
		}

		private static IrcCommand TokenizeCommand(string command)
		{
			switch (command)
			{
				case "PING":
					return IrcCommand.PING;
				case "NICK":
					return IrcCommand.NICK;
				case "PRIVMSG":
					return IrcCommand.PRIVMSG;
				case "NOTICE":
					return IrcCommand.NOTICE;
				case "QUIT":
					return IrcCommand.QUIT;
				case "JOIN":
					return IrcCommand.JOIN;
				case "PART":
					return IrcCommand.PART;
				case "TOPIC":
					return IrcCommand.TOPIC;
				case "INVITE":
					return IrcCommand.INVITE;
				case "KICK":
					return IrcCommand.KICK;
				case "MODE":
					return IrcCommand.MODE;
				case "CAP":
					return IrcCommand.CAP;
				default:
					throw new System.ArgumentException("unknown IRC command: " + command);
			}
		}

		internal static IrcMessage Parse(string data)
		{
			string[] tokens = data.Split(' ');
			IrcTags tags = null;
			IrcPrefix prefix = null;
			int tokenIndex = 0;

			if (tokens[0].StartsWith("@"))
			{
				tags = IrcTags.Parse(tokens[0].Substring(1));
				tokenIndex++;
			}

			if (tokens[tokenIndex].StartsWith(":"))
			{
				prefix = IrcPrefix.Parse(tokens[tokenIndex].Substring(1));
				tokenIndex++;
			}

			string commandString = tokens[tokenIndex];
			tokenIndex++;

			List<string> parameters = new List<string>(tokens.Length - tokenIndex);
			StringBuilder param = new StringBuilder();
			for (; tokenIndex < tokens.Length; tokenIndex++)
			{
				param.Append(tokens[tokenIndex]);

				if (!tokens[tokenIndex].StartsWith(":"))
				{
					parameters.Add(param.ToString());
					param.Clear();
				}
			}

			// If there's an unterminated parameter, just add it to the end of the parameter list.
			if (param.Length != 0)
				parameters.Add(param.ToString());

			IrcCode code;
			if (IrcCode.TryParse(commandString, out code))
				return new IrcMessage(code, tags, prefix, parameters.ToArray());
			else
			{
				// tags and prefix are dropped
				return new IrcMessage(TokenizeCommand(commandString), parameters.ToArray());
			}
		}
	}
}
