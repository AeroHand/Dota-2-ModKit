using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleForker
{
    public class Addon
    {
        private string name;
        private string contentPath;
        private string gamePath;
        private string npcPath;
        private string copyPath;
        private string relativeParticlePath;
        private List<string> particlePaths;

        public Addon(string _gamePath)
        {
            gamePath = _gamePath;
            name = gamePath.Substring(gamePath.LastIndexOf('\\') + 1);
            
            /*string[] path = _gamePath.Split('\\');
            string str = "";
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] != "dota_ugc")
                {
                    str += path[i] + "\\";
                }
                else
                {
                    str += "dota_ugc\\";
                    break;
                }
            }
            str += "content\\" + name;

            contentPath = str;*/
        }

        public Addon(string _contentPath, string _gamePath)
        {
            contentPath = _contentPath;
            gamePath = _gamePath;
            name = _contentPath.Substring(_contentPath.LastIndexOf('\\') + 1);
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

    }
}
