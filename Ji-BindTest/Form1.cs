using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ji_BindTest
{
    public partial class FormMain : Form
    {

        bool isControlPressed = false;
        public FormMain()
        {
            InitializeComponent();
        }



        #region Utility Classes and Functions

        ///TODO - ob
        private PictureBox addFileToList(string Filename)
        {

            return null;
        }

        #region File Insert and Load

        //TODO - this
        private void pasteItem()
        {
            IDataObject iData = Clipboard.GetDataObject();
            //TODO check this
            if (iData.GetDataPresent(DataFormats.FileDrop))
            {
                //addFileToList();
            }

            if (iData.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (var v in Clipboard.GetFileDropList())
                {
                    try
                    {
                        //addPictureBox(ImageFromFile(v));
                    }
                    catch
                    {
                        Console.WriteLine("Text does not leat do a valid image file");
                    }
                }
            }
        }

        //TODO
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
            this.Dispose();
            this.Close();
            Application.Exit();
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

        #endregion

        #region Key Events
        private void FormMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            //orderToolStripMenuItem.Text = "" +(int)e.KeyChar;
            switch (e.KeyChar)
            {
                case ((char)27):
                    this.Dispose();
                    this.Close();
                    Application.Exit();
                    break;
            }
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case (Keys.O):
                    LoadFileFromDialog();
                    break;
                case (Keys.D):
                    ClearWorkspace();
                    break;
                case (Keys.V):
                    if (isControlPressed)
                    {
                        pasteCtrlVToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case (Keys.ControlKey):
                    isControlPressed = true;
                    break;
                case (Keys.Delete):
                //intentional fallthrough
                case (Keys.Back):
                    removeSelectedFile();
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

        #region Other Events

        private void tableLayoutPanel1_DoubleClick(object sender, EventArgs e)
        {
            LoadFileFromDialog();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
        }

        #endregion

        #endregion

    }
}
