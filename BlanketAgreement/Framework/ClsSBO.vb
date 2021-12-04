Imports System.IO
Imports SAPbouiCOM
Imports SAPbobsCOM


Public Class ClsSBO

#Region "Declaration"
    'To check the login credentials
    Public PasswordCheck As Boolean = False
    Public LatestLogInOperatorCode As String = ""
    Public LatestLogInOperatorName As String = ""
    Public LatestLogInEmpId As String = ""
    Public LogEntFindFlag As Integer = 0

    Dim x As SAPbouiCOM.ApplicationClass
    Public WithEvents SBO_Appln As SAPbouiCOM.Application
    Public oCompany As SAPbobsCOM.Company

    'Variable Declaration
    Private SboGuiApi As SAPbouiCOM.SboGuiApi
    Private objApplication As SAPbouiCOM.Application
    Private objCompany As SAPbouiCOM.Application
    Private objform As SAPbouiCOM.Form
    Private objEdit As SAPbouiCOM.EditText
    Public LastErrorDescription As String
    Public LastErrorCode As Integer

    'Master
    Public objBlanketInv As clsBlanketInvoices

    ''Public objGenSetting As clsGeneralSetting
    ''Public objInspection As clsInspection
    ''Public objCFLInspection As clsCFLInspection

    'Public objBulkEInv As clsBulkEInvoice
    ''Public objGenSetting As clsGeneralSetting
    ''Public objItemMaster As clsItemMaster
    'Public objSAPAREInvoice As clsSAPAREInvoice
    'Public objEWayBill As ClassEWayBill
    'Public objLogDetailsEI As clsLogDetailsEI
    'Public objSAPARCreMemo As clsSAPARCreMemo
    ''Public objEInvoice As ClassEInvoice

    Public AutoBachSel_FormId As String = ""
    Public AutoBachSel_TypeCount As Integer = 1
    Public ArrList_Project As ArrayList
    Public ArrList_CostCenter As ArrayList
    Public ArrList_HeatNo As ArrayList
    Public ArrList_PrcmNo As ArrayList
    Public ArrList_Pcrmdate As ArrayList
    Public AutoBachSel_FormMode As String = ""
    Public AutoBachSel_BatchNo As String = ""
    Public AutoBatch_LineId As String

    'Revision
    Public ArrList_Detail As ArrayList
    Public ArrList_Reason As ArrayList
    Public ArrList_Remarks As ArrayList
    Public ArrList_ItemCode As ArrayList
    Public ArrList_RowCount As ArrayList
    Public ArrList_Date As ArrayList
    Public ArrList_Date_Mat As ArrayList
    Public ArrList_BatchList As ArrayList

    'Packing List

    'For Licence
    'Dim TespaLicenseFlag As Boolean = False
    Public ShowFolderBrowserThread As Threading.Thread
    Private strpath As String
    ' Dim hKey As String = ""
    ' Private VALIDDATE As Date
    ' Public strPodocentry As String
    Dim blnHardwarekeyflag As Boolean = False
    Dim blnLicDateFlag As Boolean = False


#End Region

#Region "Methods"

#Region "Application Initialization"
    '*****************************************************************
    'Type               : Procedure    
    'Name               : SetApplication
    'Parameter          : 
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To set SBO Application
    '******************************************************************

    Private Sub SetApplication()
        Dim sConnectionString As String = Environment.GetCommandLineArgs.GetValue(1)
        SboGuiApi = New SAPbouiCOM.SboGuiApi
        SboGuiApi.Connect(sConnectionString)
        SBO_Appln = SboGuiApi.GetApplication()

    End Sub
#End Region

