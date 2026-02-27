//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
//using Kingdee.BOS.Core.DynamicForm;
//using Kingdee.BOS.Core.Metadata;
//using Kingdee.BOS.Core.DynamicForm.PlugIn;
//using Kingdee.BOS.Orm.DataEntity;

//using Kingdee.BOS.ServiceHelper;
//using Kingdee.BOS.Core.Bill.PlugIn;
//using Kingdee.BOS.App.Data;
//using Kingdee.BOS.Core.List;
//using Kingdee.BOS.Core.Metadata.EntityElement;
//using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
//using System.Text;
//using Kingdee.BOS.Util;
//using Kingdee.BOS.Orm;

//namespace BD.Standard.KangLian.SettlementBill26
//{
//    [Description("本地结算单插件，点击按钮打开动态表单插件")]
//    [Kingdee.BOS.Util.HotUpdate]

//    public class Settlements : AbstractBillPlugIn
//    {
//        public override void AfterBindData(EventArgs e)
//        {
//             base.AfterBindData(e);
//             this.View.Model.SetValue("Ftext","");
//             //DynamicObject aa = this.Model.DataObject;
//             //this.View.ShowMessage(aa["Ftext"]==null?"": aa["Ftext"].ToString());
//            //this.View.ShowMessage(this.View.Model.GetValue("Ftext").ToString());

//        }
//        public override void EntryBarItemClick(BarItemClickEventArgs e)
//        {
//            base.EntryBarItemClick(e);
//            try
//            {

              
//                string billtype = ((DynamicObject)this.View.Model.GetValue("FBillTypeID"))["number"].ToString();
//                if (e.BarItemKey.Equals("ANtbButton") && billtype.Equals("YSD01_SYS"))
//                {
//                    if (((DynamicObject)this.Model.DataObject)["id"].ToString().Equals("0"))
//                    {
//                        this.View.InvokeFormOperation(FormOperationEnum.Save);
//                    }
//                    //打开单据查看实例
//                    DynamicFormShowParameter para = new DynamicFormShowParameter();
//                    //打开样式,打开动态表单
//                    para.OpenStyle.ShowType = ShowType.NonModal;
//                    para.SyncCallBackAction = true;
//                    para.FormId = "k8d290e8de5c74ebb8562617d08ba2fd6";//本地
//                    Entity entity = this.View.BillBusinessInfo.GetEntity("FEntityDetail");
//                    DynamicObjectCollection dy = this.View.Model.GetEntityDataObject(entity);
//                    StringBuilder masterid = new StringBuilder();
//                    foreach (var itemjia in dy)
//                    {
//                        masterid.Append(itemjia["MATERIALID_Id"].ToString() + ",");
//                    }
//                    masterid.Remove(masterid.Length - 1, 1);
//                    if ((DynamicObject)this.View.Model.GetValue("FSALEDEPTID") == null)
//                    {
//                        this.View.ShowErrMessage("部门为空");
//                        return;
//                    }
//                    string deptid = ((DynamicObject)this.View.Model.GetValue("FSALEDEPTID"))["id"].ToString();
//                    string masterids = masterid.ToString();
//                    para.CustomParams.Add("FSALEDEPTID", deptid);//销售部门
//                    para.CustomParams.Add("FMATERIAL", masterids);//费用项目 e.Row
//                    para.ParentPageId = this.View.ParentFormView.PageId;
//                    this.View.ShowForm(para, delegate (FormResult result)
//                    {
//                        //this.View.ShowForm(para, new Action<FormResult>((result) => { 

//                        //动态表单返回数据
//                        if (result.ReturnData != null)
//                        {
//                            List<Dictionary<string, object>> list = result.ReturnData as List<Dictionary<string, object>>;
//                            //this.View.Model.DeleteEntryData("费用明细标识");
//                            DynamicObject obj = this.Model.DataObject;
//                            string id= obj["id"].ToString();

