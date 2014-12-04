using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D2ModKit
{
    public partial class ParticleRenameForm : Form
    {
        private string[] paths;

        public string[] Paths
        {
            get { return paths; }
            set { paths = value; }
        }

        private Button submit;

        public Button Submit
        {
            get { return button1; }
            set { submit = value; }
        }

        private TextBox pTextBox;

        public TextBox PTextBox
        {
            get { return textBox1; }
            set { pTextBox = value; }
        }

        public ParticleRenameForm(string[] paths)
        {
            InitializeComponent();
            Paths = paths;
            textBox1.TextChanged += particleRenameTextBox_TextChanged;
            Submit = button1;
            foreach (string p in paths)
            {
                string name = p.Substring(p.LastIndexOf('\\')+1);
                //tableLayoutPanel1.
            }

        }

        void particleRenameTextBox_TextChanged(object sender, EventArgs e)
        {
           
        }
        

    }
}
