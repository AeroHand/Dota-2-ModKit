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

namespace ParticleForker
{
    public partial class MainForm : Form
    {
        private string gameDirectory;
        private string contentDirectory;
        private List<string> addonNames;
        private List<string> gameAddonPaths;
        private List<string> contentAddonPaths;
        //private Hashtable abilitySpecials = new Hashtable();
        List<List<string>> abilitySpecials = new List<List<string>>();

        private List<Addon> addons = new List<Addon>();
        private Addon currAddon;

        private string ugcPath = "";
        private string settings;
        private bool hasSettings = false;

        private List<List<string>> AbilitySpecials
        {
            get { return abilitySpecials; }
            set { abilitySpecials = value;}
        }

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

        public string Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public MainForm()
        {
            InitializeComponent();
            string[] files = Directory.GetFiles(Environment.CurrentDirectory);
            Settings = Path.Combine(Environment.CurrentDirectory,"settings.txt");
            currentAddonDropDown.DropDownItemClicked += currentAddonDropDown_DropDownItemClicked;

            if (System.IO.File.Exists(Settings))
            {
                // check if it's not just blank.
                string text = System.IO.File.ReadAllText(Settings);
                if (text.Length > 3)
                {
                    HasSettings = true;
                }
            }
            else // settings.txt doesn't exist.
            {
                Debug.WriteLine("Creating settings.");
                FileStream file = System.IO.File.Create(Settings);
                file.Close();
            }

            if (HasSettings) {
                string[] lines = System.IO.File.ReadAllLines(Settings);
                UGCPath = lines[lines.Count() - 1];
                // get the last dota_ugc path in the settings.txt.
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
                // unzip particles first
                while (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "decompiled_particles"))) {
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

                // Auto-find the dota_ugc path.
                string programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string possiblePath = Path.Combine(programfiles, "Steam", "SteamApps", "common", "dota 2 beta", "dota_ugc");
                string possiblePath2 = Path.Combine(programfiles, "Steam", "SteamApps", "common", "dota 2", "dota_ugc");
                if (Directory.Exists(possiblePath))
                {
                    ugcPath = possiblePath;
                    MessageBox.Show("Path to dota_ugc detected: " + possiblePath, "ParticleForker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (Directory.Exists(possiblePath2))
                {
                    ugcPath = possiblePath2;
                    MessageBox.Show("Path to dota_ugc detected: " + possiblePath2, "ParticleForker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // get the ugc path from the user.
                else
                {
                    MessageBox.Show("Please select the path to your dota_ugc folder.", "ParticleForker", MessageBoxButtons.OK);
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

                // write it to settings.txt.
                StreamWriter sw = System.IO.File.AppendText(Settings);
                sw.Write(ugcPath + "\n");
                sw.Close();

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


            OpenFileDialog fileDialog = new OpenFileDialog();
            Debug.WriteLine("Current directory: " + Environment.CurrentDirectory);
            fileDialog.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "decompiled_particles");
            fileDialog.Multiselect = true;
            fileDialog.Title = "Select Particles To Copy";
            DialogResult res = fileDialog.ShowDialog();
            // check if we actually have filenames, or the user closed the box.
            if (res == DialogResult.OK)
            {
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
                    // make sure the user didn't click cancel before we procede.
                    if (folderPath != "")
                    {
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
                                        Debug.WriteLine("New ref: " + newRef);
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
                            MessageBox.Show("No particles have been copied over.", "ParticleForker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {

                            MessageBox.Show("Particles have been copied to: " + relativePathWin32 + 
                                " and their child references have been updated.", 
                                "ParticleForker", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            StreamWriter sw = System.IO.File.AppendText(Settings);
            sw.Write(f.SelectedPath);
            sw.Close();

            getAddons();
            resetAddonNames();
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
                            a.ContentPath = UGCPath + @"\content\dota_addons\" + a.Name;
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
            //AddonNames.Clear();
            bool first = false;
            foreach (string name in AddonNames)
            {
                //string addon = path.Substring(path.LastIndexOf('\\') + 1);
                //AddonNames.Add(addon);
                currentAddonDropDown.DropDownItems.Add(name);
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
            currentAddonDropDown.Text = "Current Addon: " + currAddon.Name;

            this.Text = "Particle Forker - " + currAddon.Name;
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.Show();
        }

        private void generateAddonEnglish_Click(object sender, EventArgs e)
        {
            //first take the existing addon_english and store the keys and values, so we don't override the ones already defined.
            string addon_englishPath = Path.Combine(currAddon.GamePath, "resource", "addon_english.txt");
            string abilities_customPath = Path.Combine(currAddon.GamePath, "scripts", "npc", "npc_abilities_custom.txt");

            if (!System.IO.File.Exists(addon_englishPath))
            {
                return;
            }

            // Parse addon_english.txt KV
            KeyValue[] addonEnglishKeyVals = KVParser.ParseAllKVRootNodes(System.IO.File.ReadAllText(addon_englishPath));
            for (int i = 0; i < addonEnglishKeyVals.Length; i++)
            {
                //Debug.WriteLine(addonEnglishKeyVals[i].ToString());
            }

            // Parse abilities_custom.txt KV
            KeyValue[] abilitiesCustomKeyVals = null;
            //try
           // {
                abilitiesCustomKeyVals = KVParser.ParseAllKVRootNodes(System.IO.File.ReadAllText(abilities_customPath));
            //}
           // catch (IndexOutOfRangeException e2) {

            //}

            IEnumerable<KeyValue> abilityNames = abilitiesCustomKeyVals[0].Children;
            for (int i = 1; i < abilityNames.Count(); i++)
            {
                // Start at 1 because the first key is "Version"
                KeyValue ability = abilityNames.ElementAt(i);
                if (ability.HasChildren)
                {

                    IEnumerable<KeyValue> abilChildren = ability.Children;
                    // Find the abilityspecial stuff.
                    for (int j = 0; j < abilChildren.Count(); j++)
                    {
                        KeyValue child = abilChildren.ElementAt(j);
                        if (child.Key == "AbilitySpecial")
                        {
                            // We have the AbilitySpecial now. See if there is actually anything in it.
                            if (child.HasChildren)
                            {
                                IEnumerable<KeyValue> more = child.Children;
                                for (int k = 0; k < more.Count(); k++)
                                {
                                    KeyValue child2 = more.ElementAt(k);
                                    if (child2.HasChildren)
                                    {
                                        IEnumerable<KeyValue> children2 = child2.Children;
                                        List<string> kvs = new List<string>();
                                        for (int l = 0; l < children2.Count(); l++)
                                        {
                                            // Map ability name to its ability specials.

                                            kvs.Add(children2.ElementAt(l).GetString());
                                            Debug.WriteLine(children2.ElementAt(l));
                                        }
                                        abilitySpecials.Add(kvs);
                                    }
                                }
                            }

                        }
                    }

                }
                //Debug.WriteLine(abilityNames.ElementAt(i).Key);
            }
            //parseAbilitySpecials();

            /*for (int i = 0; i < abilitiesCustomKeyVals.Count(); i++)
            {
                Debug.WriteLine("Count is: " + i);
                KeyValue ability = abilitiesCustomKeyVals[i];

                if (ability.HasChildren)
                {
                    IEnumerable<KeyValue> children = ability.Children;
                    for (int j = 0; j < children.Count(); j++)
                    {
                        Debug.WriteLine(children.ElementAt(j).Key);
                        //Debug.WriteLine(children.ElementAt(j).ToString());
                    }
                }
            }*/
        }

       

        private void copyToGitFolder_Click(object sender, EventArgs e)
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

            //DialogResult r = MessageBox.Show("Would you like this addon to copy to this location everytime the \"Copy To Folder\" button is clicked?", "ParticleForker",
            //    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        }
    }
}
