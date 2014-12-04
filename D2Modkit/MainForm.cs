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
using KVLib;
using Microsoft.Win32;

namespace D2ModKit
{
    public partial class MainForm : Form
    {
        private string gameDirectory;
        private string contentDirectory;
        private List<string> addonNames;
        private List<string> gameAddonPaths;
        private List<string> contentAddonPaths;

        private List<Addon> addons = new List<Addon>();
        private Addon currAddon;

        private string ugcPath = "";
        private bool hasSettings = false;

        private bool HasSettings
        {
            get { return hasSettings; }
            set { hasSettings = value; }
        }

        public List<string> ContentAddonPaths
        {
            get
            {
                List<string> paths = new List<string>();
                foreach (Addon a in addons)
                {
                    paths.Add(a.ContentPath);
                }
                return paths;
            }
            set { contentAddonPaths = value; }
        }

        public List<string> GameAddonPaths
        {
            get
            {
                List<string> paths = new List<string>();
                foreach (Addon a in addons)
                {
                    paths.Add(a.GamePath);
                }
                return paths;
            }
            set { gameAddonPaths = value; }
        }

        public List<string> AddonNames
        {
            get
            {
                List<string> names = new List<string>();
                foreach (Addon a in addons)
                {
                    names.Add(a.Name);
                }
                return names;
            }
            set { addonNames = value; }
        }

        public string UGCPath
        {
            get { return ugcPath; }
            set { ugcPath = value; }
        }

        public string GameDirectory
        {
            get { return gameDirectory; }
            set { gameDirectory = value; }
        }

        public string ContentDirectory
        {
            get { return contentDirectory; }
            set { contentDirectory = value; }
        }

        public Addon CurrentAddon
        {
            get { return currAddon; }
            set { currAddon = value; }
        }

        public List<Addon> Addons
        {
            get { return addons; }
            set { addons = value; }
        }

