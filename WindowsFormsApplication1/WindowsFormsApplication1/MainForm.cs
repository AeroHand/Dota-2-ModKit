using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;

namespace ParticleForker
{
    public partial class MainForm : Form
    {
        public static string GameDirectory;
        public static string ContentDirectory;
        public static string CurrAddonName;

        public static ArrayList ContentAddonPaths = new ArrayList();
        public static ArrayList GameAddonPaths = new ArrayList();
        public static ArrayList AddonNames = new ArrayList();
        public static string[] ParticlePaths;

        public static string CurrAddonGamePath;
        public static string CurrAddonContentPath;

        public static string SettingsPath;
        public static bool HasSettings = false;

        public MainForm()
        {
            InitializeComponent();
            string[] files = Directory.GetFiles(Environment.CurrentDirectory);
            SettingsPath = Environment.CurrentDirectory + "\\settings.txt";

            if (System.IO.File.Exists(SettingsPath))
            {
                // check if it's not just blank.
                string text = System.IO.File.ReadAllText(SettingsPath);
                if (text.Length > 3)
                {
                    HasSettings = true;
                }
            }
            else // settings.txt doesn't exist.
            {
                Debug.WriteLine("Creating settings.");
                FileStream file = System.IO.File.Create(SettingsPath);
                file.Close();
            }

            if (HasSettings) {
                string[] lines = System.IO.File.ReadAllLines(SettingsPath);
                // get the last dota_ugc path in the settings.txt.
                // and use that to find the game and content dirs.
                getGameAndContentDirs(lines[lines.Count() - 1]);
                resetAddonNames();
            }

            else
            {
                getUGCPath();
            }

        }

        private bool getUGCPath()
        {
            while (!HasSettings)
            {
                // unzip particles first
                while (!Directory.Exists(Environment.CurrentDirectory + @"\decompiled_particles")) {
                    DialogResult res = MessageBox.Show("No decompiled_particles folder detected. Please extract decompiled_particles.rar into the ParticleForker folder before proceding.", "ParticleForker",
                        MessageBoxButtons.OKCancel,MessageBoxIcon.Exclamation);

                    if (res == DialogResult.Cancel)
                    {
                        Environment.Exit(1);
                    }

                    // this unfortunately gives an Unauthorized access exception on Directory.Move.
                    // Also, can't unzip directly into the Environment.CurrentDirectory because of the PathTooLong exception.

                    /*string zipPath = Environment.CurrentDirectory + @"\particles.zip";
                    string extractPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\decompiled_particles";
                    MessageBox.Show("No decompiled particles detected. Extracting particles; please stand by.", "ParticleForker");
                    ZipFile.ExtractToDirectory(zipPath, extractPath);
                    // move the zipfiles over to the correct folder once done with extracting.
                    Directory.Move(extractPath, Environment.CurrentDirectory + @"\decompiled_particles");*/
                }

                MessageBox.Show("Please select the path to your dota_ugc folder.", "ParticleForker", MessageBoxButtons.OK);
                // get the ugc path from the user.
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    Debug.WriteLine("DialogResult OK");
                }

                string ugcPath = dialog.SelectedPath;
                // check if this is valid.
                string ugc = ugcPath.Substring(ugcPath.LastIndexOf('\\') + 1);
                if (ugc != "dota_ugc")
                {
                    DialogResult res = MessageBox.Show("That is not a path to your dota_ugc folder.", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand);

                    if (res == DialogResult.Retry)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                // write it to settings.txt.
                StreamWriter sw = System.IO.File.AppendText(SettingsPath);
                sw.Write(ugcPath + "\n");
                sw.Close();

                // get the game and content dirs from the ugc path.
                getGameAndContentDirs(ugcPath);
                resetAddonNames();
                HasSettings = true;
                return true;
            }
            return false;
        }

