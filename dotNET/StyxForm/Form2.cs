using Rigsarkiv.Asta.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rigsarkiv.StyxForm
{
    public partial class Form2 : Form
    {
        /// <summary>
        /// Constructors
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        public Form2(string srcPath, string destPath, LogManager logManager)
        {
            InitializeComponent();
        }
    }
}
