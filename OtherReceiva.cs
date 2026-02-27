
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace BD.Standard.KangLian.SettlementBill26
{
    [Description("动态表单插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class OtherReceiva : AbstractDynamicFormPlugIn
    {
        //动态表单加载事件，获取到id后在存储查询其他应收单得到对应数据，构建动态表单并赋值。
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                string FSALEDEPTID = this.View.OpenParameter.GetCustomParameter("FSALEDEPTID").ToString();//销售部门已修改为客户
                string FMATERIAL = this.View.OpenParameter.GetCustomParameter("FMATERIAL").ToString();//物料
                this.View.Model.SetValue("F_XBSY_Text", FSALEDEPTID);
                this.View.Model.SetValue("F_XBSY_Text1", FMATERIAL);

                string sql = string.Format("exec queryBalance '{0}','{1}'", FSALEDEPTID, FMATERIAL);

                DynamicObjectCollection dyoc = DBUtils.ExecuteDynamicObject(this.Context, sql) as DynamicObjectCollection;
                foreach (DynamicObject dy in dyoc)//结果对象  对象名称  in  被循环的对象 
                {
                    this.Model.CreateNewEntryRow("F_YDIE_Entity");//构造动态表单
                    int iRow = View.Model.GetEntryCurrentRowIndex("F_YDIE_Entity");//获取单据体行
                    this.Model.SetItemValueByID("FMATERIAL", Convert.ToInt32(dy["FMATERIAL"].ToString()), iRow);//物料
                    this.Model.SetItemValueByID("Fcost", Convert.ToInt32(dy["FCOST"].ToString()), iRow);//费用项目
                    this.Model.SetValue("FQty", Convert.ToDecimal(dy["FQty"].ToString()), iRow);//数量
                    this.Model.SetValue("fsrcbillno", dy["fsrcbillno"].ToString(), iRow);//其他应付单
                    this.Model.SetValue("fsrcid", dy["fsrcid"].ToString(), iRow);//其他应付单
                    this.Model.SetValue("fsrcentryid", dy["fsrcentryid"].ToString(), iRow);//其他应付单
                }
            }
            catch (Exception ex)
            {
                this.View.ShowErrMessage(ex.Message);
            }
        }

        //动态表单按钮事件，获取当前选中行的数据进行封装传回结算单
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
            try
            {

                if (e.BarItemKey.Equals("XBSY_tbButton"))
                {
                    //int row = this.Model.GetEntryCurrentRowIndex("F_YDIE_Entity");//获取选中的索引下标
                    //int[] rowsId = this.View.GetControl<EntryGrid>("F_YDIE_Entity").GetSelectedRows();//获取多条选中的索引数据
                    //获取动态表单明细数据 DynamicObjectCollection
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    Entity entity = this.View.BillBusinessInfo.GetEntity("F_YDIE_Entity");
                    DynamicObjectCollection dynamics = this.View.Model.GetEntityDataObject(entity);
                    //获取复选框标识为true if(复选框标识的属性值=true){ 以下内容}
                    StringBuilder str = new StringBuilder();
                    foreach (DynamicObject current in dynamics)
                    {
                        if (Convert.ToBoolean(current["FCheckBox"].ToString()))
                        {
                            int row = Convert.ToInt32(current["seq"]) - 1;
                            //获取当前标识下索引的值
                            DynamicObject FMATERIAL = this.View.Model.GetValue("FMATERIAL", row) as DynamicObject;//物料
                            DynamicObject Fcost = this.View.Model.GetValue("Fcost", row) as DynamicObject;//费用项目
                            Decimal FQty = Convert.ToDecimal(this.View.Model.GetValue("FQty", row));//数量
                            string fsrcbillno = this.View.Model.GetValue("fsrcbillno", row).ToString();//其他应付单
                            string fsrcid = this.View.Model.GetValue("fsrcid", row).ToString();//其他应付单
                            string fsrcentryid = this.View.Model.GetValue("fsrcentryid", row).ToString();//其他应付明细id
                            Dictionary<string, object> dymat = new Dictionary<string, object>();
                            dymat.Add("FMATERIAL", FMATERIAL);
                            dymat.Add("Fcost", Fcost);
                            dymat.Add("FQty", FQty);
                            dymat.Add("fsrcbillno", fsrcbillno);
                            dymat.Add("fsrcid", fsrcid);
                            dymat.Add("fsrcentryid", fsrcentryid);
                            list.Add(dymat);
                        }
                    }
                    if (list != null)
                    {
                        //最后返回父类窗口
                        this.View.ReturnToParentWindow(list);
                        this.View.Close();
                        this.View.ParentFormView.Refresh();
                        this.View.SendAynDynamicFormAction(this.View.ParentFormView);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.log(ex.Message);
                
            }
            
        }
    }
}

