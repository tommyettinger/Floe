using System;
using System.Collections.Generic;

namespace Floe.Net
{
    public class IrcTags : Dictionary<string, string>
    {
		internal static IrcTags Parse(string tagstring)
        {
            IrcTags tags = new IrcTags();

            if (!string.IsNullOrEmpty(tagstring))
            {
                string[] split = tagstring.Split(new char[] { ';' });

                foreach (string s in split)
                {
                    string[] kv = s.Split(new char[] { '=' }, 2);
                    string key = kv[0];
                    string value = "";
                    if (kv.Length >= 2)
                    {
                        value = kv[1];
                    }

                    tags[key] = value;
                }
            }

            return tags;
        }
    }
}
