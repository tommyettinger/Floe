using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Floe.Net;

namespace Floe.UI
{
    public partial class ChatControl : ChatPage
    {
        private string[] _nickCandidates;
        private NicknameList _nickList;

        public NicknameList Nicknames
        {
            get { return _nickList; }
        }
        
        private string GetNickWithLevel(string nick)
        {
            return this.IsChannel && _nickList.Contains(nick) ? _nickList[nick].ToString() : nick;
        }

        private string GetNickWithoutLevel(string nick)
        {
            return (nick.Length > 1 && (nick[0] == '@' || nick[0] == '+' || nick[0] == '%')) ? nick.Substring(1) : nick;
        }

        private Tuple<int, int> findNickCharsFromCaret(bool expectingNick)
        {
            int start = 0;
            int end = 0;

            if (txtInput.Text.Length > 0)
            {
                // We're going to use start and end to define a selection of a possible nickname fragment
                // in the txtInput field, centered around the input caret.
                start = Math.Max(0, txtInput.CaretIndex - 1);
                end = start < txtInput.Text.Length ? start + 1 : start;

                // If we're hitting "tab" again to complete to another nick, we will expect either ": " or " " to 
                // be following the actual range we want. Let's shift start and end accordingly.
                if (expectingNick && (txtInput.Text.Length > 2 && start >= 2))
                {
                    if (string.Compare(": ", txtInput.Text.Substring(start-1, 2)) == 0)
                    {
                        start = start - 2;
                        end = end - 1;
                    }
                    else if (string.Compare(" ", txtInput.Text.Substring(start, 1)) == 0)
                    {
                        start = start - 1;
                    }
                }
                // Hunt backwards from start, testing each character in the text input for valid IRC nick characters.
                while (start >= 0 && NicknameItem.IsNickChar(txtInput.Text[start]))
                {
                    start--;
                }
                start++;

                // Hunt forwards from end, testing each character in the text input for valid IRC nick characters.
                while (end < txtInput.Text.Length && NicknameItem.IsNickChar(txtInput.Text[end]))
                {
                    end++;
                }
            }
            else
            {
                start = end = 0;
            }

            return Tuple.Create(start, end);
        }

        private string appendProperEnding(string nextNick, int start, int end, string totalText)
        {
            if (totalText.Length <=2 && !totalText.Contains(' '))
            {
                nextNick += ": ";
            }
            else if (string.Compare(totalText.Substring(end-1, 2), ": ") == 0 ||
                        !totalText.Contains(' '))
            {
                nextNick += ": ";
            }
            else
            {
                nextNick += " ";
            }
            return nextNick;
        }

        private Tuple<int, string> DoNickCompletion(Tuple<int, string> tabData)
        {
            int start = 0, end = 0;
            int tabCount = tabData.Item1;
            string originalString = tabData.Item2;
            Tuple<int, int> result;
            string completionString;

            result = findNickCharsFromCaret(tabCount > 0);
            start = result.Item1;
            end = result.Item2;

            if (originalString != "")
            {
                completionString = originalString;
            }
            else
            {
                completionString = txtInput.Text.Substring(start, end - start);
            }

            if (end - start > 0)
            {
                string nickPart = completionString;
                string nextNick = null;
                int candidateIndex = 0;
                if (_nickCandidates == null)
                {
                    _nickCandidates = (from n in this.Nicknames
                                       where n.Nickname.StartsWith(nickPart, StringComparison.InvariantCultureIgnoreCase)
                                       orderby n.Nickname.ToLowerInvariant()
                                       select n.Nickname).ToArray();
                }

                if (_nickCandidates.Length > 0)
                {
                    if (tabCount >= _nickCandidates.Length)
                    {
                        candidateIndex = tabCount % _nickCandidates.Length;
                    }
                    else
                    {
                        candidateIndex = tabCount;
                    }
                    nextNick = _nickCandidates[candidateIndex];
                }
                // If nextNick perfectly matches an existing nick and there's other nickCandidates, use the next candidate
                // instead of this one.
                for (int i = candidateIndex; i < _nickCandidates.Length; i++)
                {
                    if (string.Compare(_nickCandidates[i], nickPart, StringComparison.InvariantCulture) == 0)
                    {
                        nextNick = i < _nickCandidates.Length - 1 ? _nickCandidates[i + 1] : _nickCandidates[0];
                        break;
                    }
                }

                var keepNickCandidates = _nickCandidates;
                if (nextNick != null)
                {
                    nextNick = appendProperEnding(nextNick, start, end, txtInput.Text); 
                    txtInput.Text = txtInput.Text.Substring(0, start) + nextNick + txtInput.Text.Substring(end);
                    txtInput.CaretIndex = start + nextNick.Length;
                }
                _nickCandidates = keepNickCandidates;
            }
            return Tuple.Create(tabCount + 1, completionString);
        }
    }
}
