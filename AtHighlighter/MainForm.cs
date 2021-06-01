using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AtHighlighter.Data;
using AtHighlighter.Overlay;
using AtHighlighter.WindowsAPI;
using Newtonsoft.Json;

namespace AtHighlighter
{
    public partial class MainForm : Form
    {
        private IntPtr clipboardViewerNext;

        public MainForm()
        {
            InitializeComponent();
            RegisterClipboardViewer();
        }

        private void RegisterClipboardViewer()
        {
            clipboardViewerNext = User32.SetClipboardViewer(Handle);
        }

        /// <summary>
        ///     Remove this form from the Clipboard Viewer list
        /// </summary>
        private void UnRegisterClipboardViewer()
        {
            User32.ChangeClipboardChain(Handle, clipboardViewerNext);
        }


        private void GetClipboardDataAndShowOverlay()
        {
            // Data on the clipboard uses the 
            // IDataObject interface
            IDataObject iData;

            try
            {
                iData = Clipboard.GetDataObject();
            }
            catch (ExternalException externEx)
            {
                // Copying a field definition in Access 2002 causes this sometimes?
                Debug.WriteLine("InteropServices.ExternalException: {0}", externEx.Message);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            // 
            // Get Text if it is present
            //
            if (iData == null || !iData.GetDataPresent(DataFormats.Text)) return;

            var text = (string) iData.GetData(DataFormats.Text);
            Debug.WriteLine(text);

            if (!TryParseRect(text, out var rect)) return;

            var overlayManager = new WinFormsOverlayManager();
            overlayManager.ShowBlocking(
                new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top), Color.Red,
                rect.Duration);
        }

        private bool TryParseRect(string text, out AtRect rect)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<AtRect>(text);

                if (result == null)
                {
                    rect = null;
                    return false;
                }

                if (result.Left >= result.Right || result.Top >= result.Bottom || result.Duration <= 0)
                {
                    rect = null;
                    return false;
                }
                rect = result;
                return true;
            }
            catch (JsonException)
            {
                rect = null;
                return false;
            }
        }


        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                //
                // The WM_DRAWCLIPBOARD message is sent to the first window 
                // in the clipboard viewer chain when the content of the 
                // clipboard changes. This enables a clipboard viewer 
                // window to display the new content of the clipboard. 
                //
                case (int) WindowsMessages.WM_DRAWCLIPBOARD:
                {
                    Debug.WriteLine("WindowProc DRAWCLIPBOARD: " + m.Msg, "WndProc");

                    GetClipboardDataAndShowOverlay();

                    //
                    // Each window that receives the WM_DRAWCLIPBOARD message 
                    // must call the SendMessage function to pass the message 
                    // on to the next window in the clipboard viewer chain.
                    //
                    User32.SendMessage(clipboardViewerNext, (uint) m.Msg, m.WParam, m.LParam);
                    break;
                }

                //
                // The WM_CHANGECBCHAIN message is sent to the first window 
                // in the clipboard viewer chain when a window is being 
                // removed from the chain. 
                //
                case (int) WindowsMessages.WM_CHANGECBCHAIN:
                {
                    Debug.WriteLine("WM_CHANGECBCHAIN: lParam: " + m.LParam, "WndProc");

                    // When a clipboard viewer window receives the WM_CHANGECBCHAIN message, 
                    // it should call the SendMessage function to pass the message to the 
                    // next window in the chain, unless the next window is the window 
                    // being removed. In this case, the clipboard viewer should save 
                    // the handle specified by the lParam parameter as the next window in the chain. 

                    //
                    // wParam is the Handle to the window being removed from 
                    // the clipboard viewer chain 
                    // lParam is the Handle to the next window in the chain 
                    // following the window being removed. 
                    if (m.WParam == clipboardViewerNext)
                        //
                        // If wParam is the next clipboard viewer then it
                        // is being removed so update pointer to the next
                        // window in the clipboard chain
                        //
                        clipboardViewerNext = m.LParam;
                    else
                        User32.SendMessage(clipboardViewerNext, (uint) m.Msg, m.WParam, m.LParam);

                    break;
                }

                default:
                {
                    //
                    // Let the form process the messages that we are
                    // not interested in
                    //
                    base.WndProc(ref m);
                    break;
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnRegisterClipboardViewer();
        }
    }
}