//                            FormMetadata ExpMeta = MetaDataServiceHelper.Load(this.Context, "AR_receivable", true) as FormMetadata;
//                            DynamicObject Expobj = BusinessDataServiceHelper.LoadSingle(this.Context, id, ExpMeta.BusinessInfo.GetDynamicObjectType());
//                            if (list!=null&&list.Count > 0)
//                            {
//                                foreach (var returndata in list)
//                                {
//                                    string materialid = ((DynamicObject)returndata["FMATERIAL"])["id"].ToString();
//                                    Decimal qty = Convert.ToDecimal(returndata["FQty"]);
//                                    Decimal qty1 = Convert.ToDecimal(returndata["FQty"]);
//                                    bool flag = false;
//                                    Decimal qty2 = 0; //数量合计
//                                    Decimal qty3 = 0; //可用数量
//                                    int i = 0;
//                                    DynamicObjectCollection entry = Expobj["F_Sljk_Entity"] as DynamicObjectCollection;//费用明细实体
//                                    if (entry.Count > 0)
//                                    {
//                                        i = entry.Count;
//                                        foreach (var item in entry)
//                                        {
//                                            if (returndata["fsrcentryid"].ToString().Equals(item["fsrcentryid"]))
//                                            {
//                                                qty2 += Convert.ToDecimal(item["F_Sljk_Amount"]);
//                                                qty3 = Convert.ToDecimal(item["F_Sljk_Amounted"]);
//                                            }
//                                        }
//                                        if (decimal.Subtract(qty3, qty2) > 0)
//                                        {
//                                            qty1 = decimal.Subtract(qty3, qty2);
//                                        }
//                                        else if (qty3 == 0 && qty2 == 0)
//                                        {
//                                            qty1 = qty;
//                                        }
//                                        else
//                                        {
//                                            flag = true;
//                                        }
//                                    }
//                                    if (flag) continue;
//                                    DynamicObjectCollection dy1 = Expobj["AP_PAYABLEENTRY"] as DynamicObjectCollection;//明细实体
//                                    if (dy1 != null || dy1.Count > 0)
//                                    {
//                                        foreach (var item in dy1)
//                                        {
//                                            if (item["MaterialId_Id"].ToString().Equals(materialid))
//                                            {
//                                                decimal FYJSQTY = 0;
//                                                item["FWJSqty"] = Convert.ToDecimal(item["PriceQty"]) - Convert.ToDecimal(item["FYJSQTY"]);
//                                                if (Convert.ToInt32(item["FWJSqty"]) == 0) break;
//                                                Decimal FWJSqty = Convert.ToDecimal(item["FWJSqty"]);
//                                                if (decimal.Subtract(FWJSqty, qty1) >= 0)
//                                                {
//                                                    FYJSQTY = Convert.ToDecimal(item["FYJSQTY"]) + qty1;
//                                                    item["FYJSQTY"] = FYJSQTY;
//                                                    item["FWJSqty"] = Convert.ToDecimal(item["FWJSqty"]) - qty1;
//                                                }
//                                                else
//                                                {
//                                                    qty1 = Convert.ToDecimal(item["FWJSqty"]);
//                                                    FYJSQTY = Convert.ToDecimal(item["FYJSQTY"]) + qty1;
//                                                    item["FYJSQTY"] = FYJSQTY;
//                                                    item["FWJSqty"] = Convert.ToDecimal(item["FWJSqty"]) - qty1;
//                                                }
//                                                DynamicObject accept = entry.DynamicCollectionItemPropertyType.CreateInstance() as DynamicObject;
//                                                accept["seq"] = ++i;
//                                                accept["FMasterBase"] = returndata["FMATERIAL"];
//                                                accept["FMasterBase_Id"] = materialid;
//                                                accept["F_SLJK_BASE"] = returndata["Fcost"];
//                                                accept["F_SLJK_BASE_Id"] = ((DynamicObject)returndata["Fcost"])["id"].ToString();
//                                                accept["F_Sljk_Amount"] = qty1;
//                                                accept["F_Sljk_Amounted"] = qty;
//                                                accept["fsrcbillno"] = returndata["fsrcbillno"];
//                                                accept["fsrcid"] = returndata["fsrcid"];
//                                                accept["fsrcentryid"] = returndata["fsrcentryid"];
//                                                entry.Add(accept);

