USE [SBODemo]
GO
/****** Object:  StoredProcedure [dbo].[BAA_Service]    Script Date: 04-12-2021 8:28:20 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- exec BAA_Service '20210901', '20210930', 'S',''
ALTER procedure [dbo].[BAA_Service] (
	@FrmDate nvarchar(20),
	@ToDate nvarchar(20),
	@Type char(1),
	@CardCode nvarchar(100)
)
AS
BEGIN
	IF (@Type = 'S')
	BEGIN
		IF (isnull(@CardCode,'') = '')
		BEGIN
		  select ROW_NUMBER() OVER(ORDER BY U_PRevTyp DESC) AS LineId,U_DocDate,U_PRevTyp,U_OldBAEn,U_OldBANo,U_NewBAEn,U_NewBANo,U_CardCode,U_CardName,
		  U_CustNum,U_OldICode,U_NewICode,U_OldIName,U_NewIName,U_OldBALNo,U_NewBALNo,U_InvEnt,U_InvNum,U_InvDate,U_InvDTot,U_InvICode,U_InvWCode,U_InvQty,
		  U_EffDate,U_OldPrice,U_NewPrice,U_PriceDff,U_ILineNum,U_ITaxCode,U_InvHSN,U_InvDisc,U_InvDepmt
		  from (
			select
			ROW_NUMBER() OVER(ORDER BY I0.DocEntry ASC) AS LineId,
			GETDATE() U_DocDate,
			'Blanket Agreement' as "U_PRevTyp",
			I1.AgrNo  as "U_OldBAEn",
			(select Number from OOAT where AbsID = I1.AgrNo) as "U_OldBANo",
			I1.U_NewEntry as "U_NewBAEn",
			I1.U_NewNo  as "U_NewBANo",
			I0.CardCode as "U_CardCode" ,
			(select "CardName" from OCRD where "CardCode" =I0.CardCode) as "U_CardName",
			I0.NumAtCard U_CustNum,
			I1."ItemCode" as "U_OldICode",
			I1.U_NewICode  as "U_NewICode",
			I1.Dscription  as "U_OldIName",
			(select ItemName from OITM where ItemCode = I1.U_NewICode)  as "U_NewIName",
			I1.AgrLnNum as "U_OldBALNo",
			I1.U_NewLNum  as "U_NewBALNo",
			I1."DocEntry" as "U_InvEnt",
			cast(I0."DocNum" as nvarchar(100)) as "U_InvNum",
			I0."DocDate" as "U_InvDate",
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode" as "U_InvWCode",
			I1."OpenQty" as "U_InvQty",
			I1.U_EffDate as "U_EffDate",
			I1.Price as "U_OldPrice",
			I1.U_NewPrice as "U_NewPrice",
			ABS(I1.U_NewPrice- I1.Price) as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from OINV I0
			inner join INV1 I1 on I0.DocEntry = I1.DocEntry  
			where I0.DocDate Between @FrmDate and @ToDate --and I0.DocDate >= I1.U_EffDate 
			and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' --and isnull(I1.BaseEntry,'') <> ''
			and I0.doctype='I' and I0.GSTTranTyp='GA'
			and isnull(I1."U_BAPosted",'N')='N' and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) not in ('108','109')	


			union all

			select * from(
			select
			ROW_NUMBER() OVER(ORDER BY I0.NumAtCard ASC) AS LineId,
			GETDATE() U_DocDate,
			'Sales Order' as "U_PRevTyp",
			I1.BaseEntry as "U_OldBAEn", 
			I1.BaseRef as "U_OldBANo",
			I1.U_NewEntry as U_NewBAEn,
			I1.U_NewNo as U_NewBANo,
			I0."CardCode" as "U_CardCode",
			(select "CardName" from OCRD where "CardCode" = I0."CardCode") as "U_CardName",
			(select NumAtCard from ORDR where DocEntry = I1.BaseEntry) U_CustNum,				
			I1."ItemCode" as "U_OldICode",
			I1.U_NewICode as U_NewICode,
			I1."Dscription" as "U_OldIName",
			(select ItemName from OITM where ItemCode = I1.U_NewICode) as U_NewIName,
			I1."LineNum" as "U_OldBALNo",
			I1.U_NewLNum as U_NewBALNo,
			I0."DocEntry"  as "U_InvEnt" ,
			I0."NumAtCard" as "U_InvNum",
			I0."DocDate" as "U_InvDate", 
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode"  as "U_InvWCode",  
			I1."OpenQty" as "U_InvQty",
			I1.U_EffDate,
			--case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then isnull(I1.U_NewPrice,0) else isnull(I1.Price,0) end as "U_OldPrice",
			isnull(I1.Price,0) as "U_OldPrice",
			I1.U_NewPrice as U_NewPrice,
			--(I1.U_NewPrice - (case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then isnull(I1.U_NewPrice,0) else isnull(I1.Price,0) end) - ((isnull(I1.U_CRNPrice,0)*-1) + isnull(I1.U_DBNPrice,0))) as "U_PriceDff",
			0 as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from OINV I0
			inner join INV1 I1 on I0.DocEntry = I1.DocEntry 
			where I0.DocDate Between @FrmDate and @ToDate --and I0.DocDate >= I1.U_EffDate 
			--and I0.DocDate >= isnull(R1.U_CRDNCOMPDATE ,'19000101') 
			and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' --and isnull(I1.BaseEntry,'') <> ''
			and I0.doctype='I' and I0.GSTTranTyp='GA'
			and isnull(I1."U_BAPosted",'N')='N' and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) not in ( '108' , '109')

			) a --where ("U_OldPrice" - "U_NewPrice" ) != 0 

			union all

			select * from (
			select
			ROW_NUMBER() OVER(ORDER BY I0.NumAtCard ASC) AS LineId,
			GETDATE() U_DocDate,
			'Credit Note' as "U_PRevTyp",
			I1.BaseEntry as "U_OldBAEn", 
			I1.BaseRef as "U_OldBANo",
			I1.U_NewEntry as U_NewBAEn,
			I1.U_NewNo as U_NewBANo,
			I0."CardCode" as "U_CardCode",
			(select "CardName" from OCRD where "CardCode" = I0."CardCode") as "U_CardName",
			(select NumAtCard from ORDR where DocEntry = I1.BaseEntry) U_CustNum,				
			I1."ItemCode" as "U_OldICode",
			I1.U_NewICode as U_NewICode,
			I1."Dscription" as "U_OldIName",
			(select ItemName from OITM where ItemCode = I1.U_NewICode) as U_NewIName,
			I1."LineNum" as "U_OldBALNo",
			I1.U_NewLNum as U_NewBALNo,
			I0."DocEntry"  as "U_InvEnt" ,
			I0."NumAtCard" as "U_InvNum",
			I0."DocDate" as "U_InvDate", 
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode"  as "U_InvWCode",  
			I1."OpenQty" as "U_InvQty",
			I1.U_EffDate,
			--case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then isnull(I1.U_NewPrice,0) else isnull(I1.Price,0) end as "U_OldPrice",
			 isnull(I1.U_NewPrice,0) "U_OldPrice",
			I1.U_NewPrice as U_NewPrice,
			--(I1.U_NewPrice - (case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then isnull(I1.U_NewPrice,0) else isnull(I1.Price,0) end) - ((isnull(I1.U_CRNPrice,0)*-1) + isnull(I1.U_DBNPrice,0))) 
			I1.U_NewPrice - isnull(I1.Price,0)  as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from ORIN I0
			inner join RIN1 I1 on I0.DocEntry = I1.DocEntry 
			where I0.DocDate Between @FrmDate and @ToDate --and I0.DocDate >= I1.U_EffDate 
			--and I0.DocDate >= isnull(R1.U_CRDNCOMPDATE ,'19000101') 
			and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' --and isnull(I1.BaseEntry,'') <> ''
			and I0.doctype='I' and I0.GSTTranTyp='GA'
			and isnull(I1."U_BAPosted",'N')='N' and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) not in ( '108' , '109')

			) a 
		  )b
		END
		ELSE
		BEGIN
		  select ROW_NUMBER() OVER(ORDER BY U_PRevTyp DESC) AS LineId,U_DocDate,U_PRevTyp,U_OldBAEn,U_OldBANo,U_NewBAEn,U_NewBANo,U_CardCode,U_CardName
			  U_CustNum,U_OldICode,U_NewICode,U_OldIName,U_NewIName,U_OldBALNo,U_NewBALNo,U_InvEnt,U_InvNum,U_InvDate,U_InvDTot,U_InvICode,U_InvWCode,U_InvQty,
			  U_EffDate,U_OldPrice,U_NewPrice,U_PriceDff,U_ILineNum,U_ITaxCode,U_InvHSN,U_InvDisc,U_InvDepmt
			from (
			select
			ROW_NUMBER() OVER(ORDER BY I0.DocEntry ASC) AS LineId,
			GETDATE() U_DocDate,
			'Blanket Agreement' as "U_PRevTyp",
			I1.AgrNo  as "U_OldBAEn",
			(select Number from OOAT where AbsID = I1.AgrNo) as "U_OldBANo",
			I1.U_NewEntry as "U_NewBAEn",
			I1.U_NewNo  as "U_NewBANo",
			I0.CardCode as "U_CardCode" ,
			(select "CardName" from OCRD where "CardCode" =I0.CardCode) as "U_CardName",
			I0.NumAtCard U_CustNum,
			I1."ItemCode" as "U_OldICode",
			I1.U_NewICode  as "U_NewICode",
			I1.Dscription  as "U_OldIName",
			(select ItemName from OITM where ItemCode = I1.U_NewICode)  as "U_NewIName",
			I1.AgrLnNum as "U_OldBALNo",
			I1.U_NewLNum  as "U_NewBALNo",
			I1."DocEntry" as "U_InvEnt",
			cast(I0."DocNum" as nvarchar(100)) as "U_InvNum",
			I0."DocDate" as "U_InvDate",
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode" as "U_InvWCode",
			I1."OpenQty" as "U_InvQty",
			I1.U_EffDate as "U_EffDate",
			I1.Price as "U_OldPrice",
			I1.U_NewPrice as "U_NewPrice",
			ABS(I1.U_NewPrice- I1.Price) as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from OINV I0
			inner join INV1 I1 on I0.DocEntry = I1.DocEntry  
			where I0.DocDate Between @FrmDate and @ToDate and I0.CardCode = @CardCode  --and I0.DocDate >= I1.U_EffDate 
			and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' --and isnull(I1.BaseEntry,'') <> ''
			and I0.doctype='I' and I0.GSTTranTyp='GA'
			and isnull(I1."U_BAPosted",'N')='N' and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) not in ('108','109')	



			union all

			select * from (
			select
			ROW_NUMBER() OVER(ORDER BY I0.NumAtCard ASC) AS LineId,
			GETDATE() U_DocDate,
			'Sales Order' as "U_PRevTyp",
			I1.BaseEntry as "U_OldBAEn", 
			I1.BaseRef as "U_OldBANo",
			I1.U_NewEntry as U_NewBAEn,
			I1.U_NewNo as U_NewBANo,
			I0."CardCode" as "U_CardCode",
			(select "CardName" from OCRD where "CardCode" = I0."CardCode") as "U_CardName",
			(select NumAtCard from ORDR where DocEntry = I1.BaseEntry) U_CustNum,				
			I1."ItemCode" as "U_OldICode",
			I1.U_NewICode as U_NewICode,
			I1."Dscription" as "U_OldIName",
			(select ItemName from OITM where ItemCode = I1.U_NewICode) as U_NewIName,
			I1."LineNum" as "U_OldBALNo",
			I1.U_NewLNum as U_NewBALNo,
			I0."DocEntry"  as "U_InvEnt" ,
			I0."NumAtCard" as "U_InvNum",
			I0."DocDate" as "U_InvDate", 
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode"  as "U_InvWCode",  
			I1."OpenQty" as "U_InvQty",
			I1.U_EffDate,
			--case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then I1.U_NewPrice else isnull(I1.Price,0) end 
			isnull(I1.Price,0) as "U_OldPrice",
			I1.U_NewPrice as U_NewPrice,
			--(I1.U_NewPrice - (case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then I1.U_NewPrice else isnull(I1.Price,0) end) - ((isnull(I1.U_CRNPrice,0)*-1) + isnull(I1.U_DBNPrice,0))) 
			I1.U_NewPrice  - isnull(I1.Price,0) as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from OINV I0
			inner join INV1 I1 on I0.DocEntry = I1.DocEntry  
			where I0.DocDate Between @FrmDate and @ToDate and I0.CardCode = @CardCode --and I0.DocDate >= I1.U_EffDate 
			and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' --and isnull(I1.BaseEntry,'') <> ''
			and I0.doctype='I' and I0.GSTTranTyp='GA'
			and isnull(I1."U_BAPosted",'N')='N' and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) not in ('108','109')
			) a 

			union all

			select * from (
			select
			ROW_NUMBER() OVER(ORDER BY I0.NumAtCard ASC) AS LineId,
			GETDATE() U_DocDate,
			'Credit Note' as "U_PRevTyp",
			I1.BaseEntry as "U_OldBAEn", 
			I1.BaseRef as "U_OldBANo",
			I1.U_NewEntry as U_NewBAEn,
			I1.U_NewNo as U_NewBANo,
			I0."CardCode" as "U_CardCode",
			(select "CardName" from OCRD where "CardCode" = I0."CardCode") as "U_CardName",
			(select NumAtCard from ORDR where DocEntry = I1.BaseEntry) U_CustNum,				
			I1."ItemCode" as "U_OldICode",
			I1.U_NewICode as U_NewICode,
			I1."Dscription" as "U_OldIName",
			(select ItemName from OITM where ItemCode = I1.U_NewICode) as U_NewIName,
			I1."LineNum" as "U_OldBALNo",
			I1.U_NewLNum as U_NewBALNo,
			I0."DocEntry"  as "U_InvEnt" ,
			I0."NumAtCard" as "U_InvNum",
			I0."DocDate" as "U_InvDate", 
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode"  as "U_InvWCode",  
			I1."OpenQty" as "U_InvQty",
			I1.U_EffDate,
			--case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then I1.U_NewPrice else isnull(I1.Price,0) end 
			isnull(I1.Price,0) as "U_OldPrice",
			I1.U_NewPrice as U_NewPrice,
			--(I1.U_NewPrice - (case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then I1.U_NewPrice else isnull(I1.Price,0) end) - ((isnull(I1.U_CRNPrice,0)*-1) + isnull(I1.U_DBNPrice,0))) 
			I1.U_NewPrice - isnull(I1.Price,0) as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from ORIN I0
			inner join RIN1 I1 on I0.DocEntry = I1.DocEntry  
			where I0.DocDate Between @FrmDate and @ToDate and I0.CardCode = @CardCode --and I0.DocDate >= I1.U_EffDate 
			and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' --and isnull(I1.BaseEntry,'') <> ''
			and I0.doctype='I' and I0.GSTTranTyp='GA'
			and isnull(I1."U_BAPosted",'N')='N' and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) not in ('108','109')
			) a 
		)b
		END
		
	END
	IF (@Type = 'P')
	BEGIN
		IF(isnull(@CardCode,'') = '')
		BEGIN
		   select ROW_NUMBER() OVER(ORDER BY U_PRevTyp DESC) AS LineId,GETDATE() as U_DocDate,U_PRevTyp,U_OldBAEn,U_OldBANo,U_NewBAEn,U_NewBANo,U_CardCode,U_CardName
			  U_CustNum,U_OldICode,U_NewICode,U_OldIName,U_NewIName,U_OldBALNo,U_NewBALNo,U_InvEnt,U_InvNum,U_InvDate,U_InvDTot,U_InvICode,U_InvWCode,U_InvQty,
			  U_EffDate,U_OldPrice,U_NewPrice,U_PriceDff,U_ILineNum,U_ITaxCode,U_InvHSN,U_InvDisc,U_InvDepmt
			from (
			select
			'Blanket Agreement' as "U_PRevTyp",
			I1.AgrNo  as "U_OldBAEn",
			(select Number from OOAT where AbsID = I1.AgrNo) as "U_OldBANo",
			I1.U_NewEntry  as "U_NewBAEn",
			I1.U_NewNo as "U_NewBANo",
			I0.CardCode as "U_CardCode" ,
			I0.NumAtCard U_CustNum,	
			(select "CardName" from OCRD where "CardCode" = I0."CardCode") as "U_CardName",
			I1."ItemCode" as "U_OldICode",
			I1.U_NewICode as "U_NewICode",
			I1.Dscription as "U_OldIName",
			(select ItemName from OITM where ItemCode = I1.U_NewICode) as "U_NewIName",
			I1.AgrLnNum  as "U_OldBALNo",
			I1.U_NewLNum as "U_NewBALNo",
			I1."DocEntry" as "U_InvEnt",
			cast(I0."DocNum" as nvarchar(100)) as "U_InvNum",
			I0."DocDate" as "U_InvDate",
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode" as "U_InvWCode",
			I1."OpenQty" as "U_InvQty",
			I1.U_EffDate as "U_EffDate",
			I1.Price as "U_OldPrice",
			I1.U_NewPrice as "U_NewPrice",
			ABS(I1.U_NewPrice  - I1.Price) as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from OPCH I0
			inner join PCH1 I1 on I0.DocEntry = I1.DocEntry  
			where I0.DocDate Between @FrmDate  and @ToDate --and I0.DocDate >= I1.U_EffDate 
			and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' --and isnull(I1.BaseEntry,'') <> ''
			and I0.doctype='I' and I0.GSTTranTyp='GA'
			and isnull(I1."U_BAPosted",'N')='N' --and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) not in ('108','109')	
		

			union all
		
			select
			'Purchase Order' as "U_PRevTyp",
			T0."DocEntry" as "U_OldBAEn", 
			T0."DocNum" as "U_OldBANo",
			T2."DocEntry" as "U_NewBAEn", 
			T2."DocNum" as "U_NewBANo",
			T0."CardCode" as "U_CardCode",
			T0.NumAtCard U_CustNum,	
			(select "CardName" from OCRD where "CardCode" = T0."CardCode") as "U_CardName", 
			T1."ItemCode" as "U_OldICode",
			T3."ItemCode" as "U_NewICode", 
			T1."Dscription" as "U_OldIName",
			T3."Dscription" as "U_NewIName", 
			T1."LineNum" as "U_OldBALNo",
			T3."LineNum" as "U_NewBALNo",
			I0."DocEntry"  as "U_InvEnt" ,
			cast(I0."DocNum" as nvarchar(100)) as "U_InvNum", 
			I0."DocDate" as "U_InvDate", 
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode"  as "U_InvWCode",  
			I1."OpenQty" as "U_InvQty",
			isnull(T0."DocDate",'') as "U_EffDate",

			T1."Price" as "U_OldPrice",
			T3."Price" as "U_NewPrice",
			ABS(T3."Price" - T1."Price") as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from OPOR T0 
			inner join POR1 T1 on T0."DocEntry"  = T1."DocEntry"
			inner join OPOR T2 on T2."U_OldSoNum" = T0."DocNum"
			inner join POR1 T3 on T2."DocEntry" = T3."DocEntry" and (T1."ItemCode" = T3."ItemCode" or T1.ItemCode = (select ItemCode from OITM where ItemCode = T1.ItemCode))
			inner join PDN1 P1 on P1."BaseEntry" = T0."DocEntry" and P1."BaseLine" = T1."LineNum" and P1."BaseType" =  '22'
			inner join PCH1 I1 on I1."BaseEntry" = P1."DocEntry" and I1."BaseLine" = P1."LineNum" and I1."BaseType" = '20'
			inner join OPCH I0 on I0."DocEntry" = I1."DocEntry"
			where (T3."Price" - T1."Price") != 0 and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' and  
			isnull(I1."U_BAPosted",'N')='N'  and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) <> 108
			and	I0."DocDate" Between @FrmDate and @ToDate

			union all

			select * from (
			select
			'Credit Note' as "U_PRevTyp",
			I1.BaseEntry as "U_OldBAEn", 
			I1.BaseRef as "U_OldBANo",
			I1.U_NewEntry as U_NewBAEn,
			I1.U_NewNo as U_NewBANo,
			I0."CardCode" as "U_CardCode",
			(select NumAtCard from ORDR where DocEntry = I1.BaseEntry) U_CustNum,
			(select "CardName" from OCRD where "CardCode" = I0."CardCode") as "U_CardName",
			I1."ItemCode" as "U_OldICode",
			I1.U_NewICode as U_NewICode,
			I1."Dscription" as "U_OldIName",
			(select ItemName from OITM where ItemCode = I1.U_NewICode) as U_NewIName,
			I1."LineNum" as "U_OldBALNo",
			I1.U_NewLNum as U_NewBALNo,
			I0."DocEntry"  as "U_InvEnt" ,
			cast(I0."NumAtCard" as nvarchar(100)) as "U_InvNum",
			I0."DocDate" as "U_InvDate", 
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode"  as "U_InvWCode",  
			I1."OpenQty" as "U_InvQty",
			I1.U_EffDate,
			--case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then I1.U_NewPrice else isnull(I1.Price,0) end 
			isnull(I1.Price,0) as "U_OldPrice",
			I1.U_NewPrice as U_NewPrice,
			--(I1.U_NewPrice - (case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then I1.U_NewPrice else isnull(I1.Price,0) end) - ((isnull(I1.U_CRNPrice,0)*-1) + isnull(I1.U_DBNPrice,0))) 
			I1.U_NewPrice -  isnull(I1.Price,0) as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from ORPC I0
			inner join RPC1 I1 on I0.DocEntry = I1.DocEntry  
			where I0.DocDate Between @FrmDate  and @ToDate --and I0.DocDate >= I1.U_EffDate 
			and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' --and isnull(I1.BaseEntry,'') <> ''
			and I0.doctype='I' and I0.GSTTranTyp='GA'
			and isnull(I1."U_BAPosted",'N')='N' and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) not in ('108','109')
			) a 
			)b
		END
		ELSE
		BEGIN
		select ROW_NUMBER() OVER(ORDER BY U_PRevTyp DESC) AS LineId,GETDATE() as U_DocDate,U_PRevTyp,U_OldBAEn,U_OldBANo,U_NewBAEn,U_NewBANo,U_CardCode,U_CardName
			  U_CustNum,U_OldICode,U_NewICode,U_OldIName,U_NewIName,U_OldBALNo,U_NewBALNo,U_InvEnt,U_InvNum,U_InvDate,U_InvDTot,U_InvICode,U_InvWCode,U_InvQty,
			  U_EffDate,U_OldPrice,U_NewPrice,U_PriceDff,U_ILineNum,U_ITaxCode,U_InvHSN,U_InvDisc,U_InvDepmt
			from (
			select
			'Blanket Agreement' as "U_PRevTyp",
			I1.AgrNo  as "U_OldBAEn",
			(select Number from OOAT where AbsID = I1.AgrNo) as "U_OldBANo",
			I1.U_NewEntry  as "U_NewBAEn",
			I1.U_NewNo as "U_NewBANo",
			I0.CardCode as "U_CardCode" ,
			I0.NumAtCard U_CustNum,	
			(select "CardName" from OCRD where "CardCode" = I0."CardCode") as "U_CardName",
			I1."ItemCode" as "U_OldICode",
			I1.U_NewICode as "U_NewICode",
			I1.Dscription as "U_OldIName",
			(select ItemName from OITM where ItemCode = I1.U_NewICode) as "U_NewIName",
			I1.AgrLnNum  as "U_OldBALNo",
			I1.U_NewLNum as "U_NewBALNo",
			I1."DocEntry" as "U_InvEnt",
			cast(I0."DocNum" as nvarchar(100)) as "U_InvNum",
			I0."DocDate" as "U_InvDate",
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode" as "U_InvWCode",
			I1."OpenQty" as "U_InvQty",
			I1.U_EffDate as "U_EffDate",
			I1.Price as "U_OldPrice",
			I1.U_NewPrice as "U_NewPrice",
			ABS(I1.U_NewPrice  - I1.Price) as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from OPCH I0
			inner join PCH1 I1 on I0.DocEntry = I1.DocEntry  
			where I0.DocDate Between @FrmDate  and @ToDate and I0.CardCode = @CardCode --and I0.DocDate >= I1.U_EffDate 
			and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' --and isnull(I1.BaseEntry,'') <> ''
			and I0.doctype='I' and I0.GSTTranTyp='GA'
			and isnull(I1."U_BAPosted",'N')='N' --and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) not in ('108','109')	
		

			union all
		
			select
			'Purchase Order' as "U_PRevTyp",
			T0."DocEntry" as "U_OldBAEn", 
			T0."DocNum" as "U_OldBANo",
			T2."DocEntry" as "U_NewBAEn", 
			T2."DocNum" as "U_NewBANo",
			T0."CardCode" as "U_CardCode",
			T0.NumAtCard U_CustNum,	
			(select "CardName" from OCRD where "CardCode" = T0."CardCode") as "U_CardName", 
			T1."ItemCode" as "U_OldICode",
			T3."ItemCode" as "U_NewICode", 
			T1."Dscription" as "U_OldIName",
			T3."Dscription" as "U_NewIName", 
			T1."LineNum" as "U_OldBALNo",
			T3."LineNum" as "U_NewBALNo",
			I0."DocEntry"  as "U_InvEnt" ,
			cast(I0."DocNum" as nvarchar(100)) as "U_InvNum", 
			I0."DocDate" as "U_InvDate", 
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode"  as "U_InvWCode",  
			I1."OpenQty" as "U_InvQty",
			isnull(T0."DocDate",'') as "U_EffDate",

			T1."Price" as "U_OldPrice",
			T3."Price" as "U_NewPrice",
			ABS(T3."Price" - T1."Price") as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from OPOR T0 
			inner join POR1 T1 on T0."DocEntry"  = T1."DocEntry"
			inner join OPOR T2 on T2."U_OldSoNum" = T0."DocNum"
			inner join POR1 T3 on T2."DocEntry" = T3."DocEntry" and (T1."ItemCode" = T3."ItemCode" or T1.ItemCode = (select ItemCode from OITM where ItemCode = T1.ItemCode))
			inner join PDN1 P1 on P1."BaseEntry" = T0."DocEntry" and P1."BaseLine" = T1."LineNum" and P1."BaseType" =  '22'
			inner join PCH1 I1 on I1."BaseEntry" = P1."DocEntry" and I1."BaseLine" = P1."LineNum" and I1."BaseType" = '20'
			inner join OPCH I0 on I0."DocEntry" = I1."DocEntry" and I0.CardCode = @CardCode 
			where (T3."Price" - T1."Price") != 0 and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' and  
			isnull(I1."U_BAPosted",'N')='N'  and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) <> 108
			and (
				--((I0.DocDate Between @FrmDate and @ToDate) and (select U_BADtTyp from OCRD where CardCode = I0.CardCode) = 1)
				(I0.DocDate Between @FrmDate and @ToDate)
				or
				--((I0.TaxDate  Between @FrmDate and @ToDate) and (select U_BADtTyp from OCRD where CardCode = I0.CardCode) = 2)
				(I0.TaxDate  Between @FrmDate and @ToDate)
			)

			union all

			select * from (
			select
			'Credit Note' as "U_PRevTyp",
			I1.BaseEntry as "U_OldBAEn", 
			I1.BaseRef as "U_OldBANo",
			I1.U_NewEntry as U_NewBAEn,
			I1.U_NewNo as U_NewBANo,
			I0."CardCode" as "U_CardCode",
			(select NumAtCard from ORDR where DocEntry = I1.BaseEntry) U_CustNum,
			(select "CardName" from OCRD where "CardCode" = I0."CardCode") as "U_CardName",
			I1."ItemCode" as "U_OldICode",
			I1.U_NewICode as U_NewICode,
			I1."Dscription" as "U_OldIName",
			(select ItemName from OITM where ItemCode = I1.U_NewICode) as U_NewIName,
			I1."LineNum" as "U_OldBALNo",
			I1.U_NewLNum as U_NewBALNo,
			I0."DocEntry"  as "U_InvEnt" ,
			cast(I0."NumAtCard" as nvarchar(100)) as "U_InvNum",
			I0."DocDate" as "U_InvDate", 
			I0.DocTotal as U_InvDTot,
			I1."ItemCode" as "U_InvICode",
			I1."WhsCode"  as "U_InvWCode",  
			I1."OpenQty" as "U_InvQty",
			I1.U_EffDate,
			--case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then I1.U_NewPrice else isnull(I1.Price,0) end 
			isnull(I1.Price,0) as "U_OldPrice",
			I1.U_NewPrice as U_NewPrice,
			--(I1.U_NewPrice - (case when I0.DocDate <= isnull(I1.U_CRDNCOMPDATE ,'19000101') then I1.U_NewPrice else isnull(I1.Price,0) end) - ((isnull(I1.U_CRNPrice,0)*-1) + isnull(I1.U_DBNPrice,0))) 
			I1.U_NewPrice - isnull(I1.Price,0) as "U_PriceDff",
			I1."LineNum"  as "U_ILineNum",
			I1."TaxCode" as "U_ITaxCode",
			I1."HsnEntry"  as "U_InvHSN",
			I1."DiscPrcnt" as "U_InvDisc",
			I1."OcrCode2" as "U_InvDepmt"
			from ORPC I0
			inner join RPC1 I1 on I0.DocEntry = I1.DocEntry  
			where I0.DocDate Between @FrmDate  and @ToDate  and I0.CardCode = @CardCode  --and I0.DocDate >= I1.U_EffDate 
			and I0."CANCELED" != 'Y' and isnull(I1."U_BaseEntry",'')  = '' --and isnull(I1.BaseEntry,'') <> ''
			and I0.doctype='I' and I0.GSTTranTyp='GA'
			and isnull(I1."U_BAPosted",'N')='N' and (select ItmsGrpCod from OITM where ItemCode = I1.ItemCode) not in ('108','109')
			) a 
			)b --and	I0."DocDate" Between @FrmDate and @ToDate
		END
		
	END
END