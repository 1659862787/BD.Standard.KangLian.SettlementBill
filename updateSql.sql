----根据结算单明细反算借条出库单（已结算、未结算数量）
----update  d set d.FYJSQTY=a.FYJSQTY,d.FWJSQTY=a.FWJSQTY
--select Fpriceqty,a.FYJSQTY,a.FWJSQTY,d.FYJSQTY,d.FWJSQTY,* 
--from  t_AR_receivableentry  a
--inner join t_AR_receivable b on a.FID=b.fid
--inner join T_AR_RECEIVABLEENTRY_LK c on a.FENTRYID=c.fentryid
--left join T_SAL_OUTSTOCKENTRY d on c.FSID=d.FENTRYID
--where FBILLTYPEID='652de056a84984'  and  (a.FYJSQTY<>d.FYJSQTY or d.FWJSQTY<>a.FWJSQTY)

----根据可用余额单查询结算单,得出差异数据
----update a set F_PUB_AMOUNT=F_SHKL_AMOUNT-isnull(F_SLJK_AMOUNTED1,0) 
--select FBILLNO,F_SHKL_AMOUNT,F_PUB_AMOUNT,F_SHKL_AMOUNT-F_PUB_AMOUNT,isnull(F_SLJK_AMOUNTED1,0) 
--from T_AP_OTHERPAYABLEENTRY a
--left join (select sum(F_SLJK_AMOUNTED1) as F_SLJK_AMOUNTED1,fsrcentryid from Sljk_t_Cust_Entry100002 group by fsrcentryid
--) b on a.FENTRYID=b.fsrcentryid
--left join T_AP_OTHERPAYABLE c on a.FID=c.fid
--where FBILLTYPEID='652e06e5a8679f'
--and F_SHKL_AMOUNT-F_PUB_AMOUNT<>isnull(F_SLJK_AMOUNTED1,0)


--获取剩余可用数量为负数的结算单明细
select a1.fbillno [数量可用余额单],a.F_SHKL_AMOUNT [总数量],a.F_PUB_AMOUNT [剩余可用数量],b1.fbillno [结算单据],b.F_SLJK_AMOUNTED1 [已使用数量],b.F_SLJK_AMOUNTED [剩余可用]
 from T_AP_OTHERPAYABLEENTRY  a
inner join T_AP_OTHERPAYABLE a1 on a.FID=a1.FID
inner join Sljk_t_Cust_Entry100002  b on a.fentryid=b.fsrcentryid
inner join t_AR_receivable b1 on b.FID=b1.FID
where a.F_PUB_AMOUNT<0
order by a1.fbillno 

