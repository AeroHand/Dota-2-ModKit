using KVLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D2ModKit
{
    public class Addon
    {
        private string name;
        private string contentPath;
        private string gamePath;
        private string npcPath;
        private string copyPath;
        private string addonEnglishPath;
        private string generatedTooltips;
        private string abilitiesCustomPath;
        private string itemCustomPath;
        private string unitsCustomPath;
        private string heroesCustomPath;
        private string relativeParticlePath;
        private List<AbilityEntry> abilityEntries = new List<AbilityEntry>();
        private List<AbilityEntry> itemEntries = new List<AbilityEntry>();
        private List<UnitEntry> unitEntries = new List<UnitEntry>();
        private List<HeroEntry> heroesEntries = new List<HeroEntry>();
        private List<ModifierEntry> modifierItemEntries = new List<ModifierEntry>();
        private List<ModifierEntry> modifierAbilityEntries = new List<ModifierEntry>();
        private List<ModifierEntry> hiddenModifierEntries = new List<ModifierEntry>();
        private HashSet<string> alreadyHasKeys = new HashSet<string>();

        public List<AbilityEntry> AbilityEntries
        {
            get { return abilityEntries; }
            set { abilityEntries = value; }
        }
        private List<string> particlePaths;

        private void getMorePaths()
        {
            AddonEnglishPath = Path.Combine(GamePath, "resource", "addon_english.txt");
            AbilitiesCustomPath = Path.Combine(GamePath, "scripts", "npc", "npc_abilities_custom.txt");
            ItemsCustomPath = Path.Combine(GamePath, "scripts", "npc", "npc_items_custom.txt");
            UnitsCustomPath = Path.Combine(GamePath, "scripts", "npc", "npc_units_custom.txt");
            HeroesCustomPath = Path.Combine(GamePath, "scripts", "npc", "npc_heroes_custom.txt");
            GeneratedTooltips = Path.Combine(GamePath, "resource", "tooltips.txt");
        }

        public Addon(string _gamePath)
        {
            gamePath = _gamePath;
            name = gamePath.Substring(gamePath.LastIndexOf('\\') + 1);
            getMorePaths();
        }

        public Addon(string _contentPath, string _gamePath)
        {
            contentPath = _contentPath;
            gamePath = _gamePath;
            name = _contentPath.Substring(_contentPath.LastIndexOf('\\') + 1);
            getMorePaths();
        }

        public string ItemsCustomPath
        {
            get { return itemCustomPath; }
            set { itemCustomPath = value; }
        }

        public string UnitsCustomPath
        {
            get { return unitsCustomPath; }
            set { unitsCustomPath = value; }
        }

        public string HeroesCustomPath
        {
            get { return heroesCustomPath; }
            set { heroesCustomPath = value; }
        }

        public string GeneratedTooltips
        {
            get { return generatedTooltips; }
            set { generatedTooltips = value; }
        }

        public string AddonEnglishPath
        {
            get { return addonEnglishPath; }
            set { addonEnglishPath = value; }
        }

        public string AbilitiesCustomPath
        {
            get { return abilitiesCustomPath; }
            set { abilitiesCustomPath = value; }
        }

        public string RelativeParticlePath
        {
            get { return relativeParticlePath; }
            set { relativeParticlePath = value; }
        }

        public string CopyPath
        {
            get { return copyPath; }
            set { copyPath = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string ContentPath
        {
            get { return contentPath; }
            set { contentPath = value; }
        }

        public string GamePath
        {
            get { return gamePath; }
            set { gamePath = value; }
        }

        public string NPCPath
        {
            get { return npcPath;  }
            set { npcPath = value; }
        }

        public List<string> ParticlePaths
        {
            get { return particlePaths; }
            set { particlePaths = value; }
        }

        public void getCurrentAddonEnglish()
        {
            // Parse addon_english.txt KV
            KeyValue[] addonEnglishKeyVals = KVParser.ParseAllKVRootNodes(System.IO.File.ReadAllText(addonEnglishPath));
            for (int i = 0; i < addonEnglishKeyVals.Length; i++)
            {
                
            }
        }

        public void getAbilityTooltips(bool items)
        {
            if (items)
            {
                itemEntries.Clear();
                modifierItemEntries.Clear();
            }
            else
            {
                AbilityEntries.Clear();
                modifierAbilityEntries.Clear();
            }
            //hiddenModifierEntries.Clear();

            // Parse abilities_custom.txt KV

            KeyValue[] abilitiesCustomKeyVals = KVParser.ParseAllKVRootNodes(System.IO.File.ReadAllText(AbilitiesCustomPath));
            if (items)
            {
                abilitiesCustomKeyVals = KVParser.ParseAllKVRootNodes(System.IO.File.ReadAllText(ItemsCustomPath));
            }

            IEnumerable<KeyValue> abilityNames = abilitiesCustomKeyVals[0].Children;
            for (int i = 0; i < abilityNames.Count(); i++)
            {
                KeyValue ability = abilityNames.ElementAt(i);
                if (ability.Key == "Version")
                {
                    continue;
                }
                // NOTE: can't have a blank comment (//) above the ability or else the Key will be blank.
                //Debug.WriteLine("Abil name: " + ability.Key);
                if (ability.HasChildren)
                {

                    IEnumerable<KeyValue> children = ability.Children;
                    // Find the abilityspecial stuff.
                    for (int j = 0; j < children.Count(); j++)
                    {
                        KeyValue child = children.ElementAt(j);
                        if (child.Key == "AbilitySpecial" || child.Key == "Modifiers")
                        {
                            bool modifiers = false;
                            if (child.Key == "Modifiers")
                            {
                                modifiers = true;
                            }

                            // We have the AbilitySpecial now. See if there is actually anything in it.
                            if (child.HasChildren)
                            {
                                List<string> kvs = new List<string>();
                                IEnumerable<KeyValue> children2 = child.Children;
                                for (int k = 0; k < children2.Count(); k++)
                                {
                                    KeyValue child2 = children2.ElementAt(k);
                                    bool isHidden = false;
                                    if (child2.HasChildren)
                                    {
                                        IEnumerable<KeyValue> children3 = child2.Children;
                                        for (int l = 0; l < children3.Count(); l++)
                                        {
                                            KeyValue child3 = children3.ElementAt(l);
                                            if (modifiers)
                                            {
                                                if (child3.Key == "IsHidden")
                                                {
                                                    // Ensure it's actually hidden.
                                                    Debug.WriteLine("IsHidden? " + child3.GetString());
                                                    if (child3.GetString() == "1")
                                                    {
                                                        isHidden = true;
                                                    }
                                                }
                                            }
                                            else // we have a modifier, not ability.
                                            {
                                                // Map item name to its item specials.
                                                if (child3.Key != "var_type")
                                                {
                                                    kvs.Add(child3.Key);
                                                }
                                            }
                                        }
                                    }
                                    // we're done going through all the children of this ability/modifier.
                                    if (modifiers)
                                    {
                                        if (!isHidden)
                                        {
                                            if (items)
                                            {
                                                modifierItemEntries.Add(new ModifierEntry(child2.Key));
                                            }
                                            else
                                            {
                                                modifierAbilityEntries.Add(new ModifierEntry(child2.Key));
                                            }
                                        }
                                        else
                                        {
                                            //hiddenModifierEntries.Add(new ModifierEntry(child2.Key));
                                        }
                                    }
                                }
                                if (!modifiers)
                                {
                                    if (items)
                                    {
                                        itemEntries.Add(new AbilityEntry(ability.Key, kvs));
                                    }
                                    else
                                    {
                                        AbilityEntries.Add(new AbilityEntry(ability.Key, kvs));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void getHeroesTooltips()
        {
            heroesEntries.Clear();
            KeyValue[] heroesKeyVals = KVParser.ParseAllKVRootNodes(System.IO.File.ReadAllText(heroesCustomPath));
            IEnumerable<KeyValue> children = heroesKeyVals[0].Children;
            for (int i = 0; i < children.Count(); i++)
            {
                KeyValue child = children.ElementAt(i);
                if (child.HasChildren)
                {
                    IEnumerable<KeyValue> children2 = child.Children;
                    for (int j = 0; j < children2.Count(); j++)
                    {
                        KeyValue child2 = children2.ElementAt(j);
                        if (child2.Key == "override_hero")
                        {
                            HeroEntry h = new HeroEntry(child2.GetString());
                            //Debug.WriteLine(h.ToString());
                        }

                    }
                }

            }
        }

        public void getUnitTooltips()
        {
            unitEntries.Clear();
            KeyValue[] unitsKeyVals = KVParser.ParseAllKVRootNodes(System.IO.File.ReadAllText(unitsCustomPath));
            IEnumerable<KeyValue> children = unitsKeyVals[0].Children;
            for (int i = 0; i < children.Count(); i++)
            {
                string unit = children.ElementAt(i).Key;
                if (unit != "Version")
                {
                    unitEntries.Add(new UnitEntry(unit));
                }
            }
            for (int i = 0; i < unitEntries.Count(); i++)
            {
               // Debug.WriteLine(unitEntries.ElementAt(i).ToString());
            }

        }

        public void writeTooltips()
        {
            alreadyHasKeys.Clear();
            if (Directory.Exists(addonEnglishPath))
            {
                KeyValue[] currAddonEnglish = KVParser.ParseAllKVRootNodes(System.IO.File.ReadAllText(addonEnglishPath));
                IEnumerable<KeyValue> children = currAddonEnglish[0].Children;

                for (int i = 0; i < children.Count(); i++)
                {
                    KeyValue child = children.ElementAt(i);
                    if (child.HasChildren)
                    {
                        IEnumerable<KeyValue> children2 = child.Children;
                        for (int j = 0; j < children2.Count(); j++)
                        {
                            KeyValue child2 = children2.ElementAt(j);
                            alreadyHasKeys.Add(child2.Key);
                        }
                    }
                }
            }

            // WriteAllText will clear the contents of this file first
            string header = 
                "// **********************************************************************************************************************\n" +
                "// This file contains generated tooltips created from the files in the scripts/npc directory of this mod.\n" +
                "// It does not contain tooltips already defined in addon_english.txt, nor modifiers with the property \"IsHidden\" \"1\".\n" +
                "// **********************************************************************************************************************\n";
            System.IO.File.WriteAllText(generatedTooltips, header, Encoding.Unicode);


            string head1 = "\n// ******************** HEROES ********************\n";
            System.IO.File.AppendAllText(GeneratedTooltips, head1, Encoding.Unicode);
            for (int i = 0; i < heroesEntries.Count(); i++)
            {
                HeroEntry hero = heroesEntries.ElementAt(i);
                if (!alreadyHasKeys.Contains(hero.Hero.Key))
                {
                    System.IO.File.AppendAllText(GeneratedTooltips, hero.ToString(), Encoding.Unicode);

                }
            }

            string head2 = "\n// ******************** UNITS ********************\n";
            System.IO.File.AppendAllText(GeneratedTooltips, head2, Encoding.Unicode);
            for (int i = 0; i < unitEntries.Count(); i++)
            {
                UnitEntry unit = unitEntries.ElementAt(i);
                if (!alreadyHasKeys.Contains(unit.Name.Key))
                {
                    System.IO.File.AppendAllText(GeneratedTooltips, unit.ToString(), Encoding.Unicode);
                }
            }

            string head3 = "\n// ******************** ABILITY MODIFIERS ********************\n";
            System.IO.File.AppendAllText(GeneratedTooltips, head3, Encoding.Unicode);
            for (int i = 0; i < modifierAbilityEntries.Count(); i++)
            {
                ModifierEntry mod = modifierAbilityEntries.ElementAt(i);
                if (!alreadyHasKeys.Contains(mod.Name.Key))
                {
                    System.IO.File.AppendAllText(GeneratedTooltips, mod.ToString() + "\n", Encoding.Unicode);

                }
            }

            string head6 = "\n// ******************** ITEM MODIFIERS ********************\n";
            System.IO.File.AppendAllText(GeneratedTooltips, head6, Encoding.Unicode);
            for (int i = 0; i < modifierItemEntries.Count(); i++)
            {
                ModifierEntry mod = modifierItemEntries.ElementAt(i);
                if (!alreadyHasKeys.Contains(mod.Name.Key))
                {
                    System.IO.File.AppendAllText(GeneratedTooltips, mod.ToString() + "\n", Encoding.Unicode);
                }
            }

            string head4 = "\n// ******************** ABILITIES ********************\n";
            System.IO.File.AppendAllText(GeneratedTooltips, head4, Encoding.Unicode);
            for (int i = 0; i < abilityEntries.Count(); i++)
			{
                AbilityEntry abil = abilityEntries.ElementAt(i);
                if (!alreadyHasKeys.Contains(abil.Name.Key))
                {
                    System.IO.File.AppendAllText(GeneratedTooltips, abil.ToString() + "\n", Encoding.Unicode);
                }
			}

            string head5 = "\n// ******************** ITEMS ********************\n";
            System.IO.File.AppendAllText(GeneratedTooltips, head5, Encoding.Unicode);
            for (int i = 0; i < itemEntries.Count(); i++)
            {
                AbilityEntry item = itemEntries.ElementAt(i);
                if (!alreadyHasKeys.Contains(item.Name.Key))
                {
                    System.IO.File.AppendAllText(GeneratedTooltips, item.ToString() + "\n", Encoding.Unicode);
                }
            }

            /*string head7 = "\n// ******************** HIDDEN MODIFIERS ********************\n";
            System.IO.File.AppendAllText(GeneratedTooltips, head7, Encoding.Unicode);
            for (int i = 0; i < hiddenModifierEntries.Count(); i++)
            {
                ModifierEntry mod = hiddenModifierEntries.ElementAt(i);
                if (!alreadyHasKeys.Contains(mod.Name.Key))
                {
                    System.IO.File.AppendAllText(GeneratedTooltips, mod.ToString() + "\n", Encoding.Unicode);
                }
            }*/

            // open the tooltips.txt in a text editor
            Process.Start(Path.Combine(gamePath, "resource", "tooltips.txt"));

            //MessageBox.Show("Tooltips successfully generated in: " + Path.Combine(gamePath,"resource", "tooltips.txt"), "Success",
            //    MessageBoxButtons.OK, MessageBoxIcon.Information);

        }
    }
}
