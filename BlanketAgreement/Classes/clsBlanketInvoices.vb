Imports System.Threading
Imports System.IO
Imports System.Xml

Public Class clsBlanketInvoices

#Region "Declaration"
    Private objSBOAPI As ClsSBO
    Private objUtility As clsUtilities
    Private oDBs_Head, oDBs_Detail, oDBs_Detai2 As SAPbouiCOM.DBDataSource
    Dim oMatrix, oMatrix1 As SAPbouiCOM.Matrix
    Private objForm, oForm, cflForm As SAPbouiCOM.Form
    Dim oRS, oRS1, oRS3 As SAPbobsCOM.Recordset
    Public Const FormType = "UDO_BAAS"
    Public strsql As String
    Dim invEntries As String
    Dim InvoiceTableName, MemoTableName, BpType, OrderType, nulltype, BaseType As String
    Dim dtBAAS As DataTable
    Dim oCRNSeries, oDBNSeries, oPurchaseCRNSeries, oPurchaseDBNSeries As String
    Dim SuccessMessage As String = "SUCCESS"
    Dim dt_time As String = Today.ToString("yyyyMMdd") & "\Log_" & Now.ToString("HH_mm_ss")

    Dim xmlDoc As New XmlDocument()
    Dim SalesQuery As String
    Dim PurchaseQuery As String
#End Region

#Region "Constructors"
    Public Sub New(ByVal objSBO As ClsSBO)
        objSBOAPI = objSBO
        objUtility = New clsUtilities(objSBOAPI)
    End Sub
#End Region

#Region "Methods"

