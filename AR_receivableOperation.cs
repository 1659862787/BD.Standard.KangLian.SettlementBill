using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.Standard.KangLian.SettlementBill
{
    [Description("应收单删除操作插件")]
    [HotUpdate]
    public class AR_receivableOperation : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            List<Field> fieldList = base.BusinessInfo.GetFieldList();
            foreach (Field field in fieldList)
            {
                e.FieldKeys.Add(field.Key);
            }
        }

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            string operation = base.FormOperation.Operation;
            try
            {
                foreach (DynamicObject dynamicObject in e.DataEntitys)
                {
                    string id = dynamicObject[0].ToString();
                    
                    string sql = $"/*dialect*/select a.* from  Sljk_t_Cust_Entry100002 a inner join t_AR_receivable b on a.FID=b.FID where FBILLTYPEID='652de056a84984' and F_SLJK_BASE<>0 and F_SLJK_AMOUNTED1>0 and b.fid={id}";
                    DynamicObjectCollection dy = DBUtils.ExecuteDynamicObject(base.Context, sql);
                    if (dy.Count>0)
                    {
                        throw new Exception("费用明细中已经存在关联的数量可用余额单，请删除费用明细并保存单据后，再执行删除操作！");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
