﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using ScreenToGif.Capture;
using ScreenToGif.Encoding;
using ScreenToGif.Pages;
using ScreenToGif.Properties;
using ScreenToGif.Util;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

//TODO: Make the text change event to rename the labelTitle.

namespace ScreenToGif
{
    /// <summary>
    /// Modern
    /// </summary>
    public partial class Modern : Form
    {
        #region Variables

        /// <summary>
        /// The animated gif encoder, this encodes the list of frames to a gif format.
        /// </summary>
        AnimatedGifEncoder _encoder = new AnimatedGifEncoder();
        /// <summary>
        /// This object retrieves the icon of the cursor.
        /// </summary>
        readonly CaptureScreen _capture = new CaptureScreen();
        /// <summary>
        /// The object of the keyboard hook.
        /// </summary>
        private readonly UserActivityHook _actHook;
        /// <summary>
        /// The editor may increase the size of the form, use this to go back to the last size (The size before opening the editor).
        /// </summary>
        private Size _lastSize;
        /// <summary>
        /// To hold the update of the size of the form while typing in the size textBoxes.
        /// </summary>
        private bool _screenSizeEdit;
        /// <summary>
        /// The amount of seconds of the pre start delay, plus 1 (1+1=2);
        /// </summary>
        private int _preStartCount = 1;
        /// <summary>
        /// The output path of the recording.
        /// </summary>
        private string _outputpath;
        /// <summary>
        /// The actual stage of the program.
        /// </summary>
        private int _stage = 0; //0 Stoped, 1 Recording, 2 Paused, 3 PreStart, 4 Editing, 5 Encoding
        /// <summary>
        /// The list of bitmaps recorded.
        /// </summary>
        private List<Bitmap> _listBitmap;
        /// <summary>
        /// The list of information about the cursor.
        /// </summary>
        private List<CursorInfo> _listCursor = new List<CursorInfo>(); //List that stores the icon
        /// <summary>
        /// Object that holds the information of the cursor.
        /// </summary>
        private CursorInfo _cursorInfo;
        /// <summary>
        /// Stores the last position of the click, this is used in the drag of the window.
        /// </summary>
        static Point _lastClick;
        /// <summary>
        /// Holds the position of the cursor.
        /// </summary>
        private Point _posCursor;
        /// <summary>
        /// The maximum size of the recording. Also the maximum size of the window.
        /// </summary>
        private Point _sizeScreen = new Point(SystemInformation.PrimaryMonitorSize);
        private Bitmap _bt;
        private Graphics _gr;
        private Thread _workerThread;

        #region Page Flags

        /// <summary>
        /// Tells if the page "Gif Settings" is open or not
        /// </summary>
        private bool _isPageGifOpen;
        /// <summary>
        /// Tells if the page "App Settings" is open or not
        /// </summary>
        private bool _isPageAppOpen;
        /// <summary>
        /// Tells if the page "Information" is open or not
        /// </summary>
        private bool _isPageInfoOpen;

        #endregion

        private enum Stage : int
        {
            Stoped = 0,
            Recording = 1,
            Paused = 2,
            PreStarting = 3,
            Editing = 4,
            Encoding = 5
        };

        #endregion

        #region Native

        //[DllImport("user32.dll")]
        //private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        //[DllImport("gdi32.dll")]
        //private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        //[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        //private static extern bool DeleteObject(System.IntPtr hObject);

        //For the hit test area
        private const int cGrip = 20;
        private const int cCaption = 35;
        private const int cBorder = 7;

        /// <summary>
        /// Indicates the position of the cursor hot spot.
        /// </summary>
        public enum HitTest : int
        {
            /// <summary>
            /// On the screen background or on a dividing line between windows (same as HTNOWHERE, except that the DefWindowProc function produces a system beep to indicate an error).
            /// </summary>
            HTERROR = -2,

            /// <summary>
            /// In a window currently covered by another window in the same thread (the message will be sent to underlying windows in the same thread until one of them returns a code that is not HTTRANSPARENT).
            /// </summary>
            HTTRANSPARENT = -1,

            /// <summary>
            /// On the screen background or on a dividing line between windows.
            /// </summary>
            HTNOWHERE = 0,

            /// <summary>
            /// In a client area.
            /// </summary>
            HTCLIENT = 1,

            /// <summary>
            /// In a title bar.
            /// </summary>
            HTCAPTION = 2,

            /// <summary>
            /// In a window menu or in a Close button in a child window.
            /// </summary>
            HTSYSMENU = 3,

            /// <summary>
            /// In a size box (same as HTSIZE).
            /// </summary>
            HTGROWBOX = 4,

            /// <summary>
            /// In a size box (same as HTGROWBOX).
            /// </summary>
            HTSIZE = 4,

            /// <summary>
            /// In a menu.
            /// </summary>
            HTMENU = 5,

            /// <summary>
            /// In a horizontal scroll bar.
            /// </summary>
            HTHSCROLL = 6,

            /// <summary>
            /// In the vertical scroll bar.
            /// </summary>
            HTVSCROLL = 7,

            /// <summary>
            /// In a Minimize button.
            /// </summary>
            HTMINBUTTON = 8,

            /// <summary>
            /// In a Minimize button.
            /// </summary>
            HTREDUCE = 8,

            /// <summary>
            /// In a Maximize button.
            /// </summary>
            HTMAXBUTTON = 9,

            /// <summary>
            /// In a Maximize button.
            /// </summary>
            HTZOOM = 9,

