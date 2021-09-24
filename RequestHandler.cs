using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
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
using Color = System.Drawing.Color;

namespace DelegateTest
{
    public class RequestHandler : IExternalEventHandler
    {
        public delegate void MD_GetWallData(Document m_doc, DataGridView dgv);
        public static event MD_GetWallData DataSendEvent;
                
        public static UIDocument m_uidoc = null;
        public static UIApplication m_uiapp = null;
        public static Document m_doc = null;

        private Request m_request = new Request();
        public Request Request
        {
            get { return m_request; }
        }
        public String GetName()
        {
            return "testing";
        }

        public void Execute(UIApplication uiapp)
        {
            try
            {
                switch (Request.Take())
                {
                    case RequestId.None:
                        {
                            CreateBOJ(uiapp);
                            break;
                        }
                    case RequestId.Change:
                        {
                            Change_(uiapp);
                            break;
                        }
                }
            }
            finally
            {
                App.thisApp.WakeFormUp();
            }
            return;
        }

        private void CreateBOJ(UIApplication uiapp)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            m_uidoc = uidoc;
        }

        private void Change_(UIApplication uiapp)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            m_uidoc = uidoc;

            DataTable dt = Main.m_dt;

            foreach (DataRow item in dt.Rows)
            {
                string elementId = item.ItemArray[0].ToString();
                string familyName = item.ItemArray[2].ToString();
                bool st_state = Convert.ToBoolean(item.ItemArray[4]);

                if (familyName.Contains("S-"))
                {
                    if(st_state == false)
                    {
                        int idInt = Convert.ToInt32(elementId);
                        ElementId eId = new ElementId(idInt);
                        Element e = Command.m_doc.GetElement(eId);

                        if(e is Wall)
                        {
                            using (Transaction tx = new Transaction(Command.m_doc, "ee"))
                            {
                                tx.Start();

                                e.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT).Set(1);

                            tx.Commit();
                            }
                        }
                        
                    }
                }

                else if (familyName.Contains("A-"))
                {
                    if (st_state == true)
                    {
                        int idInt = Convert.ToInt32(elementId);
                        ElementId eId = new ElementId(idInt);
                        Element e = Command.m_doc.GetElement(eId);

                        if (e is Wall)
                        {
                            using (Transaction tx = new Transaction(Command.m_doc, "ee"))
                            {
                                tx.Start();

                                e.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT).Set(0);

                                tx.Commit();
                            }
                        }

                    }
                }
            }    
            //delegate 
            DataSendEvent(Command.m_doc, Main.m_dataGridView1);

            MessageBox.Show("업데이트완료");
            
        }
    }
}
