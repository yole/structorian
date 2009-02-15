using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Structorian.Engine;

namespace Structorian
{
    public partial class FindStructureDialog : Form
    {
        public FindStructureDialog(StructFile structFile)
        {
            InitializeComponent();
            cmbStructures.Items.AddRange(structFile.Structs.ToArray());
        }

        public StructDef SelectedDef
        {
            get { return (StructDef) cmbStructures.SelectedItem; }
        }

        public string Expression
        {
            get { return edtExpression.Text; }
        }
    }
}
