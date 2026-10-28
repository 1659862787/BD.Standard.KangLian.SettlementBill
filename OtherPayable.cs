using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.Standard.KangLian.SettlementBill
{
    [Description("其他应付单表单插件")]
    [HotUpdate]
    public class OtherPayable : AbstractDynamicFormPlugIn
    {
        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            base.BeforeF7Select(e);
            if (StringUtils.EqualsIgnoreCase(e.FieldKey, "F_AP_OtherPayable"))
            {
            
                DynamicObject FYFANMATERIALIDD = (DynamicObject)this.View.Model.GetValue("FYFANMATERIALIDD", 0);
                DynamicObject FCONTACTUNIT = (DynamicObject)this.View.Model.GetValue("FCONTACTUNIT", 0);
                decimal F_PUB_Amount = (decimal)this.View.Model.GetValue("F_PUB_Amount", 0);
                decimal F_SHKL_Amount = (decimal)this.View.Model.GetValue("F_SHKL_Amount", 0);
                if (FYFANMATERIALIDD == null || FCONTACTUNIT == null)
                {
                    e.Cancel = true;
                    this.View.ShowMessage("请先选择客户和物料");
                    return;
                }
                if (F_SHKL_Amount >= 0m)
                {
                    e.Cancel = true;
                    this.View.ShowMessage("当前单据不是负数数量可用余额单，不需要选择对应数量可用余额单");
                    return;
                }
                decimal num2 = Math.Abs(F_PUB_Amount);
                ListShowParameter para = new ListShowParameter
                {
                    Context = this.Context,
                    FormId = "AP_OtherPayable",
                    IsShowApproved = true,
                    ParentPageId = this.View.PageId,
                    IsLookUp = true,
                    MultiSelect = false,
                    PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad"
                };
                ((DynamicFormShowParameter)para).OpenStyle.ShowType = (ShowType)6;
                para.ListFilterParameter.Filter = string.Format($"FDocumentStatus='C' and FYFANMATERIALIDD={FYFANMATERIALIDD[0]} and FCONTACTUNIT={FCONTACTUNIT[0]} and F_PUB_Amount>={num2}");
                this.View.ShowForm(para, delegate (FormResult result)
                {
                    object returnData = result.ReturnData;
                    ListSelectedRowCollection val4 = (ListSelectedRowCollection)((returnData is ListSelectedRowCollection) ? returnData : null);
                    if (val4 != null)
                    {
                        this.View.Model.SetValue("F_AP_OtherPayable", (val4)[0].DataRow["Fbillno"]);
                    }
                });
            }
        }
    }
}
