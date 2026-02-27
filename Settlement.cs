using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;

using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Metadata.EntityElement;
using System.Text;

namespace BD.Standard.KangLian.SettlementBill26
{
    [Description("结算单插件，点击按钮打开动态表单插件")]
    [Kingdee.BOS.Util.HotUpdate]

    public class Settlement: AbstractBillPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
             base.AfterBindData(e);
             this.View.Model.SetValue("Ftext","");

        }
        public override void EntryBarItemClick(BarItemClickEventArgs e)
        {
            base.EntryBarItemClick(e);
            try
            {

              
                string billtype = ((DynamicObject)this.View.Model.GetValue("FBillTypeID"))["number"].ToString();
                if (e.BarItemKey.Equals("ANtbButton") && billtype.Equals("借条结算单"))
                {
                    if (((DynamicObject)this.Model.DataObject)["id"].ToString().Equals("0"))
                    {
                        this.View.InvokeFormOperation(FormOperationEnum.Save);
                    }
                    //打开单据查看实例
                    DynamicFormShowParameter para = new DynamicFormShowParameter();
                    //打开样式,打开动态表单
                    para.OpenStyle.ShowType = ShowType.NonModal;
                    para.SyncCallBackAction = true;
                    //数量可用余额单
                    para.FormId = "SHKL_AvailableBalance";
                    Entity entity = this.View.BillBusinessInfo.GetEntity("FEntityDetail");
                    DynamicObjectCollection dy = this.View.Model.GetEntityDataObject(entity);
                    StringBuilder masterid = new StringBuilder();
                    foreach (var itemjia in dy)
                    {
                        masterid.Append(itemjia["MATERIALID_Id"].ToString() + ",");
                    }
                    masterid.Remove(masterid.Length - 1, 1);
                    if ((DynamicObject)this.View.Model.GetValue("FCUSTOMERID") == null)
                    {
                        this.View.ShowErrMessage("客户为空");
                        return;
                    }
                    string deptid = ((DynamicObject)this.View.Model.GetValue("FCUSTOMERID"))["id"].ToString();
                    //if ((DynamicObject)this.View.Model.GetValue("FSALEDEPTID") == null)
                    //{
                    //    this.View.ShowErrMessage("部门为空");
                    //    return;
                    //}
                    //string deptid = ((DynamicObject)this.View.Model.GetValue("FSALEDEPTID"))["id"].ToString();
                    string masterids = masterid.ToString();
                    para.CustomParams.Add("FSALEDEPTID", deptid);//销售部门
                    para.CustomParams.Add("FMATERIAL", masterids);//费用项目 e.Row
                    para.ParentPageId = this.View.ParentFormView.PageId;
                    this.View.ShowForm(para, delegate (FormResult result)
                    {
                        //this.View.ShowForm(para, new Action<FormResult>((result) => { 
                        //动态表单返回数据
                        if (result.ReturnData != null)
                        {
                            List<Dictionary<string, object>> list = result.ReturnData as List<Dictionary<string, object>>;
                            DynamicObject obj = this.Model.DataObject;
                            string id = obj["id"].ToString();

                            FormMetadata ExpMeta = MetaDataServiceHelper.Load(this.Context, "AR_receivable", true) as FormMetadata;
                            DynamicObject Expobj = BusinessDataServiceHelper.LoadSingle(this.Context, id, ExpMeta.BusinessInfo.GetDynamicObjectType());
                            if (list != null && list.Count > 0)
                            {
                                foreach (var returndata in list)
                                {
                                    //获取动态表单回填金额
                                    string materialid = ((DynamicObject)returndata["FMATERIAL"])["id"].ToString();
                                    Decimal qty = Convert.ToDecimal(returndata["FQty"]);
                                    Decimal qty1 = Convert.ToDecimal(returndata["FQty"]);
                                    bool flag = false;
                                    Decimal qty2 = 0; //数量合计
                                    Decimal qty3 = 0; //可用数量
                                    int i = 0;
                                    // 查询费用明细数据，不存在明细，跳过。
                                    DynamicObjectCollection entry = Expobj["Sljk_Cust_Entry100002"] as DynamicObjectCollection;//费用明细实体
                                    if (entry.Count > 0)
                                    {
                                        i = entry.Count;
                                        foreach (var item in entry)
                                        {
                                            //判断动态表单返回得源单明细id是否已经存在
                                            //将明细中的所有匹配的数量汇总
                                            if (returndata["fsrcentryid"].ToString().Equals(item["fsrcentryid"]))
                                            {
                                                qty2 += Convert.ToDecimal(item["F_Sljk_Amounted1"]);
                                                qty3 = Convert.ToDecimal(item["F_Sljk_Amounted"]);
                                            }
                                        }
                                        //用于已存在数量数据，可用数量-合计数量=qty1,用于新增行的数量
                                        if (decimal.Subtract(qty3, qty2) > 0)
                                        {
                                            qty1 = decimal.Subtract(qty3, qty2);
                                        }
                                        //当明细没有匹配到明细id时，防止flag为true,内容无影响。
                                        else if (qty3 == 0 && qty2 == 0)
                                        {
                                            qty1 = qty;
                                        }
                                        //qty2=qty3    当数量合计=可用数量，熟练已全部使用，跳过当前数据
                                        else
                                        {
                                            flag = true;
                                        }
                                    }
                                    if (flag) continue;
                                    DynamicObjectCollection dy1 = Expobj["AP_PAYABLEENTRY"] as DynamicObjectCollection;//明细实体
                                    if (dy1 != null || dy1.Count > 0)
                                    {
                                        foreach (var item in dy1)
                                        {
                                            //明细对应物料行id计算已结算、未结算金额
                                            if (item["MaterialId_Id"].ToString().Equals(materialid))
                                            {
                                                decimal FYJSQTY = 0;
                                                item["FWJSqty"] = Convert.ToDecimal(item["PriceQty"]) - Convert.ToDecimal(item["FYJSQTY"]);
                                                if (Convert.ToInt32(item["FWJSqty"]) == 0) break;
                                                Decimal FWJSqty = Convert.ToDecimal(item["FWJSqty"]);
                                                if (decimal.Subtract(FWJSqty, qty1) >= 0)
                                                {
                                                    FYJSQTY = Convert.ToDecimal(item["FYJSQTY"]) + qty1;
                                                    FWJSqty = Convert.ToDecimal(item["FWJSqty"]) - qty1;
                                                    item["FYJSQTY"] = FYJSQTY;
                                                    item["FWJSqty"] = FWJSqty;
                                                }
                                                else
                                                {
                                                    qty1 = Convert.ToDecimal(item["FWJSqty"]);
                                                    FYJSQTY = Convert.ToDecimal(item["FYJSQTY"]) + qty1;
                                                    FWJSqty = Convert.ToDecimal(item["FWJSqty"]) - qty1;
                                                    item["FYJSQTY"] = FYJSQTY;
                                                    item["FWJSqty"] = FWJSqty;
                                                }
                                                //动态表单新增明细
                                                DynamicObject accept = entry.DynamicCollectionItemPropertyType.CreateInstance() as DynamicObject;
                                                accept["seq"] = ++i;
                                                accept["FMasterBase"] = returndata["FMATERIAL"];
                                                accept["FMasterBase_Id"] = materialid;
                                                accept["F_SLJK_BASE"] = returndata["Fcost"];
                                                accept["F_SLJK_BASE_Id"] = ((DynamicObject)returndata["Fcost"])["id"].ToString();
                                                accept["F_Sljk_Amounted1"] = qty1;
                                                accept["F_Sljk_Amounted"] = qty;
                                                accept["fsrcbillno"] = returndata["fsrcbillno"];
                                                accept["fsrcid"] = returndata["fsrcid"];
                                                accept["fsrcentryid"] = returndata["fsrcentryid"];
                                                entry.Add(accept);

                                                //其他应付单未核销金额反写
                                                FormMetadata ExpMeta1 = MetaDataServiceHelper.Load(this.Context, "AP_OtherPayable", true) as FormMetadata;
                                                DynamicObject Expobj1 = BusinessDataServiceHelper.LoadSingle(this.Context, returndata["fsrcid"].ToString(), ExpMeta1.BusinessInfo.GetDynamicObjectType());
                                                DynamicObjectCollection otherEntry = Expobj1["FEntity"] as DynamicObjectCollection;
                                                decimal dec = 0;
                                                foreach (DynamicObject oEntry in otherEntry)
                                                {
                                                    if (oEntry["id"].ToString().Equals(returndata["fsrcentryid"].ToString()))
                                                    {
                                                        dec = Convert.ToDecimal(oEntry["F_Sljk_Amount"]) - qty1;
                                                        oEntry["F_Sljk_Amount"] = Convert.ToDecimal(oEntry["F_Sljk_Amount"]) - qty1;
                                                        break;
                                                    }
                                                }
                                                BusinessDataServiceHelper.Save(this.Context, Expobj1);
                                                //销售出库单已结算金额反写
                                                FormMetadata ExpMeta2 = MetaDataServiceHelper.Load(this.Context, "SAL_OUTSTOCK", true) as FormMetadata;
                                                DynamicObject Expobj2 = BusinessDataServiceHelper.LoadSingle(this.Context, item["Foutsrcid"].ToString(), ExpMeta2.BusinessInfo.GetDynamicObjectType());
                                                DynamicObjectCollection outEntry = Expobj2["SAL_OUTSTOCKENTRY"] as DynamicObjectCollection;
                                                foreach (DynamicObject oEntry in outEntry)
                                                {
                                                    if (oEntry["id"].ToString().Equals(item["Foutsrcentryid"].ToString()))
                                                    {
                                                        oEntry["FYJSQTY"] = FYJSQTY;
                                                        oEntry["FWJSqty"] = FWJSqty;
                                                        break;
                                                    }
                                                }
                                                BusinessDataServiceHelper.Save(this.Context, Expobj2);
                                                StringBuilder sb = new StringBuilder();
                                                sb.Append(" 单据编号：" + this.View.Model.GetValue("Fbillno").ToString() + "执行查询数量可用余额操作 ");
                                                sb.Append("\r\n其他应付单未核销反写金额：" + dec);
                                                sb.Append("\r\n销售出库单反写已结算金额：" + FYJSQTY);
                                                sb.Append("\r\n销售出库单反写未结算金额：" + FWJSqty);
                                                Log.log(sb.ToString());
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            BusinessDataServiceHelper.Save(this.Context, Expobj);
  
                        }
                        this.View.Refresh();
                   
                    });
                    
                }
                if (e.BarItemKey.Equals("Sljk_tbButton_1") && billtype.Equals("借条结算单"))
                {
                    Dictionary<string, decimal> dic = new Dictionary<string, decimal>();
                    //获取费用明细元数据
                    Entity entity = this.View.BillBusinessInfo.GetEntity("F_Sljk_Entity");
                    DynamicObjectCollection dy = this.View.Model.GetEntityDataObject(entity);
                    int row = this.Model.GetEntryCurrentRowIndex("F_Sljk_Entity");//获取选中的索引下标
                    StringBuilder sb = new StringBuilder();
                    string text=this.View.Model.GetValue("FText").ToString();
                    foreach (var itemjia in dy)
                    {
                        string FMASTERBASE = itemjia["FMasterBase_Id"].ToString();
                        if (!FMASTERBASE.Equals("0") && Convert.ToInt32(itemjia["seq"]) - 1 == row)
                        {
                            Decimal FQTYBASE = Convert.ToDecimal(itemjia["F_Sljk_Amounted1"]);
                            if (dic.ContainsKey(FMASTERBASE))
                            {
                                FQTYBASE += dic[FMASTERBASE];
                            }
                            dic[FMASTERBASE] = FQTYBASE;

                            sb.Append(this.View.Model.GetValue("FText").ToString() + itemjia["fsrcid"] + "," + itemjia["fsrcentryid"] + "," + FQTYBASE + ";");
                            this.View.Model.SetValue("FText", sb.Remove(sb.Length - 1, 1).ToString());
                            break;
                        }
                    }
                    //明细
                    Entity entity1 = this.View.BillBusinessInfo.GetEntity("FEntityDetail");
                    DynamicObjectCollection dy1 = this.View.Model.GetEntityDataObject(entity1);
                    foreach (var itemjia in dy1)
                    {
                        string FMASTERBASE = itemjia["MATERIALID_Id"].ToString();
                        Decimal FQTY = Convert.ToDecimal(itemjia["PriceQty"]);
                        int seq = Convert.ToInt32(itemjia["seq"]);
                        //判断dic中有没有明细物料
                        if (dic.ContainsKey(FMASTERBASE))
                        {
                            Decimal FYJSQTY = Convert.ToDecimal(this.View.Model.GetValue("FYJSQTY", seq - 1));
                            FYJSQTY = decimal.Subtract(FYJSQTY, Convert.ToDecimal(dic[FMASTERBASE]));
                            decimal FWJSqty = decimal.Subtract(FQTY, FYJSQTY);
                            this.View.Model.SetValue("FYJSQTY", FYJSQTY, seq - 1);//已结算数量
                            this.View.Model.SetValue("FWJSqty", FWJSqty, seq - 1);
                        }
                    }
                    StringBuilder sb1 = new StringBuilder();
                    sb1.Append(" 单据编号：" + this.View.Model.GetValue("Fbillno").ToString() + "执行费用明细删除行 ");
                    sb1.Append("\r\n记录删除行数据：" + this.View.Model.GetValue("FText")!=null? this.View.Model.GetValue("FText").ToString():"null");
                    Log.log(sb1.ToString());


                    this.View.Model.Save();
                }
                



                //保存校验
            }catch(Exception ex)
            {
                Log.log(ex.Message);
                throw new Exception(ex.Message);
            }
        }
        public override void BarItemClick(BarItemClickEventArgs e) 
        {
            base.BarItemClick(e);
            try {
                
                string billtype = ((DynamicObject)this.View.Model.GetValue("FBillTypeID"))["number"].ToString();
                //if (e.BarItemKey.Equals("tbSplitSave") && billtype.Equals("借条结算单"))
                if (e.BarItemKey.Equals("tbSplitSave") && billtype.Equals("借条结算单"))
                {
                    
                    Entity entity1 = this.View.BillBusinessInfo.GetEntity("FEntityDetail");
                    DynamicObjectCollection dy1 = this.View.Model.GetEntityDataObject(entity1);
                    foreach (var itemjia in dy1)
                    {
                        //if(itemjia["Foutsrcid"]==null && itemjia["Foutsrcentryid"]==null) { continue; }
                        //销售出库单已结算金额反写
                        FormMetadata ExpMeta2 = MetaDataServiceHelper.Load(this.Context, "SAL_OUTSTOCK", true) as FormMetadata;
                        DynamicObject Expobj2 = BusinessDataServiceHelper.LoadSingle(this.Context, itemjia["Foutsrcid"].ToString(), ExpMeta2.BusinessInfo.GetDynamicObjectType());
                        DynamicObjectCollection outEntry = Expobj2["SAL_OUTSTOCKENTRY"] as DynamicObjectCollection;
                        foreach (DynamicObject oEntry in outEntry)
                        {
                            if (oEntry["id"].ToString().Equals(itemjia["Foutsrcentryid"].ToString()))
                            {
                                oEntry["FYJSQTY"] = Convert.ToDecimal(itemjia["FYJSQTY"]);
                                oEntry["FWJSQTY"] = Convert.ToDecimal(itemjia["FWJSQTY"]);
                                break;
                            }
                        }
                        BusinessDataServiceHelper.Save(this.Context, Expobj2);
                    }
                    object text = this.View.Model.GetValue("Ftext");
                    if (text!=null)
                    {
                        StringBuilder sb1 = new StringBuilder();
                        sb1.Append(" 单据编号：" + this.View.Model.GetValue("Fbillno").ToString() + "执行保存操作");
                        string[] texts = text.ToString().Split(';');
                        foreach (string ts in texts)
                        {
                            if (!string.IsNullOrEmpty(ts))
                            {
                                string[] s = ts.Split(',');
                                //其他应付单未核销金额反写
                                FormMetadata ExpMeta1 = MetaDataServiceHelper.Load(this.Context, "AP_OtherPayable", true) as FormMetadata;
                                DynamicObject Expobj1 = BusinessDataServiceHelper.LoadSingle(this.Context, s[0].ToString(), ExpMeta1.BusinessInfo.GetDynamicObjectType());
                                DynamicObjectCollection otherEntry = Expobj1["FEntity"] as DynamicObjectCollection;
                                foreach (DynamicObject oEntry in otherEntry)
                                {
                                    if (oEntry["id"].ToString().Equals(s[1].ToString()))
                                    {
                                        sb1.Append("\r\n其他应付单：" + Expobj1["Billno"]);
                                        sb1.Append("\r\n其他应付单未核销金额反写：" + (Convert.ToDecimal(oEntry["F_Sljk_Amount"]) + Convert.ToDecimal(s[2])));
                                        //oEntry["FNOTWRITTENOFFAMOUNTFOR"] = Convert.ToDecimal(oEntry["FNOTWRITTENOFFAMOUNTFOR"])+Convert.ToDecimal(s[2]);
                                        oEntry["F_Sljk_Amount"] = Convert.ToDecimal(oEntry["F_Sljk_Amount"]) + Convert.ToDecimal(s[2]);
                                        break;
                                    }
                                }
                                BusinessDataServiceHelper.Save(this.Context, Expobj1);
                            }
                        }
                        this.View.Model.SetValue("Ftext", "");
                        Log.log(sb1.ToString());
                    }
                }
            }catch (Exception ex) { throw new Exception(ex.Message); }
        }
    }
}