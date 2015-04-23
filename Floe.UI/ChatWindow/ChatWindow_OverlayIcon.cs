using System;
using System.Windows;
using System.Windows.Media;

namespace Floe.UI
{
    public enum OverlayIconState
    {
        None,
        ChatActivity,
        OwnNickname
    }

    public partial class ChatWindow : Window
    {

        private OverlayIconState state = OverlayIconState.None;

        public void clearOverlayIcon()
        {
            if (state != OverlayIconState.None)
            {
                state = OverlayIconState.None;
                TaskbarItemInfo.Overlay = null;
            }
        }

        public void setOverlayIcon(OverlayIconState newState)
        {
            if (newState == OverlayIconState.ChatActivity && this.state == OverlayIconState.None)
            {
                state = OverlayIconState.ChatActivity;
                if (App.Settings.Current.Formatting.OverlayIconOnOwnNickname)
                {
                    TaskbarItemInfo.Overlay = (ImageSource)Resources["GreenBubble"];
                }
            }
            else if (newState == OverlayIconState.OwnNickname && this.state != OverlayIconState.OwnNickname)
            {
                state = OverlayIconState.OwnNickname;
                if (App.Settings.Current.Formatting.OverlayIconOnChatActivity)
                {
                    TaskbarItemInfo.Overlay = (ImageSource)Resources["MagentaBubble"];
                }
            }
            else if (newState == OverlayIconState.None && this.state != OverlayIconState.None)
            {
                state = OverlayIconState.None;
                TaskbarItemInfo.Overlay = null;
            }
        }
    }
}
