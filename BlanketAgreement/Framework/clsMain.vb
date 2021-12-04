Public Class clsMain

#Region "Declaration"
    Public objSBOAPI As ClsSBO
    Private objutity As clsUtilities
    Private objForm As SAPbouiCOM.Form
    Dim sValue As String
    Dim sPath, strConnectioninfo As String
#End Region

#Region "Methods"
    Public Sub New()
        objSBOAPI = New ClsSBO
        objutity = New clsUtilities(objSBOAPI)
    End Sub

#Region "Initialise"
    '*****************************************************************
    'Type               : Function    
    'Name               : Initialise
    'Parameter          :
    'Return Value       : Boolean
    'Author             : QL Sivaprakash
    'Created Date       : 30/1/2019
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Initialise the Application and Create Table
    '******************************************************************

    Public Function Initialise() As Boolean
        Dim objMenu As SAPbouiCOM.MenuItem

        If (Not objSBOAPI.Connect()) Then Return False 'Connect SAP using Command Line Argument

        If objSBOAPI.oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB Then
            DataBaseType = "HANA"
        Else
            DataBaseType = "SQL"
        End If

        objSBOAPI.AddAlphaFieldDefault("OADM", "TabCreFlag", "Table Creation", 20, "N,Y", "No,Yes", "N", "", True)
        Dim TableCreationFlag As String = objutity.getSingleValue("Select ""U_TabCreFlag"" from ""OADM""")

        If TableCreationFlag.ToString.Trim <> "N" Then
            If (Not createtables()) Then Return False 'Create table
            objSBOAPI.CreatedUDO_BlanketAgreement()  'UDO BlanketInvoice
        End If

        objSBOAPI.SetFilter() 'Set Filter 
        objSBOAPI.LoadMenu(System.Windows.Forms.Application.StartupPath & "\XML\Menu.xml")
        objMenu = objSBOAPI.SBO_Appln.Menus.Item("BAAS")
        objMenu.Image = System.Windows.Forms.Application.StartupPath & "\XML\BAAS.bmp"

        Return True
    End Function
