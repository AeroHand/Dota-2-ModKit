using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2ModKit
{
    public class Pair
    {
        private string key, val;

        public string Val
        {
            get { return val; }
            set { val = value; }
        }

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        public Pair(string _key, string _val)
        {
            key = _key;
            val = _val;
        }
        
        public override string ToString()
        {
            // determine amount of whitespace
            string whitespace = "";
            int count = 80 - key.Length;
            if (count > 2)
            {
                for (int i = 0; i < count; i++)
                {
                    whitespace += " ";
                }
            }
            // the key is really long, so just add some tabs.
            else
            {
                whitespace += "\t\t";
            }

            string str = "\"" + key + "\"" + whitespace + "\"" + val + "\"\n";
            return str;
        }
    }

    public class AddonEnglishEntry
    {
        private int numPairs;

        public int NumPairs
        {
            get { return numPairs; }
            set { numPairs = value; }
        }

        public AddonEnglishEntry()
        {
            NumPairs = 1;
        }

        public AddonEnglishEntry(int _numPairs)
        {
            NumPairs = _numPairs;
        }
    }
    public class ModifierEntry : AddonEnglishEntry
    {
        private Pair name, description;

        public Pair Description
        {
            get { return description; }
            set { description = value; }
        }

        public ModifierEntry(string _name)
        {
            // prevent modifier_modifier names
            name = new Pair("DOTA_Tooltip_modifier_" + _name, "");
            if (_name.Length > 8)
            {
                if (_name.Substring(0, 8) == "modifier")
                {
                    name = new Pair("DOTA_Tooltip_" + _name, "");
                }
            }
            description = new Pair(name.Key + "_Description", "");
        }

        public Pair Name
        {
            get { return name; }
            set { name = value; }
        }
        public override string ToString()
        {
            string str = "";
            str += Name.ToString();
            str += Description.ToString();
            return str;
        }
    }

    public class UnitEntry : AddonEnglishEntry
    {
        private Pair name;

        public UnitEntry(string _name)
        {
            name = new Pair(_name, "");
        }

        public Pair Name
        {
            get { return name; }
            set { name = value; }
        }
        public override string ToString()
        {
            return name.ToString();
        }
    }

    public class HeroEntry : AddonEnglishEntry
    {
        private string heroName;

        public string HeroName
        {
            get { return heroName; }
            set { heroName = value; }
        }
        private Pair hero;

        public Pair Hero
        {
            get { return hero; }
            set { hero = value; }
        }

        public HeroEntry(string _name)
        {
            hero = new Pair(_name, "");
        }

        public override string ToString()
        {
            return hero.ToString();
        }
    }

    public class AbilityEntry : AddonEnglishEntry
    {
        // description and lore are default for abilities.
        private Pair name, description, lore, note0;

        private List<Pair> abilitySpecials = new List<Pair>();

        public List<Pair> AbilitySpecials
        {
            get { return abilitySpecials; }
            set { abilitySpecials = value; }
        }

        public Pair Name
        {
            get { return name; }
            set { name = value; }
        }

        public Pair Description
        {
            get { return description; }
            set { description = value; }
        }

        public Pair Lore
        {
            get { return lore; }
            set { lore = value; }
        }

        public Pair Note0
        {
            get { return note0; }
            set { note0 = value; }
        }

        public AbilityEntry(string _name, List<string> keys) : base(keys.Count()+1)
        {
            name = new Pair("DOTA_Tooltip_ability_" + _name, "");
            description = new Pair(name.Key + "_Description", "");
            note0 = new Pair(name.Key + "_Note0", "");
            lore = new Pair(name.Key + "_Lore", "");
            AbilitySpecials = new List<Pair>(keys.Count());

            for (int i = 0; i < keys.Count(); i++)
            {
                AbilitySpecials.Add(new Pair(name.Key + "_" + keys.ElementAt(i), ""));
            }

        }
        public override string ToString()
        {
            string str = "";
            str += Name.ToString();
            str += Description.ToString();
            str += Note0.ToString();
            str += Lore.ToString();
            for (int i = 0; i < abilitySpecials.Count(); i++)
            {
                str += abilitySpecials.ElementAt(i).ToString();
            }
            return str;
        }

    }

}
