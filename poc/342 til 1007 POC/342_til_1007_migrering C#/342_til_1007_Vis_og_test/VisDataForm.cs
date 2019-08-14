using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace _342_til_1007_vis_test_konverter
{
    public partial class VisDataForm : Form
    {

        //DialogResult res = new DialogResult();

        int pos = 0;
        int postNummer = 0;
        BinaryReader br = null;

        List<MyRow> _rowList = null; 
        string _postType = string.Empty;
        string _tabelFilenavn = string.Empty;
        int totalBredde = 0;
        long totalRowcount = 0;

        public VisDataForm()
        {
            InitializeComponent();
        }


        public VisDataForm(string tabelFilenavn, List<MyRow> rowList, string postType)
        {
            InitializeComponent();

            //Forbered dataGridView
            dataGridView1.RowCount = rowList.Count;
            int i = 0;
            foreach (MyRow row in rowList)
            {
                dataGridView1.Rows[i].HeaderCell.Value = (i+1).ToString();//nummerering

                dataGridView1.Rows[i].Cells[0].Value = row.feltnavn;
                dataGridView1.Rows[i].Cells[0].ToolTipText = row.feltBeskrivelse;
                dataGridView1.Rows[i].Cells[1].Value = row.datatype;
                dataGridView1.Rows[i].Cells[2].Value = row.data342;
                dataGridView1.Rows[i].Cells[3].Value = row.data1007;
             
                i++;

                totalBredde = totalBredde + Convert.ToInt32(row.bredde);
            }

            
            //Herefter kan filen læses post for post
            if (File.Exists(tabelFilenavn) == false) MessageBox.Show(tabelFilenavn + " findes ikke !!!");
          
            br = new BinaryReader(File.Open(tabelFilenavn, FileMode.Open, FileAccess.Read));
            
            if (postType == "fast")
            {
                totalRowcount = (br.BaseStream.Length / totalBredde);
               // MessageBox.Show(br.BaseStream.Length.ToString() + "  " + totalBredde.ToString());
            }
            
            
            _tabelFilenavn = tabelFilenavn;
            _postType = postType;
            _rowList = rowList;
            //Første rekord
            VisTabelPost(_rowList, _postType);
           // MessageBox.Show(tabelFilenavn);
        }


        private DataTypeKonverteringsResultat DataTypeTest(string dataString342, string datatype342)
        {
            Int64 tempInt = -1;//TODO: DataTypeTest, er det muligt at initialisere som null?
            float tempFloat = -1;//TODO: DataTypeTest, er det muligt at initialisere som null?

            DataTypeKonverteringsResultat result = new DataTypeKonverteringsResultat();

            if (dataString342 == "") return (result);

            //Benyttes til at returnere konverteret data 
            //string dataString1007 = string.Empty;

            switch (datatype342)
            {
                case "string":
                    result.data1007 = System.Security.SecurityElement.Escape(dataString342);//XML beskyttede tegn skal konverteres fx & < > ;
                    break;

                case "num":
                    if (dataString342.Trim() == "")
                    {
                        result.data1007 = "";//NULL
                    }
                  else
                    if (Int64.TryParse(dataString342, out tempInt) == false)
                    {
                        result.fejlmeddelelse = "NUM fejl: '" + dataString342 + "'" + "\r\n";
                        result.data1007 = dataString342.Trim();//data returneres på trods af fejl
                    }
                 else
                    {
                        result.data1007 = dataString342.Trim();//Ingen konvertering (foranstillede nuller kan have betydning fx CPR og journalnumre
                    }
                    break;

                case "real":
                    if (float.TryParse(dataString342, out tempFloat) == false)
                    {
                        result.fejlmeddelelse = "REAL fejl: '" + dataString342 + "'" + "\r\n";
                        result.data1007 = dataString342.Trim();//Ingen konvertering (hvad når der er fejl?)
                    }
                    else
                    {
                        //TODO: DataTypeTest, vil kunne strippe nuller ved at benytte dataString1007 = tempFloat.ToString();
                        //MessageBox.Show(dataString342 + " --> " + tempFloat.ToString());
                        result.data1007 = tempFloat.ToString();
                    }
                    break;

                case "exp":
                    //TODO: DataTypeTest, mangler test og konvertering af exp
                    MessageBox.Show("EXP datatype konvertering ikke implementeret: '" + dataString342 + "'");
                    break;

                case "date":
                    if (dataString342.Trim() == "")
                    {
                        result.data1007 = "";
                    }
                  else
                    if (dataString342.Trim().Length != 8)
                    {
                        result.fejlmeddelelse = "Fejl i dato bredde: " + dataString342 + "\r\n";
                        result.data1007 = dataString342;//Ingen konvertering (hvad når der er fejl?)
                    }
                  else
                    {
                        DateTime datoTest;
                        if (DateTime.TryParseExact(dataString342.Trim(), "yyyyMMdd", CultureInfo.GetCultureInfo("da-DK"), DateTimeStyles.None, out datoTest) == false)
                        {
                            result.fejlmeddelelse = "Fejl i dato: " + dataString342 + "\r\n";
                            result.data1007 = dataString342;//Ingen konvertering (hvad når der er fejl?)
                        }
                        else
                        {
                            result.data1007 = datoTest.ToString("yyyy-MM-dd", CultureInfo.GetCultureInfo("da-DK"));//bindestreger tilføjes
                        }
                    }

                    break;

                case "time":
                   if (dataString342.Trim() == "")
                    {
                        result.data1007 = "";
                    }
                    else
                        if (dataString342.Trim().Length < 6)
                        {
                            result.fejlmeddelelse = "Fejl i time: " + dataString342.Trim() + "\r\n";
                            result.data1007 = dataString342;//Ingen konvertering
                        }
                        else
                        {
                            DateTime datoTest;

                            if (DateTime.TryParseExact(dataString342.Trim(), "hhmmss,FFFFFFF", CultureInfo.GetCultureInfo("da-DK"), DateTimeStyles.None, out datoTest) == false)
                            {
                                result.fejlmeddelelse = "Fejl i time: " + dataString342 + "\r\n";
                                result.data1007 = dataString342.Trim();//Ingen konvertering
                            }
                        }
                    break;

                case "timestamp":
                    if (dataString342.Trim() == "")
                    {
                        result.data1007 = "";
                    }
                    else
                        if (dataString342.Trim().Length < 17)
                        {
                            result.fejlmeddelelse = "Fejl i timestamp: " + dataString342.Trim() + "\r\n";
                            result.data1007 = dataString342;//Ingen konvertering
                        }
                        else
                        {
                            DateTime datoTest;

                            if (DateTime.TryParseExact(dataString342.Trim(), "yyyyMMddThhmmss,FFFFFFF", CultureInfo.GetCultureInfo("da-DK"), DateTimeStyles.None, out datoTest) == false)
                            {
                                result.fejlmeddelelse = "Fejl i timestamp: " + dataString342 + "\r\n";
                                result.data1007 = dataString342.Trim();//Ingen konvertering
                            }
                        }

                    break;
                default:
                    MessageBox.Show("Datatype: '" + datatype342 + "' ikke understøttet!!!");
                    result.fejlmeddelelse = "Datatype: '" + datatype342 + "' ikke understøttet!!!" + "\r\n";
                    result.data1007 = dataString342;
                    break;

            }

            //TODO: DataTypeTest, mangler test og konvertering af float ???

            return (result);
        }




        private void VisTabelPost(List<MyRow> rowList, string postType)
        {
            byte[] byteBuffer = new byte[1024];
            int length = (int)br.BaseStream.Length;

            int fejlCount = 0;
            textBox1.Text = "";
            textBox1.BackColor = SystemColors.Window;


            if (length > 0)
            {
                postNummer++;
                this.Text = "Tabel: " + Path.GetFileName(_tabelFilenavn) + " Postnr: " + postNummer.ToString() + " udaf " + totalRowcount.ToString();
            }
            else
            {
                this.Text = "Tabel: " + Path.GetFileName(_tabelFilenavn) + " indeholder ingen poster (0 fil)";
                return;
            }

            if (postType.ToLower() == "fast")
            {
                br.BaseStream.Position = pos;
                while (pos < length)
                {
                    for (int i = 0; i < rowList.Count; i++)
                    {
                        int bredde = -1;
                        int.TryParse(rowList[i].bredde, out bredde);
                        //Selve læsningen ind i buffer
                        byteBuffer = br.ReadBytes(bredde);
                        pos = pos + bredde;

                        string dataString342 = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(byteBuffer, 0, bredde);

                        //*** Her foretages test og konvertering af data ***
                        DataTypeKonverteringsResultat  result = DataTypeTest(dataString342, rowList[i].datatype.Trim());
                      
                        dataGridView1.Rows[i].Cells[2].Value = dataString342;
                        dataGridView1.Rows[i].Cells[3].Value = result.data1007;

                        if (result.fejlmeddelelse != "")
                        {
                            //MessageBox.Show(result.fejlmeddelelse);
                            dataGridView1.Rows[i].Cells[2].Style.BackColor = Color.IndianRed;
                            dataGridView1.Rows[i].Cells[2].ToolTipText = result.fejlmeddelelse;

                            dataGridView1.Rows[i].Cells[3].Style.BackColor = Color.IndianRed;

                            fejlCount++;
                            textBox1.Text = "Indeholder " + fejlCount.ToString() + " fejl"; 
                            textBox1.BackColor = Color.IndianRed;
                            if (checkBoxStopVedFejl.Checked)
                            {
                                button2.BackColor = SystemColors.Control;
                                timer1.Enabled = false;
                            }
                        }
                        else
                        {
                            dataGridView1.Rows[i].Cells[2].Style.BackColor = Color.White;
                            dataGridView1.Rows[i].Cells[3].Style.BackColor = Color.White;
                            dataGridView1.Rows[i].Cells[3].ToolTipText = "";
                        }
                               
                    }//for (int i = 0; i < tabelMetadata.feltDefList.Count; i++)

                    break;//Der skal kun vises 1 post af gangen
                }//while (pos < length)
            }
            else
            {
                //Her angiver bredde hvor mange bytes som skal læses fra stream for at vide hvor mange bytes det pågældende feltindhold er  
                MessageBox.Show("Håndtering af variabel feltbredde ikke implementeret endnu !!!");
                //TODO: VisTabelPost, Håndtering af variabel feltbredde ikke implementeret endnu
            }

            button1.Enabled = pos < length;
            button2.Enabled = pos < length;
            if (pos == length)
            {
                timer1.Enabled = false;
                button2.BackColor = SystemColors.Control;
            }

        }



        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            VisTabelPost(_rowList, _postType);
        }

        private void VisDataForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
            Application.DoEvents();

            br.Close();//inputfil lukkes
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            VisTabelPost(_rowList, _postType);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled; 
            if (timer1.Enabled == false)
            {
                button2.BackColor = SystemColors.Control;
            }
            else
            {
                button2.BackColor = Color.LightGreen;
            }
        }

        private void progressBar1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if ((e.X > 0) && (e.X < 101))
                {
                    progressBar1.Value = e.X;
                    timer1.Interval = 1000 - (e.X * 10);
                }
            }
        }

        private void progressBar1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if ((e.X > 0) && (e.X < 100))
                {
                    progressBar1.Value = e.X;
                    timer1.Interval = 1000 - (e.X * 10);
                }
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            //Gå til start
            br.BaseStream.Position = 0;
            pos = 0;
            postNummer = 0;
            VisTabelPost(_rowList, _postType);
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (_postType == "fast")
            {
                if (pos - (totalBredde * 2) > -1)
                {
                    br.BaseStream.Position = br.BaseStream.Position - (totalBredde * 2);
                    pos = pos - (totalBredde * 2);
                    postNummer = postNummer -2;
                    VisTabelPost(_rowList, _postType);
                }

            }
        }


              

    }
}
