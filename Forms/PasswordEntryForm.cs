using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudioSteganography
{
    public partial class PasswordEntryForm : Form
    {
        public PasswordEntryForm()
        {
            InitializeComponent();
        }

        public string Password1 { get { return Password1Box.Text; } }
        public string Password2 { get { return Password2Box.Text; } }

        private void OkButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
