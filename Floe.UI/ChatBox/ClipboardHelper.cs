using System.Windows;

namespace Floe.UI
{
    using System.Media;

    public static class ClipboardHelper
    {
        private const int MAX_SET_RETRY_ATTEMPTS = 100;

        /// <summary>
        /// Sets the clipboard text.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void SetText(string text)
        {
            var tries = 0;

            do
            {
                try
                {
                    Clipboard.SetText(text);
                    return;
                }
                catch { }
            }
            while (tries++ < MAX_SET_RETRY_ATTEMPTS);

            // notify user of failure
            SystemSounds.Beep.Play();
        }
    }
}