#Region "Init Form Methods"

    Sub LoadForm()
        Try

            objForm = objSBOAPI.LoadForm(System.Windows.Forms.Application.StartupPath & "\XML\FrmBlanketAgreement.xml", FormType)
            oDBs_Head = objForm.DataSources.DBDataSources.Item(0)
            oDBs_Detail = objForm.DataSources.DBDataSources.Item(1)
            oMatrix = objForm.Items.Item("Matrix").Specific

            InitiallizeForm(objForm)
            DefineDataTable()
            DefineSeries()
            LoadQuery()
            'write_log("-----------------BAA Service Form Loaded Successfully-----------------")
        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("LoadForm Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        End Try
    End Sub

    Public Sub LoadQuery()
        Try
            Dim strPath As String = Application.StartupPath.ToString().Trim() + "\XML\BAPriceAutoQuery.xml"
            Dim strXML As String = File.ReadAllText(strPath)
            xmlDoc.LoadXml(strXML)
            Dim parentNode As XmlNodeList = xmlDoc.GetElementsByTagName("BAPRICEQUERY")
            For Each child1 As XmlNode In parentNode
                SalesQuery = child1.Item("SALES").InnerText
                PurchaseQuery = child1.Item("PURCHASE").InnerText
            Next
        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("LoadQuery Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        End Try
    End Sub

    Public Sub InitiallizeForm(ByVal objform As SAPbouiCOM.Form, Optional ByVal btnname As String = "2")
        Try
            objform.Freeze(True)

            If btnname <> "1" Then
                Dim cmb As SAPbouiCOM.ComboBox
                cmb = objform.Items.Item("U_BAType").Specific
                cmb.Select("S", SAPbouiCOM.BoSearchKey.psk_ByDescription)
            End If

            Dim curDate As String = Date.Today.Day.ToString() + "/" + Date.Today.Month.ToString() + "/" + Date.Today.Year.ToString()
            objform.Items.Item("U_FromDate").Specific.String = curDate
            objform.Items.Item("U_ToDate").Specific.String = curDate

            If objform.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE Then
                objform.Items.Item("btnPosting").Enabled = True
            Else
                objform.Items.Item("btnPosting").Enabled = False
            End If


        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("InitiallizeForm Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        Finally
            objform.Freeze(False)
        End Try
    End Sub

    Sub DefineSeries()
        Try
            strsql = "select top 1 ""U_ARCreSer"",""U_ARDebSer"",""U_APCreSer"",""U_APDebSer"" from ""@QL_BAAS"" where ""U_Default"" = 'Y'"
            oRS1 = objUtility.DoQuery(strsql)
            If (oRS1.RecordCount > 0) Then
                oCRNSeries = oRS1.Fields.Item("U_ARCreSer").Value.ToString()
                oDBNSeries = oRS1.Fields.Item("U_ARDebSer").Value.ToString()
                oPurchaseCRNSeries = oRS1.Fields.Item("U_APCreSer").Value.ToString()
                oPurchaseDBNSeries = oRS1.Fields.Item("U_APDebSer").Value.ToString()
            End If
        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("DefineSeries Falied." + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        End Try
    End Sub

    Sub DefineDataTable()
        Try
            dtBAAS = Nothing
            dtBAAS = New DataTable
            Dim LineNum As DataColumn = New DataColumn("LineNum")
            LineNum.DataType = System.Type.GetType("System.String")

            'Dim PostingType As DataColumn = New DataColumn("PostingType")
            'PostingType.DataType = System.Type.GetType("System.String")

            Dim PriceRType As DataColumn = New DataColumn("PriceRType")
            PriceRType.DataType = System.Type.GetType("System.String")

            Dim CardCode As DataColumn = New DataColumn("CardCode")
            CardCode.DataType = System.Type.GetType("System.String")

            Dim DocDate As DataColumn = New DataColumn("DocDate")
            DocDate.DataType = System.Type.GetType("System.DateTime")

            Dim ItemCode As DataColumn = New DataColumn("ItemCode")
            ItemCode.DataType = System.Type.GetType("System.String")

            Dim Qty As DataColumn = New DataColumn("Qty")
            Qty.DataType = System.Type.GetType("System.Decimal")

            Dim WhsCode As DataColumn = New DataColumn("WhsCode")
            WhsCode.DataType = System.Type.GetType("System.String")

            Dim DiffPrice As DataColumn = New DataColumn("DiffPrice")
            DiffPrice.DataType = System.Type.GetType("System.Decimal")

            Dim TaxCode As DataColumn = New DataColumn("TaxCode")
            TaxCode.DataType = System.Type.GetType("System.String")

            Dim BaseEntry As DataColumn = New DataColumn("BaseEntry")
            BaseEntry.DataType = System.Type.GetType("System.String")

            Dim BaseLine As DataColumn = New DataColumn("BaseLine")
            BaseLine.DataType = System.Type.GetType("System.String")

            Dim BaseNum As DataColumn = New DataColumn("BaseNum")
            BaseNum.DataType = System.Type.GetType("System.String")

            Dim HSNEntry As DataColumn = New DataColumn("HSNEntry")
            HSNEntry.DataType = System.Type.GetType("System.String")

            Dim CostingCode2 As DataColumn = New DataColumn("CostingCode2")
            CostingCode2.DataType = System.Type.GetType("System.String")

            Dim PostType As DataColumn = New DataColumn("PostType")
            PostType.DataType = System.Type.GetType("System.String")
            'Dim DiscountPercent As DataColumn = New DataColumn("DiscountPercent")
            'DiscountPercent.DataType = System.Type.GetType("System.Decimal")

            dtBAAS.Columns.Add(LineNum)
            'dtBAAS.Columns.Add(PostingType)
            dtBAAS.Columns.Add(PriceRType)
            dtBAAS.Columns.Add(CardCode)
            dtBAAS.Columns.Add(DocDate)

            dtBAAS.Columns.Add(ItemCode)
            dtBAAS.Columns.Add(Qty)
            dtBAAS.Columns.Add(WhsCode)
            dtBAAS.Columns.Add(TaxCode)
            dtBAAS.Columns.Add(DiffPrice)
            dtBAAS.Columns.Add(BaseEntry)
            dtBAAS.Columns.Add(BaseLine)
            dtBAAS.Columns.Add(BaseNum)
            dtBAAS.Columns.Add(HSNEntry)
            dtBAAS.Columns.Add(CostingCode2)
            dtBAAS.Columns.Add(PostType)
            'dtBAAS.Columns.Add(DiscountPercent)

        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("DefineDataTable Falied." + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        End Try

    End Sub

    Function ValidateAll() As Boolean
        Try
            If oMatrix.RowCount <= 0 Then
                objSBOAPI.SBO_Appln.StatusBar.SetText("Add Atleast 1 Matrix Record To Add.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            End If
            Return True
        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("ValidatePosting Falied." + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            Return False
        End Try
    End Function

    Function ValidatePosting() As Boolean
        Try
            Dim Flag As Boolean = False
            If IsNothing(objForm) Then
                objForm = objSBOAPI.SBO_Appln.Forms.ActiveForm
            End If
            If IsNothing(oMatrix) Then
                oMatrix = objForm.Items.Item("Matrix").Specific
            End If
            For i As Integer = 0 To oMatrix.RowCount - 1
                Dim chkSelect As SAPbouiCOM.CheckBox
                chkSelect = oMatrix.Columns.Item("U_Select").Cells.Item(i + 1).Specific
                If chkSelect.Checked = True Then
                    Flag = True
                    Exit For
                End If
            Next

            If Flag = False Then
                objSBOAPI.SBO_Appln.StatusBar.SetText("Select Atleast 1 Matrix Record For Posting.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            End If

            Return True
        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("ValidatePosting Falied." + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            Return False
        End Try
    End Function

#End Region

#Region "Other Forms Methods"

    Public Sub LoadMatrixViaDataSource(ByVal objform As SAPbouiCOM.Form, ByVal FrmDate As String, ByVal ToDate As String)
        Try
            If PurchaseQuery.Trim() = "" Then
                LoadQuery()
            End If

            If IsNothing(objform) Then
                objform = objSBOAPI.SBO_Appln.Forms.ActiveForm
            End If
            If IsNothing(oMatrix) Then
                oMatrix = objform.Items.Item("Matrix").Specific
            End If
            Dim BAType As String = objform.Items.Item("U_BAType").Specific.Value
            If BAType = "S" Then
                strsql = Replace(Replace(SalesQuery, "%%FromDate%%", FrmDate), "%%ToDate%%", ToDate)
            ElseIf BAType = "P" Then
                strsql = Replace(Replace(PurchaseQuery, "%%FromDate%%", FrmDate), "%%ToDate%%", ToDate)
            End If

            oRS = objUtility.DoQuery(strsql)

            'write_log("--------------Executed Query : " + vbNewLine + strsql)


            If IsNothing(oMatrix) Then
                oMatrix = objform.Items.Item("Matrix").Specific
            End If
            If oRS.RecordCount > 100 Then
                Dim ithReturnValue As Integer
                ithReturnValue = objSBOAPI.SBO_Appln.MessageBox("Total record count is " + oRS.RecordCount.ToString() + ", this may take few minutes, On loading matrix you can do other works also. Do you want to continue ?", 1, "Continue", "Cancel", "")
                If ithReturnValue <> 1 Then
                    Exit Sub
                End If
            End If

            oMatrix.Clear()
            If Not IsNothing(oRS) Then
                If oRS.RecordCount > 0 Then
                    '-------------------------------------------------Form Freeze-------
                    objform = objSBOAPI.SBO_Appln.Forms.ActiveForm
                    objform.Freeze(True)

                    '-------------------------------------------------Init Matrix---------
                    If IsNothing(oMatrix) Then
                        oMatrix = objform.Items.Item("Matrix").Specific
                    End If

                    If BAType = "S" Then
                        InvoiceTableName = "INV"
                        MemoTableName = "RDR"
                        BpType = "C"
                        OrderType = "Sales Order"
                        BaseType = "17"
                    ElseIf BAType = "P" Then
                        InvoiceTableName = "PCH"
                        MemoTableName = "POR"
                        BpType = "S"
                        OrderType = "Purchase Order"
                        BaseType = "22"
                    Else
                        InvoiceTableName = ""
                        MemoTableName = ""
                        BpType = ""
                        OrderType = ""
                    End If

                    Dim oColumns As SAPbouiCOM.Columns = oMatrix.Columns
                    Dim oColumn As SAPbouiCOM.Column = oMatrix.Columns.Item("U_InvEnt")
                    Dim oLink As SAPbouiCOM.LinkedButton = oColumn.ExtendedObject

                    If BAType = "S" Then
                        oLink.LinkedObject = SAPbouiCOM.BoLinkedObject.lf_Invoice
                    ElseIf BAType = "P" Then
                        oLink.LinkedObject = SAPbouiCOM.BoLinkedObject.lf_PurchaseInvoice
                    End If
                    invEntries = ""
                    If objform.DataSources.DataTables.Count <= 0 Then
                        objform.DataSources.DataTables.Add("QL_BAA1")
                    End If
                    objform.DataSources.DataTables.Item("QL_BAA1").Clear()
                    Dim sSQL As String = strsql
                    objform.DataSources.DataTables.Item("QL_BAA1").ExecuteQuery(sSQL)

                    oMatrix = objform.Items.Item("Matrix").Specific
                    'oMatrix.Columns.Item("V_Line").DataBind.TableName
                    oMatrix.Columns.Item("V_Line").DataBind.Bind("QL_BAA1", "LineId")
                    oMatrix.Columns.Item("U_DocDate").DataBind.Bind("QL_BAA1", "U_DocDate")
                    oMatrix.Columns.Item("U_PRevTyp").DataBind.Bind("QL_BAA1", "U_PRevTyp")
                    oMatrix.Columns.Item("U_DocDate").DataBind.Bind("QL_BAA1", "U_DocDate")
                    oMatrix.Columns.Item("U_OldBAEn").DataBind.Bind("QL_BAA1", "U_OldBAEn")
                    oMatrix.Columns.Item("U_OldBANo").DataBind.Bind("QL_BAA1", "U_OldBANo")
                    oMatrix.Columns.Item("U_NewBAEn").DataBind.Bind("QL_BAA1", "U_NewBAEn")
                    oMatrix.Columns.Item("U_NewBANo").DataBind.Bind("QL_BAA1", "U_NewBANo")
                    oMatrix.Columns.Item("U_CardCode").DataBind.Bind("QL_BAA1", "U_CardCode")
                    oMatrix.Columns.Item("U_CustNum").DataBind.Bind("QL_BAA1", "U_CustNum")
                    oMatrix.Columns.Item("U_CardName").DataBind.Bind("QL_BAA1", "U_CardName")
                    oMatrix.Columns.Item("U_OldICode").DataBind.Bind("QL_BAA1", "U_OldICode")
                    oMatrix.Columns.Item("U_NewICode").DataBind.Bind("QL_BAA1", "U_NewICode")
                    oMatrix.Columns.Item("U_OldIName").DataBind.Bind("QL_BAA1", "U_OldIName")
                    oMatrix.Columns.Item("U_NewIName").DataBind.Bind("QL_BAA1", "U_NewIName")
                    oMatrix.Columns.Item("U_InvEnt").DataBind.Bind("QL_BAA1", "U_InvEnt")
                    oMatrix.Columns.Item("U_InvNum").DataBind.Bind("QL_BAA1", "U_InvNum")
                    oMatrix.Columns.Item("U_InvDate").DataBind.Bind("QL_BAA1", "U_InvDate")
                    oMatrix.Columns.Item("U_InvQty").DataBind.Bind("QL_BAA1", "U_InvQty")
                    oMatrix.Columns.Item("U_ITaxCode").DataBind.Bind("QL_BAA1", "U_ITaxCode")
                    oMatrix.Columns.Item("U_InvHSN").DataBind.Bind("QL_BAA1", "U_InvHSN")
                    oMatrix.Columns.Item("U_InvDisc").DataBind.Bind("QL_BAA1", "U_InvDisc")
                    oMatrix.Columns.Item("U_EffDate").DataBind.Bind("QL_BAA1", "U_EffDate")
                    oMatrix.Columns.Item("U_OldPrice").DataBind.Bind("QL_BAA1", "U_OldPrice")
                    oMatrix.Columns.Item("U_NewPrice").DataBind.Bind("QL_BAA1", "U_NewPrice")
                    oMatrix.Columns.Item("U_PriceDff").DataBind.Bind("QL_BAA1", "U_PriceDff")
                    oMatrix.Columns.Item("U_OldBALNo").DataBind.Bind("QL_BAA1", "U_OldBALNo")
                    oMatrix.Columns.Item("U_NewBALNo").DataBind.Bind("QL_BAA1", "U_NewBALNo")
                    oMatrix.Columns.Item("U_ILineNum").DataBind.Bind("QL_BAA1", "U_ILineNum")
                    oMatrix.Columns.Item("U_InvICode").DataBind.Bind("QL_BAA1", "U_InvICode")
                    oMatrix.Columns.Item("U_InvWCode").DataBind.Bind("QL_BAA1", "U_InvWCode")
                    oMatrix.Columns.Item("U_InvDepmt").DataBind.Bind("QL_BAA1", "U_InvDepmt")
                    oMatrix.LoadFromDataSource()

                    For i As Integer = 0 To oMatrix.RowCount - 1
                        invEntries += vbNewLine + " update " + InvoiceTableName + "1 set ""U_BAPosted"" = 'Y' where ""DocEntry"" = '" + oRS.Fields.Item("U_InvEnt").Value.ToString() + "' and ""LineNum"" = '" + oRS.Fields.Item("U_ILineNum").Value.ToString() + "'"
                    Next

                    objSBOAPI.SBO_Appln.StatusBar.SetText("Matrix Loaded", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)

                Else
                    objSBOAPI.SBO_Appln.StatusBar.SetText("No Records Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                End If
            End If



            'oMatrix.Columns.Item("V_Line").Cells.Item(i + 1).Specific.Value = (i + 1).ToString()
            'oMatrix.Columns.Item("U_DocDate").Cells.Item(i + 1).Specific.Value = DateTime.Now().ToString("dd/MM/yyyy")
            'oMatrix.Columns.Item("U_PRevTyp").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_PRevTyp").Value
            'oMatrix.Columns.Item("U_OldBAEn").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldBAEn").Value
            'oMatrix.Columns.Item("U_OldBANo").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldBANo").Value
            'oMatrix.Columns.Item("U_NewBAEn").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewBAEn").Value
            'oMatrix.Columns.Item("U_NewBANo").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewBANo").Value
            'oMatrix.Columns.Item("U_CardCode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_CardCode").Value
            'oMatrix.Columns.Item("U_CardName").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_CardName").Value
            'oMatrix.Columns.Item("U_OldICode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldICode").Value
            'oMatrix.Columns.Item("U_NewICode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewICode").Value
            'oMatrix.Columns.Item("U_OldIName").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldIName").Value
            'oMatrix.Columns.Item("U_NewIName").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewIName").Value
            'oMatrix.Columns.Item("U_InvEnt").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvEnt").Value
            'oMatrix.Columns.Item("U_InvNum").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvNum").Value
            'oMatrix.Columns.Item("U_InvDate").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvDate").Value
            'oMatrix.Columns.Item("U_InvQty").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvQty").Value
            'oMatrix.Columns.Item("U_ITaxCode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_ITaxCode").Value
            'oMatrix.Columns.Item("U_InvHSN").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvHSN").Value
            'oMatrix.Columns.Item("U_InvDisc").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvDisc").Value
            'oMatrix.Columns.Item("U_EffDate").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_EffDate").Value
            'oMatrix.Columns.Item("U_OldPrice").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldPrice").Value
            'oMatrix.Columns.Item("U_NewPrice").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewPrice").Value
            'oMatrix.Columns.Item("U_PriceDff").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_PriceDff").Value
            'oMatrix.Columns.Item("U_OldBALNo").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldBALNo").Value
            'oMatrix.Columns.Item("U_NewBALNo").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewBALNo").Value
            'oMatrix.Columns.Item("U_ILineNum").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_ILineNum").Value
            'oMatrix.Columns.Item("U_InvICode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvICode").Value
            'oMatrix.Columns.Item("U_InvWCode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvWCode").Value
            'oMatrix.Columns.Item("U_InvDepmt").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvDepmt").Value

            'oRS = objUtility.DoQuery(strsql)
        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("Load Matrix Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        Finally
            objform.Freeze(False)
        End Try
    End Sub

    Public Sub LoadMatrix(ByVal objform As SAPbouiCOM.Form, ByVal FrmDate As String, ByVal ToDate As String)
        Try
            If PurchaseQuery.Trim() = "" Then
                LoadQuery()
            End If

            If IsNothing(objform) Then
                objform = objSBOAPI.SBO_Appln.Forms.ActiveForm
            End If
            If IsNothing(oMatrix) Then
                oMatrix = objform.Items.Item("Matrix").Specific
            End If

            'write_log("--------------Matrix Loading-----------")
            Dim BAType As String = objform.Items.Item("U_BAType").Specific.Value

            '------------Load Record------------


            If BAType = "S" Then
                InvoiceTableName = "INV"
                MemoTableName = "RDR"
                BpType = "C"
                OrderType = "Sales Order"
                BaseType = "17"
            ElseIf BAType = "P" Then
                InvoiceTableName = "PCH"
                MemoTableName = "POR"
                BpType = "S"
                OrderType = "Purchase Order"
                BaseType = "22"
            Else
                InvoiceTableName = ""
                MemoTableName = ""
                BpType = ""
                OrderType = ""
            End If

            'If DataBaseType = "HANA" Then
            '    nulltype = "ifnull"
            'Else
            '    nulltype = "isnull"
            'End If

            'strsql = vbNewLine + " select"
            'strsql += vbNewLine + " 'Blanket Agreement' as ""U_PRevTyp"","
            'strsql += vbNewLine + " T0.""AbsID""  as ""U_OldBAEn"","
            'strsql += vbNewLine + " T0.""Number"" as ""U_OldBANo"","
            'strsql += vbNewLine + " T2.""AbsID"" as ""U_NewBAEn"","
            'strsql += vbNewLine + " T2.""Number"" as ""U_NewBANo"","
            'strsql += vbNewLine + " T0.""BpCode"" as ""U_CardCode"" ,"
            'strsql += vbNewLine + " (select ""CardName"" from OCRD where ""CardCode"" = T0.""BpCode"") as ""U_CardName"","
            'strsql += vbNewLine + " T1.""ItemCode"" as ""U_OldICode"","
            'strsql += vbNewLine + " T3.""ItemCode"" as ""U_NewICode"","
            'strsql += vbNewLine + " T1.""ItemName"" as ""U_OldIName"","
            'strsql += vbNewLine + " T3.""ItemName"" as ""U_NewIName"","
            'strsql += vbNewLine + " T1.""AgrLineNum""  as ""U_OldBALNo"","
            'strsql += vbNewLine + " T3.""AgrLineNum"" as ""U_NewBALNo"","
            'strsql += vbNewLine + " I1.""DocEntry"" as ""U_InvEnt"","
            'strsql += vbNewLine + " I0.""DocNum"" as ""U_InvNum"","
            'strsql += vbNewLine + " I0.""DocDate"" as ""U_InvDate"","
            'strsql += vbNewLine + " I1.""ItemCode"" as ""U_InvICode"","
            'strsql += vbNewLine + " I1.""WhsCode"" as ""U_InvWCode"","
            'strsql += vbNewLine + " I1.""OpenQty"" as ""U_InvQty"","
            'strsql += vbNewLine + " T0.""StartDate"" as ""U_EffDate"","
            'strsql += vbNewLine + " T1.""UnitPrice"" as ""U_OldPrice"","
            'strsql += vbNewLine + " T3.""UnitPrice"" as ""U_NewPrice"","
            'strsql += vbNewLine + " (T1.""UnitPrice"" - T3.""UnitPrice"") as ""U_PriceDff"","
            'strsql += vbNewLine + " I1.""LineNum""  as ""U_ILineNum"","
            'strsql += vbNewLine + " I1.""TaxCode"" as ""U_ITaxCode"","
            'strsql += vbNewLine + " I1.""HsnEntry""  as ""U_InvHSN"","
            'strsql += vbNewLine + " I1.""DiscPrcnt"" as ""U_InvDisc"","
            'strsql += vbNewLine + " I1.""OcrCode2"" as ""U_InvDepmt"""

            'strsql += vbNewLine + " from OOAT T0 "
            'strsql += vbNewLine + " inner join OAT1 T1 on T0.""AbsID"" = T1.""AgrNo"""
            'strsql += vbNewLine + " and T0.""BpType"" = '" + BpType + "'"
            'strsql += vbNewLine + " inner join OOAT T2 on " + nulltype + "(T2.""U_oldBlnkNum"",0) = T0.""Number"""
            'strsql += vbNewLine + " and T2.""BpType"" = '" + BpType + "'"
            'strsql += vbNewLine + " inner join OAT1 T3 on T2.""AbsID"" = T3.""AgrNo"""
            'strsql += vbNewLine + " inner join " + InvoiceTableName + "1 I1 on I1.""AgrNo"" = T0.""AbsID""  and I1.""AgrLnNum"" = T1.""AgrLineNum"""
            'strsql += vbNewLine + " inner join O" + InvoiceTableName + " I0 on I0.""DocEntry"" = I1.""DocEntry"""
            'strsql += vbNewLine + " where (T1.""UnitPrice"" - T3.""UnitPrice"") <> 0 and I0.""CANCELED"" <> 'Y' and " + nulltype + "(I1.""U_BaseEntry"",'')  = '' and " + nulltype + "(I1.""U_BAPosted"",'N')='N' "
            'strsql += vbNewLine + " and  T0.""BpType"" = '" + BpType + "' "
            'strsql += vbNewLine + " and T2.""StartDate""  Between '" + FrmDate + "' and '" + ToDate + "' "
            'strsql += vbNewLine + " "
            'strsql += vbNewLine + " union all"
            'strsql += vbNewLine + " "
            'strsql += vbNewLine + " select"
            'strsql += vbNewLine + " '" + OrderType + "' as ""U_PRevTyp"","
            'strsql += vbNewLine + " T0.""DocEntry"" as ""U_OldBAEn"", "
            'strsql += vbNewLine + " T0.""DocNum"" as ""U_OldBANo"","
            'strsql += vbNewLine + " T2.""DocEntry"" as ""U_NewBAEn"", "
            'strsql += vbNewLine + " T2.""DocNum"" as ""U_NewBANo"","
            'strsql += vbNewLine + " T0.""CardCode"" as ""U_CardCode"","
            'strsql += vbNewLine + " (select ""CardName"" from OCRD where ""CardCode"" = T0.""CardCode"") as ""U_CardName"", "
            'strsql += vbNewLine + " T1.""ItemCode"" as ""U_OldICode"","
            'strsql += vbNewLine + " T3.""ItemCode"" as ""U_NewICode"", "
            'strsql += vbNewLine + " T1.""Dscription"" as ""U_OldIName"","
            'strsql += vbNewLine + " T3.""Dscription"" as ""U_NewIName"", "
            'strsql += vbNewLine + " T1.""LineNum"" as ""U_OldBALNo"","
            'strsql += vbNewLine + " T3.""LineNum"" as ""U_NewBALNo"","
            'strsql += vbNewLine + " I0.""DocEntry""  as ""U_InvEnt"" ,"
            'strsql += vbNewLine + " I0.""DocNum"" as ""U_InvNum"", "
            'strsql += vbNewLine + " I0.""DocDate"" as ""U_InvDate"", "
            'strsql += vbNewLine + " I1.""ItemCode"" as ""U_InvICode"","
            'strsql += vbNewLine + " I1.""WhsCode""  as ""U_InvWCode"",  "
            'strsql += vbNewLine + " I1.""OpenQty"" as ""U_InvQty"","
            'strsql += vbNewLine + " T0.""DocDate"" as ""U_EffDate"","
            'strsql += vbNewLine + " T1.""Price"" as ""U_OldPrice"","
            'strsql += vbNewLine + " T3.""Price"" as ""U_NewPrice"","
            'strsql += vbNewLine + " (T1.""Price"" - T3.""Price"") as ""U_PriceDff"","
            'strsql += vbNewLine + " I1.""LineNum""  as ""U_ILineNum"","
            'strsql += vbNewLine + " I1.""TaxCode"" as ""U_ITaxCode"","
            'strsql += vbNewLine + " I1.""HsnEntry""  as ""U_InvHSN"","
            'strsql += vbNewLine + " I1.""DiscPrcnt"" as ""U_InvDisc"","
            'strsql += vbNewLine + " I1.""OcrCode2"" as ""U_InvDepmt"""

            'strsql += vbNewLine + " from O" + MemoTableName + " T0 "
            'strsql += vbNewLine + " inner join " + MemoTableName + "1 T1 on T0.""DocEntry""  = T1.""DocEntry"""
            'strsql += vbNewLine + " inner join O" + MemoTableName + " T2 on T2.""U_OldSoNum"" = T0.""DocNum"""
            'strsql += vbNewLine + " inner join " + MemoTableName + "1 T3 on T2.""DocEntry"" = T3.""DocEntry"""

            'If BAType = "S" Then
            '    strsql += vbNewLine + " inner join INV1 I1 on I1.""BaseEntry"" = T0.""DocEntry"" and I1.""BaseLine"" = T1.""LineNum"" and I1.""BaseType"" =  '17'"
            'ElseIf BAType = "P" Then
            '    strsql += vbNewLine + " inner join PDN1 P1 on P1.""BaseEntry"" = T0.""DocEntry"" and P1.""BaseLine"" = T1.""LineNum"" and P1.""BaseType"" =  '22'"
            '    strsql += vbNewLine + " inner join PCH1 I1 on I1.""BaseEntry"" = P1.""DocEntry"" and I1.""BaseLine"" = P1.""LineNum"" and I1.""BaseType"" = '20'"
            'End If
            'strsql += vbNewLine + " inner join O" + InvoiceTableName + " I0 on I0.""DocEntry"" = I1.""DocEntry"""
            'strsql += vbNewLine + " where (T1.""Price"" - T3.""Price"") <> 0 and I0.""CANCELED"" <> 'Y' and " + nulltype + "(I1.""U_BaseEntry"",'')  = '' and  " + nulltype + "(I1.""U_BAPosted"",'N')='N' and"

            'If BAType = "S" Then
            '    strsql += vbNewLine + " T3.""U_EffDate"" Between '" + FrmDate + "' and '" + FrmDate + "'"
            'ElseIf BAType = "P" Then
            '    strsql += vbNewLine + " T2.""U_EffDate"" Between '" + FrmDate + "' and '" + ToDate + "'"
            'End If

            Dim CFLCardCode As String = objform.Items.Item("U_BPCode").Specific.Value

            If BAType = "S" Then
                strsql = Replace(Replace(Replace(SalesQuery, "%%FromDate%%", FrmDate), "%%ToDate%%", ToDate), "%%CardCode%%", CFLCardCode)
            ElseIf BAType = "P" Then
                strsql = Replace(Replace(Replace(PurchaseQuery, "%%FromDate%%", FrmDate), "%%ToDate%%", ToDate), "%%CardCode%%", CFLCardCode)
            End If
            write_log(strsql)
            oRS = objUtility.DoQuery(strsql)

            'write_log("--------------Executed Query : " + vbNewLine + strsql)


            If IsNothing(oMatrix) Then
                oMatrix = objform.Items.Item("Matrix").Specific
            End If
            
            If Not IsNothing(oRS) Then

                If oRS.RecordCount > 100 Then
                    Dim ithReturnValue As Integer
                    ithReturnValue = objSBOAPI.SBO_Appln.MessageBox("Total record count is " + oRS.RecordCount.ToString() + " , this may take few minutes, On loading matrix you can do other work also. Do you want to continue ?", 1, "Continue", "Cancel", "")
                    If ithReturnValue <> 1 Then
                        Exit Sub
                    End If
                End If

                oMatrix.Clear()

                If oRS.RecordCount > 0 Then
                    '-------------------------------------------------Form Freeze-------
                    objform = objSBOAPI.SBO_Appln.Forms.ActiveForm
                    objform.Freeze(True)

                    '-------------------------------------------------Init Matrix---------
                    If IsNothing(oMatrix) Then
                        oMatrix = objform.Items.Item("Matrix").Specific
                    End If

                    Dim oColumns As SAPbouiCOM.Columns = oMatrix.Columns
                    Dim oColumn As SAPbouiCOM.Column = oMatrix.Columns.Item("U_InvEnt")
                    Dim oLink As SAPbouiCOM.LinkedButton = oColumn.ExtendedObject

                    If BAType = "S" Then
                        oLink.LinkedObject = SAPbouiCOM.BoLinkedObject.lf_Invoice
                    ElseIf BAType = "P" Then
                        oLink.LinkedObject = SAPbouiCOM.BoLinkedObject.lf_PurchaseInvoice
                    End If
                    invEntries = ""
                    'Dim thread As New Thread(
                    'Sub()

                    For i As Integer = 0 To oRS.RecordCount - 1
                        oMatrix.AddRow()
                        oMatrix.Columns.Item("V_Line").Cells.Item(i + 1).Specific.Value = (i + 1).ToString()
                        oMatrix.Columns.Item("U_DocDate").Cells.Item(i + 1).Specific.Value = DateTime.Now().ToString("dd/MM/yyyy")
                        oMatrix.Columns.Item("U_PRevTyp").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_PRevTyp").Value
                        oMatrix.Columns.Item("U_OldBAEn").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldBAEn").Value
                        oMatrix.Columns.Item("U_OldBANo").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldBANo").Value
                        oMatrix.Columns.Item("U_NewBAEn").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewBAEn").Value
                        oMatrix.Columns.Item("U_NewBANo").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewBANo").Value
                        oMatrix.Columns.Item("U_CardCode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_CardCode").Value
                        oMatrix.Columns.Item("U_CustNum").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_CustNum").Value
                        oMatrix.Columns.Item("U_CardName").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_CardName").Value
                        oMatrix.Columns.Item("U_OldICode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldICode").Value
                        oMatrix.Columns.Item("U_NewICode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewICode").Value
                        oMatrix.Columns.Item("U_OldIName").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldIName").Value
                        oMatrix.Columns.Item("U_NewIName").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewIName").Value
                        oMatrix.Columns.Item("U_InvEnt").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvEnt").Value
                        oMatrix.Columns.Item("U_InvNum").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvNum").Value
                        oMatrix.Columns.Item("U_InvDate").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvDate").Value
                        oMatrix.Columns.Item("U_InvQty").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvQty").Value
                        oMatrix.Columns.Item("U_ITaxCode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_ITaxCode").Value
                        oMatrix.Columns.Item("U_InvHSN").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvHSN").Value
                        oMatrix.Columns.Item("U_InvDisc").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvDisc").Value
                        oMatrix.Columns.Item("U_EffDate").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_EffDate").Value
                        oMatrix.Columns.Item("U_OldPrice").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldPrice").Value
                        oMatrix.Columns.Item("U_NewPrice").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewPrice").Value
                        oMatrix.Columns.Item("U_PriceDff").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_PriceDff").Value
                        oMatrix.Columns.Item("U_OldBALNo").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_OldBALNo").Value
                        oMatrix.Columns.Item("U_NewBALNo").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_NewBALNo").Value
                        oMatrix.Columns.Item("U_ILineNum").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_ILineNum").Value
                        oMatrix.Columns.Item("U_InvICode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvICode").Value
                        oMatrix.Columns.Item("U_InvWCode").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvWCode").Value
                        oMatrix.Columns.Item("U_InvDepmt").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvDepmt").Value
                        oMatrix.Columns.Item("U_InvDTot").Cells.Item(i + 1).Specific.Value = oRS.Fields.Item("U_InvDTot").Value
                        'If Not (oRS.Fields.Item("U_PRevTyp").Value.ToString().Contains("Credit")) Then
                        '    invEntries += vbNewLine + " update " + InvoiceTableName + "1 set ""U_BAPosted"" = 'Y' where ""DocEntry"" = '" + oRS.Fields.Item("U_InvEnt").Value.ToString() + "' and ""LineNum"" = '" + oRS.Fields.Item("U_ILineNum").Value.ToString() + "'"
                        'End If
                        oRS.MoveNext()
                    Next
                    'End Sub
                    ')
                    'thread.Start()
                    objSBOAPI.SBO_Appln.StatusBar.SetText("Matrix Loaded", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)

                Else
                    objSBOAPI.SBO_Appln.StatusBar.SetText("No Records Found.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                End If
            End If


        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("Load Matrix Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        Finally
            objform.Freeze(False)
        End Try
    End Sub

#Region "Posting CardCode By CardCode"
    Function PostARCreditNote(ByVal dtTable As DataTable)
        Dim oCreditNotes As SAPbobsCOM.Documents = objSBOAPI.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes)
        Try

            Dim ThisFlag As Boolean = False

            Dim NewRecord, NewDocNum, strQuery As String
            strQuery = ""
            Dim LineNums(dtTable.Rows.Count - 1) As String

            '-------Header Data------
            oCreditNotes.CardCode = dtTable(0)("CardCode").ToString()
            oCreditNotes.DocDate = dtTable(0)("DocDate")
            oCreditNotes.Series = Val(oCRNSeries)
            oCreditNotes.OriginalRefNo = dtTable(0)("BaseNum").ToString()
            oCreditNotes.OriginalRefDate = dtTable(0)("DocDate")

            write_log("<HeadData>" + vbNewLine + "CardCode" + dtTable(0)("CardCode").ToString() + ", DocDate/OriginalRefDate : " + dtTable(0)("DocDate") + ", OriginalRefNo" + dtTable(0)("BaseNum").ToString() + ", Series :" + oCRNSeries + vbNewLine + "<LineItems>")
            Dim LineDataLog As String = ""
            '-------Line Data------
            Dim i As Integer
            For i = 0 To dtTable.Rows.Count - 1
                LineNums(i) = dtTable(i)("LineNum").ToString()
                oCreditNotes.Lines.ItemCode = dtTable(i)("ItemCode").ToString()
                oCreditNotes.Lines.Quantity = Val(dtTable(i)("Qty").ToString())
                oCreditNotes.Lines.WarehouseCode = dtTable(i)("WhsCode").ToString()
                oCreditNotes.Lines.Price = Val(dtTable(i)("DiffPrice").ToString())
                oCreditNotes.Lines.TaxCode = dtTable(i)("TaxCode").ToString()
                oCreditNotes.Lines.HSNEntry = Val(dtTable(i)("HSNEntry").ToString())
                oCreditNotes.Lines.CostingCode2 = dtTable(i)("CostingCode2").ToString()

                oCreditNotes.Lines.WithoutInventoryMovement = SAPbobsCOM.BoYesNoEnum.tYES
                oCreditNotes.Lines.UserFields.Fields.Item("U_BaseType").Value = "ARInvoice"
                oCreditNotes.Lines.UserFields.Fields.Item("U_BaseEntry").Value = dtTable(i)("BaseEntry").ToString()
                oCreditNotes.Lines.UserFields.Fields.Item("U_BaseLine").Value = dtTable(i)("BaseLine").ToString()
                oCreditNotes.Lines.UserFields.Fields.Item("U_BaseNum").Value = dtTable(i)("BaseNum").ToString()
                'oCreditNotes.Lines.BaseLine = dtTable("BaseLine").ToString().Trim()
                'oCreditNotes.Lines.BaseEntry = dtTable("BaseEntry").ToString().Trim()
                'oCreditNotes.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oInvoices
                'oCreditNote.Lines.ActualBaseEntry = 391 'DocEntry of Delivery Document
                'oCreditNote.Lines.ActualBaseLine = 0 'Line Number from the Delivery Document Lines
                oCreditNotes.Lines.Add()

                LineDataLog += " LineNum : " + dtTable(i)("LineNum").ToString() + " ItemCode : " + dtTable(i)("ItemCode").ToString()
                LineDataLog += " Qty : " + dtTable(i)("Qty").ToString() + " WarehouseCode : " + dtTable(i)("WhsCode").ToString()
                LineDataLog += " TaxCode : " + dtTable(i)("TaxCode").ToString() + " Price : " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString()
                LineDataLog += " HSNEntry : " + dtTable(i)("HSNEntry").ToString() + " CostingCode2 : " + dtTable(i)("CostingCode2").ToString()
                LineDataLog += " U_BaseType : APInvoice" + " U_BaseEntry : " + dtTable(i)("BaseEntry").ToString()
                LineDataLog += " U_BaseLine : " + dtTable(i)("BaseLine").ToString() + " U_BaseNum : " + dtTable(i)("BaseNum").ToString()
                LineDataLog += vbNewLine + vbNewLine

                strQuery += vbNewLine + "Update ""@QL_BAA1"" set ""U_PDocType"" = 'AR_CRN',""U_PDocNum"" ='%NewDocNum%',""U_PDocEnt""='%NewRecord%',""U_ARCRNEnt""='%NewRecord%',""U_PLinNum""='" + dtTable(i)("BaseLine").ToString().Trim() + "' where ""DocEntry"" = '" + objForm.Items.Item("U_DocEntry").Specific.Value.ToString() + "' and ""LineId"" = '" + dtTable(i)("LineNum").ToString() + "'"
                If dtTable(i)("PostType").ToString().Contains("Credit") Then
                    strQuery += vbNewLine + "Update ""RIN1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AR_CRN', ""U_BaseNum""='%NewDocNum%', ""U_BaseEntry""='%NewRecord%', ""U_BaseLine"" = '" + dtTable(i)("LineNum").ToString() + "',""U_CRNPrice"" = isnull(""U_CRNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + "  where ""DocEntry"" = '" + dtTable(i)("BaseEntry").ToString().Trim() + "' and ""LineNum"" = '" + dtTable(i)("BaseLine").ToString().Trim() + "'"
                Else
                    strQuery += vbNewLine + "Update ""INV1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AR_CRN', ""U_BaseNum""='%NewDocNum%', ""U_BaseEntry""='%NewRecord%', ""U_BaseLine"" = '" + dtTable(i)("LineNum").ToString() + "',""U_CRNPrice"" = isnull(""U_CRNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + "  where ""DocEntry"" = '" + dtTable(i)("BaseEntry").ToString().Trim() + "' and ""LineNum"" = '" + dtTable(i)("BaseLine").ToString().Trim() + "'"
                End If

                'strQuery += vbNewLine + "Update ""INV1"" set ""U_CRNPrice"" = isnull(""U_CRNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + " where DocEntry = " + dtTable(i)("BaseEntry").ToString()
                ThisFlag = True
            Next

            write_log(LineDataLog)

            Dim ErrCode = oCreditNotes.Add()

            Dim LineNum1 = String.Join(", ", LineNums)
            If ThisFlag Then
                If ErrCode <> 0 Then
                    objSBOAPI.SBO_Appln.StatusBar.SetText("AR Credit Note For Line Number " + LineNum1 + " Posting Error : " & objSBOAPI.oCompany.GetLastErrorDescription)
                    SuccessMessage = "ERROR"
                Else
                    SuccessMessage = "SUCCESS"
                    NewRecord = "" : objSBOAPI.oCompany.GetNewObjectCode(NewRecord)
                    NewDocNum = objUtility.getSingleValue("Select ""DocNum""  from  ORIN where ""DocEntry"" = '" & NewRecord & "'")

                    strQuery = Replace(Replace(strQuery, "%NewDocNum%", NewDocNum), "%NewRecord%", NewRecord)
                    objUtility.DoQuery(strQuery)

                    'For Each LineNum In LineNums
                    '    Dim LineNo As Integer = Val(LineNum)
                    '    oMatrix.Columns.Item("U_PDocType").Cells.Item(LineNo).Specific.Value = "AR_CRN"
                    '    oMatrix.Columns.Item("U_PDocNum").Cells.Item(LineNo).Specific.Value = NewRecord
                    '    oMatrix.Columns.Item("U_PDocEnt").Cells.Item(LineNo).Specific.Value = NewDocNum
                    'Next

                End If
            End If


            Return True
        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("PostARCreditNote Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None)
            Return False
        Finally
            oCreditNotes = Nothing
            GC.Collect()
        End Try
    End Function

    Function PostARDebitNote(ByVal dtTable As DataTable)
        Dim oInvoices As SAPbobsCOM.Documents = objSBOAPI.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
        Try
            Dim ThisFlag As Boolean = False

            Dim NewRecord, NewDocNum, strQuery As String
            Dim LineNums(dtTable.Rows.Count - 1) As String
            strQuery = ""
            '-------Header Data------
            oInvoices.CardCode = dtTable(0)("CardCode").ToString()
            oInvoices.DocDate = dtTable(0)("DocDate")
            oInvoices.OriginalRefNo = dtTable(0)("BaseNum").ToString()
            oInvoices.OriginalRefDate = dtTable(0)("DocDate")
            oInvoices.GSTTransactionType = SAPbobsCOM.GSTTransactionTypeEnum.gsttrantyp_GSTDebitMemo
            oInvoices.Series = Val(oDBNSeries)

            write_log("<HeadData>" + vbNewLine + "CardCode" + dtTable(0)("CardCode").ToString() + ", DocDate/OriginalRefDate : " + dtTable(0)("DocDate") + ", OriginalRefNo" + dtTable(0)("BaseNum").ToString() + ", Series :" + oDBNSeries + vbNewLine + "<LineItems>")
            Dim LineDataLog As String = ""

            '-------Line Data------
            Dim i As Integer
            For i = 0 To dtTable.Rows.Count - 1
                LineNums(i) = dtTable(i)("LineNum").ToString()
                oInvoices.Lines.ItemCode = dtTable(i)("ItemCode").ToString()
                oInvoices.Lines.Quantity = Val(dtTable(i)("Qty").ToString())
                oInvoices.Lines.WarehouseCode = dtTable(i)("WhsCode").ToString()
                oInvoices.Lines.Price = Val(dtTable(i)("DiffPrice").ToString()) * -1
                oInvoices.Lines.TaxCode = dtTable(i)("TaxCode").ToString()
                oInvoices.Lines.HSNEntry = Val(dtTable(i)("HSNEntry").ToString())
                oInvoices.Lines.CostingCode2 = dtTable(i)("CostingCode2").ToString()

                oInvoices.Lines.WithoutInventoryMovement = SAPbobsCOM.BoYesNoEnum.tYES
                oInvoices.Lines.UserFields.Fields.Item("U_BaseType").Value = "ARInvoice"
                oInvoices.Lines.UserFields.Fields.Item("U_BaseEntry").Value = dtTable(i)("BaseEntry").ToString()
                oInvoices.Lines.UserFields.Fields.Item("U_BaseLine").Value = dtTable(i)("BaseLine").ToString()
                oInvoices.Lines.UserFields.Fields.Item("U_BaseNum").Value = dtTable(i)("BaseNum").ToString()
                'oInvoices.Lines.BaseLine = dtTable(i)("BaseLine").ToString().Trim()
                'oInvoices.Lines.BaseEntry = dtTable(i)("BaseEntry").ToString().Trim()
                'oInvoices.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oOrders
                'oInvoices.Lines.TaxCode = dtRow("").ToString().Trim()
                'oInvoices.Lines.Quantity = dtTable(i)("Qty").ToString().Trim()
                'oInvoices.Lines.ActualBaseEntry = 391 'DocEntry of Delivery Document
                'oInvoices.Lines.ActualBaseLine = 0 'Line Number from the Delivery Document Lines
                oInvoices.Lines.Add()

                LineDataLog += " LineNum : " + dtTable(i)("LineNum").ToString() + " ItemCode : " + dtTable(i)("ItemCode").ToString()
                LineDataLog += " Qty : " + dtTable(i)("Qty").ToString() + " WarehouseCode : " + dtTable(i)("WhsCode").ToString()
                LineDataLog += " TaxCode : " + dtTable(i)("TaxCode").ToString() + " Price : " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString()
                LineDataLog += " HSNEntry : " + dtTable(i)("HSNEntry").ToString() + " CostingCode2 : " + dtTable(i)("CostingCode2").ToString()
                LineDataLog += " U_BaseType : APInvoice" + " U_BaseEntry : " + dtTable(i)("BaseEntry").ToString()
                LineDataLog += " U_BaseLine : " + dtTable(i)("BaseLine").ToString() + " U_BaseNum : " + dtTable(i)("BaseNum").ToString()
                LineDataLog += vbNewLine + vbNewLine

                strQuery += vbNewLine + "Update ""@QL_BAA1"" set ""U_PDocType"" = 'AR_DBN',""U_PDocNum"" ='%NewDocNum%',""U_PDocEnt""='%NewRecord%',""U_ARDBNEnt""='%NewRecord%',""U_PLinNum""='" + dtTable(i)("BaseLine").ToString().Trim() + "' where ""DocEntry"" = '" + objForm.Items.Item("U_DocEntry").Specific.Value.ToString() + "' and ""LineId"" = '" + dtTable(i)("LineNum").ToString() + "'"
                If dtTable(i)("PostType").ToString().Contains("Credit") Then
                    strQuery += vbNewLine + "Update ""RIN1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AR_DBN', ""U_BaseNum""='%NewDocNum%', ""U_BaseEntry""='%NewRecord%', ""U_BaseLine"" = '" + dtTable(i)("LineNum").ToString() + "',""U_DBNPrice"" = isnull(""U_DBNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + "  where ""DocEntry"" = '" + dtTable(i)("BaseEntry").ToString().Trim() + "' and ""LineNum"" = '" + dtTable(i)("BaseLine").ToString().Trim() + "'"
                Else
                    strQuery += vbNewLine + "Update ""INV1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AR_DBN', ""U_BaseNum""='%NewDocNum%', ""U_BaseEntry""='%NewRecord%', ""U_BaseLine"" = '" + dtTable(i)("LineNum").ToString() + "',""U_DBNPrice"" = isnull(""U_DBNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + "  where ""DocEntry"" = '" + dtTable(i)("BaseEntry").ToString().Trim() + "' and ""LineNum"" = '" + dtTable(i)("BaseLine").ToString().Trim() + "'"
                End If

                'strQuery += vbNewLine + "Update ""INV1"" set ""U_DBNPrice"" = isnull(""U_DBNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + " where ""DocEntry"" = " + dtTable(i)("BaseEntry").ToString()

                ThisFlag = True

            Next

            write_log(LineDataLog)

            Dim ErrCode = oInvoices.Add()
            Dim LineNum1 = String.Join(", ", LineNums)
            If ThisFlag Then
                If ErrCode <> 0 And ThisFlag Then
                    objSBOAPI.SBO_Appln.StatusBar.SetText("AR Debit Note For Line Number " + LineNum1 + " Posting Error : " & objSBOAPI.oCompany.GetLastErrorDescription)
                    SuccessMessage = "ERROR"
                Else
                    SuccessMessage = "SUCCESS"
                    NewRecord = "" : objSBOAPI.oCompany.GetNewObjectCode(NewRecord)
                    NewDocNum = objUtility.getSingleValue("Select ""DocNum""  from  OINV where ""DocEntry"" = '" & NewRecord & "'")

                    strQuery = Replace(Replace(strQuery, "%NewDocNum%", NewDocNum), "%NewRecord%", NewRecord)
                    objUtility.DoQuery(strQuery)

                    'For Each LineNum In LineNums
                    '    Dim LineNo As Integer = Val(LineNum)
                    '    oMatrix.Columns.Item("U_PDocType").Cells.Item(LineNo).Specific.Value = "AR_DBN"
                    '    oMatrix.Columns.Item("U_PDocNum").Cells.Item(LineNo).Specific.Value = NewRecord
                    '    oMatrix.Columns.Item("U_PDocEnt").Cells.Item(LineNo).Specific.Value = NewDocNum
                    'Next

                End If
            End If


            Return True
        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("PostARDebitNote Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None)
            Return False
        Finally
            oInvoices = Nothing
            GC.Collect()
        End Try
    End Function

    Function PostAPCreditNote(ByVal dtTable As DataTable)
        write_log("*****************PostAPCreditNote*****************")
        Dim oPurchaseCreditNotes As SAPbobsCOM.Documents = objSBOAPI.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseCreditNotes)
        Try

            Dim ThisFlag As Boolean = False

            Dim NewRecord, NewDocNum, strQuery As String
            strQuery = ""
            Dim LineNums(dtTable.Rows.Count - 1) As String

            '-------Header Data------
            oPurchaseCreditNotes.CardCode = dtTable(0)("CardCode").ToString()
            oPurchaseCreditNotes.DocDate = dtTable(0)("DocDate")
            oPurchaseCreditNotes.OriginalRefNo = dtTable(0)("BaseNum").ToString()
            oPurchaseCreditNotes.OriginalRefDate = dtTable(0)("DocDate")
            oPurchaseCreditNotes.Series = Val(oPurchaseCRNSeries)

            write_log("<HeadData>" + vbNewLine + "CardCode" + dtTable(0)("CardCode").ToString() + ", DocDate/OriginalRefDate : " + dtTable(0)("DocDate") + ", OriginalRefNo" + dtTable(0)("BaseNum").ToString() + ", Series :" + oPurchaseCRNSeries + vbNewLine + "<LineItems>")
            Dim LineDataLog As String = ""
            '-------Line Data------
            Dim i As Integer
            For i = 0 To dtTable.Rows.Count - 1
                LineNums(i) = dtTable(i)("LineNum").ToString()
                oPurchaseCreditNotes.Lines.ItemCode = dtTable(i)("ItemCode").ToString()
                oPurchaseCreditNotes.Lines.Quantity = Val(dtTable(i)("Qty").ToString())
                oPurchaseCreditNotes.Lines.WarehouseCode = dtTable(i)("WhsCode").ToString()
                oPurchaseCreditNotes.Lines.Price = Val(dtTable(i)("DiffPrice").ToString())
                oPurchaseCreditNotes.Lines.TaxCode = dtTable(i)("TaxCode").ToString()
                oPurchaseCreditNotes.Lines.HSNEntry = Val(dtTable(i)("HSNEntry").ToString())
                oPurchaseCreditNotes.Lines.CostingCode2 = dtTable(i)("CostingCode2").ToString()

                oPurchaseCreditNotes.Lines.WithoutInventoryMovement = SAPbobsCOM.BoYesNoEnum.tYES
                oPurchaseCreditNotes.Lines.UserFields.Fields.Item("U_BaseType").Value = "APInvoice"
                oPurchaseCreditNotes.Lines.UserFields.Fields.Item("U_BaseEntry").Value = dtTable(i)("BaseEntry").ToString()
                oPurchaseCreditNotes.Lines.UserFields.Fields.Item("U_BaseLine").Value = dtTable(i)("BaseLine").ToString()
                oPurchaseCreditNotes.Lines.UserFields.Fields.Item("U_BaseNum").Value = dtTable(i)("BaseNum").ToString()

                oPurchaseCreditNotes.Lines.Add()

                LineDataLog += " LineNum : " + dtTable(i)("LineNum").ToString() + " ItemCode : " + dtTable(i)("ItemCode").ToString()
                LineDataLog += " Qty : " + dtTable(i)("Qty").ToString() + " WarehouseCode : " + dtTable(i)("WhsCode").ToString()
                LineDataLog += " TaxCode : " + dtTable(i)("TaxCode").ToString() + " Price : " + dtTable(i)("DiffPrice").ToString()
                LineDataLog += " HSNEntry : " + dtTable(i)("HSNEntry").ToString() + " CostingCode2 : " + dtTable(i)("CostingCode2").ToString()
                LineDataLog += " U_BaseType : APInvoice" + " U_BaseEntry : " + dtTable(i)("BaseEntry").ToString()
                LineDataLog += " U_BaseLine : " + dtTable(i)("BaseLine").ToString() + " U_BaseNum : " + dtTable(i)("BaseNum").ToString()
                LineDataLog += vbNewLine + vbNewLine

                strQuery += vbNewLine + "Update ""@QL_BAA1"" set ""U_PDocType"" = 'AP_CRN',""U_PDocNum"" ='%NewDocNum%',""U_PDocEnt""='%NewRecord%',""U_APCRNEnt""='%NewRecord%',""U_PLinNum""='" + dtTable(i)("BaseLine").ToString().Trim() + "' where ""DocEntry"" = '" + objForm.Items.Item("U_DocEntry").Specific.Value.ToString() + "' and ""LineId"" = '" + dtTable(i)("LineNum").ToString() + "'"
                If dtTable(i)("PostType").ToString().Contains("Credit") Then
                    strQuery += vbNewLine + "Update ""RPC1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AP_CRN', ""U_BaseNum""='%NewDocNum%', ""U_BaseEntry""='%NewRecord%', ""U_BaseLine"" = '" + dtTable(i)("LineNum").ToString() + "',""U_CRNPrice"" = isnull(""U_CRNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + "  where ""DocEntry"" = '" + dtTable(i)("BaseEntry").ToString().Trim() + "' and ""LineNum"" = '" + dtTable(i)("BaseLine").ToString().Trim() + "'"
                Else
                    strQuery += vbNewLine + "Update ""PCH1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AP_CRN', ""U_BaseNum""='%NewDocNum%', ""U_BaseEntry""='%NewRecord%', ""U_BaseLine"" = '" + dtTable(i)("LineNum").ToString() + "',""U_CRNPrice"" = isnull(""U_CRNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + "  where ""DocEntry"" = '" + dtTable(i)("BaseEntry").ToString().Trim() + "' and ""LineNum"" = '" + dtTable(i)("BaseLine").ToString().Trim() + "'"
                End If
               
                'strQuery += vbNewLine + "Update ""PCH1"" set ""U_CRNPrice"" = isnull(""U_CRNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + " where DocEntry = " + dtTable(i)("BaseEntry").ToString()

                ThisFlag = True
            Next

            write_log(LineDataLog)

            Dim ErrCode = oPurchaseCreditNotes.Add()
            Dim LineNum1 = String.Join(", ", LineNums)

            If ThisFlag Then
                If ErrCode <> 0 Then
                    objSBOAPI.SBO_Appln.StatusBar.SetText("AP Credit Note For Line Number " + LineNum1 + "  Posting Error : " & objSBOAPI.oCompany.GetLastErrorDescription)
                    SuccessMessage = "ERROR"
                Else
                    SuccessMessage = "SUCCESS"
                    NewRecord = "" : objSBOAPI.oCompany.GetNewObjectCode(NewRecord)
                    NewDocNum = objUtility.getSingleValue("Select ""DocNum""  from  ORPC where ""DocEntry"" = '" & NewRecord & "'")

                    strQuery = Replace(Replace(strQuery, "%NewDocNum%", NewDocNum), "%NewRecord%", NewRecord)
                    objUtility.DoQuery(strQuery)

                    'For Each LineNum In LineNums
                    '    Dim LineNo As Integer = Val(LineNum)
                    '    oMatrix.Columns.Item("U_PDocType").Cells.Item(LineNo).Specific.Value = "AP_CRN"
                    '    oMatrix.Columns.Item("U_PDocNum").Cells.Item(LineNo).Specific.Value = NewRecord
                    '    oMatrix.Columns.Item("U_PDocEnt").Cells.Item(LineNo).Specific.Value = NewDocNum
                    'Next
                End If
            End If


            Return True
        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("PostAPCreditNote Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None)
            Return False
        Finally
            oPurchaseCreditNotes = Nothing
            GC.Collect()
        End Try
    End Function

    Function PostAPDebitNote(ByVal dtTable As DataTable)
        write_log("*****************PostAPDebitNote*****************")
        Dim oPurchaseInvoices As SAPbobsCOM.Documents = objSBOAPI.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices)
        Try

            Dim ThisFlag As Boolean = False

            Dim NewRecord, NewDocNum, strQuery As String
            Dim LineNums(dtTable.Rows.Count - 1) As String
            strQuery = ""

            '-------Header Data------
            oPurchaseInvoices.CardCode = dtTable(0)("CardCode").ToString()
            oPurchaseInvoices.DocDate = dtTable(0)("DocDate")
            oPurchaseInvoices.OriginalRefNo = dtTable(0)("BaseNum").ToString()
            oPurchaseInvoices.OriginalRefDate = dtTable(0)("DocDate")
            oPurchaseInvoices.GSTTransactionType = SAPbobsCOM.GSTTransactionTypeEnum.gsttrantyp_GSTDebitMemo
            oPurchaseInvoices.Series = Val(oPurchaseDBNSeries)

            write_log("<HeadData>" + vbNewLine + "CardCode" + dtTable(0)("CardCode").ToString() + ", DocDate/OriginalRefDate : " + dtTable(0)("DocDate") + ", OriginalRefNo" + dtTable(0)("BaseNum").ToString() + ", Series :" + oPurchaseDBNSeries + vbNewLine + "<LineItems>")
            Dim LineDataLog As String = ""
            '-------Line Data------
            Dim i As Integer
            For i = 0 To dtTable.Rows.Count - 1
                LineNums(i) = dtTable(i)("LineNum").ToString()
                oPurchaseInvoices.Lines.ItemCode = dtTable(i)("ItemCode").ToString()
                oPurchaseInvoices.Lines.Quantity = Val(dtTable(i)("Qty").ToString())
                oPurchaseInvoices.Lines.WarehouseCode = dtTable(i)("WhsCode").ToString()
                oPurchaseInvoices.Lines.TaxCode = dtTable(i)("TaxCode").ToString()
                oPurchaseInvoices.Lines.Price = Val(dtTable(i)("DiffPrice").ToString()) * -1
                oPurchaseInvoices.Lines.HSNEntry = Val(dtTable(i)("HSNEntry").ToString())
                oPurchaseInvoices.Lines.CostingCode2 = dtTable(i)("CostingCode2").ToString()

                oPurchaseInvoices.Lines.WithoutInventoryMovement = SAPbobsCOM.BoYesNoEnum.tYES
                oPurchaseInvoices.Lines.UserFields.Fields.Item("U_BaseType").Value = "APInvoice"
                oPurchaseInvoices.Lines.UserFields.Fields.Item("U_BaseEntry").Value = dtTable(i)("BaseEntry").ToString()
                oPurchaseInvoices.Lines.UserFields.Fields.Item("U_BaseLine").Value = dtTable(i)("BaseLine").ToString()
                oPurchaseInvoices.Lines.UserFields.Fields.Item("U_BaseNum").Value = dtTable(i)("BaseNum").ToString()
                oPurchaseInvoices.Lines.Add()

                LineDataLog += " LineNum : " + dtTable(i)("LineNum").ToString() + " ItemCode : " + dtTable(i)("ItemCode").ToString()
                LineDataLog += " Qty : " + dtTable(i)("Qty").ToString() + " WarehouseCode : " + dtTable(i)("WhsCode").ToString()
                LineDataLog += " TaxCode : " + dtTable(i)("TaxCode").ToString() + " Price : " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString()
                LineDataLog += " HSNEntry : " + dtTable(i)("HSNEntry").ToString() + " CostingCode2 : " + dtTable(i)("CostingCode2").ToString()
                LineDataLog += " U_BaseType : APInvoice" + " U_BaseEntry : " + dtTable(i)("BaseEntry").ToString()
                LineDataLog += " U_BaseLine : " + dtTable(i)("BaseLine").ToString() + " U_BaseNum : " + dtTable(i)("BaseNum").ToString()
                LineDataLog += vbNewLine + vbNewLine

                strQuery += vbNewLine + "Update ""@QL_BAA1"" set ""U_PDocType"" = 'AP_DBN',""U_PDocNum"" ='%NewDocNum%',""U_PDocEnt""='%NewRecord%',""U_APDBNEnt""='%NewRecord%',""U_PLinNum""='" + dtTable(i)("BaseLine").ToString().Trim() + "' where ""DocEntry"" = '" + objForm.Items.Item("U_DocEntry").Specific.Value.ToString() + "' and ""LineId"" = '" + dtTable(i)("LineNum").ToString() + "'"
                If dtTable(i)("PostType").ToString().Contains("Credit") Then
                    strQuery += vbNewLine + "Update ""RPC1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AP_DBN', ""U_BaseNum""='%NewDocNum%', ""U_BaseEntry""='%NewRecord%', ""U_BaseLine"" = '" + dtTable(i)("LineNum").ToString() + "',""U_DBNPrice"" = isnull(""U_DBNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + "  where ""DocEntry"" = '" + dtTable(i)("BaseEntry").ToString().Trim() + "' and ""LineNum"" = '" + dtTable(i)("BaseLine").ToString().Trim() + "'"
                Else
                    strQuery += vbNewLine + "Update ""PCH1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AP_DBN', ""U_BaseNum""='%NewDocNum%', ""U_BaseEntry""='%NewRecord%', ""U_BaseLine"" = '" + dtTable(i)("LineNum").ToString() + "',""U_DBNPrice"" = isnull(""U_DBNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + "  where ""DocEntry"" = '" + dtTable(i)("BaseEntry").ToString().Trim() + "' and ""LineNum"" = '" + dtTable(i)("BaseLine").ToString().Trim() + "'"
                End If

                'strQuery += vbNewLine + "Update ""PCH1"" set ""U_DBNPrice"" = isnull(""U_DBNPrice"" ,0) + " + (Val(dtTable(i)("DiffPrice").ToString()) * -1).ToString() + " where DocEntry = " + dtTable(i)("BaseEntry").ToString()

                ThisFlag = True
            Next

            write_log(LineDataLog)

            Dim ErrCode = oPurchaseInvoices.Add()
            Dim LineNum1 = String.Join(", ", LineNums)
            If ThisFlag Then
                If ErrCode <> 0 And ThisFlag Then
                    objSBOAPI.SBO_Appln.StatusBar.SetText("AP Debit Note For Line Number " + LineNum1 + "  Posting Error : " & objSBOAPI.oCompany.GetLastErrorDescription)
                    SuccessMessage = "ERROR"
                Else
                    SuccessMessage = "SUCCESS"
                    NewRecord = "" : objSBOAPI.oCompany.GetNewObjectCode(NewRecord)
                    NewDocNum = objUtility.getSingleValue("Select ""DocNum""  from  OPCH where ""DocEntry"" = '" & NewRecord & "'")

                    strQuery = Replace(Replace(strQuery, "%NewDocNum%", NewDocNum), "%NewRecord%", NewRecord)
                    objUtility.DoQuery(strQuery)

                    'For Each LineNum In LineNums
                    '    Dim LineNo As Integer = Val(LineNum)
                    '    oMatrix.Columns.Item("U_PDocType").Cells.Item(LineNo).Specific.Value = "AP_DBN"
                    '    oMatrix.Columns.Item("U_PDocNum").Cells.Item(LineNo).Specific.Value = NewRecord
                    '    oMatrix.Columns.Item("U_PDocEnt").Cells.Item(LineNo).Specific.Value = NewDocNum
                    'Next
                End If
            End If

            Return True
        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("PostAPDebitNote Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None)
            Return False
        Finally
            oPurchaseInvoices = Nothing
            GC.Collect()
        End Try
    End Function

#End Region

    '#Region "Posting Row By Row"
    '    Function PostARCreditNote(ByVal dtTable As DataRow)
    '        Dim oCreditNotes As SAPbobsCOM.Documents = objSBOAPI.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes)
    '        Try

    '            'write_log("--------------Posting AR Credit Note-----------")

    '            Dim NewRecord, NewDocNum, strQuery As String
    '            strQuery = ""

    '            '-------Header Data------
    '            oCreditNotes.CardCode = dtTable("CardCode").ToString()
    '            oCreditNotes.DocDate = dtTable("DocDate")
    '            oCreditNotes.Series = Val(oCRNSeries)
    '            oCreditNotes.OriginalRefNo = dtTable("BaseNum").ToString()
    '            oCreditNotes.OriginalRefDate = dtTable("DocDate")

    '            '-------Line Data------
    '            oCreditNotes.Lines.ItemCode = dtTable("ItemCode").ToString()
    '            oCreditNotes.Lines.Quantity = Val(dtTable("Qty").ToString())
    '            oCreditNotes.Lines.WarehouseCode = dtTable("WhsCode").ToString()
    '            oCreditNotes.Lines.Price = Val(dtTable("DiffPrice").ToString())
    '            oCreditNotes.Lines.TaxCode = dtTable("TaxCode").ToString()
    '            oCreditNotes.Lines.HSNEntry = Val(dtTable("HSNEntry").ToString())
    '            oCreditNotes.Lines.CostingCode2 = dtTable("CostingCode2").ToString()

    '            oCreditNotes.Lines.WithoutInventoryMovement = SAPbobsCOM.BoYesNoEnum.tYES
    '            oCreditNotes.Lines.UserFields.Fields.Item("U_BaseType").Value = "ARInvoice"
    '            oCreditNotes.Lines.UserFields.Fields.Item("U_BaseEntry").Value = dtTable("BaseEntry").ToString()
    '            oCreditNotes.Lines.UserFields.Fields.Item("U_BaseLine").Value = dtTable("BaseLine").ToString()
    '            oCreditNotes.Lines.UserFields.Fields.Item("U_BaseNum").Value = dtTable("BaseNum").ToString()
    '            'oCreditNotes.Lines.BaseLine = dtTable("BaseLine").ToString()
    '            'oCreditNotes.Lines.BaseEntry = dtTable("BaseEntry").ToString()
    '            'oCreditNotes.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oInvoices
    '            'oCreditNote.Lines.ActualBaseEntry = 391 'DocEntry of Delivery Document
    '            'oCreditNote.Lines.ActualBaseLine = 0 'Line Number from the Delivery Document Lines
    '            oCreditNotes.Lines.Add()

    '            Dim ErrCode = oCreditNotes.Add()

    '            If ErrCode <> 0 Then
    '                objSBOAPI.SBO_Appln.StatusBar.SetText("AR Credit Note For Line Number " + dtTable("LineNum").ToString() + " Posting Error : " & objSBOAPI.oCompany.GetLastErrorDescription, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '                SuccessMessage = "ERROR"
    '            Else

    '                NewRecord = "" : objSBOAPI.oCompany.GetNewObjectCode(NewRecord)
    '                NewDocNum = objUtility.getSingleValue("Select ""DocNum""  from  ORIN where ""DocEntry"" = '" & NewRecord & "'")

    '                strQuery += vbNewLine + "Update ""@QL_BAA1"" set ""U_PDocType"" = 'AR_CRN',""U_PDocNum"" ='" + NewDocNum + "',""U_PDocEnt""='" + NewRecord + "',""U_ARCRNEnt""='" + NewRecord + "',""U_PLinNum""='" + dtTable("BaseLine").ToString() + "' where ""DocEntry"" = '" + objForm.Items.Item("U_DocEntry").Specific.Value.ToString() + "' and ""LineId"" = '" + dtTable("LineNum").ToString() + "'"
    '                strQuery += vbNewLine + "Update ""RIN1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AR_CRN', ""U_BaseNum""='" + NewDocNum + "', ""U_BaseEntry""='" + NewRecord + "', ""U_BaseLine"" = '" + dtTable("LineNum").ToString() + "'  where ""DocEntry"" = '" + dtTable("BaseEntry").ToString() + "' and ""LineNum"" = '" + dtTable("BaseLine").ToString() + "'"

    '                objUtility.DoQuery(strQuery)
    '            End If

    '            'write_log("--------------Posted AR Credit Note Successfully.-----------")

    '            Return True
    '        Catch ex As Exception
    '            SuccessMessage = "ERROR"
    '            objSBOAPI.SBO_Appln.StatusBar.SetText("PostARCreditNote Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
    '            Return False
    '        Finally
    '            oCreditNotes = Nothing
    '            GC.Collect()
    '        End Try
    '    End Function

    '    Function PostARDebitNote(ByVal dtTable As DataRow)
    '        Dim oInvoices As SAPbobsCOM.Documents = objSBOAPI.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
    '        Try
    '            'write_log("--------------Posting AR Debit Note-----------")
    '            Dim NewRecord, NewDocNum, strQuery As String
    '            strQuery = ""

    '            '-------Header Data------
    '            oInvoices.CardCode = dtTable("CardCode").ToString()
    '            oInvoices.DocDate = dtTable("DocDate")
    '            oInvoices.OriginalRefNo = dtTable("BaseNum").ToString()
    '            oInvoices.OriginalRefDate = dtTable("DocDate")
    '            oInvoices.GSTTransactionType = SAPbobsCOM.GSTTransactionTypeEnum.gsttrantyp_GSTDebitMemo
    '            oInvoices.Series = Val(oDBNSeries)

    '            '-------Line Data------
    '            oInvoices.Lines.ItemCode = dtTable("ItemCode").ToString()
    '            oInvoices.Lines.Quantity = Val(dtTable("Qty").ToString())
    '            oInvoices.Lines.WarehouseCode = dtTable("WhsCode").ToString()
    '            oInvoices.Lines.Price = Val(dtTable("DiffPrice").ToString()) * -1
    '            oInvoices.Lines.TaxCode = dtTable("TaxCode").ToString()
    '            oInvoices.Lines.HSNEntry = Val(dtTable("HSNEntry").ToString())
    '            oInvoices.Lines.CostingCode2 = dtTable("CostingCode2").ToString()

    '            oInvoices.Lines.WithoutInventoryMovement = SAPbobsCOM.BoYesNoEnum.tYES
    '            oInvoices.Lines.UserFields.Fields.Item("U_BaseType").Value = "ARInvoice"
    '            oInvoices.Lines.UserFields.Fields.Item("U_BaseEntry").Value = dtTable("BaseEntry").ToString()
    '            oInvoices.Lines.UserFields.Fields.Item("U_BaseLine").Value = dtTable("BaseLine").ToString()
    '            oInvoices.Lines.UserFields.Fields.Item("U_BaseNum").Value = dtTable("BaseNum").ToString()
    '            'oInvoices.Lines.BaseLine = dtTable("BaseLine").ToString()
    '            'oInvoices.Lines.BaseEntry = dtTable("BaseEntry").ToString()
    '            'oInvoices.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oOrders
    '            'oInvoices.Lines.TaxCode = dtRow("").ToString()
    '            'oInvoices.Lines.Quantity = dtTable("Qty").ToString()
    '            'oInvoices.Lines.ActualBaseEntry = 391 'DocEntry of Delivery Document
    '            'oInvoices.Lines.ActualBaseLine = 0 'Line Number from the Delivery Document Lines
    '            oInvoices.Lines.Add()


    '            Dim ErrCode = oInvoices.Add()
    '            If ErrCode <> 0 Then
    '                objSBOAPI.SBO_Appln.StatusBar.SetText("AR Debit Note For Line Number " + dtTable("LineNum").ToString() + " Posting Error : " & objSBOAPI.oCompany.GetLastErrorDescription, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '                SuccessMessage = "ERROR"
    '            Else

    '                NewRecord = "" : objSBOAPI.oCompany.GetNewObjectCode(NewRecord)
    '                NewDocNum = objUtility.getSingleValue("Select ""DocNum""  from  OINV where ""DocEntry"" = '" & NewRecord & "'")

    '                strQuery += vbNewLine + "Update ""@QL_BAA1"" set ""U_PDocType"" = 'AR_DBN',""U_PDocNum"" ='" + NewDocNum + "',""U_PDocEnt""='" + NewRecord + "',""U_ARDBNEnt""='" + NewRecord + "',""U_PLinNum""='" + dtTable("BaseLine").ToString() + "' where ""DocEntry"" = '" + objForm.Items.Item("U_DocEntry").Specific.Value.ToString() + "' and ""LineId"" = '" + dtTable("LineNum").ToString() + "'"
    '                strQuery += vbNewLine + "Update ""INV1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AR_DBN', ""U_BaseNum""='" + NewDocNum + "', ""U_BaseEntry""='" + NewRecord + "', ""U_BaseLine"" = '" + dtTable("LineNum").ToString() + "'  where ""DocEntry"" = '" + dtTable("BaseEntry").ToString() + "' and ""LineNum"" = '" + dtTable("BaseLine").ToString() + "'"

    '                objUtility.DoQuery(strQuery)

    '            End If

    '            'write_log("--------------Posted AR Debit Note Successfully-----------")

    '            Return True
    '        Catch ex As Exception
    '            SuccessMessage = "ERROR"
    '            objSBOAPI.SBO_Appln.StatusBar.SetText("PostARDebitNote Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
    '            Return False
    '        Finally
    '            oInvoices = Nothing
    '            GC.Collect()
    '        End Try
    '    End Function

    '    Function PostAPCreditNote(ByVal dtTable As DataRow)
    '        Dim oPurchaseCreditNotes As SAPbobsCOM.Documents = objSBOAPI.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseCreditNotes)
    '        Try
    '            'write_log("--------------Posting AP Credit Note-----------")
    '            Dim NewRecord, NewDocNum, strQuery As String
    '            strQuery = ""

    '            '-------Header Data------
    '            oPurchaseCreditNotes.CardCode = dtTable("CardCode").ToString()
    '            oPurchaseCreditNotes.DocDate = dtTable("DocDate")
    '            oPurchaseCreditNotes.OriginalRefNo = dtTable("BaseNum").ToString()
    '            oPurchaseCreditNotes.OriginalRefDate = dtTable("DocDate")

    '            oPurchaseCreditNotes.Series = Val(oPurchaseCRNSeries)

    '            '-------Line Data------
    '            oPurchaseCreditNotes.Lines.ItemCode = dtTable("ItemCode").ToString()
    '            oPurchaseCreditNotes.Lines.Quantity = Val(dtTable("Qty").ToString())
    '            oPurchaseCreditNotes.Lines.WarehouseCode = dtTable("WhsCode").ToString()
    '            oPurchaseCreditNotes.Lines.Price = Val(dtTable("DiffPrice").ToString())
    '            oPurchaseCreditNotes.Lines.TaxCode = dtTable("TaxCode").ToString()
    '            oPurchaseCreditNotes.Lines.HSNEntry = Val(dtTable("HSNEntry").ToString())
    '            oPurchaseCreditNotes.Lines.CostingCode2 = dtTable("CostingCode2").ToString()

    '            oPurchaseCreditNotes.Lines.WithoutInventoryMovement = SAPbobsCOM.BoYesNoEnum.tYES
    '            oPurchaseCreditNotes.Lines.UserFields.Fields.Item("U_BaseType").Value = "APInvoice"
    '            oPurchaseCreditNotes.Lines.UserFields.Fields.Item("U_BaseEntry").Value = dtTable("BaseEntry").ToString()
    '            oPurchaseCreditNotes.Lines.UserFields.Fields.Item("U_BaseLine").Value = dtTable("BaseLine").ToString()
    '            oPurchaseCreditNotes.Lines.UserFields.Fields.Item("U_BaseNum").Value = dtTable("BaseNum").ToString()

    '            'oPurchaseCreditNotes.Lines.BaseLine = dtTable("BaseLine").ToString()
    '            'oPurchaseCreditNotes.Lines.BaseEntry = dtTable("BaseEntry").ToString()
    '            'oPurchaseCreditNotes.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oInvoices
    '            'oPurchaseCreditNotes.Lines.Quantity = dtTable("Qty").ToString()
    '            'oPurchaseCreditNotes.Lines.ActualBaseEntry = 391 'DocEntry of Delivery Document
    '            'oPurchaseCreditNotes.Lines.ActualBaseLine = 0 'Line Number from the Delivery Document Lines
    '            oPurchaseCreditNotes.Lines.Add()


    '            Dim ErrCode = oPurchaseCreditNotes.Add()

    '            If ErrCode <> 0 Then
    '                objSBOAPI.SBO_Appln.StatusBar.SetText("AP Credit Note For Line Number " + dtTable("LineNum").ToString() + "  Posting Error : " & objSBOAPI.oCompany.GetLastErrorDescription, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '                SuccessMessage = "ERROR"
    '            Else

    '                NewRecord = "" : objSBOAPI.oCompany.GetNewObjectCode(NewRecord)
    '                NewDocNum = objUtility.getSingleValue("Select ""DocNum""  from  ORPC where ""DocEntry"" = '" & NewRecord & "'")

    '                strQuery += vbNewLine + "Update ""@QL_BAA1"" set ""U_PDocType"" = 'AP_CRN',""U_PDocNum"" ='" + NewDocNum + "',""U_PDocEnt""='" + NewRecord + "',""U_APCRNEnt""='" + NewRecord + "',""U_PLinNum""='" + dtTable("BaseLine").ToString() + "' where ""DocEntry"" = '" + objForm.Items.Item("U_DocEntry").Specific.Value.ToString() + "' and ""LineId"" = '" + dtTable("LineNum").ToString() + "'"
    '                strQuery += vbNewLine + "Update ""RPC1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AP_CRN', ""U_BaseNum""='" + NewDocNum + "', ""U_BaseEntry""='" + NewRecord + "', ""U_BaseLine"" = '" + dtTable("LineNum").ToString() + "'  where ""DocEntry"" = '" + dtTable("BaseEntry").ToString() + "' and ""LineNum"" = '" + dtTable("BaseLine").ToString() + "'"

    '                objUtility.DoQuery(strQuery)

    '            End If

    '            'write_log("--------------Posted AP Credit Note Successfully-----------")

    '            Return True
    '        Catch ex As Exception
    '            SuccessMessage = "ERROR"
    '            objSBOAPI.SBO_Appln.StatusBar.SetText("PostAPCreditNote Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
    '            Return False
    '        Finally
    '            oPurchaseCreditNotes = Nothing
    '            GC.Collect()
    '        End Try
    '    End Function

    '    Function PostAPDebitNote(ByVal dtTable As DataRow)
    '        Dim oPurchaseInvoices As SAPbobsCOM.Documents = objSBOAPI.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices)
    '        Try
    '            'write_log("--------------Posting AP Debit Note-----------")
    '            Dim NewRecord, NewDocNum, strQuery As String
    '            strQuery = ""

    '            '-------Header Data------
    '            oPurchaseInvoices.CardCode = dtTable("CardCode").ToString()
    '            oPurchaseInvoices.DocDate = dtTable("DocDate")
    '            oPurchaseInvoices.OriginalRefNo = dtTable("BaseNum").ToString()
    '            oPurchaseInvoices.OriginalRefDate = dtTable("DocDate")
    '            oPurchaseInvoices.GSTTransactionType = SAPbobsCOM.GSTTransactionTypeEnum.gsttrantyp_GSTDebitMemo
    '            oPurchaseInvoices.Series = Val(oPurchaseDBNSeries)

    '            '-------Line Data------
    '            oPurchaseInvoices.Lines.ItemCode = dtTable("ItemCode").ToString()
    '            oPurchaseInvoices.Lines.Quantity = Val(dtTable("Qty").ToString())
    '            oPurchaseInvoices.Lines.WarehouseCode = dtTable("WhsCode").ToString()
    '            oPurchaseInvoices.Lines.TaxCode = dtTable("TaxCode").ToString()
    '            oPurchaseInvoices.Lines.Price = Val(dtTable("DiffPrice").ToString()) * -1
    '            oPurchaseInvoices.Lines.HSNEntry = Val(dtTable("HSNEntry").ToString())
    '            oPurchaseInvoices.Lines.CostingCode2 = dtTable("CostingCode2").ToString()

    '            oPurchaseInvoices.Lines.WithoutInventoryMovement = SAPbobsCOM.BoYesNoEnum.tYES
    '            oPurchaseInvoices.Lines.UserFields.Fields.Item("U_BaseType").Value = "APInvoice"
    '            oPurchaseInvoices.Lines.UserFields.Fields.Item("U_BaseEntry").Value = dtTable("BaseEntry").ToString()
    '            oPurchaseInvoices.Lines.UserFields.Fields.Item("U_BaseLine").Value = dtTable("BaseLine").ToString()
    '            oPurchaseInvoices.Lines.UserFields.Fields.Item("U_BaseNum").Value = dtTable("BaseNum").ToString()
    '            'oPurchaseInvoices.Lines.BaseLine = dtTable("BaseLine").ToString()
    '            'oPurchaseInvoices.Lines.BaseEntry = dtTable("BaseEntry").ToString()
    '            'oPurchaseInvoices.Lines.BaseType = SAPbobsCOM.BoObjectTypes.oOrders
    '            ''oPurchaseInvoices.Lines.TaxCode = dtTable("").ToString()
    '            'oPurchaseInvoices.Lines.Quantity = dtTable("Qty").ToString()
    '            ''oPurchaseInvoices.Lines.ActualBaseEntry = 391 'DocEntry of Delivery Document
    '            ''oPurchaseInvoices.Lines.ActualBaseLine = 0 'Line Number from the Delivery Document Lines
    '            oPurchaseInvoices.Lines.Add()


    '            Dim ErrCode = oPurchaseInvoices.Add()
    '            If ErrCode <> 0 Then
    '                objSBOAPI.SBO_Appln.StatusBar.SetText("AP Debit Note For Line Number " + dtTable("LineNum").ToString() + "  Posting Error : " & objSBOAPI.oCompany.GetLastErrorDescription, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '                SuccessMessage = "ERROR"
    '            Else

    '                NewRecord = "" : objSBOAPI.oCompany.GetNewObjectCode(NewRecord)
    '                NewDocNum = objUtility.getSingleValue("Select ""DocNum""  from  OPCH where ""DocEntry"" = '" & NewRecord & "'")

    '                strQuery += vbNewLine + "Update ""@QL_BAA1"" set ""U_PDocType"" = 'AP_DBN',""U_PDocNum"" ='" + NewDocNum + "',""U_PDocEnt""='" + NewRecord + "',""U_APDBNEnt""='" + NewRecord + "',""U_PLinNum""='" + dtTable("BaseLine").ToString() + "' where ""DocEntry"" = '" + objForm.Items.Item("U_DocEntry").Specific.Value.ToString() + "' and ""LineId"" = '" + dtTable("LineNum").ToString() + "'"
    '                strQuery += vbNewLine + "Update ""PCH1"" set  ""U_BAPosted"" = 'Y', ""U_BaseType"" = 'AP_DBN', ""U_BaseNum""='" + NewDocNum + "', ""U_BaseEntry""='" + NewRecord + "', ""U_BaseLine"" = '" + dtTable("LineNum").ToString() + "'  where ""DocEntry"" = '" + dtTable("BaseEntry").ToString() + "' and ""LineNum"" = '" + dtTable("BaseLine").ToString() + "'"

    '                objUtility.DoQuery(strQuery)

    '            End If

    '            'write_log("--------------Posted AP Debit Note Successfully-----------")

    '            Return True
    '        Catch ex As Exception
    '            SuccessMessage = "ERROR"
    '            objSBOAPI.SBO_Appln.StatusBar.SetText("PostAPDebitNote Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
    '            Return False
    '        Finally
    '            oPurchaseInvoices = Nothing
    '            GC.Collect()
    '        End Try
    '    End Function

    '#End Region
   
    Public Function BAAS_GeneratePosting()
        Try
            objSBOAPI.SBO_Appln.StatusBar.SetText("Please Wait Posting Started...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            If IsNothing(oMatrix) Then
                If IsNothing(objForm) Then
                    objForm = objSBOAPI.SBO_Appln.Forms.ActiveForm
                End If
                oMatrix = objForm.Items.Item("Matrix").Specific
            End If
            If ValidatePosting() = True Then
                ' write_log("-------------BAAS_GeneratePosting Started----------------")
                'DefineDataTable()
                dtBAAS.Rows.Clear()
                Dim txtBAType As String = objForm.Items.Item("U_BAType").Specific.Value
                Dim postflag = False
                For i As Integer = 0 To oMatrix.RowCount - 1
                    Dim chkSelect As SAPbouiCOM.CheckBox
                    chkSelect = oMatrix.Columns.Item("U_Select").Cells.Item(i + 1).Specific
                    If chkSelect.Checked = True Then

                        Dim thisLineNum As String = oMatrix.Columns.Item("V_Line").Cells.Item(i + 1).Specific.Value.ToString()
                        Dim thisDocEntry As String = objForm.Items.Item("U_DocEntry").Specific.Value.ToString()
                        'write_log("select ""U_PDocEnt"" from ""@QL_BAA1"" where ""DocEntry"" = '" + thisDocEntry + "' and ""LineId"" = '" + thisLineNum + "'")
                        Dim thisPostedDocEntry As String = objUtility.getSingleValue("select ""U_PDocEnt"" from ""@QL_BAA1"" where ""DocEntry"" = '" + thisDocEntry + "' and ""LineId"" = '" + thisLineNum + "'")
                        'write_log("PostedDocEntry : " + thisPostedDocEntry)
                        If thisPostedDocEntry.Trim() = "" Then
                            postflag = True
                            'write_log("-------------Adding dtBASSRow - " + i.ToString() + "----------------")
                            Dim PriceDiffval As Double
                            Dim dtBASSRow As DataRow
                            dtBASSRow = Nothing
                            PriceDiffval = Val(oMatrix.Columns.Item("U_OldPrice").Cells.Item(i + 1).Specific.Value.ToString().Trim()) - Val(oMatrix.Columns.Item("U_NewPrice").Cells.Item(i + 1).Specific.Value.ToString().Trim())
                            dtBASSRow = dtBAAS.NewRow()
                            dtBASSRow("LineNum") = thisLineNum
                            dtBASSRow("PriceRType") = oMatrix.Columns.Item("U_PRevTyp").Cells.Item(i + 1).Specific.Value.ToString().Trim()
                            dtBASSRow("CardCode") = oMatrix.Columns.Item("U_CardCode").Cells.Item(i + 1).Specific.Value.ToString().Trim()
                            dtBASSRow("DocDate") = oMatrix.Columns.Item("U_DocDate").Cells.Item(i + 1).Specific.Value
                            dtBASSRow("ItemCode") = oMatrix.Columns.Item("U_InvICode").Cells.Item(i + 1).Specific.Value.ToString().Trim()
                            dtBASSRow("Qty") = Val(oMatrix.Columns.Item("U_InvQty").Cells.Item(i + 1).Specific.Value.ToString())
                            dtBASSRow("WhsCode") = oMatrix.Columns.Item("U_InvWCode").Cells.Item(i + 1).Specific.Value.ToString().Trim()
                            dtBASSRow("TaxCode") = oMatrix.Columns.Item("U_ITaxCode").Cells.Item(i + 1).Specific.Value.ToString().Trim()
                            dtBASSRow("DiffPrice") = PriceDiffval 'Val(oMatrix.Columns.Item("U_PriceDff").Cells.Item(i + 1).Specific.Value.ToString())
                            dtBASSRow("BaseEntry") = oMatrix.Columns.Item("U_InvEnt").Cells.Item(i + 1).Specific.Value.ToString().Trim()
                            dtBASSRow("BaseLine") = oMatrix.Columns.Item("U_ILineNum").Cells.Item(i + 1).Specific.Value.ToString().Trim()
                            dtBASSRow("BaseNum") = oMatrix.Columns.Item("U_InvNum").Cells.Item(i + 1).Specific.Value.ToString().Trim()
                            dtBASSRow("HSNEntry") = oMatrix.Columns.Item("U_InvHSN").Cells.Item(i + 1).Specific.Value.ToString().Trim()
                            dtBASSRow("CostingCode2") = oMatrix.Columns.Item("U_InvDepmt").Cells.Item(i + 1).Specific.Value.ToString().Trim()
                            dtBASSRow("PostType") = oMatrix.Columns.Item("U_PRevTyp").Cells.Item(i + 1).Specific.Value.ToString().Trim()
                            'dtBASSRow("DiscountPercent") = Val(oMatrix.Columns.Item("U_InvDisc").Cells.Item(i + 1).Specific.Value.ToString().Trim())
                            dtBAAS.Rows.Add(dtBASSRow)

                            'Dim postingData As String = ""
                            'For Each row As DataRow In dtBAAS.Rows
                            '    For Each col As DataColumn In dtBAAS.Columns
                            '        postingData += vbNewLine + col.ColumnName.ToString() + " : " + row(col).ToString()
                            '    Next
                            'Next
                            ' write_log("--------------Before Posting Data-----------" + postingData)

                            'Select Case txtBAType
                            '    Case "S"
                            '        If (PriceDiffval > 0) Then
                            '            PostARCreditNote(dtBASSRow)
                            '        ElseIf (PriceDiffval < 0) Then
                            '            PostARDebitNote(dtBASSRow)
                            '        End If
                            '        Exit Select
                            '    Case "P"
                            '        If (PriceDiffval > 0) Then
                            '            PostAPCreditNote(dtBASSRow)
                            '        ElseIf (PriceDiffval < 0) Then
                            '            PostAPDebitNote(dtBASSRow)
                            '        End If
                            '        Exit Select
                            'End Select


                        End If

                    End If
                Next

                Select Case txtBAType
                    Case "S"
                        If dtBAAS.Rows.Count > 0 Then
                            dtBAAS.DefaultView.Sort = "CardCode"
                            Dim view As DataView = New DataView(dtBAAS)
                            Dim distinctCardCode As DataTable = dtBAAS.Clone()

                            Dim query = From row In dtBAAS
                                        Group row By CardCode = row.Field(Of String)("CardCode") Into CardCodeGroup = Group
                                        Select New With {Key CardCode}

                            distinctCardCode.Rows.Clear()
                            For Each item In query
                                distinctCardCode.Rows.Add("", "", item.CardCode)
                            Next

                            For j As Integer = 0 To distinctCardCode.Rows.Count - 1

                                Dim thisCardCode = distinctCardCode(j)("CardCode").ToString().Trim()

                                Dim CRN_Result = From dtrow In dtBAAS.AsEnumerable()
                                             Where dtrow.Field(Of String)("CardCode").Trim() = thisCardCode And dtrow.Field(Of Decimal)("DiffPrice") > 0
                                             Select dtrow
                                Dim CRN_Rows As DataRow() = CRN_Result.ToArray()
                                If CRN_Rows.Length > 0 Then
                                    Dim dtCardCodeCRNList As DataTable = CRN_Rows.CopyToDataTable()
                                    PostARCreditNote(dtCardCodeCRNList)
                                End If

                                Dim DBN_Result = From dtrow In dtBAAS.AsEnumerable()
                                             Where dtrow.Field(Of String)("CardCode").Trim() = thisCardCode And dtrow.Field(Of Decimal)("DiffPrice") < 0
                                             Select dtrow
                                Dim DBN_Rows As DataRow() = DBN_Result.ToArray()
                                If DBN_Rows.Length > 0 Then
                                    Dim dtCardCodeDBNList As DataTable = DBN_Rows.CopyToDataTable()
                                    PostARDebitNote(dtCardCodeDBNList)
                                End If

                            Next

                        End If

                        Exit Select
                    Case "P"
                        If dtBAAS.Rows.Count > 0 Then
                            dtBAAS.DefaultView.Sort = "CardCode"
                            Dim view As DataView = New DataView(dtBAAS)
                            Dim distinctCardCode As DataTable = dtBAAS.Clone()

                            Dim query = From row In dtBAAS
                                        Group row By CardCode = row.Field(Of String)("CardCode") Into CardCodeGroup = Group
                                        Select New With {Key CardCode}

                            distinctCardCode.Rows.Clear()
                            For Each item In query
                                distinctCardCode.Rows.Add("", "", item.CardCode)
                            Next

                            For j As Integer = 0 To distinctCardCode.Rows.Count - 1

                                Dim thisCardCode = distinctCardCode(j)("CardCode").ToString().Trim()

                                Dim CRN_Result = From dtrow In dtBAAS.AsEnumerable()
                                             Where dtrow.Field(Of String)("CardCode").Trim() = thisCardCode And dtrow.Field(Of Decimal)("DiffPrice") > 0
                                             Select dtrow
                                Dim CRN_Rows As DataRow() = CRN_Result.ToArray()
                                If CRN_Rows.Length > 0 Then
                                    Dim dtCardCodeCRNList As DataTable = CRN_Rows.CopyToDataTable()
                                    PostAPCreditNote(dtCardCodeCRNList)
                                End If

                                Dim DBN_Result = From dtrow In dtBAAS.AsEnumerable()
                                             Where dtrow.Field(Of String)("CardCode").Trim() = thisCardCode And dtrow.Field(Of Decimal)("DiffPrice") < 0
                                             Select dtrow
                                Dim DBN_Rows As DataRow() = DBN_Result.ToArray()
                                If DBN_Rows.Length > 0 Then
                                    Dim dtCardCodeDBNList As DataTable = DBN_Rows.CopyToDataTable()
                                    PostAPDebitNote(dtCardCodeDBNList)
                                End If

                            Next
                        End If

                        Exit Select
                End Select

                If postflag = False Then
                    SuccessMessage = ""
                End If
            End If


            Return True
        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("BAAS_GeneratePosting Falied." + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            Return False
        Finally
            GC.Collect()
        End Try
    End Function

#End Region

#Region "Util Function"

    Private Sub write_log(ByVal status As String)
        Dim fs As FileStream
        Dim objWriter As StreamWriter
        Dim chatlog As String
        Try
            If dt_time = "" Then dt_time = Today.ToString("yyyyMMdd") & "\Log_" & Now.ToString("HH_mm_ss")
            Dim di As DirectoryInfo = New DirectoryInfo("C:\Common\BAPriceAutomation_" & Today.ToString("yyyyMMdd") & "")
            If di.Exists Then
            Else
                di.Create()
            End If
            chatlog = "C:\Common\BAPriceAutomation_" & dt_time & ".txt"

            If File.Exists(chatlog) Then
            Else
                fs = New FileStream(chatlog, FileMode.Create, FileAccess.Write)
                fs.Close()
            End If

            objWriter = New StreamWriter(chatlog, True)
            If status <> "" Then objWriter.WriteLine(Now & " : " & status)
            objWriter.Close()
        Catch ex As Exception
            MsgBox("BA Price Automation Log Error Please Contact Partner: " + vbNewLine + ex.Message())
        End Try
    End Sub

#End Region

#End Region

#Region "Events"

    Public Sub SBO_Appln_MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        Try
            If IsNothing(objForm) Then
                objForm = objSBOAPI.SBO_Appln.Forms.ActiveForm
            End If
            If IsNothing(oMatrix) Then
                oMatrix = objForm.Items.Item("Matrix").Specific
            End If

            If pVal.BeforeAction = False Then
                Select Case pVal.MenuUID
                    Case "1282"
                        InitiallizeForm(objForm)
                        objForm.Items.Item("U_BAType").Enabled = True
                        objForm.Items.Item("U_FromDate").Enabled = True
                        objForm.Items.Item("U_ToDate").Enabled = True
                    Case "1288", "1289", "1290", "1291"
                        objForm.Items.Item("U_BAType").Enabled = False
                        objForm.Items.Item("U_FromDate").Enabled = False
                        objForm.Items.Item("U_ToDate").Enabled = False
                        Dim oColumns As SAPbouiCOM.Columns = oMatrix.Columns

                        Dim BAType As String = objForm.Items.Item("U_BAType").Specific.Value
                        Dim oColumn As SAPbouiCOM.Column = oMatrix.Columns.Item("U_InvEnt")
                        Dim oLink As SAPbouiCOM.LinkedButton = oColumn.ExtendedObject
                        If BAType = "S" Then
                            oLink.LinkedObject = SAPbouiCOM.BoLinkedObject.lf_Invoice
                        ElseIf BAType = "P" Then
                            oLink.LinkedObject = SAPbouiCOM.BoLinkedObject.lf_PurchaseInvoice
                        End If
                End Select
            Else

            End If


        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("MenuEvent Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        End Try
    End Sub

    Public Sub SBO_Appln_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean, ByVal objform As SAPbouiCOM.Form)
        Try
            objform = objSBOAPI.SBO_Appln.Forms.ActiveForm
            If IsNothing(objform) Then
                objform = objSBOAPI.SBO_Appln.Forms.ActiveForm
            End If
            If IsNothing(oMatrix) Then
                oMatrix = objform.Items.Item("Matrix").Specific
            End If

            If pVal.Before_Action = False Then
                Select Case pVal.EventType
                    Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                        Exit Select
                    Case SAPbouiCOM.BoEventTypes.et_CLICK
                        If objform.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE Then
                            objform.Items.Item("btnPosting").Enabled = True
                            If pVal.ItemUID = "btnPosting" And pVal.ActionSuccess = True Then
                                objform.Items.Item("btnPosting").Enabled = False
                                If ValidatePosting() = True Then
                                    If BAAS_GeneratePosting() = True Then
                                        If SuccessMessage = "SUCCESS" Then
                                            objSBOAPI.SBO_Appln.StatusBar.SetText("Posing Completed Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                                            SuccessMessage = "SUCCESS"
                                        ElseIf SuccessMessage = "ERROR" Then
                                            objSBOAPI.SBO_Appln.StatusBar.SetText("Posing Completed Partially. Please Check System Message Log For More...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                                            objSBOAPI.SBO_Appln.StatusBar.SetText("Posing Completed Partially. Please Check System Message Log For More...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                                            SuccessMessage = "SUCCESS"
                                        Else
                                            objSBOAPI.SBO_Appln.StatusBar.SetText("No Documents Pending To Post...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                                        End If
                                        objSBOAPI.SBO_Appln.ActivateMenuItem("1304")
                                    End If
                                End If
                                objform.Items.Item("btnPosting").Enabled = True
                            End If
                        Else
                            objform.Items.Item("btnPosting").Enabled = False
                        End If

                        Exit Select

                    Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN
                        If pVal.ItemUID = "Matrix" Then
                            If pVal.ColUID = "U_NewPrice" Then
                                oMatrix.Columns.Item("U_PriceDff").Cells.Item(pVal.Row).Specific.Value = Val(oMatrix.Columns.Item("U_OldPrice").Cells.Item(pVal.Row).Specific.Value) - Val(oMatrix.Columns.Item("U_NewPrice").Cells.Item(pVal.Row).Specific.Value)
                            End If
                        End If
                        Exit Select

                End Select
            Else
                Select Case pVal.EventType
                    Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                        Exit Select
                    Case SAPbouiCOM.BoEventTypes.et_CLICK
                        If objform.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Then
                            If pVal.ItemUID = "btnLoad" Then
                                Dim frmDate = objform.Items.Item("U_FromDate").Specific.Value
                                Dim toDate = objform.Items.Item("U_ToDate").Specific.Value
                                objSBOAPI.SBO_Appln.StatusBar.SetText("Matrix Loading By New Thread Please Wait...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                                Dim t1 As Thread = New Thread(
                                    Sub()
                                        LoadMatrix(objform, frmDate, toDate)
                                    End Sub
                                )
                                t1.Start()
                            End If
                            If pVal.ItemUID = "1" Then
                                If ValidateAll() = False Then

                                    BubbleEvent = False
                                    Exit Sub
                                Else
                                    objUtility.DoQuery(invEntries)
                                End If
                            End If
                        End If
                        Exit Select


                End Select
            End If



        Catch ex As Exception

            objSBOAPI.SBO_Appln.StatusBar.SetText("ItemEvent Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        End Try
    End Sub

    Public Sub SBO_Appln_FormDataEvent(ByRef pVal As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        Try
            If IsNothing(objForm) Then
                objForm = objSBOAPI.SBO_Appln.Forms.ActiveForm
            End If
            If IsNothing(oMatrix) Then
                oMatrix = objForm.Items.Item("Matrix").Specific
            End If


            Select Case pVal.EventType
                Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD
                    'If pVal.BeforeAction = True Then
                    '    If ValidateAll() = False Then

                    '        BubbleEvent = False
                    '        Exit Sub
                    '    Else
                    '        objUtility.DoQuery(invEntries)
                    '    End If
                    'End If
                    If pVal.BeforeAction = False Then
                        objForm.Items.Item("U_BAType").Enabled = True
                        objForm.Items.Item("U_FromDate").Enabled = True
                        objForm.Items.Item("U_ToDate").Enabled = True
                        InitiallizeForm(objForm, "1")
                    End If
                    

                Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE
                    objForm.Items.Item("U_BAType").Enabled = False
                    objForm.Items.Item("U_FromDate").Enabled = False
                    objForm.Items.Item("U_ToDate").Enabled = False

            End Select



        Catch ex As Exception
            objSBOAPI.SBO_Appln.StatusBar.SetText("FormDataEvent Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        End Try
    End Sub

#End Region

End Class