        private void trackingProj_Click(object sender, EventArgs e)
        {

        }
        private void linearProj_Click(object sender, EventArgs e)
        {

        }
        private void targetMin_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText("Hello. I've been copied to the clipboard!");
        }
        private void targetMax_Click(object sender, EventArgs e)
        {

        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }

        private void Customize_Click(object sender, EventArgs e)
        {

        }

        /*private void newAddonBarebones_click(object sender, EventArgs e)
        {

            WebClient wc = new WebClient();
            //Task t = wc.DownloadFileTaskAsync(new Uri("https://github.com/bmddota/barebones/archive/source2.zip"), "barebones.zip");

            AddonBarebonesForm form = new AddonBarebonesForm();
            form.Show();
        }*/

        private void newParticles_Click(object sender, EventArgs e)
        {
            if (!HasSettings)
            {
                if (!getUGCPath())
                {
                    MessageBox.Show("You need to select your dota_ugc path before you can use this.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
            }


            OpenFileDialog fileDialog = new OpenFileDialog();
            Debug.WriteLine("Current directory: " + Environment.CurrentDirectory);
            fileDialog.InitialDirectory = Environment.CurrentDirectory + "\\decompiled_particles";
            fileDialog.Multiselect = true;
            fileDialog.Title = "Select Particles To Copy";
            fileDialog.ShowDialog();
            // check if we actually have filenames, or the user closed the box.
            if (!(fileDialog.FileName == ""))
            {
                    ParticlePaths = fileDialog.FileNames;
                    FolderBrowserDialog browser = new FolderBrowserDialog();
                    // let the user see the particles directory first.
                    string initialPath = CurrAddonContentPath + "\\particles";
                    browser.SelectedPath = initialPath;
                    browser.ShowNewFolderButton = true;
                    browser.Description = "Browse to where the particles will be copied to. They must be placed in the particles directory.";
                    DialogResult browserResult = browser.ShowDialog();

                    if (browserResult == DialogResult.Cancel || browserResult == DialogResult.Abort)
                    {
                        return;
                    }

                    string folderPath = browser.SelectedPath;
                    // make sure the user didn't click cancel before we procede.
                    if (folderPath != "" && folderPath != initialPath)
                    {
                        string folderName = folderPath.Substring(folderPath.LastIndexOf('\\') + 1);
                        int particlesForked = 0;
                        foreach (string path in ParticlePaths)
                        {
                            bool overwriteAllowed = true;
                            string particleName = path.Substring(path.LastIndexOf('\\') + 1);
                            string targetPath = folderPath + "\\" + particleName;
                            try
                            {
                                System.IO.File.Copy(path, targetPath);
                            }
                            catch (IOException overwriteException)
                            {
                                string warn = "You are about to overwrite " + targetPath + ". Overwrite?";
                                DialogResult result = MessageBox.Show(warn, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (!result.Equals(DialogResult.Yes))
                                {
                                    overwriteAllowed = false;
                                }
                            }

                            if (overwriteAllowed)
                            {
                                // fix child refs.
                                string[] lines = System.IO.File.ReadAllLines(targetPath);
                                string allText = "";
                                bool overrwrite = false;

                                for (int i = 0; i < lines.Count(); i++)
                                {
                                    string line = lines[i];
                                    if (line.Contains("string m_ChildRef = "))
                                    {
                                        overrwrite = true;
                                        // we need to completely overwrite this line.
                                        // get the child specified.
                                        string childParticle = line.Substring(line.LastIndexOf('/') + 1);
                                        Debug.WriteLine("Child particle: " + childParticle);
                                        string foldName = folderPath.Substring(folderPath.LastIndexOf('\\') + 1);
                                        string relativePath = "particles/" + foldName + "/";

                                        string newRef = "string m_ChildRef = \"" + relativePath + childParticle + "\n";
                                        lines[i].Remove(0);
                                        lines[i] = newRef;
                                        Debug.WriteLine("New ref: " + newRef);
                                    }
                                    allText += lines[i] + "\n";
                                }
                                if (overrwrite)
                                {
                                    // everything in the array is now correct. copy the array to the new file.
                                    System.IO.File.WriteAllText(targetPath, allText);
                                    particlesForked++;
                                }
                            }
                             
                         }
                        if (particlesForked == 0)
                        {
                            MessageBox.Show("No particles have been copied over.");
                        }
                        else
                        {
                            MessageBox.Show("All selected particles have been copied to: " + folderPath + 
                                "\nand their child references have been updated.", "Success", MessageBoxButtons.OK);
                        }
                    }
                }
            
            }

        private void changeUGCDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.RootFolder = Environment.SpecialFolder.ProgramFilesX86;
            f.ShowNewFolderButton = false;
            DialogResult result = f.ShowDialog();

            if (result == DialogResult.Abort || result == DialogResult.Cancel)
            {
                return;
            }

            // write the ugc path to the end of settings.txt.
            //f.
            StreamWriter sw = System.IO.File.AppendText(SettingsPath);
            sw.Write(f.SelectedPath);
            sw.Close();

            getGameAndContentDirs(f.SelectedPath);
            resetAddonNames();
        }

        private void getGameAndContentDirs(string ugc_path)
        {
            string[] dirs = Directory.GetDirectories(ugc_path);
            foreach (string str2 in dirs)
            {
                if (str2.Contains("game"))
                {
                    //MessageBox.Show(str2);
                    GameDirectory = str2;
                    getGameAddons(str2);

                }
                if (str2.Contains("content"))
                {
                    //MessageBox.Show(str2);
                    ContentDirectory = str2;
                    getContentAddons(str2);
                }
            }
        }

        private void getContentAddons(string str2)
        {
            string[] contentDir = Directory.GetDirectories(str2);
            foreach (string path in contentDir)
            {
                if (path.Contains("dota_addons"))
                {
                    string[] addonsDir = Directory.GetDirectories(path);
                    foreach (string path2 in addonsDir)
                    {
                        ContentAddonPaths.Add(path2);
                    }
                }
            }  
        }

        private void getGameAddons(string str2)
        {
            string[] gameDir = Directory.GetDirectories(str2);
            foreach (string path in gameDir)
            {
                if (path.Contains("dota_addons"))
                {
                    string[] addonsDir = Directory.GetDirectories(path);
                    foreach (string path2 in addonsDir)
                    {
                        //MessageBox.Show(path2);
                        GameAddonPaths.Add(path2);
                    }
                }
            }
        }

        private void resetAddonNames()
        {
            currentAddonDropDown.DropDownItems.Clear();
            AddonNames.Clear();
            bool first = false;
            foreach (string path in GameAddonPaths)
            {
                string addon = path.Substring(path.LastIndexOf('\\') + 1);
                AddonNames.Add(addon);
                //item.Text = addon;
                currentAddonDropDown.DropDownItems.Add(addon);
                if (!first)
                {
                    selectCurrentAddon(addon);
                    first = true;
                }
            }
            currentAddonDropDown.DropDownItemClicked += currentAddonDropDown_DropDownItemClicked;
        }

        void currentAddonDropDown_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (AddonNames.Contains(e.ClickedItem.Text))
            {
                selectCurrentAddon(e.ClickedItem.Text);
            }
        }

        void selectCurrentAddon(string addon)
        {
            CurrAddonName = addon;
            Debug.WriteLine("Current addon: " + CurrAddonName);
            currentAddonDropDown.Text = "Current Addon: " + CurrAddonName;

            this.Text = "Particle Forker - " + CurrAddonName;

            foreach (string str in GameAddonPaths)
            {
                if (str.Contains(CurrAddonName))
                {
                    CurrAddonGamePath = str;
                }
            }
            foreach (string str in ContentAddonPaths)
            {
                if (str.Contains(CurrAddonName))
                {
                    CurrAddonContentPath = str;
                }
            }
        }

        private void currentAddon_Click(object sender, EventArgs e)
        {

        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.Visible = true;
            about.Show();
        }
    }
}