            /// <summary>
            /// In the left border of a resizable window (the user can click the mouse to resize the window horizontally).
            /// </summary>
            HTLEFT = 10,

            /// <summary>
            /// In the right border of a resizable window (the user can click the mouse to resize the window horizontally).
            /// </summary>
            HTRIGHT = 11,

            /// <summary>
            /// In the upper-horizontal border of a window.
            /// </summary>
            HTTOP = 12,

            /// <summary>
            /// In the upper-left corner of a window border.
            /// </summary>
            HTTOPLEFT = 13,

            /// <summary>
            /// In the upper-right corner of a window border.
            /// </summary>
            HTTOPRIGHT = 14,

            /// <summary>
            /// In the lower-horizontal border of a resizable window (the user can click the mouse to resize the window vertically).
            /// </summary>
            HTBOTTOM = 15,

            /// <summary>
            /// In the lower-left corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).
            /// </summary>
            HTBOTTOMLEFT = 16,

            /// <summary>
            /// In the lower-right corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).
            /// </summary>
            HTBOTTOMRIGHT = 17,

            /// <summary>
            /// In the border of a window that does not have a sizing border.
            /// </summary>
            HTBORDER = 18,

            /// <summary>
            /// In a Close button.
            /// </summary>
            HTCLOSE = 20,

            /// <summary>
            /// In a Help button.
            /// </summary>
            HTHELP = 21,
        };

        #endregion

        public Modern()
        {
            InitializeComponent();

            #region Load Save Data

            //Gets and sets the fps
            numMaxFps.Value = Properties.Settings.Default.STmaxFps;

            //Load last saved window size
            this.Size = new Size(Properties.Settings.Default.STsize.Width, Properties.Settings.Default.STsize.Height);

            #endregion

            //Gets the window size and show in the textBoxes
            tbHeight.Text = (this.Height - 71).ToString();
            tbWidth.Text = (this.Width - 24).ToString();

            //Performance and flickering tweaks
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);

            #region Global Hook
            _actHook = new UserActivityHook();
            _actHook.KeyDown += KeyHookTarget;
            _actHook.Start(false, true);
            #endregion
        }

        #region Override

        /// <summary>
        /// System's constant used to define a drop shadow of the form.
        /// </summary>
        private const int CS_DROPSHADOW = 0x00020000;
        /// <summary>
        /// System's constant that defines the hit test of the form.
        /// </summary>
        public const int WM_NCHITTEST = 0x84;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //Creates a border around the form
            Graphics g = e.Graphics;
            var rectOutside = new Rectangle(new Point(0, 0), new Size(this.Width - 1, this.Height - 1));
            var rectInside = new Rectangle(new Point(11, 33), new Size(panelTransparent.Width + 1, panelTransparent.Height + 1));

            Pen pen;

            if (_stage == 1)
            {
                pen = new Pen(Color.FromArgb(255, 25, 0));
            }
            else if (_stage == 4)
            {
                pen = new Pen(Color.FromArgb(30, 180, 30));
            }
            else
            {
                pen = new Pen(Color.FromArgb(0, 151, 251));
            }

            g.DrawRectangle(pen, rectOutside);
            g.DrawRectangle(pen, rectInside);

            //Create a round border, not so beautiful
            //System.IntPtr ptrBorder = CreateRoundRectRgn(0, 0,
            //this.ClientSize.Width, this.ClientSize.Height, 15, 15);
            //SetWindowRgn(this.Handle, ptrBorder, true);

            //Create a rectangle around the form, to make a hit test
            Rectangle rc = new Rectangle(this.ClientSize.Width - cGrip,
                this.ClientSize.Height - cGrip, cGrip, cGrip);