#Region "Connect Company"
    '*****************************************************************
    'Type               : Procedure    
    'Name               : ConnectCompany
    'Parameter          : 
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Connect SBO Company
    '******************************************************************

    Private Sub HardwareKeyValidation()

        'HardwareKey Validation
        objApplication.Menus.Item("257").Activate()
        Dim CrrHWKEY As String = objApplication.Forms.ActiveForm.Items.Item("79").Specific.Value.ToString.Trim
        objApplication.Forms.ActiveForm.Close()
        Dim HW() As String = {"D1016400232", "S0118982778", "F0096661869"}
        blnHardwarekeyflag = False
        For i As Integer = 0 To HW.Length() - 1
            If HW(i).ToString.Trim = CrrHWKEY Then
                SBO_Appln.StatusBar.SetText(HW(i).ToString().Trim(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None)
                blnHardwarekeyflag = True
                Exit For
            End If
        Next
        blnLicDateFlag = True
        'Date Validation
        'Dim strCurDate As String = CDate(Today.Date).ToString("yyyyMMdd")
        'Dim strExpDate As String = "20220201"
        'If strCurDate <= strExpDate Then
        '    SBO_Appln.StatusBar.SetText(strExpDate.ToString().Trim(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None)
        '    blnLicDateFlag = True
        'Else
        '    blnLicDateFlag = False
        'End If

    End Sub

    Private Function ConnectCompany() As Boolean
        'Dim connectstr As String
        'Dim lngConnect As Long
        'Dim rsCompany As SAPbobsCOM.Recordset
        oCompany = New SAPbobsCOM.Company
        'Dim sUsers As SAPbobsCOM.Users
        'Dim usr1 As SAPbobsCOM.Users
        'Dim conectstr2 As String
        Dim ocookies As String
        Dim ocookiecontext As String
        Try
            oCompany = New SAPbobsCOM.Company
            ocookies = oCompany.GetContextCookie
            ocookiecontext = SBO_Appln.Company.GetConnectionContext(ocookies)
            oCompany.SetSboLoginContext(ocookiecontext)
            If oCompany.Connect <> 0 Then
                SBO_Appln.StatusBar.SetText("Connection Error", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            Else
                If oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB Then
                    DataBaseType = "HANA"
                Else
                    DataBaseType = "SQL"
                End If

            End If

            '------------------------
            ' blnHardwarekeyflag = False
            '   blnLicDateFlag = False
            '  Me.HardwareKeyValidation()
            '  If blnHardwarekeyflag = False Then
            'SBO_Appln.StatusBar.SetText("Invalid Hardware Key....Contact iSolution Pvt.Ltd", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            ' Return False
            ' End If


            ' If blnLicDateFlag = False Then
            'SBO_Appln.StatusBar.SetText("Addon Licence Expired.....", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            'SBO_Appln.StatusBar.SetText("AMS is getting expired on 30-04-2020, contact for AMS renewal......", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)

            ' Return False
            ' End If


            blnHardwarekeyflag = False
            blnLicDateFlag = False
            Me.HardwareKeyValidation()
            If blnHardwarekeyflag = False Or blnLicDateFlag = False Then
                SBO_Appln.StatusBar.SetText("", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            End If

            'SBO_Appln.Menus.Item("257").Activate()
            'Dim CrrHWKEY As String = SBO_Appln.Forms.ActiveForm.Items.Item("79").Specific.Value.ToString.Trim
            'SBO_Appln.Forms.ActiveForm.Close()
            'If lickey(CrrHWKEY) = True Then
            'Else

            '    '  MsgBox("License Mismatch..... Kindly Check with SAP Team..", MsgBoxStyle.OkOnly, "License Management")
            '    Return False
            'End If

        Catch ex As Exception
            SBO_Appln.MessageBox(ex.Message)
            Return False
        End Try
        oCompany.XmlExportType = SAPbobsCOM.BoXmlExportTypes.xet_ExportImportMode
        Return True
    End Function

    'Private Function lickey(ByRef hKey As String)

    '    Return True
    '    'Dim MaxDate As String = "20"
    '    'Dim s, alnu, hk1, ye, mo, da, saaa As String
    '    'Dim hk As String = ""
    '    'Dim NoL As String = ""
    '    'Dim a, b, c, d, k1, k2, k3, k4, k5, k6, k8, k9, k10, k11 As Integer
    '    'Dim a11, b11, c11, d11, k111, k211, k311, k411, k511, k611, k811, k911, k1011, k1111 As Integer

    '    ''Dim a, b, c As String
    '    'Dim a1, b1, H1, H2, H3, H4, H5, H6, H8, H9, H10, H11 As Integer
    '    ''s = ReadTextFile(Environment.CurrentDirectory & "\ReadMe.txt")
    '    Try
    '        Try

    '            oRecordset1 = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
    '            If DataBaseType.ToString.Trim = "SQL" Then
    '                oRecordset1.DoQuery("Select U_LIC_NO  from [@LIC_CONFIG] WHERE Code = 1")
    '            Else
    '                oRecordset1.DoQuery("Select ""U_LIC_NO""  from ""@LIC_CONFIG"" WHERE ""Code"" = 1")
    '            End If

    '            oRecordset1.MoveFirst()

    '            's = ReadTextFile("C:\Tespa\LicKey.txt")
    '            s = oRecordset1.Fields.Item("U_LIC_NO").Value.ToString
    '        Catch ex As Exception
    '            SBO_Appln.StatusBar.SetText("Cannot Import License file." + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '            'End
    '        End Try
    '        alnu = "QA5ZW7SX6ED4CR8FV9TG3BY0HN2UJ1MIKOLP"
    '        'valid = "5647839201"
    '        a = s.Length()

    '        'Changed for database license string retrieval as its length is 4 characters less than from text file.
    '        'a = s.Substring(2, 2)
    '        'b = s.Substring(4, 4)
    '        'c = s.Substring(8, 2)

    '        a = s.Substring(2, 2)
    '        b = s.Substring(4, 2)
    '        c = s.Substring(6, 2)


    '        'ye = s.Substring(15, 2)
    '        'MessageBox.Show("vv " + ye)
    '        'MessageBox.Show(a & "  " & b & "  " & c & "  " & ye)
    '        'End
    '        c = c + d + 12
    '        d = 180 + c


    '        hk1 = ""
    '        'Changed for database license string retrieval as its length is 4 characters less than from text file.
    '        'a1 = a + b + 20 
    '        a1 = a + b + 20 - 4
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        hk1 = hk1 + hk


    '        a1 = a1 + 10
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        hk1 = hk1 + hk


    '        a1 = a1 + 15
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        hk1 = hk1 + hk

    '        a1 = a1 + 15
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        hk1 = hk1 + hk


    '        a1 = a1 + 10
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        hk1 = hk1 + hk

    '        a1 = a1 + 12
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        hk1 = hk1 + hk

    '        a1 = a1 + 37
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        hk1 = hk1 + hk

    '        a1 = a1 + 20
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        hk1 = hk1 + hk


    '        a1 = a1 + 18
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        hk1 = hk1 + hk


    '        a1 = a1 + 22
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        hk1 = hk1 + hk



    '        a1 = a1 + 15
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        hk1 = hk1 + hk

    '        a1 = a1 + 40
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        NoL = NoL + hk

    '        a1 = a1 + 32
    '        k1 = s.Substring(a1, 2)
    '        hk = alnu.Substring(k1, 1)
    '        NoL = NoL + hk


    '        '---------------------------------------------
    '        a1 = a1 + 40
    '        da = s.Substring(a1, 2)
    '        'hk = alnu.Substring(k1, 1)
    '        MaxDate = MaxDate + da

    '        a1 = a1 + 7
    '        da = s.Substring(a1, 2)
    '        'hk = alnu.Substring(k1, 1)
    '        MaxDate = MaxDate + "/" + da

    '        a1 = a1 + 7
    '        da = s.Substring(a1, 2)
    '        'hk = alnu.Substring(k1, 1)
    '        MaxDate = MaxDate + "/" + da
    '        'Dim oRecordSet As SAPbobsCOM.Recordset
    '        'Dim qry As String = ""
    '        'Dim dat1, dat2 As Date
    '        dat2 = MaxDate
    '        oRecordSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)

    '        'qry = "SELECT CURRENT_DATE ""date1"" FROM DUMMY"

    '        If DataBaseType.ToString.Trim = "SQL" Then
    '            qry = "SELECT getdate() as date1 FROM OCRD"
    '        Else
    '            qry = "SELECT CURRENT_DATE ""date1"" FROM DUMMY"
    '        End If

    '        oRecordSet.DoQuery(qry)
    '        dat1 = oRecordSet.Fields.Item("date1").Value

    '        'Dim daysno As Integer = DateDiff(DateInterval.Day, dat1, dat2)


    '        'If DateDiff(DateInterval.Day, dat1, dat2) < 1 Then
    '        '    SBO_Application.StatusBar.SetText("Invalid License Key", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '        '    'End
    '        'End If

    '        'If DateDiff(DateInterval.Day, dat1, dat2) < 1 Then
    '        '    SBO_Appln.StatusBar.SetText("License Key has Expired.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '        '    Return False
    '        'ElseIf DateDiff(DateInterval.Day, dat1, dat2) <= 15 Then
    '        '    SBO_Appln.MessageBox("License expires in " & DateDiff(DateInterval.Day, dat1, dat2) & " days.")
    '        'End If

    '        ''===============================================================================================================

    '        'If hk1 <> hKey Then
    '        '    SBO_Appln.StatusBar.SetText("Invalid License Key. Hardware key does not match.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '        '    Return False
    '        'End If


    '        If hk1 <> hKey Then
    '            SBO_Appln.MessageBox("Invalid Hardware key...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '            Return False
    '        End If


    '        If DateDiff(DateInterval.Day, dat1, dat2) < 0 Then
    '            SBO_Appln.MessageBox(" License for AMS is Expired...Please Contact .", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '            ' Return False
    '        ElseIf DateDiff(DateInterval.Day, dat1, dat2) <= 15 Then
    '            SBO_Appln.MessageBox("License for AMS is getting Expired by  " & DateDiff(DateInterval.Day, dat1, dat2) + 1 & " days.")
    '        End If


    '        'MessageBox.Show(k1 & " - " & hk1)
    '        'End
    '        VALIDDATE = dat2
    '    Catch ex As Exception
    '        SBO_Appln.StatusBar.SetText("Exception raised when checking the license file... " & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '        Return False
    '    End Try

    '    HardwareKey = hKey
    '    NoOfLic = CInt(NoL)
    '    'TespaLicenseFlag = True
    '    Return True

    'End Function

    Public Function Connect() As Boolean
        If (Not initialiseApplication()) Then
            Return False
        End If
        If (Not ConnectCompany()) Then Return False
        createobjects()
        Return True
    End Function

    Public Function initialiseApplication() As Boolean
        Try
            Dim strConstr As String
            Dim objGUI As SAPbouiCOM.SboGuiApiClass
            objGUI = New SAPbouiCOM.SboGuiApiClass

            strConstr = System.Environment.GetCommandLineArgs(1)
            objGUI.Connect(strConstr)
            objApplication = objGUI.GetApplication()
            SBO_Appln = objApplication
        Catch ex As Exception
            LastErrorCode = -100001
            LastErrorDescription = ex.Message
            Return False
        End Try

        Return True
    End Function

    Public Function initialiseCompany() As Boolean
        Dim strCookie As String
        Dim strConStr As String
        Dim intReturnCode As Integer
        objCompany = New SAPbobsCOM.Company
        strCookie = objCompany.GetContextCookie()
        strConStr = objApplication.Company.GetConnectionContext(strCookie)
        objCompany.SetSboLoginContext(strConStr)
        intReturnCode = objCompany.Connect()
        If (intReturnCode <> 0) Then
            updateLastErrorDetails(-102)
            Return False
        End If

        Return True


    End Function

#End Region

    Sub SetNewLine(ByVal oMatrix As SAPbouiCOM.Matrix, ByVal oDBDSDetail As SAPbouiCOM.DBDataSource, Optional ByVal RowID As Integer = 1, Optional ByVal ColumnUID As String = "")
        Try
            If ColumnUID.Equals("") = False Then
                If oMatrix.VisualRowCount > 0 Then
                    If oMatrix.Columns.Item(ColumnUID).Cells.Item(RowID).Specific.Value.Equals("") = False And RowID = oMatrix.VisualRowCount Then
                        oMatrix.FlushToDataSource()
                        oMatrix.AddRow()
                        oDBDSDetail.InsertRecord(oDBDSDetail.Size)
                        oDBDSDetail.Offset = oMatrix.VisualRowCount - 1
                        oDBDSDetail.SetValue("LineID", oDBDSDetail.Offset, oMatrix.VisualRowCount)

                        oMatrix.SetLineData(oMatrix.VisualRowCount)
                        oMatrix.FlushToDataSource()
                    End If
                Else
                    oMatrix.FlushToDataSource()
                    oMatrix.AddRow()
                    oDBDSDetail.InsertRecord(oDBDSDetail.Size)
                    oDBDSDetail.Offset = oMatrix.VisualRowCount - 1
                    oDBDSDetail.SetValue("LineID", oDBDSDetail.Offset, oMatrix.VisualRowCount)

                    oMatrix.SetLineData(oMatrix.VisualRowCount)
                    oMatrix.FlushToDataSource()
                End If

            Else
                oMatrix.FlushToDataSource()
                oMatrix.AddRow()
                oDBDSDetail.InsertRecord(oDBDSDetail.Size)
                oDBDSDetail.Offset = oMatrix.VisualRowCount - 1
                oDBDSDetail.SetValue("LineID", oDBDSDetail.Offset, oMatrix.VisualRowCount)

                oMatrix.SetLineData(oMatrix.VisualRowCount)
                oMatrix.FlushToDataSource()
            End If
        Catch ex As Exception
            SBO_Appln.StatusBar.SetText("SetNewLine Method Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        Finally
        End Try
    End Sub

#Region "Update Error Details"
    Private Sub updateLastErrorDetails(ByVal ErrorCode As Integer)
        LastErrorCode = ErrorCode
        LastErrorDescription = oCompany.GetLastErrorCode() & ":" & oCompany.GetLastErrorDescription()
    End Sub
#End Region

#Region "Filters"
    '*****************************************************************
    'Type               : Procedure    
    'Name               : Filters
    'Parameter          : EventFilters
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To set Event Filters to the Application
    '******************************************************************
    Public Sub SetFilter(ByVal Filters As SAPbouiCOM.EventFilters)
        SBO_Appln.SetFilter(Filters)
    End Sub

    '*****************************************************************
    'Type               : Procedure    
    'Name               : Filters
    'Parameter          : 
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To set Event Filters to the Application
    '*****************************************************************
    Public Sub SetFilter()
        Dim objFilters As SAPbouiCOM.EventFilters
        Dim objFilter As SAPbouiCOM.EventFilter
        objFilters = New SAPbouiCOM.EventFilters

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_MENU_CLICK)

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED)


        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT)
        'objFilter.AddEx("UDO_GSET")


        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_VALIDATE)
        'objFilter.AddEx("UDO_GSET")


        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED)
        'objFilter.AddEx("UDO_GSET")
        'objFilter.AddEx("CflGrnInsp")
        'objFilter.AddEx("UDO_INSP")


        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_LOST_FOCUS)
        'objFilter.AddEx("UDO_GSET")
        'objFilter.AddEx("UDO_INSP")


        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_KEY_DOWN)
        objFilter.AddEx("UDO_BAAS")

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST)
        'objFilter.AddEx("UDO_GSET")
        'objFilter.AddEx("CflGrnInsp")
        'objFilter.AddEx("UDO_INSP")
        objFilter.AddEx("UDO_BAAS")

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_CLICK)
        'objFilter.AddEx("UDO_INSP")
        objFilter.AddEx("UDO_BAAS")

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_LOAD)
        'objFilter.AddEx("UDO_GSET")
        'objFilter.AddEx("UDO_INSP")
        objFilter.AddEx("UDO_BAAS")

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD)
        'objFilter.AddEx("UDO_GSET")
        'objFilter.AddEx("UDO_INSP")
        objFilter.AddEx("UDO_BAAS")

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD)
        'objFilter.AddEx("UDO_GSET")
        'objFilter.AddEx("UDO_INSP")
        objFilter.AddEx("UDO_BAAS")

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_RIGHT_CLICK)


        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE)
        'objFilter.AddEx("UDO_GSET")
        objFilter.AddEx("UDO_BAAS")

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_RESIZE)

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_CLOSE)

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_ACTIVATE)
        'objFilter.AddEx("UDO_INSP")


        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_GOT_FOCUS)
        'objFilter.AddEx("UDO_INSP")


        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_DOUBLE_CLICK)

        objFilter = objFilters.Add(SAPbouiCOM.BoEventTypes.et_PICKER_CLICKED)

        SetFilter(objFilters)

    End Sub