//                                                //其他应付单未核销金额反写
//                                                FormMetadata ExpMeta1 = MetaDataServiceHelper.Load(this.Context, "AP_OtherPayable", true) as FormMetadata;
//                                                DynamicObject Expobj1 = BusinessDataServiceHelper.LoadSingle(this.Context, returndata["fsrcid"].ToString(), ExpMeta1.BusinessInfo.GetDynamicObjectType());
//                                                DynamicObjectCollection otherEntry = Expobj1["FEntity"] as DynamicObjectCollection;
//                                                foreach (DynamicObject oEntry in otherEntry)
//                                                {
//                                                    if (oEntry["id"].ToString().Equals(returndata["fsrcentryid"].ToString()))
//                                                    {
//                                                        oEntry["FNOTWRITTENOFFAMOUNTFOR"] = Convert.ToDecimal(oEntry["FNOTWRITTENOFFAMOUNTFOR"]) - qty1;
//                                                        //oEntry["F_Sljk_Amount"] = Convert.ToDecimal(oEntry["F_Sljk_Amount"]) - qty1;
//                                                        break;
//                                                    }
//                                                }
//                                                BusinessDataServiceHelper.Save(this.Context, Expobj1);
//                                                //销售出库单已结算金额反写
//                                                FormMetadata ExpMeta2 = MetaDataServiceHelper.Load(this.Context, "SAL_OUTSTOCK", true) as FormMetadata;
//                                                DynamicObject Expobj2 = BusinessDataServiceHelper.LoadSingle(this.Context, item["Foutsrcid"].ToString(), ExpMeta2.BusinessInfo.GetDynamicObjectType());
//                                                DynamicObjectCollection outEntry = Expobj2["SAL_OUTSTOCKENTRY"] as DynamicObjectCollection;
//                                                foreach (DynamicObject oEntry in outEntry)
//                                                {
//                                                    if (oEntry["id"].ToString().Equals(item["Foutsrcentryid"].ToString()))
//                                                    {
//                                                        oEntry["FYJSQTY"] = FYJSQTY;
//                                                        break;
//                                                    }
//                                                }
//                                                BusinessDataServiceHelper.Save(this.Context, Expobj2);
//                                                break;
//                                            }
//                                        }
//                                    }
//                                }   
//                            }
//                            BusinessDataServiceHelper.Save(this.Context, Expobj);
//                        }
//                        this.View.Refresh();
                   
//                    });
                    
//                }
//                if (e.BarItemKey.Equals("Sljk_tbButton_1") && billtype.Equals("YSD01_SYS"))
//                {
//                    Dictionary<string, decimal> dic = new Dictionary<string, decimal>();
//                    //获取费用明细元数据
//                    Entity entity = this.View.BillBusinessInfo.GetEntity("F_Sljk_Entity");
//                    DynamicObjectCollection dy = this.View.Model.GetEntityDataObject(entity);
//                    int row = this.Model.GetEntryCurrentRowIndex("F_Sljk_Entity");//获取选中的索引下标
//                    StringBuilder sb = new StringBuilder();
//                    string text=this.View.Model.GetValue("FText").ToString();
//                    foreach (var itemjia in dy)
//                    {
//                        string FMASTERBASE = itemjia["FMasterBase_Id"].ToString();
//                        if (!FMASTERBASE.Equals("0") && Convert.ToInt32(itemjia["seq"]) - 1 == row)
//                        {
//                            Decimal FQTYBASE = Convert.ToDecimal(itemjia["F_Sljk_Amount"]);
//                            if (dic.ContainsKey(FMASTERBASE))
//                            {
//                                FQTYBASE += dic[FMASTERBASE];
//                            }
//                            dic[FMASTERBASE] = FQTYBASE;