#End Region

    Private Function createtables() As Boolean
        Dim oProgressBar As SAPbouiCOM.ProgressBar = Nothing
        Try
            GC.Collect()
            oProgressBar = objSBOAPI.SBO_Appln.StatusBar.CreateProgressBar("Initilizing Add-On...", 100, True)
            oProgressBar.Value = 0
            oProgressBar.Value = oProgressBar.Value + 5

            '---------------Blanket Agreement Automation Head-----------------
            objSBOAPI.CreateTable("QL_OBAA", "BA_Automation_Header", SAPbobsCOM.BoUTBTableType.bott_Document)
            objSBOAPI.AddDateField("QL_OBAA", "FromDate", "From Date", SAPbobsCOM.BoFldSubTypes.st_None, True)
            objSBOAPI.AddDateField("QL_OBAA", "ToDate", "To Date", SAPbobsCOM.BoFldSubTypes.st_None, True)
            objSBOAPI.AddAlphaFieldDefault("QL_OBAA", "BAType", "Blanket Agreement Type", 20, "S,P", "Sales,Purchase", "", "", True)
            objSBOAPI.AddAlphaField("QL_OBAA", "Remarks", "Remarks", 250, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_OBAA", "BPCode", "BPCode", 100, "", "", "", "", False)

            '---------------Blanket Agreement Automation Line----------------'
            objSBOAPI.CreateTable("QL_BAA1", "BA_Automation_Details", SAPbobsCOM.BoUTBTableType.bott_DocumentLines)
            objSBOAPI.AddAlphaField("QL_BAA1", "PRevTyp", "Price Revision Type", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "OldBANo", "Old Blanket Agreement No", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "NewBANo", "New Blanket Agreement No", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "CardCode", "Customer Code", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "CustNum", "Customer PO Number", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "CardName", "Customer Name", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "OldICode", "Old Item Code", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "NewICode", "New Item Code", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "OldIName", "Old Item Name", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "NewIName", "New Item Name", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "InvNum", "Invoice No", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "InvDate", "Invoice Date", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "InvQty", "Invoice Quantity", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "EffDate", "Effective Date", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "OldPrice", "Old Price", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "NewPrice", "New Price", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "PriceDff", "Price Difference", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "Select", "Select", 1, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "OldBAEn", "Old Blanket Agreement Entry", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "NewBAEn", "New Blanket Agreement Entry", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "InvEnt", "Invoice Entry", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "InvICode", "Invoice Item Code", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "InvWCode", "Invoice Whs Code", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "ILineNum", "Invoice Line No", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "ITaxCode", "Posted DocEntry", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "DocDate", "Posting Doc Date", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "OldBALNo", "Old BA Line No", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "NewBALNo", "New BA Line No", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "PDocType", "Posted DocType", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "PDocNum", "Posted DocNum", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "PDocEnt", "Posted DocEntry", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "PLinNum", "Posted LineNum", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "ARCRNEnt", "AR CRN DocEntry", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "ARDBNEnt", "AR DBN DocEntry", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "APCRNEnt", "AP CRN DocEntry", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "APDBNEnt", "AP DBN DocEntry", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "InvHSN", "Invoice HSN Code", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "InvDisc", "Invoice Discount", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "InvDepmt", "Invoice Department", 100, "", "", "", "", False)
            'objSBOAPI.AddAlphaField("QL_BAA1", "OrgInvNo", "Original Invoice No", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAA1", "InvDTot", "Invoice DovTotal", 100, "", "", "", "", False)

            '---------------Blanket Agreement Automation Series No Object Table----------------'
            objSBOAPI.CreateTable("QL_BAAS", "BA_Automation_Series", SAPbobsCOM.BoUTBTableType.bott_NoObject)
            objSBOAPI.AddAlphaField("QL_BAAS", "ARCreSer", "AR Credit Series", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAAS", "ARDebSer", "AR Debit Series", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAAS", "APCreSer", "AP Credit Series", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("QL_BAAS", "APDebSer", "AP Debit Series", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaFieldDefault("QL_BAAS", "Default", "Set As Default", 20, "Y,N", "Yes,No", "", "", True)

            '----------------Old Blanket and Sales Order Fields for BAAS---------------------
            objSBOAPI.AddAlphaField("ORDR", "OldSoNum", "OldSOorPONumber", 50, "", "", "", "", False)
            objSBOAPI.AddAlphaField("OOAT", "oldBlnkNum", "OldBlnketNumber", 50, "", "", "", "", False)

            '--------------- INV1 and PCH1 Fiels for BAAS ------------------------------------
            objSBOAPI.AddAlphaField("INV1", "BaseType", "BaseType", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("INV1", "BaseEntry", "BaseEntry", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("INV1", "BaseLine", "BaseLine", 100, "", "", "", "", False)
            objSBOAPI.AddAlphaField("INV1", "BaseNum", "BaseNum", 100, "", "", "", "", False)
            objSBOAPI.AddDateField("RDR1", "EffDate", "Effective Date", SAPbobsCOM.BoFldSubTypes.st_None, True)
            objSBOAPI.AddDateField("OPOR", "EffDate", "Effective Date", SAPbobsCOM.BoFldSubTypes.st_None, True)
            objSBOAPI.AddAlphaFieldDefault("INV1", "BAPosted", "Is BAAS Posted", 20, "Y,N", "Yes,No", "", "", True)
            objSBOAPI.AddFloatField("INV1", "CRNPrice", "CRN Price", SAPbobsCOM.BoFldSubTypes.st_Price, False)
            objSBOAPI.AddFloatField("INV1", "DBNPrice", "DBN Price", SAPbobsCOM.BoFldSubTypes.st_Price, False)
            objSBOAPI.AddFloatField("INV1", "NewPrice", "New Price", SAPbobsCOM.BoFldSubTypes.st_Price, False)
            objSBOAPI.AddAlphaField("INV1", "NewEntry", "New Entry", 50, "", "", "", "", False)
            objSBOAPI.AddAlphaField("INV1", "NewNo", "New No", 50, "", "", "", "", False)
            objSBOAPI.AddAlphaField("INV1", "NewICode", "New Item Code", 50, "", "", "", "", False)
            objSBOAPI.AddAlphaField("INV1", "NewIName", "New Item Name", 50, "", "", "", "", False)
            objSBOAPI.AddAlphaField("INV1", "NewLNum", "New Line Num", 50, "", "", "", "", False)

            'objSBOAPI.CreateTable("EINV_OBLK", "Bulk_E-Inv_Generation", SAPbobsCOM.BoUTBTableType.bott_Document)
            'objSBOAPI.AddDateField("EINV_OBLK", "FrmDate", "FrmDate", SAPbobsCOM.BoFldSubTypes.st_None, True)
            'objSBOAPI.AddAlphaFieldDefault("EINV_OBLK", "Status", "EInvoice Status", 10, "No Need,Open,Generated", "N,O,G", "", "", True)
            'objSBOAPI.AddAlphaField("EINV_OBLK", "DocEntry", "DocEntry", 100, "", "", "", "", False)
            'objSBOAPI.AddAlphaMemoField("EINV_OBLK", "CustName", "Customer Name", 550, True)
            'objSBOAPI.AddFloatField("EINV_OBLK", "TotalQty", "TotalQty", SAPbobsCOM.BoFldSubTypes.st_Quantity, False)

            oProgressBar.Value = oProgressBar.Value + 5
            oProgressBar.Stop()
            Return True
        Catch ex As Exception
            If Not oProgressBar Is Nothing Then
                oProgressBar.Stop()
            End If
            Return False
        Finally
            oProgressBar = Nothing
        End Try
        Return True

    End Function

#End Region

End Class