#End Region

#Region "Create Objects"
    '*****************************************************************
    'Type               : Procedure    
    'Name               : createObjects
    'Parameter          : 
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Create instants for the Class
    '*****************************************************************  
    Private Sub createobjects()
        objBlanketInv = New clsBlanketInvoices(Me)
        'objGenSetting = New clsGeneralSetting(Me)
        'objInspection = New clsInspection(Me)
        'objCFLInspection = New clsCFLInspection(Me)
    End Sub

#End Region

#Region "Database Function"

#Region "Get Code"
    '*****************************************************************
    'Type               : Function    
    'Name               : GetCode
    'Parameter          : Tablename
    'Return Value       : Maximum Code value in String Format
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Get Maximum Code field value for given Table
    '*****************************************************************
    Public Function GetCode(ByVal sTableName As String) As String
        'Dim oRec As SAPbobsCOM.Recordset
        Dim sQuery As String
        'Dim intCode As Integer
        Try
            Dim oRecSet As SAPbobsCOM.Recordset
            oRecSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            sQuery = "SELECT Top 1 ""Code"" FROM """ & sTableName + """ ORDER BY Cast(""Code"" as Integer) desc"
            oRecSet.DoQuery(sQuery)
            If Not oRecSet.EoF Then
                GetCode = Convert.ToInt32(oRecSet.Fields.Item(0).Value.ToString()) + 1
            Else
                GetCode = "1"
            End If

        Catch ex As Exception
            MsgBox("GetCode: " + ex.Message)
            Return ""
        End Try
    End Function

#End Region

#Region "Add Column"

    '*****************************************************************
    'Type               : Procedure    
    'Name               : AddCol
    'Parameter          : Tablename,FieldName,TableDescription,FieldType,Size,Sub Field Type
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Add Field to Table
    '*****************************************************************
    Private Sub AddCol(ByVal strTab As String, ByVal strCol As String, ByVal strDesc As String, ByVal nType As Integer, Optional ByVal nEditSize As Integer = 10, Optional ByVal nSubType As Integer = 0)

        Dim oUFields As SAPbobsCOM.UserFieldsMD
        Dim nError As Integer

        oUFields = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields)
        oUFields.TableName = strTab
        oUFields.Name = strCol
        oUFields.Type = nType
        oUFields.SubType = nSubType
        oUFields.Description = strDesc
        oUFields.EditSize = nEditSize
        nError = oUFields.Add()
        System.Runtime.InteropServices.Marshal.ReleaseComObject(oUFields)
        GC.Collect()
        GC.WaitForPendingFinalizers()
        If nError <> 0 Then
            'MsgBox(strCol & " table could not be added")
        End If
    End Sub

#End Region

#Region "Create Table"
    '*****************************************************************
    'Type               : Function    
    'Name               : CreateTable
    'Parameter          : Tablename,TableDescription,TableType
    'Return Value       : Boolean
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Create New  Table
    '*****************************************************************

    Public Function CreateTable(ByVal TableName As String, ByVal TableDescription As String, ByVal TableType As SAPbobsCOM.BoUTBTableType) As Boolean
        Dim intRetCode As Integer
        'Dim nError As Integer
        'Dim strColname As String
        Dim objUserTableMD As SAPbobsCOM.UserTablesMD
        objUserTableMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables)
        Try
            If (Not objUserTableMD.GetByKey(TableName)) Then
                objUserTableMD.TableName = TableName
                objUserTableMD.TableDescription = TableDescription
                objUserTableMD.TableType = TableType
                intRetCode = objUserTableMD.Add()
                If (intRetCode = 0) Then
                    Return True
                End If
            Else
                Return False
            End If
        Catch ex As Exception
            SBO_Appln.MessageBox(ex.Message)

        Finally
            System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserTableMD)
            GC.Collect()

        End Try
        Return True
    End Function

#End Region

#Region "Field Creations"
    '*****************************************************************
    'Type               : Procedure    
    'Name               : AddAlphaField
    'Parameter          : Tablename,FieldName,TableDescription,Size
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Add Alphabet Field to Table
    '*****************************************************************

    'Public Sub AddAlphaField(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal Size As Integer, ByVal blnSysTable As Boolean)
    '    Try
    '        If blnSysTable = False Then
    '            addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Alpha, Size, SAPbobsCOM.BoFldSubTypes.st_None, "", "", "", False)
    '        Else
    '            addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Alpha, Size, SAPbobsCOM.BoFldSubTypes.st_None, "", "", "", True)
    '        End If

    '    Catch ex As Exception
    '        MsgBox(ex.Message)
    '    End Try
    'End Sub
    '*****************************************************************
    'Type               : Procedure    
    'Name               : AddAlphaMemoField
    'Parameter          : Tablename,FieldName,TableDescription,Size
    'Return Value       : 
    '4or             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Add Alphabet Memo Field to Table
    '*****************************************************************


    Public Sub AddAlphaMemoField(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal Size As Integer, ByVal blnSysTable As Boolean)

        Try
            If blnSysTable = False Then
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Memo, Size, SAPbobsCOM.BoFldSubTypes.st_None, "", "", "", "", False)
            Else
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Memo, Size, SAPbobsCOM.BoFldSubTypes.st_None, "", "", "", "", True)
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub


    Public Sub AddAlphaMemoField_Link(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal Size As Integer, ByVal blnSysTable As Boolean)

        Try
            If blnSysTable = False Then
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Memo, Size, SAPbobsCOM.BoFldSubTypes.st_Link, "", "", "", "", False)
            Else
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Memo, Size, SAPbobsCOM.BoFldSubTypes.st_Link, "", "", "", "", True)
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Public Sub AddAlphaFieldDefault(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal Size As Integer, ByVal ValidValues As String, ByVal ValidDescriptions As String, ByVal SetValidValue As String, ByVal Defaultval As String, ByVal blnSysTable As Boolean)
        Try
            If blnSysTable = False Then
                addFieldDefault(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Alpha, Size, SAPbobsCOM.BoFldSubTypes.st_None, ValidValues, ValidDescriptions, SetValidValue, Defaultval, False)
            Else
                addFieldDefault(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Alpha, Size, SAPbobsCOM.BoFldSubTypes.st_None, ValidValues, ValidDescriptions, SetValidValue, Defaultval, True)
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Sub addFieldDefault(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal FieldType As SAPbobsCOM.BoFieldTypes, ByVal Size As Integer, ByVal SubType As SAPbobsCOM.BoFldSubTypes, ByVal ValidValues As String, ByVal ValidDescriptions As String, ByVal SetValidValue As String, ByVal Defaultval As String, ByVal blnSysTable As Boolean)
        Dim intLoop As Integer
        Dim strValue, strDesc As Array
        Dim objUserFieldMD As SAPbobsCOM.UserFieldsMD = Nothing
        Try

            strValue = ValidValues.Split(Convert.ToChar(","))
            strDesc = ValidDescriptions.Split(Convert.ToChar(","))
            If (strValue.GetLength(0) <> strDesc.GetLength(0)) Then
                Throw New Exception("Invalid Valid Values")
            End If

            objUserFieldMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields)

            If blnSysTable = True Then
                If (Not isColumnExist(TableName, ColumnName)) Then
                    objUserFieldMD.TableName = TableName
                    objUserFieldMD.Name = ColumnName
                    objUserFieldMD.Description = ColDescription
                    objUserFieldMD.Type = FieldType
                    If (FieldType <> SAPbobsCOM.BoFieldTypes.db_Numeric) Then
                        objUserFieldMD.Size = Size
                    Else
                        objUserFieldMD.EditSize = Size
                    End If
                    objUserFieldMD.SubType = SubType
                    objUserFieldMD.DefaultValue = SetValidValue
                    For intLoop = 0 To strValue.GetLength(0) - 1
                        objUserFieldMD.ValidValues.Value = strValue(intLoop)
                        objUserFieldMD.ValidValues.Description = strDesc(intLoop)
                        objUserFieldMD.ValidValues.Add()
                    Next
                    objUserFieldMD.DefaultValue = Defaultval
                    If (objUserFieldMD.Add() <> 0) Then
                        updateLastErrorDetails(-104)
                    End If
                End If
            Else
                If (Not isUDTColumnExist(TableName, ColumnName)) Then
                    objUserFieldMD.TableName = TableName
                    objUserFieldMD.Name = ColumnName
                    objUserFieldMD.Description = ColDescription
                    objUserFieldMD.Type = FieldType
                    If (FieldType <> SAPbobsCOM.BoFieldTypes.db_Numeric) Then
                        objUserFieldMD.Size = Size
                    Else
                        objUserFieldMD.EditSize = Size
                    End If
                    objUserFieldMD.SubType = SubType
                    objUserFieldMD.DefaultValue = SetValidValue
                    For intLoop = 0 To strValue.GetLength(0) - 1
                        objUserFieldMD.ValidValues.Value = strValue(intLoop)
                        objUserFieldMD.ValidValues.Description = strDesc(intLoop)
                        objUserFieldMD.ValidValues.Add()
                    Next
                    objUserFieldMD.DefaultValue = Defaultval
                    If (objUserFieldMD.Add() <> 0) Then
                        updateLastErrorDetails(-104)
                    End If
                End If
            End If

        Catch ex As Exception
            MsgBox(ex.Message)

        Finally

            System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserFieldMD)
            GC.Collect()

        End Try


    End Sub
    '*****************************************************************
    'Type               : Procedure    
    'Name               : AddAlphaField
    'Parameter          : Tablename,FieldName,TableDescription,Size,Validvalues,Description,Default Value
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Add Alpha Field to Table and add Validvalues and set Default Values
    '*****************************************************************
    Public Sub AddAlphaField(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal Size As Integer, ByVal ValidValues As String, ByVal ValidDescriptions As String, ByVal SetValidValue As String, ByVal SetLinkedValue As String, ByVal blnSysTable As Boolean)
        Try
            If blnSysTable = False Then
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Alpha, Size, SAPbobsCOM.BoFldSubTypes.st_None, ValidValues, ValidDescriptions, SetValidValue, SetLinkedValue, False)
            Else
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Alpha, Size, SAPbobsCOM.BoFldSubTypes.st_None, ValidValues, ValidDescriptions, SetValidValue, SetLinkedValue, True)
            End If

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    '*****************************************************************
    'Type               : Procedure    
    'Name               : addField
    'Parameter          : Tablename,FieldName,columnDescription,FieldType,Size,SubType,Validvalues,Description,Default Value
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Add Field to Table 
    '*****************************************************************

    Public Sub addField(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal FieldType As SAPbobsCOM.BoFieldTypes, ByVal Size As Integer, ByVal SubType As SAPbobsCOM.BoFldSubTypes, ByVal ValidValues As String, ByVal ValidDescriptions As String, ByVal SetValidValue As String, ByVal SetLinkedValue As String, ByVal blnSysTable As Boolean)
        Dim intLoop As Integer
        Dim strValue, strDesc As Array
        Dim objUserFieldMD As SAPbobsCOM.UserFieldsMD = Nothing
        Try

            strValue = ValidValues.Split(Convert.ToChar(","))
            strDesc = ValidDescriptions.Split(Convert.ToChar(","))
            If (strValue.GetLength(0) <> strDesc.GetLength(0)) Then
                Throw New Exception("Invalid Valid Values")
            End If

            objUserFieldMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields)

            If blnSysTable = True Then
                If (Not isColumnExist(TableName, ColumnName)) Then
                    objUserFieldMD.TableName = TableName
                    objUserFieldMD.Name = ColumnName
                    objUserFieldMD.Description = ColDescription
                    objUserFieldMD.Type = FieldType
                    If (FieldType <> SAPbobsCOM.BoFieldTypes.db_Numeric) Then
                        objUserFieldMD.Size = Size
                    Else
                        objUserFieldMD.EditSize = Size
                    End If
                    objUserFieldMD.SubType = SubType
                    objUserFieldMD.DefaultValue = SetValidValue
                    If Not SetValidValue = "" Then
                        For intLoop = 0 To strValue.GetLength(0) - 1
                            objUserFieldMD.ValidValues.Value = strValue(intLoop)
                            objUserFieldMD.ValidValues.Description = strDesc(intLoop)
                            objUserFieldMD.ValidValues.Add()
                        Next
                    End If
                    If (objUserFieldMD.Add() <> 0) Then
                        updateLastErrorDetails(-104)
                    End If
                End If
            Else
                If (Not isUDTColumnExist(TableName, ColumnName)) Then
                    objUserFieldMD.TableName = TableName
                    objUserFieldMD.Name = ColumnName
                    objUserFieldMD.Description = ColDescription
                    objUserFieldMD.Type = FieldType
                    If (FieldType <> SAPbobsCOM.BoFieldTypes.db_Numeric) Then
                        objUserFieldMD.Size = Size
                    Else
                        objUserFieldMD.EditSize = Size
                    End If
                    objUserFieldMD.SubType = SubType
                    objUserFieldMD.DefaultValue = SetValidValue
                    If Not SetValidValue = "" Then
                        For intLoop = 0 To strValue.GetLength(0) - 1
                            objUserFieldMD.ValidValues.Value = strValue(intLoop)
                            objUserFieldMD.ValidValues.Description = strDesc(intLoop)
                            objUserFieldMD.ValidValues.Add()
                        Next
                    End If
                    If Not SetLinkedValue = "" Then
                        objUserFieldMD.LinkedTable = SetLinkedValue
                    End If
                    If (objUserFieldMD.Add() <> 0) Then
                        updateLastErrorDetails(-104)
                    End If
                End If
            End If

        Catch ex As Exception
            MsgBox(ex.Message)

        Finally

            System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserFieldMD)
            GC.Collect()

        End Try


    End Sub

    '*****************************************************************
    'Type               : Procedure    
    'Name               : AddNumericField
    'Parameter          : Tablename,FieldName,columnDescription,Size
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Add  Numeric Field to Table 
    '*****************************************************************

    Public Sub AddNumericField(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal Size As Integer, ByVal blnSysTable As Boolean)
        Try
            If blnSysTable = False Then
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Numeric, Size, SAPbobsCOM.BoFldSubTypes.st_None, "", "", "", "", False)
            Else
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Numeric, Size, SAPbobsCOM.BoFldSubTypes.st_None, "", "", "", "", True)
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub


    '******************************************************************************************************
    'Type               : Procedure    
    'Name               : AddNumericField
    'Parameter          : Tablename,FieldName,ColumnDescription,Size,Validvalues,Description,Default Value
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Add Numeric Field to Table and add Validvalues and set Default Values
    '********************************************************************************************************

    Public Sub AddNumericField(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal Size As Integer, ByVal ValidValues As String, ByVal ValidDescriptions As String, ByVal DefultValue As String, ByVal blnSysTable As Boolean)
        Try
            If blnSysTable = False Then
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Numeric, Size, SAPbobsCOM.BoFldSubTypes.st_None, ValidValues, ValidDescriptions, DefultValue, "", False)
            Else
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Numeric, Size, SAPbobsCOM.BoFldSubTypes.st_None, ValidValues, ValidDescriptions, DefultValue, "", True)
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    '*****************************************************************
    'Type               : Procedure    
    'Name               : AddFloatField
    'Parameter          : Tablename,FieldName,columnDescription,SubType
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Add  Float Field to Table 
    '*****************************************************************

    Public Sub AddFloatField(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal SubType As SAPbobsCOM.BoFldSubTypes, ByVal blnSysTable As Boolean)
        Try
            If blnSysTable = False Then
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Float, 0, SubType, "", "", "", "", False)
            Else
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Float, 0, SubType, "", "", "", "", True)
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    '*****************************************************************
    'Type               : Procedure    
    'Name               : AddDateField
    'Parameter          : Tablename,FieldName,columnDescription,SubType
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Add  Date Field to Table 
    '*****************************************************************

    Public Sub AddDateField(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal SubType As SAPbobsCOM.BoFldSubTypes, ByVal blnSysTable As Boolean)
        Try
            If blnSysTable = False Then
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Date, 0, SubType, "", "", "", "", False)
            Else
                addField(TableName, ColumnName, ColDescription, SAPbobsCOM.BoFieldTypes.db_Date, 0, SubType, "", "", "", "", True)
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    '*****************************************************************
    'Type               : Function   
    'Name               : isColumnExist
    'Parameter          : Tablename,FieldName
    'Return Value       : Boolean
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Verify the Given Field already Exists or not
    '*****************************************************************

    Private Function isColumnExist(ByVal TableName As String, ByVal ColumnName As String) As Boolean
        Dim objRecordSet As SAPbobsCOM.Recordset
        objRecordSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        Try
            ' objRecordSet.DoQuery("SELECT COUNT(*) FROM CUFD WHERE TableID = '" & TableName & "' AND AliasID = '" & ColumnName & "'")

            objRecordSet.DoQuery("SELECT COUNT(*) FROM ""CUFD"" WHERE ""TableID"" = '" & TableName & "' AND ""AliasID"" = '" & ColumnName & "'")
            If (Convert.ToInt16(objRecordSet.Fields.Item(0).Value) <> 0) Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Throw ex
        Finally
            System.Runtime.InteropServices.Marshal.ReleaseComObject(objRecordSet)
            GC.Collect()
        End Try

    End Function

    Private Function isUDTColumnExist(ByVal TableName As String, ByVal ColumnName As String) As Boolean
        Dim objRecordSet As SAPbobsCOM.Recordset
        objRecordSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        TableName = "@" + TableName
        Try
            'objRecordSet.DoQuery("SELECT COUNT(*) FROM CUFD WHERE TableID = '" & TableName & "' AND AliasID = '" & ColumnName & "'")
            objRecordSet.DoQuery("SELECT COUNT(*) FROM ""CUFD"" WHERE ""TableID"" = '" & TableName & "' AND ""AliasID"" = '" & ColumnName & "'")

            If (Convert.ToInt16(objRecordSet.Fields.Item(0).Value) <> 0) Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Throw ex
        Finally
            System.Runtime.InteropServices.Marshal.ReleaseComObject(objRecordSet)
            GC.Collect()
        End Try

    End Function

#End Region

#Region "To Create UDO"

    Public Function CreateUDODocument(ByVal strUDO As String,
                            ByVal strDesc As String,
                                ByVal strTable As String,
                                    ByVal intFind As Integer,
                                        Optional ByVal strCode As String = "",
                                            Optional ByVal strName As String = "",
                                                Optional ByVal strChildTable As String = "",
                                                Optional ByVal strChildTable2 As String = "",
                                                Optional ByVal strChildTable3 As String = "",
                                                 Optional ByVal strChildTable4 As String = "",
                                                Optional ByVal strChildTable5 As String = "",
                                                Optional ByVal strChildTable6 As String = "",
                                                Optional ByVal strChildTable7 As String = "",
                                                Optional ByVal strChildTable8 As String = "",
                                                Optional ByVal strChildTable9 As String = "",
                                                Optional ByVal strChildTable10 As String = "",
                                                Optional ByVal strChildTable11 As String = "",
                                                Optional ByVal strChildTable12 As String = "",
                                                Optional ByVal strChildTable13 As String = "",
                                        Optional ByVal strChildTable14 As String = "") As Boolean
        Dim oUserObjects As SAPbobsCOM.UserObjectsMD
        'Dim lngRet As Long

        Try
            oUserObjects = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD)
            If oUserObjects.GetByKey(strUDO) = 0 Then
                oUserObjects.CanCancel = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.CanClose = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.CanCreateDefaultForm = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.CanDelete = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.CanFind = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.CanLog = SAPbobsCOM.BoYesNoEnum.tNO
                oUserObjects.LogTableName = ""
                oUserObjects.CanYearTransfer = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.ExtensionName = ""
                oUserObjects.ManageSeries = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.Code = strUDO
                oUserObjects.Name = strDesc
                oUserObjects.ObjectType = SAPbobsCOM.BoUDOObjType.boud_Document
                oUserObjects.TableName = strTable
                If strCode <> "" Then
                    If oUserObjects.CanFind = 1 Then
                        oUserObjects.FindColumns.ColumnAlias = strCode
                        oUserObjects.FindColumns.Add()
                        oUserObjects.FindColumns.SetCurrentLine(1)
                        oUserObjects.FindColumns.ColumnAlias = strName
                        oUserObjects.FindColumns.Add()
                    End If
                End If
                Try
                    If strChildTable <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(1)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable2 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable2
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(2)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable3 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable3
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(3)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable4 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable4
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(4)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable5 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable5
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(5)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable6 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable6
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(6)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable7 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable7
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(7)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable8 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable8
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(8)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable9 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable9
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(9)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable10 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable10
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(10)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable11 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable11
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(11)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable12 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable12
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(12)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable13 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable13
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(13)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable14 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable14
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(14)
                    End If
                Catch ex As Exception
                End Try
                If oUserObjects.Add() <> 0 Then
                    MsgBox(oCompany.GetLastErrorDescription)
                    MsgBox("Error adding UDO Document Data")
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserObjects)
                    oUserObjects = Nothing
                    Return False
                End If
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserObjects)
                oUserObjects = Nothing
                Return True
            End If
        Catch ex As Exception
            Throw ex
        Finally
            oUserObjects = Nothing
            GC.Collect()
        End Try
        Return True
    End Function

    Public Function CreateUDOMaster(ByVal strUDO As String,
                              ByVal strDesc As String,
                                  ByVal strTable As String,
                                      ByVal intFind As Integer,
                                          Optional ByVal strCode As String = "",
                                              Optional ByVal strName As String = "",
                                                  Optional ByVal strChildTable As String = "",
                                                  Optional ByVal strChildTable2 As String = "",
                                                  Optional ByVal strChildTable3 As String = "",
                                                   Optional ByVal strChildTable4 As String = "",
                                                  Optional ByVal strChildTable5 As String = "",
                                                  Optional ByVal strChildTable6 As String = "",
                                                  Optional ByVal strChildTable7 As String = "",
                                                  Optional ByVal strChildTable8 As String = "",
                                                  Optional ByVal strChildTable9 As String = "",
                                                  Optional ByVal strChildTable10 As String = "",
                                                  Optional ByVal strChildTable11 As String = "",
                                                  Optional ByVal strChildTable12 As String = "",
                                                  Optional ByVal strChildTable13 As String = "",
                                          Optional ByVal strChildTable14 As String = "") As Boolean
        Dim oUserObjects As SAPbobsCOM.UserObjectsMD
        'Dim lngRet As Long

        Try
            oUserObjects = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD)
            If oUserObjects.GetByKey(strUDO) = 0 Then
                oUserObjects.CanCancel = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.CanClose = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.CanCreateDefaultForm = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.CanDelete = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.CanFind = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.CanLog = SAPbobsCOM.BoYesNoEnum.tNO
                oUserObjects.LogTableName = ""
                oUserObjects.CanYearTransfer = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.ExtensionName = ""
                oUserObjects.ManageSeries = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjects.Code = strUDO
                oUserObjects.Name = strDesc
                oUserObjects.ObjectType = SAPbobsCOM.BoUDOObjType.boud_MasterData
                oUserObjects.TableName = strTable
                If strCode <> "" Then
                    If oUserObjects.CanFind = 1 Then
                        oUserObjects.FindColumns.ColumnAlias = strCode
                        oUserObjects.FindColumns.Add()
                        oUserObjects.FindColumns.SetCurrentLine(1)
                        oUserObjects.FindColumns.ColumnAlias = strName
                        oUserObjects.FindColumns.Add()
                    End If
                End If
                Try
                    If strChildTable <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(1)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable2 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable2
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(2)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable3 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable3
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(3)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable4 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable4
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(4)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable5 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable5
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(5)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable6 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable6
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(6)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable7 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable7
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(7)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable8 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable8
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(8)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable9 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable9
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(9)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable10 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable10
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(10)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable11 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable11
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(11)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable12 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable12
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(12)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable13 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable13
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(13)
                    End If
                Catch ex As Exception
                End Try
                Try
                    If strChildTable14 <> "" Then
                        oUserObjects.ChildTables.TableName = strChildTable14
                        oUserObjects.ChildTables.Add()
                        oUserObjects.ChildTables.SetCurrentLine(14)
                    End If
                Catch ex As Exception
                End Try
                If oUserObjects.Add() <> 0 Then
                    MsgBox(oCompany.GetLastErrorDescription)
                    MsgBox("Error adding UDO master Data")
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserObjects)
                    oUserObjects = Nothing
                    Return False
                End If
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserObjects)
                oUserObjects = Nothing
                Return True
            End If
        Catch ex As Exception
            Throw ex
        Finally
            oUserObjects = Nothing
            GC.Collect()
        End Try
        Return True
    End Function

    'Public Function CreatedUDO_Inspection() As Boolean
    '    If (Not CreateUDODocument("UDO_INSP", "QC.Inspection", "QL_OINSP", "1", "DocNum", "CreateDate", "QL_INSP1")) Then Return False
    '    Return True
    'End Function

    'Public Function CreatedUDO_GeneralSetting() As Boolean
    '    If (Not CreateUDOMaster("UDO_GSET", "QC.General.Setting", "QL_OGSET", "1", "Code", "Name", "QL_GSET1")) Then Return False
    '    Return True
    'End Function

    'Public Function CreatedUDO_CFLInspection() As Boolean
    '    If (Not CreateUDOMaster("UDO_OCFL", "QC.Inspection.CFL", "QL_OCFL", "1", "Code", "Name", "QL_CFL1")) Then Return False
    '    Return True
    'End Function

    Public Function CreatedUDO_BlanketAgreement() As Boolean
        If (Not CreateUDODocument("UDO_BAAS", "Blanket_Agreement_Automation", "QL_OBAA", "1", "DocNum", "Series", "QL_BAA1")) Then Return False
        Return True
    End Function

    'Routing process

    'General Setting
#End Region

#End Region ' 

#Region "Load Form"
    '*****************************************************************
    'Type               : Function   
    'Name               : LoadForm
    'Parameter          : XmlFile,FormType
    'Return Value       : SBO Form
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Load XML file based on FormType
    '*****************************************************************

    Public Function LoadForm(ByVal XMLFile As String, ByVal FormType As String) As SAPbouiCOM.Form
        Return LoadForm(XMLFile, FormType.ToString(), FormType & "_" & SBO_Appln.Forms.Count.ToString)
    End Function

    '*****************************************************************
    'Type               : Function   
    'Name               : LoadForm
    'Parameter          : XmlFile,FormType,FormUID
    'Return Value       : SBO Form
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Load XML file 
    '*****************************************************************

    Public Function LoadForm(ByVal XMLFile As String, ByVal FormType As String, ByVal FormUID As String) As SAPbouiCOM.Form

        Dim oXML As System.Xml.XmlDocument
        Dim objFormCreationParams As SAPbouiCOM.FormCreationParams
        Try
            oXML = New System.Xml.XmlDocument
            oXML.Load(XMLFile)
            objFormCreationParams = (objApplication.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams))
            objFormCreationParams.XmlData = oXML.InnerXml
            objFormCreationParams.FormType = FormType
            objFormCreationParams.UniqueID = FormUID
            Return objApplication.Forms.AddEx(objFormCreationParams)
        Catch ex As Exception
            Throw ex

        End Try


    End Function

    Public Function FormExist(ByVal FormID As String) As String
        FormExist = False
        For Each uid As SAPbouiCOM.Form In objApplication.Forms
            Dim arrvalue() As String = Split(uid.UniqueID, "_")
            If UBound(arrvalue) > 0 Then
                If arrvalue(LBound(arrvalue)) = FormID Then
                    FormExist = uid.UniqueID
                    Exit Function
                End If
            Else
                If uid.UniqueID = FormID Then
                    FormExist = ""
                    Exit Function
                End If
            End If

        Next
        If FormExist Then
            objApplication.Forms.Item(FormID).Visible = True
            objApplication.Forms.Item(FormID).Select()
        End If

    End Function

    Public Sub FormExist1(ByVal FormID As String)
        For Each uid As SAPbouiCOM.Form In objApplication.Forms
            Dim arrvalue() As String = Split(uid.UniqueID, "_")
            If UBound(arrvalue) > 0 Then
                If arrvalue(LBound(arrvalue)) = FormID Then
                    Dim objActiveForm, ThisForm As String
                    objActiveForm = SBO_Appln.Forms.ActiveForm.UniqueID
                    ThisForm = objApplication.Forms.Item(uid.UniqueID).UniqueID
                    If objActiveForm <> ThisForm Then
                        objApplication.Forms.Item(uid.UniqueID).Visible = True
                        objApplication.Forms.Item(uid.UniqueID).Select()
                        Exit For
                    End If
                End If
           
            End If

        Next

    End Sub

    Public Function LoadXML(ByVal FormId As String, ByVal FormXML As String) As SAPbouiCOM.Form
        Try
            '******************Add XML ***********************************
            Dim xmldoc As New System.Xml.XmlDocument()

            Dim str_CallingProjectName As String = System.Reflection.Assembly.GetCallingAssembly().GetName().Name

            Dim stream As System.IO.Stream = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream((str_CallingProjectName & "." & FormXML))

            Dim streamreader As New System.IO.StreamReader(stream, True)

            xmldoc.LoadXml(streamreader.ReadToEnd())

            streamreader.Close()

            Dim strTemp As String = xmldoc.InnerXml.ToString()

            objApplication.LoadBatchActions(strTemp)
            '*************************************************************
            Return objApplication.Forms.Item(FormId)

        Catch ex As Exception
            'Throw
            'ExceptionHandler(ex)
            'Msg(str_LoadXML_Method_Failed, "S", "W")
            Return Nothing
        End Try
    End Function

    '*****************************************************************
    'Type               : Procedure   
    'Name               : LoadForm
    'Parameter          : XmlFile
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Load SBO Form
    '*****************************************************************

    Public Sub LoadFromXML(ByRef FileName As String)
        Dim oXmlDoc As Xml.XmlDocument
        oXmlDoc = New Xml.XmlDocument
        'Dim sPath As String
        Try
            oXmlDoc.Load(FileName)
            SBO_Appln.LoadBatchActions(oXmlDoc.InnerXml)
        Catch ex As Exception
            MsgBox("Load From XML Exception: " + ex.Message)
        End Try

    End Sub

    '*****************************************************************
    'Type               : Procedure   
    'Name               : LoadMenu
    'Parameter          : XmlFile
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Load Menu Item 
    '*****************************************************************

    Public Sub LoadMenu(ByVal XMLFile As String)
        Dim oXML As System.Xml.XmlDocument
        Dim strXML As String
        Try
            oXML = New System.Xml.XmlDocument
            oXML.Load(XMLFile)
            strXML = oXML.InnerXml()
            objApplication.LoadBatchActions(strXML)

        Catch ex As Exception
            MsgBox("Load Menu Exception: " + ex.Message)
        End Try
    End Sub

#End Region

#Region "DI /UI Methods"

#Region "GetDateTime"

    '*****************************************************************
    'Type               : Function   
    'Name               : GetDateTimeValue
    'Parameter          : DateString
    'Return Value       : DateFormate
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Convert given string into dateTime Format
    '*****************************************************************

    Public Function GetDateTimeValue(ByVal DateString As String) As DateTime
        Dim objBridge As SAPbobsCOM.SBObob
        objBridge = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge)
        Return objBridge.Format_StringToDate(DateString).Fields.Item(0).Value
    End Function

    '*****************************************************************
    'Type               : Function   
    'Name               : GetSBODateString
    'Parameter          : DateTime
    'Return Value       : String
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Convert given  dateTime Format into string format
    '*****************************************************************
    Public Function GetSBODateString(ByVal DateVal As DateTime) As String
        Dim objBridge As SAPbobsCOM.SBObob
        objBridge = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge)
        Return objBridge.Format_DateToString(DateVal).Fields.Item(0).Value
    End Function


#End Region

#Region "Business Objects"

    '*****************************************************************
    'Type               : Function   
    'Name               : GetBusinessObject
    'Parameter          : BOobjectTypes
    'Return Value       : Object
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Create instance to the give object
    '*****************************************************************
    Public Function GetBusinessObject(ByVal ObjectType As SAPbobsCOM.BoObjectTypes) As Object
        Return oCompany.GetBusinessObject(ObjectType)

    End Function


    '*****************************************************************
    'Type               : Function   
    'Name               : CreateUIObject
    'Parameter          : BOCreatableobjectType
    'Return Value       : Object
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Create instance to the give UIObject
    '*****************************************************************
    Public Function CreateUIObject(ByVal Type As SAPbouiCOM.BoCreatableObjectType) As Object
        Return objApplication.CreateObject(Type)
    End Function

#End Region

#Region "Form Objects"


    '*****************************************************************
    'Type               : Function   
    'Name               : GetForm
    'Parameter          : FormUID
    'Return Value       : Form
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Get SBOForm object for given FormUID
    '*****************************************************************
    Public Function GetForm(ByVal FormUID As String) As SAPbouiCOM.Form
        Return SBO_Appln.Forms.Item(FormUID)
    End Function

    '************************************************************************
    'Type               : Function   
    'Name               : GetForm
    'Parameter          : FormType,Count
    'Return Value       : Form
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Get SBOForm object for given FormType,FormTypecount
    '****************************************************************************
    Public Function GetForm(ByVal Type As String, ByVal Count As Integer) As SAPbouiCOM.Form
        Return SBO_Appln.Forms.GetForm(Type, Count)
    End Function

#End Region

#Region "GetEditTextValue"
    '*****************************************************************
    'Type               : Function   
    'Name               : GetEditText
    'Parameter          : SBOForm,ItemUID / FormUID,ItemUID
    'Return Value       : String
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Return Edit Text Value
    '*****************************************************************
    Public Function GetEditText(ByVal aForm As SAPbouiCOM.Form, ByVal aUID As String) As String
        objEdit = aForm.Items.Item(aUID).Specific
        Return Convert.ToString(objEdit.Value)
    End Function
    Public Function GetEditText(ByVal aFormUID As String, ByVal aUID As String) As String
        objform = SBO_Appln.Forms.Item(aFormUID)
        objEdit = objform.Items.Item(aUID).Specific
        Return Convert.ToString(objEdit.Value)
    End Function
#End Region

#Region "SetEditTextValue"
    '*****************************************************************
    'Type               : Procedure
    'Name               : SetEditText
    'Parameter          : SBOForm,ItemUID,Value / SBOFormUID,ItemUID,value
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To set Value to Edit Text Box
    '*****************************************************************

    Public Sub SetEditText(ByVal aForm As SAPbouiCOM.Form, ByVal aUID As String, ByVal aVal As String)
        objEdit = aForm.Items.Item(aUID).Specific
        objEdit.Value = aVal
    End Sub
    Public Sub SetEditText(ByVal aFormUID As String, ByVal aUID As String, ByVal aVal As String)
        objform = SBO_Appln.Forms.Item(aFormUID)
        objEdit = objform.Items.Item(aUID).Specific
        objEdit.Value = aVal
    End Sub

#End Region

#Region "Get Tax Rate"
    '*****************************************************************
    'Type               : Function   
    'Name               : GetTaxRate
    'Parameter          : StrCode
    'Return Value       : string
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Get Tax Value for give Item code
    '*****************************************************************
    Public Function GetTaxRate(ByVal strCode As String) As String
        Dim rsCurr As SAPbobsCOM.Recordset
        Dim strsql, GetTaxRate1 As String
        strsql = ""
        GetTaxRate1 = ""
        strsql = "Select ""Rate"" from ""OVTG"" where ""Code""='" & strCode + "'"
        rsCurr = GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        rsCurr.DoQuery(strsql)
        GetTaxRate1 = rsCurr.Fields.Item(0).Value
        Return GetTaxRate1
    End Function

#End Region

#End Region

#Region "System currencies"
    Public Function GetLocalCurrency() As String
        Dim vObj As SAPbobsCOM.SBObob
        Dim rs As SAPbobsCOM.Recordset
        Dim strResult As String
        vObj = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge)
        rs = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        rs = vObj.GetLocalCurrency()
        strResult = rs.Fields.Item(0).Value
        Return (strResult)
    End Function

    Public Function GetSystemCurrency() As String
        Dim vObj As SAPbobsCOM.SBObob
        Dim rs As SAPbobsCOM.Recordset
        Dim strResult As String
        vObj = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge)
        rs = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        rs = vObj.GetSystemCurrency
        strResult = rs.Fields.Item(0).Value
        Return (strResult)
    End Function
#End Region

#End Region

#Region "Events"

#Region "FormData Event"
    Public Sub SBO_Appln_FormDataEvent(ByRef pVal As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles SBO_Appln.FormDataEvent
        BubbleEvent = True
        Select Case pVal.FormTypeEx

            'Case "UDO_GSET"
            '    If Not (pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD Or pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE) Then
            '        objGenSetting.SBO_Appln_FormDataEvent(pVal, BubbleEvent)
            '    End If

            'Case "UDO_INSP"
            '    If Not (pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD Or pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE) Then
            '        objInspection.SBO_Appln_FormDataEvent(pVal, BubbleEvent)
            '    End If
            Case "UDO_BAAS"
                If Not (pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD Or pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE) Then
                    objBlanketInv.SBO_Appln_FormDataEvent(pVal, BubbleEvent)
                End If
        End Select
    End Sub
#End Region

#Region "Item Event"
    '*****************************************************************
    'Type               : Procedure    
    'Name               : SBO_Appln_ItemEvent
    'Parameter          : FormUID, ItemEvent, BubbleEvent
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Handle Item Level Events
    '******************************************************************

    'Private Function ReadTextFile(ByVal sFileName As String) As String

    '    Dim s As String

    '    Try
    '        Dim oFile As FileStream = New FileStream(sFileName, FileMode.Open, FileAccess.Read, FileShare.Read)
    '        Dim oReader As StreamReader = New StreamReader(oFile)

    '        s = oReader.ReadToEnd

    '        oReader.Close()
    '        oFile.Close()

    '        ReadTextFile = s

    '    Catch

    '        ReadTextFile = "Unable to open file."

    '    End Try

    'End Function

    'Private Sub releaseObject(ByRef obj As Object)
    '    Try
    '        System.Runtime.InteropServices.Marshal.ReleaseComObject(obj)
    '        obj = Nothing
    '    Catch ex As Exception
    '        obj = Nothing
    '    Finally
    '        GC.Collect()
    '    End Try
    'End Sub

    'Private Sub ShowFolderBrowser_Text()
    '    Dim MyTest As New OpenFileDialog
    '    Try

    '        Dim MyProcs() As Process
    '        strpath = ""

    '        SBO_Appln.Desktop.Title = "SBO under " + SBO_Appln.Company.UserName
    '        MyProcs = Process.GetProcessesByName("SAP Business One")
    '        For i As Integer = 0 To MyProcs.Length - 1
    '            If MyProcs(i).MainWindowTitle = SBO_Appln.Desktop.Title Then
    '                Dim MyWindow As New WindowWrapper(MyProcs(i).MainWindowHandle)
    '                MyTest.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) '"C:\Documents and Settings\user\Desktop\"

    '                With MyTest
    '                    .Multiselect = False
    '                    .Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
    '                End With

    '                If MyTest.ShowDialog(MyWindow) = DialogResult.OK Then
    '                    strpath = MyTest.FileName
    '                    'Return MyTest.FileName
    '                End If
    '            End If
    '        Next

    '    Catch ex As Exception
    '        SBO_Appln.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '        ShowFolderBrowserThread.Abort()
    '        'Return ""
    '    Finally
    '        releaseObject(MyTest)
    '    End Try

    'End Sub

    Private Sub SBO_Appln_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles SBO_Appln.ItemEvent
        BubbleEvent = True

        Try


            '    Try
            '        'SBO_Appln.Menus.RemoveEx("Cancel_Doc")
            '    Catch ex As Exception
            '    End Try


            Select Case pVal.FormTypeEx

                'Case "UDO_GSET"
                '    If Not (pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD Or pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE) Then
                '        objform = GetForm(pVal.FormTypeEx, pVal.FormTypeCount)
                '        objGenSetting.SBO_Appln_ItemEvent(FormUID, pVal, BubbleEvent, objform)
                '    End If

                'Case "UDO_INSP"
                '    If Not (pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD Or pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE) Then
                '        objform = GetForm(pVal.FormTypeEx, pVal.FormTypeCount)
                '        objInspection.SBO_Appln_ItemEvent(FormUID, pVal, BubbleEvent, objform)
                '    End If
                'Case "CflGrnInsp"
                '    If Not (pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD Or pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE) Then
                '        objform = GetForm(pVal.FormTypeEx, pVal.FormTypeCount)
                '        objCFLInspection.SBO_Appln_ItemEvent(FormUID, pVal, BubbleEvent, objform)
                '    End If

                Case "UDO_BAAS"
                    If Not (pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD Or pVal.EventType = SAPbouiCOM.BoEventTypes.et_FORM_CLOSE) Then
                        objform = GetForm(pVal.FormTypeEx, pVal.FormTypeCount)
                        objBlanketInv.SBO_Appln_ItemEvent(FormUID, pVal, BubbleEvent, objform)
                    End If
            End Select
        Catch ex As Exception

        End Try

    End Sub
#End Region

#Region "Menu Events"
    '*****************************************************************
    'Type               : Procedure    
    'Name               : SBO_Appln_MenuEvent
    'Parameter          : MenuEvent, BubbelEven
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Handle Menu Events
    '******************************************************************

    Private Sub SBO_Appln_MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean) Handles SBO_Appln.MenuEvent

        Try
            Dim oMenus As SAPbouiCOM.Menus
            Dim oMenuItem As SAPbouiCOM.MenuItem
            oMenus = objApplication.Menus
            oMenuItem = objApplication.Menus.Item("2048")
            oMenus = oMenuItem.SubMenus

            If (pVal.BeforeAction = False) Then
                Select Case pVal.MenuUID

                    'Case "FrmShiftMas"
                    '    For i As Integer = 0 To oMenus.Count
                    '        If oMenus.Item(i).String.Contains("PRD_OSHFT -") Then
                    '            oMenus.Item(i).Activate()
                    '            Exit For
                    '        End If
                    '    Next

                End Select
            End If


            If (pVal.BeforeAction = False) Then

                'If (pVal.MenuUID = "MenuGenSet") Then
                '    objGenSetting.LoadForm()
                'End If

                'If (pVal.MenuUID = "MenuInsp") Then
                '    objInspection.LoadForm()
                'End If

                If (pVal.MenuUID = "MenuBlank") Then
                    objBlanketInv.LoadForm()
                End If

            End If

            'If objApplication.Forms.ActiveForm.TypeEx = "UDO_GSET" Then
            '    If (pVal.MenuUID = "1281") Or (pVal.MenuUID = "1282") Or (pVal.MenuUID = "1292") Or (pVal.MenuUID = "1293") Or (pVal.MenuUID = "1287") Or (pVal.MenuUID = "Cancel_Doc") Then
            '        objGenSetting.SBO_Appln_MenuEvent(pVal, BubbleEvent)
            '    End If
            'End If

            'If objApplication.Forms.ActiveForm.TypeEx = "UDO_INSP" Then
            '    If (pVal.MenuUID = "1281") Or (pVal.MenuUID = "1282") Or (pVal.MenuUID = "1292") Or (pVal.MenuUID = "1293") Or (pVal.MenuUID = "1287") Or (pVal.MenuUID = "Cancel_Doc") Then
            '        objInspection.SBO_Appln_MenuEvent(pVal, BubbleEvent)
            '    End If
            'End If
            '"1288", "1289", "1290", "1291"
            If objApplication.Forms.ActiveForm.TypeEx = "UDO_BAAS" Then
                If pVal.MenuUID >= "1281" And pVal.MenuUID <= "1293" Then
                    objBlanketInv.SBO_Appln_MenuEvent(pVal, BubbleEvent)
                End If
            End If

        Catch ex As Exception
            '   MsgBox(ex.Message)
        End Try
    End Sub
#End Region

#Region "RightClick Event"
    Private Sub SBO_Appln_RightClickEvent(ByRef eventInfo As SAPbouiCOM.ContextMenuInfo, ByRef BubbleEvent As Boolean) Handles SBO_Appln.RightClickEvent

        'Dim oMenuItem As SAPbouiCOM.MenuItem
        'Dim oMenus As SAPbouiCOM.Menus

        'Try
        '    SBO_Appln.Menus.RemoveEx("Cancel_EInvoice")
        '    If objApplication.Forms.ActiveForm.TypeEx <> "179" Then
        '        SBO_Appln.Menus.RemoveEx("Cancel_EWay_Bill")
        '    End If

        'Catch ex As Exception
        'End Try

        'If (objApplication.Forms.ActiveForm.TypeEx = "133" Or objApplication.Forms.ActiveForm.TypeEx = "181" Or objApplication.Forms.ActiveForm.TypeEx = "940" Or objApplication.Forms.ActiveForm.TypeEx = "179") Then
        '    Dim oCreationPackage As SAPbouiCOM.MenuCreationParams
        '    oCreationPackage = SBO_Appln.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)
        '    oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING
        '    oCreationPackage.UniqueID = "Cancel_EInvoice"
        '    oCreationPackage.String = "Cancel_EInvoice"
        '    oCreationPackage.Enabled = True
        '    oMenuItem = SBO_Appln.Menus.Item("1280")
        '    oMenus = oMenuItem.SubMenus
        '    oMenus.AddEx(oCreationPackage)

        '    If objApplication.Forms.ActiveForm.TypeEx <> "179" Then
        '        oCreationPackage = Nothing
        '        oCreationPackage = SBO_Appln.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)
        '        oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING
        '        oCreationPackage.UniqueID = "Cancel_EWay_Bill"
        '        oCreationPackage.String = "Cancel_EWay_Bill"
        '        oCreationPackage.Enabled = True
        '        oMenuItem = SBO_Appln.Menus.Item("1280")
        '        oMenus = oMenuItem.SubMenus
        '        oMenus.AddEx(oCreationPackage)
        '    End If
        'End If

        'Select Case objApplication.Forms.ActiveForm.TypeEx

        'Case "FrmOperLabSubMas"
        '    objOperationLabSubMaster.RightClickEvent(eventInfo, BubbleEvent)

        'Case "FrmQltChk"
        '    objQualityCheck.RightClickEvent(eventInfo, BubbleEvent)

        ' Case "FrmDocSet"
        'objDocSetting.RightClickEvent(eventInfo, BubbleEvent)

        'End Select

    End Sub
#End Region

#Region "Application Event"

    '*****************************************************************
    'Type               : Procedure    
    'Name               : SBO_Appln_AppEvent
    'Parameter          : Application Event Type
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Handle Application Event
    '******************************************************************
    Private Sub SBO_Appln_AppEvent(ByVal EventType As SAPbouiCOM.BoAppEventTypes) Handles SBO_Appln.AppEvent
        If (EventType = SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged Or EventType = SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition Or EventType = SAPbouiCOM.BoAppEventTypes.aet_ShutDown) Then
            SBO_Appln.StatusBar.SetText("Shutting Down BA Price Automation addon..", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
            LoadFromXML(System.Windows.Forms.Application.StartupPath & "\XML\MenuRemove.xml")

            Try
                Dim oMenuItem As SAPbouiCOM.MenuItem
                'Dim oMenus As SAPbouiCOM.Menus
                Dim oCreationPackage As SAPbouiCOM.MenuCreationParams
                oCreationPackage = SBO_Appln.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)
                SBO_Appln.StatusBar.SetText("Shutting Down BA Price Automation addon.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                oMenuItem = SBO_Appln.Menus.Item("BAAS")
                If EventType = SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged Then
                    SBO_Appln.StatusBar.SetText("Shutting Down BA Price Automation addon..", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                    If oMenuItem.SubMenus.Exists("MenuBlank") Then
                        oMenuItem.SubMenus.RemoveEx("MenuBlank")
                    End If
                    'If oMenuItem.SubMenus.Exists("QC1") Then
                    '    oMenuItem.SubMenus.RemoveEx("QC1")
                    'End If
                    'If oMenuItem.SubMenus.Exists("776") Then
                    '    oMenuItem.SubMenus.RemoveEx("776")
                    'End If
                    'If oMenuItem.SubMenus.Exists("QCCP") Then
                    '    oMenuItem.SubMenus.RemoveEx("QCCP")
                    'End If
                End If
                If EventType = SAPbouiCOM.BoAppEventTypes.aet_ShutDown Then
                    SBO_Appln.StatusBar.SetText("Shutting Down BA Price Automation addon...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                    If oMenuItem.SubMenus.Exists("MenuBlank") Then
                        oMenuItem.SubMenus.RemoveEx("MenuBlank")
                    End If
                    'If oMenuItem.SubMenus.Exists("QC1") Then
                    '    oMenuItem.SubMenus.RemoveEx("QC1")
                    'End If
                    'If oMenuItem.SubMenus.Exists("776") Then
                    '    oMenuItem.SubMenus.RemoveEx("776")
                    'End If
                    'If oMenuItem.SubMenus.Exists("P4") Then
                    '    oMenuItem.SubMenus.RemoveEx("P4")
                    'End If
                End If
                If EventType = SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition Then
                    SBO_Appln.StatusBar.SetText("Shutting Down BA Price Automation addon....", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                    If oMenuItem.SubMenus.Exists("MenuBlank") Then
                        oMenuItem.SubMenus.RemoveEx("MenuBlank")
                    End If
                    'If oMenuItem.SubMenus.Exists("QC1") Then
                    '    oMenuItem.SubMenus.RemoveEx("QC1")
                    'End If
                    'If oMenuItem.SubMenus.Exists("776") Then
                    '    oMenuItem.SubMenus.RemoveEx("776")
                    'End If
                    'If oMenuItem.SubMenus.Exists("P4") Then
                    '    oMenuItem.SubMenus.RemoveEx("P4")
                    'End If

                End If
            Catch ex As Exception
                SBO_Appln.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                'System.Windows.Forms.Application.Exit()
            End Try
            System.Windows.Forms.Application.Exit()
        End If
    End Sub

#End Region

#Region "Other Events"
    Private Sub SBO_Appln_LayoutKeyEvent(ByRef eventInfo As SAPbouiCOM.LayoutKeyInfo, ByRef BubbleEvent As Boolean) Handles SBO_Appln.LayoutKeyEvent

    End Sub

    Private Sub SBO_Appln_ServerInvokeCompletedEvent(ByRef b1iEventArgs As SAPbouiCOM.B1iEvent, ByRef BubbleEvent As Boolean) Handles SBO_Appln.ServerInvokeCompletedEvent

    End Sub

    Private Sub SBO_Appln_UDOEvent(ByRef udoEventArgs As UDOEvent, ByRef BubbleEvent As Boolean) Handles SBO_Appln.UDOEvent

    End Sub
#End Region

#End Region

End Class