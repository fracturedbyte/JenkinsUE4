using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UE4BuildHelper.UI
{
    public partial class InputBoxForm : Form
    {
        private string CachedText = null;

        public InputBoxForm()
        {
            InitializeComponent();
        }

        public void Init(string Message)
        {
            MessageL.Text = Message;

            InputTextTB.Select();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            CachedText = InputTextTB.Text;

            if (!string.IsNullOrEmpty(CachedText) &&
                !string.IsNullOrWhiteSpace(CachedText) &&
                !CachedText.Contains(" "))
            {
                Close();
            }
        }

        public string GetInput()
        {
            return CachedText;
        }
    }
}
