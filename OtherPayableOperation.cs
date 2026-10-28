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
    [Description("其他应付单审核与反审核操作插件")]
    [HotUpdate]
    public class OtherPayableOperation : AbstractOperationServicePlugIn
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
                    string text = dynamicObject[0].ToString();
                    string F_AP_OtherPayable = dynamicObject["F_AP_OtherPayable"].ToString();
                    if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(F_AP_OtherPayable))
                    {
                        DynamicObjectCollection dynamicObjectCollection = dynamicObject["FEntity"] as DynamicObjectCollection;
                        if (operation.Equals("Audit"))
                        {
                            foreach (DynamicObject dy in dynamicObjectCollection)
                            {
                                DynamicObject dynamicObject3 = (DynamicObject)dy["FYFANMATERIALIDD"];
                                string FYFANMATERIALIDD = dynamicObject3[0].ToString();
                                decimal F_PUB_Amount = Convert.ToDecimal(dy["F_PUB_Amount"]);
                                string sql = $"/*dialect*/update a set F_PUB_Amount=F_PUB_Amount+({F_PUB_Amount}) from T_AP_OtherPayableentry a inner join T_AP_OtherPayable b on a.fid=b.fid where b.FBillNo='{F_AP_OtherPayable}' and FYFANMATERIALIDD={FYFANMATERIALIDD} and F_PUB_Amount+({F_PUB_Amount})>=0";
                                int result = DBUtils.Execute(base.Context, sql);
                                dy["F_PUB_Amount"] = 0;
                                if (result==0)
                                {
                                    throw new Exception("源单【" + F_AP_OtherPayable + "】剩余可用数量不足或者无法满足匹配条件，请检查！");
                                }
                            }
                            BusinessDataServiceHelper.Save(base.Context, dynamicObject);
                        }
                        else if (operation.Equals("UnAudit"))
                        {
                            foreach (DynamicObject dy in dynamicObjectCollection)
                            {
                                DynamicObject dynamicObject5 = (DynamicObject)dy["FYFANMATERIALIDD"];
                                string FYFANMATERIALIDD = dynamicObject5[0].ToString();
                                decimal F_SHKL_Amount = Convert.ToDecimal(dy["F_SHKL_Amount"]);
                                string sql =$"/*dialect*/update a set F_PUB_Amount=F_PUB_Amount-({F_SHKL_Amount}) from T_AP_OtherPayableentry as a inner join T_AP_OtherPayable b on a.fid=b.fid where b.FBillNo='{F_AP_OtherPayable}' and FYFANMATERIALIDD={FYFANMATERIALIDD} and F_PUB_Amount-({F_SHKL_Amount})<=F_SHKL_Amount";
                                int result = DBUtils.Execute(base.Context, sql);
                                dy["F_PUB_Amount"] = F_SHKL_Amount;
                                if (result==0)
                                {
                                    throw new Exception("源单【" + F_AP_OtherPayable + "】剩余可用数量与当前单据可用数之和超过源单总数量或者无法满足匹配条件，请检查！");
                                }
                            }
                            BusinessDataServiceHelper.Save(base.Context, dynamicObject);
                        }
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