        public MainForm()
        {
            InitializeComponent();
            string[] files = Directory.GetFiles(Environment.CurrentDirectory);
            currentAddonDropDown.DropDownItemClicked += currentAddonDropDown_DropDownItemClicked;

            if (Properties.Settings.Default.UGCPath != "")
            {
                UGCPath = Properties.Settings.Default.UGCPath;
                if (Directory.Exists(UGCPath))
                {
                    HasSettings = true;
                }
            }

            if (HasSettings) {
                // and use that to find the game and content dirs.
                getAddons();
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
                // Auto-find the dota_ugc path.
                RegistryKey regKey = Registry.LocalMachine;
                try
                {
                    regKey = regKey.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 570");
                    if (regKey != null)
                    {
                        string dir = regKey.GetValue("InstallLocation").ToString();
                        UGCPath = Path.Combine(dir, "dota_ugc");
                        Debug.WriteLine("Directory: " + dir);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Please select the path to your dota_ugc folder.", "D2ModKit", MessageBoxButtons.OK);
                    FolderBrowserDialog dialog = new FolderBrowserDialog();

                    DialogResult result = dialog.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        Debug.WriteLine("DialogResult OK");
                    }

                    ugcPath = dialog.SelectedPath;
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
                }

                Properties.Settings.Default.UGCPath = UGCPath;
                Properties.Settings.Default.Save();

                // get the game and content dirs from the ugc path.
                getAddons();
                resetAddonNames();
                HasSettings = true;
                return true;
            }
            return false;
        }

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

            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "decompiled_particles")))
            {
                MessageBox.Show("No decompiled_particles folder detected. Please place a decompiled_particles folder into the D2ModKit folder before proceding.", 
                    "D2ModKit",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            OpenFileDialog fileDialog = new OpenFileDialog();
            Debug.WriteLine("Current directory: " + Environment.CurrentDirectory);
            fileDialog.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "decompiled_particles");
            fileDialog.Multiselect = true;
            fileDialog.Title = "Select Particles To Copy";
            DialogResult res = fileDialog.ShowDialog();
            // check if we actually have filenames, or the user closed the box.
            if (res != DialogResult.OK)
            {
                return;
            }
            string[] particlePaths = fileDialog.FileNames;
            FolderBrowserDialog browser = new FolderBrowserDialog();
            // let the user see the particles directory first.
            string initialPath = Path.Combine(currAddon.ContentPath,"particles");
            browser.SelectedPath = initialPath;
            browser.ShowNewFolderButton = true;
            browser.Description = "Browse to where the particles will be copied to. They must be placed in the particles directory.";
            DialogResult browserResult = browser.ShowDialog();

            if (browserResult == DialogResult.Cancel || browserResult == DialogResult.Abort)
            {
                return;
            }

            string folderPath = browser.SelectedPath;

            // prompt user if he wants to change particle's color
            DialogResult r = MessageBox.Show("Would you like to change the color of this particle system?", "D2ModKit", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            bool changeColor = false;
            string[] rgb = new string[3];

            if (r == DialogResult.Yes)
            {
                changeColor = true;
                ColorDialog color = new ColorDialog();
                color.AnyColor = true;
                color.AllowFullOpen = true;
                DialogResult re = color.ShowDialog();
                if (re == DialogResult.OK)
                {
                    Color picked = color.Color;
                    rgb[0] = picked.R.ToString();
                    rgb[1] = picked.G.ToString();
                    rgb[2] = picked.B.ToString();
                }

            }

            string folderName = folderPath.Substring(folderPath.LastIndexOf('\\') + 1);
            int particlesCopied = 0;

            // this is just to make the final output look prettier.
            string relativePathWin32 = "";
            bool relativePathWin32Set = false;

            foreach (string path in particlePaths)
            {
                bool overwriteAllowed = true;
                string particleName = path.Substring(path.LastIndexOf('\\') + 1);
                string targetPath = folderPath + "\\" + particleName;
                particlesCopied++;
                try
                {
                    System.IO.File.Copy(path, targetPath);
                }
                catch (IOException overwriteException)
                {
                    string warn = "You are about to overwrite " + targetPath + ". Procede?";
                    DialogResult result = MessageBox.Show(warn, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (!result.Equals(DialogResult.Yes))
                    {
                        overwriteAllowed = false;
                        particlesCopied--;
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

                            // Get the relative folder path for the child references.
                            string[] pathArr = folderPath.Split('\\');
                            string relFolderPath = "";
                            bool start = false;
                            for (int j = 0; j < pathArr.Length; j++)
                            {
                                if (pathArr[j] == "particles")
                                {
                                    start = true;
                                }

                                if (start)
                                {
                                    relFolderPath += pathArr[j] + "/";
                                }
                            }

                            // this is just to make the output look prettier.
                            if (relativePathWin32Set == false)
                            {
                                relativePathWin32 += relFolderPath.Replace('/', '\\');
                                relativePathWin32Set = true;
                            }

                            string newRef = "string m_ChildRef = \"" + relFolderPath + childParticle + "\n";
                            //lines[i].Remove(0);
                            lines[i] = newRef;
                        }
                        if (changeColor)
                        {
                            if (line.Contains("ColorMin") || line.Contains("ColorMax") || line.Contains("ConstantColor") || line.Contains("TintMin")
                                || line.Contains("TintMax"))
                            {
                                overrwrite = true;
                                string part1 = line.Substring(0, line.LastIndexOf('=')+2);
                                string part2 = line.Substring(line.LastIndexOf('=')+2);
                                //Debug.WriteLine("Part1: " + part1);
                                //Debug.WriteLine("Part2: " + part2);
                                part2 = part2.Replace("(","");
                                part2 = part2.Replace(")","");
                                string[] nums = part2.Split(',');
                                string lastNum = nums[3];
                                string newPart2 = "(" + " " + rgb[0] + ", " + rgb[1] + ", " + rgb[2] + ", " + lastNum + " )";
                                Debug.WriteLine("New part2: " + newPart2);
                                lines[i] = part1 + newPart2;
                            }
                        }

                        allText += lines[i] + "\n";
                    }
                    if (overrwrite)
                    {
                        // everything in the array is now correct. copy the array to the new file.
                        System.IO.File.WriteAllText(targetPath, allText);
                    }
                }
            }
            if (particlesCopied == 0)
            {
                MessageBox.Show("No particles have been copied over.", "D2ModKit", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {

                MessageBox.Show("Particles have been copied to: \\" + relativePathWin32 + 
                    " and their child references have been updated.",
                    "D2ModKit", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }            
        }

        private void getAddons()
        {
            string[] dirs = Directory.GetDirectories(UGCPath);
            foreach (string str in dirs)
            {
                if (str.Contains("game"))
                {
                    GameDirectory = str;
                    string dota_addons = Path.Combine(GameDirectory, "dota_addons");
                    if (Directory.Exists(dota_addons))
                    {
                        string[] dirs2 = Directory.GetDirectories(dota_addons);
                        foreach (string str2 in dirs2)
	                    {
                            Addon a = new Addon(str2);
                            a.ContentPath = Path.Combine(UGCPath, "content", "dota_addons", a.Name);
                            addons.Add(a);
	                    }
                    }
                }
            }
            resetAddonNames();
        }

        private void resetAddonNames()
        {
            currentAddonDropDown.DropDownItems.Clear();
            bool first = false;
            foreach (string name in AddonNames)
            {
                ToolStripItem item = currentAddonDropDown.DropDownItems.Add(name);
                item.Font = new Font("Segoe UI",12f, FontStyle.Bold, GraphicsUnit.Pixel);
                if (!first)
                {
                    currAddon = getAddonFromName(name);
                    selectCurrentAddon(name);
                    first = true;
                }
            }
        }

        void currentAddonDropDown_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (AddonNames.Contains(e.ClickedItem.Text))
            {
                selectCurrentAddon(e.ClickedItem.Text);
            }
        }

        private Addon getAddonFromName(string name)
        {
            foreach (Addon a in addons)
            {
                if (a.Name == name)
                {
                    return a;
                }
            }
            return null;
        }

        void selectCurrentAddon(string addon)
        {
            currAddon = getAddonFromName(addon);
            Debug.WriteLine("Current addon: " + currAddon.Name);
            currentAddonDropDown.Text = "Addon: " + currAddon.Name;

            this.Text = "D2 ModKit - " + currAddon.Name;
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.Show();
        }

        private void generateAddonEnglish_Click(object sender, EventArgs e)
        {
            //first take the existing addon_english and store the keys and values, so we don't override the ones already defined.
            //currAddon.getCurrentAddonEnglish();
            currAddon.getAbilityTooltips(false);
            currAddon.getAbilityTooltips(true);
            currAddon.getUnitTooltips();
            currAddon.getHeroesTooltips();
            currAddon.writeTooltips();
        }

       

        private void copyToFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.RootFolder = Environment.SpecialFolder.MyDocuments;
            f.Description = "Enter the folder to copy this addon's game and content directories to:";
            DialogResult res = f.ShowDialog();
            if (res == DialogResult.OK)
            {
                currAddon.CopyPath = f.SelectedPath;
            }
            else
            {
                return;
            }
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = @"C:\WINDOWS\system32\xcopy.exe";
            Debug.WriteLine("Content: " + currAddon.ContentPath);
            Debug.WriteLine("Path: " + currAddon.CopyPath);
            proc.StartInfo.Arguments = "\"" + currAddon.ContentPath + "\" \"" + Path.Combine(currAddon.CopyPath, "content") + "\" /D /E /I /Y"; //@"C:\source C:\destination /E /I";
            proc.Start();
            proc.StartInfo.Arguments = "\"" + currAddon.GamePath + "\" \"" + Path.Combine(currAddon.CopyPath, "game") + "\" /D /E /I /Y";
            proc.Start();
            proc.Close();

            //DialogResult r = MessageBox.Show("Would you like this addon to copy to this location everytime the \"Copy To Folder\" button is clicked?", "D2ModKit",
            //    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        }

        private void gameDir_Click(object sender, EventArgs e)
        {
            Process.Start(currAddon.GamePath);
        }

        private void contentDir_Click(object sender, EventArgs e)
        {
            Process.Start(currAddon.ContentPath);
        }
    }
}
