using Autodesk.Revit.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Color = System.Drawing.Color;

namespace DelegateTest
{
    

    public partial class Main : System.Windows.Forms.Form
    {
        private RequestHandler m_Handler;
        private ExternalEvent m_ExEvent;

        public static DataTable m_dt;
        public static DataGridView m_dataGridView1;
        
        

        public Main(ExternalEvent exEvent, RequestHandler handler)
        {
            InitializeComponent();
            m_Handler = handler;
            m_ExEvent = exEvent;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            m_ExEvent.Dispose();
            m_ExEvent = null;
            m_Handler = null;

            base.OnFormClosed(e);
        }

        private void EnableCommands(bool status)
        {
            foreach (System.Windows.Forms.Control ctrl in this.Controls)
            {
                ctrl.Enabled = status;
            }
        }

        private void MakeRequest(RequestId request)
        {
            m_Handler.Request.Make(request);
            m_ExEvent.Raise();
            Main main = new Main();
            main.DozeOff();
        }

        private void DozeOff()
        {
            EnableCommands(false);
        }
        public void WakeUp()
        {
            EnableCommands(true);
        }

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            GetWallData(Command.m_doc,dataGridView1);
            ColorCheck(Command.m_doc, dataGridView1);
        }

        public static void GetWallData(Document m_doc, DataGridView dgv) //datagridview1 을 변수로 받음
        {

            FilteredElementCollector col = new FilteredElementCollector(m_doc).OfCategory(BuiltInCategory.OST_Walls)
                .OfClass(typeof(Wall));

            List<Wall> walls = new List<Wall>();

            foreach (Wall item in col)
            {
                walls.Add(item);
            }

            m_dt = new DataTable();

            m_dt.Columns.Add("ElementId");
            m_dt.Columns.Add("층");
            m_dt.Columns.Add("패밀리명");
            m_dt.Columns.Add("타입명");
            m_dt.Columns.Add(new DataColumn("구조체크", typeof(bool)));

            foreach (Wall wall in walls)
            {
                Element ele_wall = Command.m_doc.GetElement(wall.Id);
                string elementid = ele_wall.Id.ToString();
                string wall_name = ele_wall.Name;
                string wall_level = Command.m_doc.GetElement(ele_wall.LevelId).Name.ToString();
                string wall_type = ele_wall.GetType().Name.ToString();
                Parameter structural = ele_wall.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT);
                int structural_int = structural.AsInteger();
                bool st_state = false;
                if (structural_int == 1)
                {
                    st_state = true;
                }

                DataRow dr = m_dt.NewRow();
                dr[0] = elementid;
                dr[1] = wall_level;
                dr[2] = wall_name;
                dr[3] = wall_type;
                dr[4] = st_state;

                m_dt.Rows.Add(dr);
            }
            dgv.DataSource = m_dt;
            m_dataGridView1 = dgv;
        }



        public static void ColorCheck(Document m_doc, DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                string familyName = row.Cells[2].Value.ToString();
                bool st_state = Convert.ToBoolean(row.Cells[4].Value);

                if (familyName.Contains("S-") && st_state == false)
                {
                    row.DefaultCellStyle.BackColor = Color.LightPink;
                }

                else if (familyName.Contains("A-") && st_state == true)
                {
                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                }

                else
                {
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                }

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
           

            MakeRequest(RequestId.Change);

            RequestHandler.DataSendEvent += Main.GetWallData;
            RequestHandler.DataSendEvent += Main.ColorCheck;
           

            //GetWallData(Command.m_doc, dataGridView1);
            //ColorCheck(dataGridView1);
        }



        /*
        private void btnExit_Click()
        {
            MakeRequest(RequestId.None);
        }
        */
    }
}