            //Paints the size grip
            //ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor, rc);

        }

        protected override void WndProc(ref Message m)
        {
            if (_stage == 0 || _stage == 4)
            {
                if (m.Msg == (int)WM_NCHITTEST)
                {
                    #region Hit Test
                    // Trap WM_NCHITTEST
                    Point pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
                    pos = this.PointToClient(pos);

                    //Bottom Left
                    if (pos.X <= cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                    {
                        m.Result = (IntPtr)HitTest.HTBOTTOMLEFT;
                        return;
                    }

                    //Bottom Right
                    else if (pos.X >= this.ClientSize.Width - cGrip &&
                        pos.Y >= this.ClientSize.Height - cGrip)
                    {
                        m.Result = (IntPtr)HitTest.HTBOTTOMRIGHT;
                        return;
                    }

                    //Top Left
                    else if (pos.Y <= cBorder && pos.X <= cBorder)
                    {
                        m.Result = (IntPtr)HitTest.HTTOPLEFT;
                        return;
                    }

                    //Top Right
                    else if (pos.Y <= cBorder && pos.X >= this.ClientSize.Width - cBorder)
                    {
                        m.Result = (IntPtr)HitTest.HTTOPRIGHT;
                        return;
                    }

                    //Top
                    else if (pos.Y <= cBorder)
                    {
                        m.Result = (IntPtr)HitTest.HTTOP;
                        return;
                    }

                    //Caption
                    else if (pos.Y < cCaption && pos.X > 50 && pos.Y < ClientSize.Width)
                    {
                        m.Result = (IntPtr)HitTest.HTCAPTION;
                        return;
                    }

                    //Bottom
                    else if (pos.Y >= this.ClientSize.Height - cBorder)
                    {
                        m.Result = (IntPtr)HitTest.HTBOTTOM;
                        return;
                    }

                    //Left
                    else if (pos.X <= cBorder)
                    {
                        m.Result = (IntPtr)HitTest.HTLEFT;
                        return;
                    }

                    //Right
                    else if (pos.X >= this.ClientSize.Width - cBorder)
                    {
                        m.Result = (IntPtr)HitTest.HTRIGHT;
                        return;
                    }
                    #endregion
                }
            }

            base.WndProc(ref m);
        }

        #endregion

        #region TitleBar Buttons

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
                btnMaximize.Image = Resources.MaximizePlus;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                btnMaximize.Image = Resources.MaximizeMinus;
            }

        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        #endregion

        #region Main Form Move/Resize /Closing

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            Application.DoEvents();
            if (e.Button == MouseButtons.Left)
            {
                _lastClick = new Point(e.X, e.Y); //We'll need this for when the Form starts to move
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            //Application.DoEvents();
            if (e.Button == MouseButtons.Left)
            {
                //Move the Form the same difference the mouse cursor moved;
                this.Left += e.X - _lastClick.X;
                this.Top += e.Y - _lastClick.Y;
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (!_screenSizeEdit)
            {
                tbHeight.Text = (this.Height - 71).ToString();
                tbWidth.Text = (this.Width - 24).ToString();
            }

            if (this.WindowState == FormWindowState.Normal)
                btnMaximize.Image = Properties.Resources.MaximizeMinus;
        }

        /// <summary>
        /// Before close, all settings must be saved and the timer must be disposed.
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.STmaxFps = Convert.ToInt32(numMaxFps.Value);
            Properties.Settings.Default.STsize = new Size(this.Size.Width, this.Size.Height);

            Properties.Settings.Default.Save();

            _actHook.Stop();

            if (_stage != 0)
            {
                timerCapture.Stop();
                timerCapture.Dispose();
            }
        }

        #endregion

        #region Bottom buttons

        private readonly Control info = new Information();
        private readonly Control appSettings = new AppSettings(false); //false = modern, true = legacy
        private readonly Control gifSettings = new GifSettings();

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnMaximize.Enabled = true;
            btnMinimize.Enabled = true;

            Stop();
        }

        private void btnRecordPause_Click(object sender, EventArgs e)
        {
            panelTransparent.Controls.Clear();
            btnMaximize.Enabled = false;
            btnMinimize.Enabled = false;

            RecordPause();
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            Control ctrlParent = panelTransparent; //Defines the parent

            if (_isPageAppOpen)
            {
                ctrlParent.Controls.Clear(); //Removes all pages

                _isPageAppOpen = false;
            }
            else
            {
                panelTransparent.Visible = false;
                ctrlParent.Controls.Clear(); //Removes all pages
                ctrlParent.Controls.Add(appSettings);
                panelTransparent.Visible = true;
                appSettings.Dock = DockStyle.Fill;

                _isPageAppOpen = true;
                _isPageGifOpen = false;
                _isPageInfoOpen = false;
            }
        }

        private void btnGifConfig_Click(object sender, EventArgs e)
        {
            Control ctrlParent = panelTransparent; //Defines the parent

            if (_isPageGifOpen)
            {
                ctrlParent.Controls.Clear(); //Removes all pages

                _isPageGifOpen = false;
            }
            else
            {
                panelTransparent.Visible = false;
                ctrlParent.Controls.Clear(); //Removes all pages
                ctrlParent.Controls.Add(gifSettings);
                panelTransparent.Visible = true;
                gifSettings.Dock = DockStyle.Fill;

                _isPageInfoOpen = false;
                _isPageAppOpen = false;
                _isPageGifOpen = true;
            }
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            Control ctrlParent = panelTransparent; //Defines the parent

            if (_isPageInfoOpen)
            {
                ctrlParent.Controls.Clear(); //Removes all pages

                _isPageInfoOpen = false;
                GC.Collect();
            }
            else
            {
                panelTransparent.Visible = false;
                ctrlParent.Controls.Clear(); //Removes all pages
                ctrlParent.Controls.Add(info);
                panelTransparent.Visible = true;
                info.Dock = DockStyle.Fill;

                _isPageAppOpen = false;
                _isPageGifOpen = false;
                _isPageInfoOpen = true;
                GC.Collect();
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// KeyHook event method. This fires when the user press a key.
        /// </summary>
        private void KeyHookTarget(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Properties.Settings.Default.STstartPauseKey)
            {
                RecordPause();
            }
            else if (e.KeyCode == Properties.Settings.Default.STstopKey)
            {
                Stop();
            }
        }

        /// <summary>
        /// Method that starts or pauses the recording
        /// </summary>
        private void RecordPause()
        {
            if (_stage == (int)Stage.Stoped) //if stoped, starts recording
            {
                #region To Record

                timerCapture.Interval = 1000 / Convert.ToInt32(numMaxFps.Value);
                timerCapWithCursor.Interval = 1000 / Convert.ToInt32(numMaxFps.Value);

                _listBitmap = new List<Bitmap>(); //List that contains all the frames.
                _listCursor = new List<CursorInfo>(); //List that contains all the icon information

                _bt = new Bitmap(panelTransparent.Width, panelTransparent.Height);
                _gr = Graphics.FromImage(_bt);

                labelTitle.Text = "Screen To Gif (2 " + Resources.TitleSecondsToGo;
                btnRecordPause.Text = Resources.Pause;
                btnRecordPause.Image = Properties.Resources.Pause_17Blue;
                btnRecordPause.Enabled = false;
                tbHeight.Enabled = false;
                tbWidth.Enabled = false;
                btnMaximize.Enabled = false;
                btnMinimize.Enabled = false;
                _stage = (int)Stage.PreStarting;
                numMaxFps.Enabled = false;
                _preStartCount = 1; //Reset timer to 2 seconds, 1 second to trigger the timer so 1 + 1 = 2

                timerPreStart.Start();
                this.TopMost = true;
                this.Invalidate();

                #endregion
            }
            else if (_stage == (int)Stage.Recording) //if recording, pauses
            {
                #region To Pause

                labelTitle.Text = Resources.TitlePaused;
                btnRecordPause.Text = Resources.btnRecordPause_Continue;
                btnRecordPause.Image = Properties.Resources.Record;
                _stage = (int)Stage.Paused;
                this.Invalidate(); //To redraw the form, activating OnPaint again

                if (Settings.Default.STshowCursor) //if show cursor
                {
                    timerCapWithCursor.Enabled = false;
                }
                else
                {
                    timerCapture.Enabled = false;
                }

                #endregion
            }
            else if (_stage == (int)Stage.Paused) //if paused, starts recording again
            {
                #region To Record Again

                labelTitle.Text = Resources.TitleRecording;
                btnRecordPause.Text = Resources.Pause;
                btnRecordPause.Image = Properties.Resources.Pause_17Blue;
                _stage = (int)Stage.Recording;
                this.Invalidate(); //To redraw the form, activating OnPaint again

                if (Settings.Default.STshowCursor) //if show cursor
                {
                    timerCapWithCursor.Enabled = true;
                }
                else
                {
                    timerCapture.Enabled = true;
                }

                #endregion
            }
        }

        /// <summary>
        /// Stops the recording or stops the pre-start timer.
        /// </summary>
        private void Stop()
        {
            try
            {
                _actHook.Stop(); //Stops the hook.
                _actHook.KeyDown -= KeyHookTarget; //Removes the event.

                timerCapture.Stop();
                timerCapWithCursor.Stop();

                if (_stage != (int)Stage.Stoped && _stage != (int)Stage.PreStarting) //if not already stoped or pre starting, stops
                {

                    #region To Stop and Save

                    if (Settings.Default.STshowCursor) //If show cursor is true, merge all bitmaps.
                    {
                        #region Merge Cursor and Bitmap

                        this.Cursor = Cursors.WaitCursor;
                        Graphics graph;
                        int numImage = 0;
                        foreach (var bitmap in _listBitmap) //Change this to stop the hang time
                        {
                            Application.DoEvents();
                            graph = Graphics.FromImage(bitmap);
                            var rect = new Rectangle(_listCursor[numImage].Position.X, _listCursor[numImage].Position.Y, _listCursor[numImage].Icon.Width, _listCursor[numImage].Icon.Height);

                            graph.DrawIcon(_listCursor[numImage].Icon, rect);
                            graph.Flush();
                            numImage++;
                        }
                        this.Cursor = Cursors.Arrow;

                        #endregion
                    }

                    if (Settings.Default.STallowEdit) //If the user wants to edit the frames.
                    {
                        _lastSize = this.Size; //To return back to the last form size after the editor
                        _stage = (int)Stage.Editing;
                        this.Invalidate();
                        btnMaximize.Enabled = true;
                        btnMinimize.Enabled = true;
                        EditFrames();
                        flowPanel.Enabled = false;
                    }
                    else
                    {
                        _lastSize = this.Size; //Not sure why this is here
                        Save();
                    }

                    #endregion
                }
                else if (_stage == (int)Stage.PreStarting) //if pre starting, stops
                {
                    #region To Stop

                    timerPreStart.Stop();
                    _stage = (int)Stage.Stoped;

                    //Enables the controls that are disabled while recording;
                    numMaxFps.Enabled = true;
                    btnRecordPause.Enabled = true;
                    tbHeight.Enabled = true;
                    tbWidth.Enabled = true;

                    btnMaximize.Enabled = true;
                    btnMinimize.Enabled = true;

                    btnRecordPause.Image = Properties.Resources.Record;
                    btnRecordPause.Text = Resources.btnRecordPause_Record;
                    labelTitle.Text = Resources.TitleStoped;
                    this.Invalidate();

                    //Re-starts the keyboard hook.
                    _actHook.KeyDown += KeyHookTarget;
                    _actHook.Start(false, true);

                    #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                LogWriter.Log(ex, "Error in the Stop function");
            }

        }

        /// <summary>
        /// Prepares the recorded frames to be saved/edited
        /// </summary>
        private void Save()
        {
            this.Cursor = Cursors.WaitCursor;
            this.Size = _lastSize;
            if (!Settings.Default.STsaveLocation) // to choose the location to save the gif
            {
                #region If Not Save Directly to the Desktop

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "GIF file (*.gif)|*gif";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                sfd.DefaultExt = "gif";

                this.Cursor = Cursors.Arrow;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    _outputpath = sfd.FileName;

                    _workerThread = new Thread(DoWork);
                    _workerThread.IsBackground = true;
                    _workerThread.Start();
                }
                else
                {
                    flowPanel.Enabled = true;
                    this.MinimumSize = new Size(100, 100);
                    _stage = (int)Stage.Stoped; //Stoped
                    numMaxFps.Enabled = true;
                    tbWidth.Enabled = true;
                    tbHeight.Enabled = true;

                    this.TopMost = false;
                    btnRecordPause.Text = Resources.btnRecordPause_Record;
                    btnRecordPause.Image = Resources.Record;
                    labelTitle.Text = Resources.TitleStoped;
                    this.Invalidate();

                    _actHook.KeyDown += KeyHookTarget;
                    _actHook.Start(false, true);

                    return;
                }

                #endregion
            }
            else
            {
                #region Search For Filename

                bool searchForName = true;
                int numOfFile = 0;
                string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                while (searchForName)
                {
                    if (!File.Exists(path + "\\Animation " + numOfFile + ".gif"))
                    {
                        _outputpath = path + "\\Animation " + numOfFile + ".gif";
                        searchForName = false;
                    }
                    else
                    {
                        if (numOfFile > 999)
                        {
                            searchForName = false;
                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                _outputpath = saveFileDialog.FileName;
                            }
                            else
                            {
                                _outputpath = "No filename for you";
                            }
                        }
                        numOfFile++;
                    }
                }
                #endregion

                _workerThread = new Thread(DoWork);
                _workerThread.IsBackground = true;
                _workerThread.Name = "Encoding";
                _workerThread.Start();
            }

            this.Cursor = Cursors.Arrow;
            _stage = (int)Stage.Stoped;
            this.MinimumSize = new Size(100, 100);
            numMaxFps.Enabled = true;
            tbHeight.Enabled = true;
            tbWidth.Enabled = true;
            this.TopMost = false;
            this.Text = Resources.TitleStoped;
            this.Invalidate();
        }

        /// <summary>
        /// Thread method that encodes the list of frames.
        /// </summary>
        private void DoWork()
        {
            if (Settings.Default.STencodingCustom)
            {
                #region Ngif encoding

                int numImage = 0;
                int countList = _listBitmap.Count;

                using (_encoder = new AnimatedGifEncoder())
                {
                    _encoder.Start(_outputpath);
                    _encoder.SetQuality(Settings.Default.STquality);

                    _encoder.SetRepeat(Settings.Default.STloop ? (Settings.Default.STrepeatForever ? 0 : Settings.Default.STrepeatCount) : -1); // 0 = Always, -1 once

                    _encoder.SetFrameRate(Convert.ToInt32(numMaxFps.Value));

                    foreach (var image in _listBitmap)
                    {
                        numImage++;
                        try
                        {
                            this.Invoke((Action)delegate //Needed because it's a cross thread call.
                            {
                                this.labelTitle.Text = Resources.Title_Thread_ProcessingFrame + numImage + Resources.Title_Thread_out_of + countList + ")";
                            });
                        }
                        catch (Exception)
                        {
                        }
                        _encoder.AddFrame(image);
                    }
                }

                #endregion
            }
            else
            {
                #region paint.NET encoding

                var imageArray = _listBitmap.ToArray();

                var delay = 1000 / Convert.ToInt32(numMaxFps.Value);
                var repeat = (Settings.Default.STloop ? (Settings.Default.STrepeatForever ? 0 : Settings.Default.STrepeatCount) : -1); // 0 = Always, -1 once
                int countList = _listBitmap.Count;

                using (var stream = new MemoryStream())
                {
                    using (var encoderNet = new GifEncoder(stream, null, null, repeat))
                    {
                        for (int i = 0; i < _listBitmap.Count; i++)
                        {
                            encoderNet.AddFrame((_listBitmap[i] as Bitmap).CopyImage(), 0, 0,
                                TimeSpan.FromMilliseconds(delay));

                            this.Invoke((Action)delegate //Needed because it's a cross thread call.
                            {
                                this.labelTitle.Text = Resources.Title_Thread_ProcessingFrame + i + Resources.Title_Thread_out_of + countList + ")";
                            });
                        }
                    }

                    stream.Position = 0;

                    using (
                        var fileStream = new FileStream(_outputpath, FileMode.Create, FileAccess.Write, FileShare.None,
                            Constants.BufferSize, false))
                    {
                        stream.WriteTo(fileStream);
                    }
                }

                #endregion
            }

            #region Memory Clearing

            listFramesPrivate.Clear();
            listFramesUndo.Clear();
            listFramesUndoAll.Clear();

            listFramesPrivate = null;
            listFramesUndo = null;
            listFramesUndoAll = null;
            _encoder = null;

            GC.Collect(); //call the garbage colector to empty the memory

            #endregion

            #region Finish

            try
            {
                this.Invoke((Action)delegate
                {
                    labelTitle.Text = Resources.Title_EncodingDone;
                    _stage = (int)Stage.Stoped;

                    btnRecordPause.Text = Resources.btnRecordPause_Record;
                    btnRecordPause.Image = Properties.Resources.Record;
                    flowPanel.Enabled = true;
                    this.TopMost = false;

                    numMaxFps.Enabled = true;
                    tbHeight.Enabled = true;
                    tbWidth.Enabled = true;

                    btnMaximize.Enabled = true;
                    btnMinimize.Enabled = true;

                    _actHook.KeyDown += KeyHookTarget;
                    _actHook.Start(false, true);
                });
            }
            catch { }

            #endregion
        }

        #endregion

        #region Timers

        /// <summary>
        /// Timer used after clicking in Record, to give the user a shor time to prepare recording
        /// </summary>
        private void timerPreStart_Tick(object sender, EventArgs e)
        {
            if (_preStartCount >= 1)
            {
                labelTitle.Text = "Screen To Gif (" + _preStartCount + Resources.TitleSecondsToGo;
                _preStartCount--;
            }
            else
            {
                labelTitle.Text = Resources.TitleRecording;
                timerPreStart.Stop();
                _stage = (int)Stage.Recording;
                btnRecordPause.Enabled = true;
                if (Settings.Default.STshowCursor)
                {
                    this.Invalidate(); //To redraw the form, activating OnPaint again
                    timerCapWithCursor.Start(); //Record with the cursor
                }
                else
                {
                    this.Invalidate(); //To redraw the form, activating OnPaint again
                    timerCapture.Start(); //Frame recording without the cursor
                }

            }
        }

        /// <summary>
        /// Takes a screenshot of desired area and add to the list.
        /// </summary>
        private void timerCapture_Tick(object sender, EventArgs e)
        {
            //Get the actual position of the form.
            Point lefttop = new Point(this.Location.X + 12, this.Location.Y + 34);
            //Take a screenshot of the area.
            _gr.CopyFromScreen(lefttop.X, lefttop.Y, 0, 0, panelTransparent.Bounds.Size, CopyPixelOperation.SourceCopy);
            //Add the bitmap to a list
            _listBitmap.Add((Bitmap)_bt.CopyImage());
        }

        /// <summary>
        /// Takes a screenshot of desired area and add to the list, plus add to the list the position and icon of the cursor.
        /// </summary>
        private void timerCapWithCursor_Tick(object sender, EventArgs e)
        {
            _cursorInfo = new CursorInfo
            {
                Icon = _capture.CaptureIconCursor(ref _posCursor),
                Position = panelTransparent.PointToClient(_posCursor)
            };

            //Get actual icon of the cursor
            _listCursor.Add(_cursorInfo);
            //Get the actual position of the form.
            Point lefttop = new Point(this.Location.X + 12, this.Location.Y + 34);
            //Take a screenshot of the area.
            _gr.CopyFromScreen(lefttop.X, lefttop.Y, 0, 0, panelTransparent.Bounds.Size, CopyPixelOperation.SourceCopy);
            //Add the bitmap to a list
            _listBitmap.Add((Bitmap)_bt.CopyImage());
        }

        #endregion

        #region TextBox Size

        /// <summary>
        /// Prevents keys!=numbers
        /// </summary>
        private void tbSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetter(e.KeyChar) ||
                char.IsSymbol(e.KeyChar) ||
                char.IsWhiteSpace(e.KeyChar) ||
                char.IsPunctuation(e.KeyChar))
                e.Handled = true;
        }

        /// <summary>
        /// After leaving the textBox, updates the size of the form with what is typed
        /// </summary>
        private void tbSize_Leave(object sender, EventArgs e)
        {
            Application.DoEvents();
            _screenSizeEdit = true;
            int heightTb = Convert.ToInt32(tbHeight.Text);
            int widthTb = Convert.ToInt32(tbWidth.Text);

            if (_sizeScreen.Y > heightTb)
            {
                this.Size = new Size(widthTb + 24, heightTb + 71);
            }
            else
            {
                this.Size = new Size(widthTb + 24, _sizeScreen.Y - 1);
            }
            _screenSizeEdit = false;
        }

        /// <summary>
        /// User press Enter, updates the form size.
        /// </summary>
        private void tbSize_KeyDown(object sender, KeyEventArgs e)
        {
            Application.DoEvents();
            if (e.KeyData == Keys.Enter)
            {
                _screenSizeEdit = true;
                int heightTb = Convert.ToInt32(tbHeight.Text);
                int widthTb = Convert.ToInt32(tbWidth.Text);

                if (_sizeScreen.Y > heightTb)
                {
                    this.Size = new Size(widthTb + 24, heightTb + 71);
                }
                else
                {
                    this.Size = new Size(widthTb + 24, _sizeScreen.Y - 1);
                }
                _screenSizeEdit = false;
            }
        }

        #endregion

        #region Frame Edit Stuff

        private List<Bitmap> listFramesPrivate;
        private List<Bitmap> listFramesUndoAll;
        private List<Bitmap> listFramesUndo;

        /// <summary>
        /// Constructor of the Frame Edit Page.
        /// </summary>
        private void EditFrames()
        {
            listFramesPrivate = new List<Bitmap>(_listBitmap);
            listFramesUndoAll = new List<Bitmap>(_listBitmap);
            listFramesUndo = new List<Bitmap>(_listBitmap);

            Application.DoEvents();

            panelEdit.Visible = true;
            trackBar.Maximum = listFramesPrivate.Count - 1;
            trackBar.Value = 0;
            this.MinimumSize = new Size(543, 308);
            labelTitle.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);

            ResizeFormToImage();

            pictureBitmap.Image = listFramesPrivate.First();

            #region Preview Config.

            timerPlayPreview.Tick += timerPlayPreview_Tick;
            timerPlayPreview.Interval = (1000 / Convert.ToInt32(numMaxFps.Value));

            #endregion
        }

        /// <summary>
        /// Accepts all the alterations and hides this page.
        /// </summary>
        private void btnDone_Click(object sender, EventArgs e)
        {
            StopPreview();
            _listBitmap = new List<Bitmap>(listFramesPrivate);

            panelEdit.Visible = false;
            labelTitle.Text = Resources.Title_Edit_PromptToSave;
            Save();

            GC.Collect();
        }

        /// <summary>
        /// Ignores all the alterations and hides this page.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            StopPreview();
            panelEdit.Visible = false;
            Save();

            GC.Collect();
        }

        /// <summary>
        /// When the user slides the trackBar, the image updates.
        /// </summary>
        private void trackBar_Scroll(object sender, EventArgs e)
        {
            StopPreview();
            pictureBitmap.Image = listFramesPrivate[trackBar.Value];
            labelTitle.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);
        }

        private void btnDeleteFrame_Click(object sender, EventArgs e)
        {
            StopPreview();
            btnUndo.Enabled = true;
            btnReset.Enabled = true;

            if (listFramesPrivate.Count > 1)
            {
                listFramesUndo.Clear();
                listFramesUndo = new List<Bitmap>(listFramesPrivate);

                listFramesPrivate.RemoveAt(trackBar.Value);

                trackBar.Maximum = listFramesPrivate.Count - 1;
                pictureBitmap.Image = listFramesPrivate[trackBar.Value];
                labelTitle.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);
            }
            else
            {
                MessageBox.Show(Resources.MsgBox_Message_CantDelete, Resources.MsgBox_Title_CantDelete, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            StopPreview();
            listFramesPrivate.Clear();
            listFramesPrivate = new List<Bitmap>(listFramesUndo);

            trackBar.Maximum = listFramesPrivate.Count - 1;
            pictureBitmap.Image = listFramesPrivate[trackBar.Value];
            labelTitle.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);

            btnUndo.Enabled = false;

            ResizeFormToImage();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            StopPreview();
            btnUndo.Enabled = true;

            listFramesUndo.Clear();
            listFramesUndo = new List<Bitmap>(listFramesPrivate); //To undo one

            listFramesPrivate.Clear();
            listFramesPrivate = new List<Bitmap>(listFramesUndoAll);

            trackBar.Maximum = listFramesPrivate.Count - 1;
            pictureBitmap.Image = listFramesPrivate[trackBar.Value];
            labelTitle.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);

            ResizeFormToImage();
        }

        private void nenuDeleteAfter_Click(object sender, EventArgs e)
        {
            btnUndo.Enabled = true;
            btnReset.Enabled = true;

            listFramesUndo.Clear();
            listFramesUndo = new List<Bitmap>(listFramesPrivate);

            if (listFramesPrivate.Count > 1)
            {
                int countList = listFramesPrivate.Count - 1; //So we have a fixed value

                for (int i = countList; i > trackBar.Value; i--) //from the end to the middle
                {
                    listFramesPrivate.RemoveAt(i);
                }

                trackBar.Maximum = listFramesPrivate.Count - 1;
                trackBar.Value = listFramesPrivate.Count - 1;
                pictureBitmap.Image = listFramesPrivate[trackBar.Value];
                labelTitle.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);
            }
        }

        private void menuDeleteBefore_Click(object sender, EventArgs e)
        {
            btnUndo.Enabled = true;
            btnReset.Enabled = true;

            listFramesUndo.Clear();
            listFramesUndo = new List<Bitmap>(listFramesPrivate);

            if (listFramesPrivate.Count > 1)
            {
                for (int i = trackBar.Value - 1; i >= 0; i--)
                {
                    listFramesPrivate.RemoveAt(i); // I should use removeAt everywhere
                }

                trackBar.Maximum = listFramesPrivate.Count - 1;
                trackBar.Value = 0;
                pictureBitmap.Image = listFramesPrivate[trackBar.Value];
                labelTitle.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);
            }
        }

        /// <summary>
        /// Resizes the form to hold the image
        /// </summary>
        private void ResizeFormToImage()
        {
            #region Window size
            Bitmap bitmap = new Bitmap(listFramesPrivate[0]);

            Size sizeBitmap = new Size(bitmap.Size.Width + 80, bitmap.Size.Height + 160);

            if (!(sizeBitmap.Width > 550)) //550 minimum width
            {
                sizeBitmap.Width = 550;
            }

            if (!(sizeBitmap.Height > 300)) //300 minimum height
            {
                sizeBitmap.Height = 300;
            }

            this.Size = sizeBitmap;

            bitmap.Dispose();

            #endregion
        }

        private void exportFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfdExport = new SaveFileDialog();
            sfdExport.DefaultExt = "jpg";
            sfdExport.Filter = "JPG Image (*.jpg)|*.jpg";
            sfdExport.FileName = Resources.Msg_Frame + trackBar.Value;

            if (sfdExport.ShowDialog() == DialogResult.OK)
            {
                Bitmap expBitmap = listFramesPrivate[trackBar.Value];
                expBitmap.Save(sfdExport.FileName, ImageFormat.Jpeg);
                MessageBox.Show(Resources.Msg_Frame + trackBar.Value + Resources.Msg_Exported, Resources.Msg_ExportedTitle);
                expBitmap.Dispose();
            }
            sfdExport.Dispose();
        }

        private void resizeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnUndo.Enabled = true;
            btnReset.Enabled = true;

            listFramesUndo.Clear();
            listFramesUndo = new List<Bitmap>(listFramesPrivate);

            Bitmap bitmapResize = listFramesPrivate[trackBar.Value];

            Resize resize = new Resize(bitmapResize);

            if (resize.ShowDialog(this) == DialogResult.OK)
            {
                Size resized = resize.GetSize();

                listFramesPrivate = ImageUtil.ResizeAllBitmap(listFramesPrivate, resized.Width, resized.Height);

                pictureBitmap.Image = listFramesPrivate[trackBar.Value];

                ResizeFormToImage();
            }

            resize.Dispose();
        }

        private void cropAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnUndo.Enabled = true;
            btnReset.Enabled = true;

            listFramesUndo.Clear();
            listFramesUndo = new List<Bitmap>(listFramesPrivate);

            var crop = new Crop(listFramesPrivate[trackBar.Value]);

            if (crop.ShowDialog(this) == DialogResult.OK)
            {
                listFramesPrivate = ImageUtil.Crop(listFramesPrivate, crop.Rectangle);

                pictureBitmap.Image = listFramesPrivate[trackBar.Value];

                ResizeFormToImage();
            }

            crop.Dispose();
        }

        private void deleteThisFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnUndo.Enabled = true;
            btnReset.Enabled = true;

            if (listFramesPrivate.Count > 1)
            {
                listFramesUndo.Clear();
                listFramesUndo = new List<Bitmap>(listFramesPrivate);

                listFramesPrivate.RemoveAt(trackBar.Value);

                trackBar.Maximum = listFramesPrivate.Count - 1;
                pictureBitmap.Image = listFramesPrivate[trackBar.Value];
                this.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);
            }
            else
            {
                MessageBox.Show(Resources.MsgBox_Message_CantDelete, Resources.MsgBox_Title_CantDelete, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openImageDialog.ShowDialog() == DialogResult.OK)
            {

                btnUndo.Enabled = true;
                btnReset.Enabled = true;

                listFramesUndo.Clear();
                listFramesUndo = new List<Bitmap>(listFramesPrivate);

                Image openBitmap = Bitmap.FromFile(openImageDialog.FileName);

                Bitmap bitmapResized = ImageUtil.ResizeBitmap((Bitmap)openBitmap, listFramesPrivate[0].Size.Width,
                    listFramesPrivate[0].Size.Height);

                listFramesPrivate.Insert(trackBar.Value, bitmapResized);

                trackBar.Maximum = listFramesPrivate.Count - 1;
                pictureBitmap.Image = listFramesPrivate[trackBar.Value];
                this.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);
            }
        }

        private void applyFiltersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filtersForm = new Filters(listFramesPrivate);
            if (filtersForm.ShowDialog(this) == DialogResult.OK)
            {
                btnUndo.Enabled = true;
                btnReset.Enabled = true;

                listFramesUndo.Clear();
                listFramesUndo = new List<Bitmap>(listFramesPrivate);

                listFramesPrivate.Clear();
                listFramesPrivate = new List<Bitmap>(filtersForm.ListBitmap);

                pictureBitmap.Image = listFramesPrivate[trackBar.Value];

                ResizeFormToImage();
            }

            filtersForm.Dispose();
        }

        private void revertOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnUndo.Enabled = true;
            btnReset.Enabled = true;

            if (listFramesPrivate.Count > 1)
            {
                listFramesUndo.Clear();
                listFramesUndo = new List<Bitmap>(listFramesPrivate);

                listFramesPrivate = ImageUtil.Revert(listFramesPrivate);

                pictureBitmap.Image = listFramesPrivate[trackBar.Value];
                this.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);
            }
        }

        private void yoyoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnUndo.Enabled = true;
            btnReset.Enabled = true;

            if (listFramesPrivate.Count > 1)
            {
                listFramesUndo.Clear();
                listFramesUndo = new List<Bitmap>(listFramesPrivate);

                listFramesPrivate = ImageUtil.Yoyo(listFramesPrivate);

                trackBar.Maximum = listFramesPrivate.Count - 1;
                pictureBitmap.Image = listFramesPrivate[trackBar.Value];
                this.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);
            }
        }

        #endregion

        #region Play Preview

        System.Windows.Forms.Timer timerPlayPreview = new System.Windows.Forms.Timer();
        private int actualFrame = 0;

        private void pictureBitmap_Click(object sender, EventArgs e)
        {
            PlayPreview();
        }

        private void PlayPreview()
        {
            if (timerPlayPreview.Enabled)
            {
                timerPlayPreview.Stop();
                this.labelTitle.Text = Resources.Title_EditorFrame + trackBar.Value + " - " + (listFramesPrivate.Count - 1);
                btnPreview.Text = Resources.Con_PlayPreview;
                btnPreview.Image = Resources.Play_17Green;
            }
            else
            {
                this.labelTitle.Text = "Screen To Gif - Playing Animation";
                btnPreview.Text = Resources.Con_StopPreview;
                btnPreview.Image = Resources.Stop_17Red;
                actualFrame = trackBar.Value;
                timerPlayPreview.Start();
            }

        }

        private void StopPreview()
        {
            timerPlayPreview.Stop();
            btnPreview.Text = Resources.Con_PlayPreview;
            btnPreview.Image = Resources.Play_17Green;
        }

        private void timerPlayPreview_Tick(object sender, EventArgs e)
        {
            pictureBitmap.Image = listFramesPrivate[actualFrame];
            trackBar.Value = actualFrame;

            if (listFramesPrivate.Count - 1 == actualFrame)
            {
                actualFrame = 0;
            }
            else
            {
                actualFrame++;
            }
        }

        private void trackBar_Enter(object sender, EventArgs e)
        {
            StopPreview();
        }

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            StopPreview();
        }

        #endregion
    }
}