//                            sb.Append(this.View.Model.GetValue("FText").ToString() + itemjia["fsrcid"] + "," + itemjia["fsrcentryid"] + "," + FQTYBASE + ";");
//                            this.View.Model.SetValue("FText", sb.Remove(sb.Length - 1, 1).ToString());
//                            break;
//                        }
//                    }
//                    //明细
//                    Entity entity1 = this.View.BillBusinessInfo.GetEntity("FEntityDetail");
//                    DynamicObjectCollection dy1 = this.View.Model.GetEntityDataObject(entity1);
//                    foreach (var itemjia in dy1)
//                    {
//                        string FMASTERBASE = itemjia["MATERIALID_Id"].ToString();
//                        Decimal FQTY = Convert.ToDecimal(itemjia["PriceQty"]);
//                        int seq = Convert.ToInt32(itemjia["seq"]);
//                        //判断dic中有没有明细物料
//                        if (dic.ContainsKey(FMASTERBASE))
//                        {
//                            Decimal FYJSQTY = Convert.ToDecimal(this.View.Model.GetValue("FYJSQTY", seq - 1));
//                            FYJSQTY = decimal.Subtract(FYJSQTY, Convert.ToDecimal(dic[FMASTERBASE]));
//                            decimal FWJSqty = decimal.Subtract(FQTY, FYJSQTY);
//                            this.View.Model.SetValue("FYJSQTY", FYJSQTY, seq - 1);//已结算数量
//                            this.View.Model.SetValue("FWJSqty", FWJSqty, seq - 1);
//                        }
//                    }
//                    this.View.Model.Save();
//                }
                



//                //保存校验
//            }catch(Exception ex)
//            {
               
//                throw new Exception(ex.Message);
//            }
//        }
//        public override void BarItemClick(BarItemClickEventArgs e) 
//        {
//            base.BarItemClick(e);
//            try { 
//                string billtype = ((DynamicObject)this.View.Model.GetValue("FBillTypeID"))["number"].ToString();
//                if (e.BarItemKey.Equals("tbSplitSave") && billtype.Equals("YSD01_SYS"))
//                    {
//                    Entity entity1 = this.View.BillBusinessInfo.GetEntity("FEntityDetail");
//                    DynamicObjectCollection dy1 = this.View.Model.GetEntityDataObject(entity1);
//                    foreach (var itemjia in dy1)
//                    {
//                        //销售出库单已结算金额反写
//                        FormMetadata ExpMeta2 = MetaDataServiceHelper.Load(this.Context, "SAL_OUTSTOCK", true) as FormMetadata;
//                        DynamicObject Expobj2 = BusinessDataServiceHelper.LoadSingle(this.Context, itemjia["Foutsrcid"].ToString(), ExpMeta2.BusinessInfo.GetDynamicObjectType());
//                        DynamicObjectCollection outEntry = Expobj2["SAL_OUTSTOCKENTRY"] as DynamicObjectCollection;
//                        foreach (DynamicObject oEntry in outEntry)
//                        {
//                            if (oEntry["id"].ToString().Equals(itemjia["Foutsrcentryid"].ToString()))
//                            {
//                                oEntry["FYJSQTY"] = Convert.ToDecimal(itemjia["FYJSQTY"]);
//                                break;
//                            }
//                        }
//                        BusinessDataServiceHelper.Save(this.Context, Expobj2);
//                    }

//                    string text = this.View.Model.GetValue("Ftext").ToString();
//                    string[] texts=text.Split(';');
//                    foreach (string ts in texts)
//                    {
//                        if (!string.IsNullOrEmpty(ts)) 
//                        {
//                            string[] s = ts.Split(',');
//                            //其他应付单未核销金额反写
//                            FormMetadata ExpMeta1 = MetaDataServiceHelper.Load(this.Context, "AP_OtherPayable", true) as FormMetadata;
//                            DynamicObject Expobj1 = BusinessDataServiceHelper.LoadSingle(this.Context, s[0].ToString(), ExpMeta1.BusinessInfo.GetDynamicObjectType());
//                            DynamicObjectCollection otherEntry = Expobj1["FEntity"] as DynamicObjectCollection;
//                            foreach (DynamicObject oEntry in otherEntry)
//                            {
//                                if (oEntry["id"].ToString().Equals(s[1].ToString()))
//                                {
//                                    oEntry["FNOTWRITTENOFFAMOUNTFOR"] = Convert.ToDecimal(oEntry["FNOTWRITTENOFFAMOUNTFOR"])+Convert.ToDecimal(s[2]);
//                                    //oEntry["F_Sljk_Amount"] = Convert.ToDecimal(oEntry["F_Sljk_Amount"])+Convert.ToDecimal(s[2]);
                                    
//                                    break;
//                                }
//                            }
//                            BusinessDataServiceHelper.Save(this.Context, Expobj1);
//                        }
//                    }
//                    this.View.Model.SetValue("Ftext","");
//                }
//            }catch (Exception ex) { throw new Exception(ex.Message); }
//        }
//    }
//}