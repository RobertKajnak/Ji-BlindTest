using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Ji_BindTest
{
    public partial class FormMain : Form, IMessageFilter
    {
        /// <summary>
        /// or rather is control key pressed
        /// </summary>
        bool isControlPressed = false;

        /// <summary>
        /// The extension of all files needs to be kept between <see cref="renameFiles"/> and <see cref="revertFileNames"/>, as 
        /// the <see cref="Entryplet.hiddenName"/> field does not keep the extension data.
        /// <para>The exension is not kept between shuffles, but is consistent within shuttles.</para>
        /// </summary>
        private string extension = null;
        private Random RNG = new Random();
        ///TODO: allow files to be added more than once -- currently untested

        #region Graphics related variables
        /// <summary>
        /// used to detect if guess buttons need to be enabled
        /// </summary>
        bool firstGuess;
        Button selectedGuess;

        /// <summary>
        /// the TableLayout was not cooperative, so I decided to do the controls myself.
        /// I mean, working from scratch is fun, isn't it?
        /// </summary>
        List<Entryplet> loadedFiles;
        List<Button> loadedFilesButtons;
        int positionXLoadedFiles;
        int positionYLoadedFilesOrigin;
        int positionYLoadedFilesCurrent;
        int incrementYLoadedFiles;
        Size sizeLoadedFiles;

        int positionXGuesses;
        int positionYGuessesOrigin;
        int positionYGuessesCurrent;
        int incrementYGuesses;
        Size sizeGuesses;

        List<Button> guessButtons;
        #endregion

        public FormMain()
        {
            InitializeComponent();
            Application.AddMessageFilter(this);
            this.FormClosed += (s, e) => Application.RemoveMessageFilter(this);

            firstGuess = true;
            selectedGuess = null;
            loadedFiles = new List<Entryplet>();
            loadedFilesButtons = new List<Button>();

            positionXLoadedFiles = 50;
            positionYLoadedFilesOrigin = 50;
            positionYLoadedFilesCurrent = positionYLoadedFilesOrigin;
            incrementYLoadedFiles = 55;
            sizeLoadedFiles = new Size(246, 30);

            guessButtons = new List<Button>();
            positionXGuesses = 350;
            positionYGuessesOrigin = 50;
            positionYGuessesCurrent = positionYGuessesOrigin;
            incrementYGuesses = incrementYLoadedFiles;
            sizeGuesses = new Size(191,30);

            buttonShuffle.Enabled = false;
        }

        #region Utility Classes and Functions

        /// <summary>
        /// Contains the (file path, file name, hidden name(e.g. Sample 1)) set
        /// </summary>
        class Entryplet
        {
            public string filePath { get; }
            public string fileName { get; }
            public string hiddenName{ get; set;}
            ///TODO: moveextension to a static variable within class
            public string getHiddenPath(string extension)
            {
                if (this.hiddenName == null || this.hiddenName.Equals(""))
                    throw new Exception("The hidden file names are missing");
                string path = this.filePath.Substring(0, this.filePath.IndexOf(this.fileName));

                return path + this.hiddenName + extension;
            }
            
            public Entryplet(string path)
            {
                this.filePath = path;
                int lastIndex = path.LastIndexOf('\\') + 1;
                ///gets the filename without path, but if a shortened filepath is provided for some reason, displays full name
                this.fileName = lastIndex == -1 ? path : path.Substring(lastIndex);
            }
            public Entryplet(string path, string fileName)
            {
                this.filePath = path;
                this.fileName = fileName;
            }
        }
        
        /// <summary>
        /// Adds a button and the entity to the list and creates the button controls
        /// </summary>
        /// <param name="filename">the full path of the file</param>
        private void addFileToList(string filename)
        {
            labelHelp.Visible = false;
            buttonShuffle.Enabled = true;

            Button b = new Button();
            Entryplet ent = new Entryplet(filename);
            b.Text = ent.fileName;

            b.Enabled = false;
            b.Click += new System.EventHandler(this.buttonSample_Click);

            b.Size = sizeLoadedFiles;
            b.Location = new Point(positionXLoadedFiles, positionYLoadedFilesCurrent);
            b.TextAlign = ContentAlignment.MiddleCenter;
            
            positionYLoadedFilesCurrent += incrementYLoadedFiles;

            loadedFiles.Add(ent);
            loadedFilesButtons.Add(b);
            this.panelMain.Controls.Add(b);
        }

        /// <summary>
        /// Generates a button on the right, based on the label. This should be already shortened by creating the Entryplet first
        /// </summary>
        /// <param name="label">Short file name</param>
        private void addGuessButton(string label)
        {
            Button b = new Button();
            b.Click += new System.EventHandler(this.buttonGuess_Click);

            b.Text = label;
            b.Enabled = false;

            b.Size = sizeGuesses;
            b.Location = new Point(positionXGuesses, positionYGuessesCurrent);
            b.TextAlign = ContentAlignment.MiddleCenter;

            positionYGuessesCurrent += incrementYGuesses;
            
            guessButtons.Add(b);
            this.panelMain.Controls.Add(b);
        }

        /// <summary>
        /// Removes the buttons associated from the guesses, both from the list and the graphics control collection.
        /// </summary>
        private void clearGuesses()
        {
            foreach (Button b in guessButtons)
            {
                this.panelMain.Controls.Remove(b);
            }
            guessButtons = new List<Button>();
            positionYGuessesCurrent = positionYGuessesOrigin;
        }

        /// <summary>
        /// Generates the buttons on the rigth used for guess verification based on the Entryplet list loadedFiles.
        /// The list is randomized in the process
        /// </summary>
        private void generateGuessButtons()
        {
            firstGuess = true;

            clearGuesses();
            foreach (Entryplet ent in loadedFiles)
            {
                addGuessButton(ent.fileName);
            }

            loadedFiles = loadedFiles.OrderBy(a => RNG.Next()).ToList<Entryplet>();

            int i = 0;
            foreach (Entryplet ent in loadedFiles)
            {
                ent.hiddenName = "Sample " + (i+1);
                loadedFilesButtons[i].Text = ent.hiddenName;
                loadedFilesButtons[i].Enabled = true;
                i++;
            }
        }

        /// <summary>
        /// Renames all the files from the list to hide true identity when windows plays the file.
        /// <para>The exension is also renamed to be the same, but does not modify file contents</para>
        /// </summary>
        private void renameFiles()
        {
            if (this.extension != null)
            {
                revertFileNames();
            }

            int extensionIndex = loadedFiles[0].filePath.LastIndexOf('.');
            this.extension = extensionIndex == -1 ? "" : loadedFiles[0].filePath.Substring(extensionIndex);

            ///TODO if the file is already is named "Sample x.y", it cannot be renamed to the already existing file
            foreach (Entryplet ent in loadedFiles)
            {
                //MessageBox.Show(ent.filePath, ent.getHiddenPath(extension));
                System.IO.File.Move(ent.filePath, ent.getHiddenPath(extension));

            }
        }

        private void revertFileNames()
        {
            ///The files have not yet been renamed, therefore they cannot be reverted
            if (this.extension == null)
                return;

            int i = 1;
            foreach (Entryplet ent in loadedFiles)
            {
                ///if a new file is added after shuffling, the hidden filename would not work as it has not yet been renamed.
                if (i > guessButtons.Count)
                    break;
                string path = ent.filePath.Substring(0, ent.filePath.IndexOf(ent.fileName));

                //MessageBox.Show(path + ent.hiddenName + extension, ent.filePath);
                System.IO.File.Move(ent.getHiddenPath(extension), ent.filePath);
                i++;
            }
            this.extension = null;
        }

        /// <summary>
        /// Requests windows to open the file (with default application)
        /// </summary>
        /// <param name="filename">Name of the file</param>
        private void playSample(string filename)
        {
            System.Diagnostics.Process.Start(filename);
        }

        private void CloseApp()
        {
            revertFileNames();
            this.Dispose();
            this.Close();
            Application.Exit();
        }

        #region File Insert and Load

        private void pasteItem()
        {
            IDataObject iData = Clipboard.GetDataObject();

            if (iData.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (var v in Clipboard.GetFileDropList())
                {
                    try
                    {
                        addFileToList(v);
                    }
                    catch
                    {
                        Console.WriteLine("Text does not lead do a valid  file");
                    }
                }
            }
        }

        private void LoadFileFromDialog()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "All Files (*.*)|*.*|Image Files|*.jpg;*.png;*.bmp;*.gif;*.tiff;*.ico;"+
                                    "|Music files|*.mp3;*.flac;*.ogg;*.wav|Video Files|*.mp4;*.mkv;*.avi;*.flv";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (string filename in openFileDialog1.FileNames)
                {
                    addFileToList(filename);
                }
            }

        }

        //TODO
        private void ClearWorkspace()
        {
            //selectedPictureBox = null;
            //pictureBoxes.Clear();
            System.GC.Collect();
            labelHelp.Visible = true;
        }

        #endregion

        #region File Remove
        //TODO
        private void removeSelectedFile()
        {
            /*
            pictureBoxes.Remove(selectedPictureBox);
            selectedPictureBox.Image.Dispose();
            selectedPictureBox.Dispose();
            selectedPictureBox = null;
            if (this.pictureBoxes.Count == 0)
            {
                labelHelp.Visible = true;
            }*/
        }
        #endregion

        #endregion

        #region Event Handlers

        #region Menu Events

        #region File

        private void fileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.FileDrop))
            {
                pasteCtrlVToolStripMenuItem.Enabled = true;
            }
            else
            {
                pasteCtrlVToolStripMenuItem.Enabled = false;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadFileFromDialog();
        }

        private void pasteCtrlVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pasteItem();
        }

        private void clearWorkspaceCtrlDToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseApp();
        }

        #endregion

        #region Help

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String text = "Quick and easy way of creating a compilation/story from distinct images.\n" +
                "Offers the possibility to rescale the image after generation, should it be necessary.\n" +
                "\n\n" +
                "The Ignore DPI feature (on by default) only consideres the resolution of the images on compositing.\n" +
                "Disabling it will scale the images based on DPI, 1:1 pixel ratio being at 96DPI (value comes from Visual Studio or Microsoft or sthg).\n" +
                "For example: putting a 182 DPI image with 800*600 pixels will result in an image with 400*300 pixels.\n" +
                "\n\n" +
                "\"Lock-Load Files\" locks the added files to the application so they can't be modified externally.\n" +
                "Might fix some issues with some wierder file configurations, but is usually not necessary" +
                "Only has effect on files added after changing the option (i.e. does not reload files already inside the application)\n" +
                "\n\n" +
                "To report any bugs of issues visit the repo: \n" +
                "https://github.com/RobertKajnak/ImageStoryMerge";
            String caption = "Help/About";
            MessageBox.Show(text, caption);
        }
        #endregion

        #region Options

        private void checkBoxRenameFiles_CheckedChanged(object sender, EventArgs e)
        {

        }

        #endregion

        #endregion

        #region Key Events

        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN)
            {
            }
            else if (m.Msg == WM_KEYUP)
            {
                isControlPressed = false;
            }
            return false;
        }

        /// <summary>
        /// Processes the control keys in the whole form, not allowing it to be hogged by controls such as buttons
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        override protected bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case (Keys.O):
                    LoadFileFromDialog();
                    break;
                case (Keys.D):
                    ClearWorkspace();
                    break;
                    ///TODO - fix control detection
                case (Keys.V):
                    //if (isControlPressed)
                    //{
                        pasteItem();
                    //}
                    break;
                case (Keys.ControlKey):
                    isControlPressed = true;
                    break;
                case (Keys.Delete):
                //intentional fallthrough
                case (Keys.Back):
                    removeSelectedFile();
                    break;
                case (Keys.Escape):
                    CloseApp();
                    break;
            }
            return true;
        }

        private void FormMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            //orderToolStripMenuItem.Text = "" +(int)e.KeyChar;
            switch (e.KeyChar)
            {
                case ((char)27):
                    CloseApp();
                    break;
            }
        }

        private void FormMain_KeyUp(object sender, KeyEventArgs e)
        {
            //orderToolStripMenuItem.Text = "" + e.KeyCode;

            switch (e.KeyCode)
            {
                case (Keys.ControlKey):
                    isControlPressed = false;
                    break;
                default: break;
            }
        }
        #endregion

        #region Button Events

        private void buttonShuffle_Click(object sender, EventArgs e)
        {
            revertFileNames();
            generateGuessButtons();
            renameFiles();
        }

        /// <summary>
        /// Highlights the currently played sample's button and enables guess buttons, if not already enabled.
        /// <para> All sample/hidden name/loadedFileList (on the left) should implement this on creation.</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSample_Click(object sender, EventArgs e)
        {
            if (selectedGuess != null && selectedGuess.Enabled)
            {
                selectedGuess.BackColor = SystemColors.ControlLight;
            }
            selectedGuess = (Button)sender;
            selectedGuess.BackColor = Color.AliceBlue;
            string filename = loadedFiles.Find(ent => ent.hiddenName.Equals(((Button)sender).Text)).getHiddenPath(extension);

            if (firstGuess)
            {
                firstGuess = false;
                foreach (Button b in guessButtons)
                {
                    b.Enabled = true;
                }
            }

            playSample(filename);   
        }

        /// <summary>
        /// Verifies if the sample matches the selected file button. Displays a MessageBox:
        /// <para>On failure promts the user to try again<br/></para>
        /// <para>On success, congratulates and disables the "sample" and the "guess" buttons</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGuess_Click(object sender, EventArgs e)
        {
            string label = ((Button)sender).Text;
            string trueName = loadedFiles.Find(ent => ent.hiddenName.Equals(selectedGuess.Text)).fileName;
            if (trueName == label)
            {
                MessageBox.Show("You have successfully matched " + selectedGuess.Text + " to " + label, "Congratulations!");
                selectedGuess.Enabled = false;
                selectedGuess.BackColor = SystemColors.Control;
                ((Button)sender).Enabled = false;
                
                foreach (Button b in guessButtons)
                {
                    if (b.Enabled == true)
                        return;
                }
                MessageBox.Show("You have guessed all the samples!","Congratulations!");
            }
            else
            {
                MessageBox.Show("Try again", "Incorrect!");
            }
        }

        #endregion

        #region Other Events

        private void panelMain_DoubleClick(object sender, EventArgs e)
        {
            LoadFileFromDialog();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            /*/// otherwise it suppresses KeyPress events
            System.Timers.Timer t = new System.Timers.Timer(10);
            t.AutoReset = false;
            t.Elapsed += (Object source,ElapsedEventArgs ea) => 
            {
                enableCheckBox();
            };
            t.Enabled = true;*/
        }

        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                try
                {
                    addFileToList(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
        }

        private void tableLayoutPanelFiles_Paint(object sender, PaintEventArgs e)
        {
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            revertFileNames();
        }

        #endregion

        #endregion


    }
}
