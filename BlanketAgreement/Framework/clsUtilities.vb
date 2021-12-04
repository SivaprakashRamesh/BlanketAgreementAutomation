
'Imports CrystalDecisions.CrystalReports.Engine
Imports System.Net.Mail
'Imports CrystalDecisions.Shared
Imports System.Security.Cryptography
Imports System.Data.SqlClient
Imports System.IO
Imports Microsoft
'Imports Microsoft.Office.Interop

Public Class clsUtilities


#Region "Declartion"
    Private objCompany As SAPbobsCOM.Company
    Private objclsSBO As ClsSBO
    Private objRecordSet As SAPbobsCOM.Recordset
    Dim time As String = Today.ToString("yyyyMMdd") & "\Log_" & Now.ToString("HH_mm_ss")
#End Region

#Region "Methods"
#Region "get field value"
    ''' <summary>
    ''' get field value
    ''' </summary>
    ''' <param name="query"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>

    Public Function FillRecordSet(ByVal query As String) As SAPbobsCOM.Recordset
        Try
            Dim oRecordSet As SAPbobsCOM.Recordset
            oRecordSet = objclsSBO.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oRecordSet.DoQuery(query)
            Return oRecordSet
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function GetFieldValue(ByVal query As String)
        Dim strValue As String
        objRecordSet = objclsSBO.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        objRecordSet.DoQuery(query)
        strValue = objRecordSet.Fields.Item(0).Value.ToString()
        Return strValue
    End Function
#End Region

    Public Shared Function GetDateValue(ByVal strDate As String) As String
        Dim DateValue As String
        Dim Year As Integer = 0, Month As Integer = 0, Day As Integer = 0
        Dim strDay As String
        DateValue = strDate

        If DateValue.Length = 8 Then
            If IsNumeric(DateValue) Then
                Year = Convert.ToInt32(DateValue.Substring(0, 4))
                Month = Convert.ToInt32(DateValue.Substring(4, 2))
                Day = Convert.ToInt32(DateValue.Substring(6, 2))
                '[Date] = Convert.ToDateTime((MonthString(Month) & " " & Day.ToString() & " " & Year.ToString()))
                If Day < 10 Then
                    strDay = "0" + Day.ToString
                Else
                    strDay = Day.ToString
                End If
                strDate = strDay & "/" & MonthString(Month) & "/" & Year.ToString()
                ' strDate = MonthString(Month) & "/" & strDay & "/" & Year.ToString()
                Return strDate
            End If
        End If
        Return strDate
    End Function
    Public Shared Function MonthString(ByVal Value As Long) As String
        Dim MonthString_temp As String = Nothing

        Select Case Value

            Case 1
                MonthString_temp = "01"

                Exit Select
            Case 2
                MonthString_temp = "02"

                Exit Select
            Case 3
                MonthString_temp = "03"

                Exit Select
            Case 4
                MonthString_temp = "04"

                Exit Select
            Case 5
                MonthString_temp = "05"

                Exit Select
            Case 6
                MonthString_temp = "06"

                Exit Select
            Case 7
                MonthString_temp = "07"

                Exit Select
            Case 8
                MonthString_temp = "08"

                Exit Select
            Case 9
                MonthString_temp = "09"

                Exit Select
            Case 10
                MonthString_temp = "10"

                Exit Select
            Case 11
                MonthString_temp = "11"

                Exit Select
            Case 12
                MonthString_temp = "12"

                Exit Select
            Case Else
                MonthString_temp = ""

                Exit Select
        End Select

        Return MonthString_temp
    End Function

#Region "Combo methods"
#Region "remove combo values"
    ''' <summary>
    ''' remove combo values
    ''' </summary>
    ''' <param name="objComboBox"></param>    
    ''' <remarks></remarks>
    Friend Sub RemoveComboValues(objComboBox As SAPbouiCOM.ComboBox)
        For i As Integer = objComboBox.ValidValues.Count - 1 To 0 Step -1
            objComboBox.ValidValues.Remove(i, SAPbouiCOM.BoSearchKey.psk_Index)
        Next
    End Sub


    Function setComboBoxValue(ByVal oComboBox As SAPbouiCOM.ComboBox, ByVal strQry As String) As Boolean
        Try
            If oComboBox.ValidValues.Count = 0 Then
                Dim rsetValidValue As SAPbobsCOM.Recordset = DoQuery(strQry)
                rsetValidValue.MoveFirst()
                For j As Integer = 0 To rsetValidValue.RecordCount - 1
                    oComboBox.ValidValues.Add(rsetValidValue.Fields.Item(0).Value, rsetValidValue.Fields.Item(1).Value)
                    rsetValidValue.MoveNext()
                Next
            End If
            ' If oComboBox.ValidValues.Count > 0 Then oComboBox.Select(0, SAPbouiCOM.BoSearchKey.psk_Index)
        Catch ex As Exception
            objclsSBO.SBO_Appln.StatusBar.SetText("setComboBoxValue Function Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            Return True
        Finally
        End Try
        Return True

    End Function

    Friend Sub RemoveComboValues_Temp(objComboBox As SAPbouiCOM.ComboBox)
        For i As Integer = objComboBox.ValidValues.Count - 1 To 0 Step -1
            If objComboBox.ValidValues.Item(i).Value = "-" Then
                ' objComboBox.Select(objComboBox.ValidValues.Item(i).Value, SAPbouiCOM.BoSearchKey.psk_ByValue)
                Continue For
            End If

            objComboBox.ValidValues.Remove(i, SAPbouiCOM.BoSearchKey.psk_Index)
        Next
    End Sub

    Function setComboBoxValue_Temp(ByVal oComboBox As SAPbouiCOM.ComboBox, ByVal strQry As String) As Boolean
        Try

            Dim rsetValidValue As SAPbobsCOM.Recordset = DoQuery(strQry)
            rsetValidValue.MoveFirst()
            For j As Integer = 0 To rsetValidValue.RecordCount - 1
                oComboBox.ValidValues.Add(rsetValidValue.Fields.Item(0).Value, rsetValidValue.Fields.Item(1).Value)
                rsetValidValue.MoveNext()
            Next

            ' If oComboBox.ValidValues.Count > 0 Then oComboBox.Select(0, SAPbouiCOM.BoSearchKey.psk_Index)
        Catch ex As Exception
            objclsSBO.SBO_Appln.StatusBar.SetText("setComboBoxValue Function Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            Return True
        Finally
        End Try
        Return True
    End Function



#End Region

#Region "fill combo values"
    ''' <summary>
    ''' fill combo
    ''' </summary>
    ''' <param name="strSQL"></param>    
    ''' <remarks></remarks>
    Friend Sub FillComboValues(strSQL As String, objComboBox As SAPbouiCOM.ComboBox)
        objRecordSet = Nothing
        Try
            objRecordSet = objclsSBO.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            objRecordSet.DoQuery(strSQL)
            objComboBox.ValidValues.Add("", "")
            While objRecordSet.EoF = False
                objComboBox.ValidValues.Add(objRecordSet.Fields.Item(0).Value.ToString, objRecordSet.Fields.Item(1).Value.ToString)
                objRecordSet.MoveNext()
            End While
        Finally
            releaseObject(objRecordSet)
        End Try
    End Sub
#End Region
#Region "release com object"
    ''' <summary>
    ''' release com
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <remarks></remarks>
    Public Sub releaseObject(ByVal obj As Object)
        Try
            obj = Nothing
        Catch ex As Exception
            obj = Nothing
        Finally
            GC.Collect()
        End Try
    End Sub
#End Region
#End Region

#Region "Constructor"

    Public Sub New(ByVal objSBO As ClsSBO)
        objclsSBO = objSBO
    End Sub

#End Region

#Region "Show Messages"
    '*****************************************************************
    'Type               : Procedure   
    'Name               : ShowMessage
    'Parameter          : Message
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Show Message with in SBO
    '*****************************************************************
    Public Sub ShowMessage(ByVal strMessage As String)
        objclsSBO.SBO_Appln.MessageBox(strMessage)
    End Sub
    '*****************************************************************
    'Type               : Procedure   
    'Name               : ShowSuccessMessage
    'Parameter          : Message
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Show Success Message in Status Bar
    '*****************************************************************

    Public Sub ShowSuccessMessage(ByVal strMessage As String)
        objclsSBO.SBO_Appln.StatusBar.SetText(strMessage, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
    End Sub
    '*****************************************************************
    'Type               : Procedure   
    'Name               : ShowErrorMessage
    'Parameter          : Message
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Show Error Message in Status Bar
    '*****************************************************************

    Public Sub ShowErrorMessage(ByVal strMessage As String)
        objclsSBO.SBO_Appln.StatusBar.SetText(strMessage, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    End Sub

    Public Sub ShowErrorMessage_Display(ByVal strMessage As String)
        objclsSBO.SBO_Appln.StatusBar.SetText(strMessage, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
    End Sub

    '*****************************************************************
    'Type               : Procedure   
    'Name               : ShowWarningMessage
    'Parameter          : Message
    'Return Value       : 
    'Author             : QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : To Show Warning Message in Status Bar
    '*****************************************************************

    Public Sub ShowWarningMessage(ByVal strMessage As String)
        objclsSBO.SBO_Appln.StatusBar.SetText(strMessage, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
    End Sub
#End Region

#Region "DateFormat"
    '****************************************************************************
    'Type	        	    :   Procedure     
    'Name               	:   GetFormat
    'Parameter          	:   Company,Type
    'Return Value       	:	
    'Author             	:	QL Sivaprakash
    'Created Date       	:	
    'Last Modified By	    :	
    'Modified Date        	:	
    'Purpose             	:	Get the Dateformat for the Given Company
    '****************************************************************************

    'Public Function GetFormat(ByVal objcompany As SAPbobsCOM.Company, ByVal oType As Integer) As String
    '    Dim strDateFormat, strSql As String
    '    strDateFormat = "" : strSql = ""
    '    Dim oTemprecordset As SAPbobsCOM.Recordset
    '    strSql = "SELECT ""DateFormat"",""DateSep"" from OADM"
    '    oTemprecordset = objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
    '    oTemprecordset.DoQuery(strSql)
    '    Select Case oTemprecordset.Fields.Item(0).Value
    '        Case 0  'dd/mm/yy'
    '            strDateFormat = "dd" & oTemprecordset.Fields.Item(1).Value & "MM" & oTemprecordset.Fields.Item(1).Value & "yy"
    '            GetFormat = 3
    '        Case 1 'dd/mm/yyyy'
    '            strDateFormat = "dd" & oTemprecordset.Fields.Item(1).Value & "MM" & oTemprecordset.Fields.Item(1).Value & "yyyy"
    '            GetFormat = 103
    '        Case 2 'mm/dd/yyyy'
    '            strDateFormat = "MM" & oTemprecordset.Fields.Item(1).Value & "dd" & oTemprecordset.Fields.Item(1).Value & "yy"
    '            GetFormat = 1
    '        Case 3 'yyyy/dd/mm'
    '            strDateFormat = "MM" & oTemprecordset.Fields.Item(1).Value & "dd" & oTemprecordset.Fields.Item(1).Value & "yyyy"
    '            GetFormat = 120
    '        Case 4 'dd/month/yyyy'
    '            strDateFormat = "yyyy" & oTemprecordset.Fields.Item(1).Value & "dd" & oTemprecordset.Fields.Item(1).Value & "MM"
    '            GetFormat = 126
    '        Case 5 'dd/month/yyyy'
    '            strDateFormat = "dd" & oTemprecordset.Fields.Item(1).Value & "MMM" & oTemprecordset.Fields.Item(1).Value & "yyyy"
    '            GetFormat = 130
    '    End Select

    '    If oType = 1 Then
    '        GetFormat = strDateFormat
    '    Else
    '        GetFormat = GetFormat
    '    End If
    'End Function
#End Region

    'New Methods By QL Sivaprakash

    Function getSingleValue(ByVal strSQL As String) As String
        Try
            Dim rset As SAPbobsCOM.Recordset = objclsSBO.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            Dim strReturnVal As String = ""
            rset.DoQuery(strSQL)
            Return IIf(rset.RecordCount > 0, rset.Fields.Item(0).Value.ToString(), "")
        Catch ex As Exception
            'objclsSBO.SBO_Appln.StatusBar.SetText(" Get Single Value Function Failed : ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            Return ""
        End Try
    End Function

    Function LoadDocumentDate(ByVal oEditText As SAPbouiCOM.EditText) As Boolean
        Try
            oEditText.Active = True
            oEditText.String = "A"
        Catch ex As Exception
            objclsSBO.SBO_Appln.StatusBar.SetText("Load Document Date is Failure : ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            Return False
        Finally
        End Try
        Return True
    End Function

    Sub ChooseFromListFilteration(ByVal oForm As SAPbouiCOM.Form, ByVal strCFL_ID As String, ByVal strCFL_Alies As String, ByVal strQuery As String)
        Try
            Dim oCFL As SAPbouiCOM.ChooseFromList = oForm.ChooseFromLists.Item(strCFL_ID)
            Dim oConds As SAPbouiCOM.Conditions
            Dim oCond As SAPbouiCOM.Condition
            Dim oEmptyConds As New SAPbouiCOM.Conditions
            Dim rsetCFL As SAPbobsCOM.Recordset = objclsSBO.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oCFL.SetConditions(oEmptyConds)
            oConds = oCFL.GetConditions()

            rsetCFL.DoQuery(strQuery)
            rsetCFL.MoveFirst()
            For i As Integer = 1 To rsetCFL.RecordCount
                If i = (rsetCFL.RecordCount) Then
                    oCond = oConds.Add()
                    oCond.Alias = strCFL_Alies
                    oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
                    oCond.CondVal = Trim(rsetCFL.Fields.Item(0).Value)
                Else
                    oCond = oConds.Add()
                    oCond.Alias = strCFL_Alies
                    oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
                    oCond.CondVal = Trim(rsetCFL.Fields.Item(0).Value)
                    oCond.Relationship = SAPbouiCOM.BoConditionRelationship.cr_OR
                End If
                rsetCFL.MoveNext()
            Next
            If rsetCFL.RecordCount = 0 Then
                oCond = oConds.Add()
                oCond.Alias = strCFL_Alies
                oCond.Relationship = SAPbouiCOM.BoConditionRelationship.cr_NONE
                oCond.CondVal = "-1"
            End If
            oCFL.SetConditions(oConds)
        Catch ex As Exception
            'objclsSBO.SBO_Appln.StatusBar.SetText(" Get Single Value Function Failed : ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)

        Finally
        End Try
    End Sub

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
            objclsSBO.SBO_Appln.StatusBar.SetText(" Set New Line : ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        Finally
        End Try
    End Sub

    Sub Sub_SetNewLine(ByVal oMatrix As SAPbouiCOM.Matrix, ByVal oDBDSDetail As SAPbouiCOM.DBDataSource, ByVal Parent1 As Integer, Optional ByVal Parent2 As Integer = 0, Optional ByVal RowID As Integer = 1, Optional ByVal ColumnUID As String = "")
        Try
            If ColumnUID.Equals("") = False Then
                If oMatrix.VisualRowCount > 0 Then
                    If oMatrix.Columns.Item(ColumnUID).Cells.Item(RowID).Specific.Value.Equals("") = False And RowID = oMatrix.VisualRowCount Then
                        oMatrix.FlushToDataSource()
                        oMatrix.AddRow()
                        oDBDSDetail.InsertRecord(oDBDSDetail.Size)
                        oDBDSDetail.Offset = oMatrix.VisualRowCount - 1
                        oDBDSDetail.SetValue("LineID", oDBDSDetail.Offset, oMatrix.VisualRowCount)
                        oDBDSDetail.SetValue("U_Parent1", oDBDSDetail.Offset, Parent1)
                        If Parent2 <> 0 Then oDBDSDetail.SetValue("U_Parent2", oDBDSDetail.Offset, Parent2)
                        oMatrix.SetLineData(oMatrix.VisualRowCount)
                        oMatrix.FlushToDataSource()
                    End If
                Else
                    oMatrix.FlushToDataSource()
                    oMatrix.AddRow()
                    oDBDSDetail.InsertRecord(oDBDSDetail.Size)
                    oDBDSDetail.Offset = oMatrix.VisualRowCount - 1
                    oDBDSDetail.SetValue("LineID", oDBDSDetail.Offset, oMatrix.VisualRowCount)
                    oDBDSDetail.SetValue("U_Parent1", oDBDSDetail.Offset, Parent1)
                    If Parent2 <> 0 Then oDBDSDetail.SetValue("U_Parent2", oDBDSDetail.Offset, Parent2)
                    oMatrix.SetLineData(oMatrix.VisualRowCount)
                    oMatrix.FlushToDataSource()
                End If

            Else
                oMatrix.FlushToDataSource()
                oMatrix.AddRow()
                oDBDSDetail.InsertRecord(oDBDSDetail.Size)
                oDBDSDetail.Offset = oMatrix.VisualRowCount - 1
                oDBDSDetail.SetValue("LineID", oDBDSDetail.Offset, oMatrix.VisualRowCount)
                oDBDSDetail.SetValue("U_Parent1", oDBDSDetail.Offset, Parent1)
                If Parent2 <> 0 Then oDBDSDetail.SetValue("U_Parent2", oDBDSDetail.Offset, Parent2)
                oMatrix.SetLineData(oMatrix.VisualRowCount)
                oMatrix.FlushToDataSource()
            End If
            For i As Integer = 0 To oMatrix.VisualRowCount - 1
                oDBDSDetail.SetValue("LineId", i, i + 1)
            Next
            oMatrix.LoadFromDataSource()
        Catch ex As Exception
            objclsSBO.SBO_Appln.StatusBar.SetText(" Sub_Set new Line: ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)

        Finally
        End Try
    End Sub

    Sub DeleteRow(ByVal oMatrix As SAPbouiCOM.Matrix, ByVal oDBDSDetail As SAPbouiCOM.DBDataSource)
        Try
            oMatrix.FlushToDataSource()

            For i As Integer = 1 To oMatrix.VisualRowCount
                oMatrix.GetLineData(i)
                oDBDSDetail.Offset = i - 1
                oDBDSDetail.SetValue("LineID", oDBDSDetail.Offset, i)
                oMatrix.SetLineData(i)
                oMatrix.FlushToDataSource()
            Next
            oDBDSDetail.RemoveRecord(oDBDSDetail.Size - 1)
            oMatrix.LoadFromDataSource()

        Catch ex As Exception
            objclsSBO.SBO_Appln.StatusBar.SetText(" Delete Row in Matrix : ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)

        Finally
        End Try
    End Sub

    Sub DeleteEmptyRowInFormDataEvent(ByVal oMatrix As SAPbouiCOM.Matrix, ByVal ColumnUID As String, ByVal oDBDSDetail As SAPbouiCOM.DBDataSource)
        Try
            If oMatrix.VisualRowCount > 0 Then
                If oMatrix.Columns.Item(ColumnUID).Cells.Item(oMatrix.VisualRowCount).Specific.Value.Equals("") Then
                    oMatrix.DeleteRow(oMatrix.VisualRowCount)
                    oDBDSDetail.RemoveRecord(oDBDSDetail.Size - 1)
                    oMatrix.FlushToDataSource()
                End If
            End If
        Catch ex As Exception
            ' objclsSBO.SBO_Appln.StatusBar.SetText(" Delete Empty Row in Matrix : ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)


        Finally
        End Try
    End Sub

    Function DoQuery(ByVal strSql As String) As SAPbobsCOM.Recordset
        Try
            Dim rsetCode As SAPbobsCOM.Recordset = objclsSBO.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            rsetCode.DoQuery(strSql)
            Return rsetCode
        Catch ex As Exception
            'ExceptionHandler(ex)
            objclsSBO.SBO_Appln.StatusBar.SetText("DoQuery issue: ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)

            Return Nothing
        Finally
        End Try
    End Function


#Region "Date format"
    Public Function DateConvertion(ByVal date1 As String)
        Dim date2 As String = ""
        Try
            date2 = date1.Substring(0, 4) & "-" & date1.Substring(4, 2) & "-" & date1.Substring(6, 2)
        Catch ex As Exception
            Return date2
        End Try
        Return date2
    End Function
#End Region

    'Public Function ToSendemail(ByVal DocEntry As String, ByVal ReportName As String, ByVal FormName As String, ByVal WhsCode As String, ByVal cardcode As String, ByVal FormCount As String)
    '    Try

    '        Me.ShowErrorMessage_Display("Start Mail Sending ..")


    '        'Dim objRecSet As SAPbobsCOM.Recordset
    '        'Dim enc As System.Text.UTF8Encoding
    '        'Dim encryptor As ICryptoTransform
    '        'Dim decryptor As ICryptoTransform

    '        'Dim strSQL, strTemFolder As String
    '        Dim objFSO
    '        Dim CryRpt As ReportDocument
    '        CryRpt = New ReportDocument
    '        'Dim i As Integer
    '        objFSO = CreateObject("Scripting.FileSystemObject")
    '        ' Dim file As String = 'System.Windows.Forms.Application.StartupPath.ToString() & "\Report_Mail\" & "ReleasePicking.rpt"


    '        CryRpt.Load(ReportName)

    '        Dim crParameterDiscreteValue1 As ParameterDiscreteValue
    '        Dim crParameterFieldDefinitions1 As ParameterFieldDefinitions
    '        Dim crParameterFieldLocation1 As ParameterFieldDefinition
    '        Dim crParameterValues1 As ParameterValues



    '        crParameterFieldDefinitions1 = CryRpt.DataDefinition.ParameterFields
    '        crParameterFieldLocation1 = crParameterFieldDefinitions1.Item("DocEntry")
    '        crParameterValues1 = crParameterFieldLocation1.CurrentValues

    '        crParameterDiscreteValue1 = New CrystalDecisions.Shared.ParameterDiscreteValue
    '        crParameterDiscreteValue1.Value = CInt(DocEntry)
    '        crParameterValues1.Add(crParameterDiscreteValue1)
    '        crParameterFieldLocation1.ApplyCurrentValues(crParameterValues1)


    '        Dim PassWordValue As String = getSingleValue("select U_DBPassWord from [@TI_SL_EMAIL]  where Code='" & FormCount & "'")

    '        Dim crtableLogoninfos As New TableLogOnInfos
    '        Dim crtableLogoninfo As New TableLogOnInfo
    '        Dim crConnectionInfo As New ConnectionInfo
    '        Dim CrTables As Tables
    '        Dim CrTable As Table
    '        With crConnectionInfo
    '            .ServerName = Me.objclsSBO.oCompany.Server     'strServerName
    '            .DatabaseName = Me.objclsSBO.oCompany.CompanyDB ' "Shell"  strDBName
    '            .UserID = "sa" ' strSQLUserName
    '            .Password = PassWordValue  ' strSQLPassword



    '        End With



    '        CrTables = CryRpt.Database.Tables

    '        For Each CrTable In CrTables
    '            CrTables = CryRpt.Database.Tables
    '            crtableLogoninfo = CrTable.LogOnInfo
    '            crtableLogoninfo.ConnectionInfo = crConnectionInfo
    '            CrTable.ApplyLogOnInfo(crtableLogoninfo)
    '        Next
    '        Dim CrExportOptions As ExportOptions
    '        Dim CrDiskFileDestinationOptions As New  _
    '        DiskFileDestinationOptions()
    '        Dim CrFormatTypeOptions As New PdfRtfWordFormatOptions()
    '        '   Dim strNo As String = (objRecSet.Fields.Item("CardCode").Value & "_" & objRecSet.Fields.Item("CntctPrsn").Value & " ").ToString.Trim.Replace("-", "_")
    '        Dim strNo As String = DocEntry & "_" & Now.Millisecond
    '        CrDiskFileDestinationOptions.DiskFileName = System.Windows.Forms.Application.StartupPath & "\Report_PDF\" & FormName & "_" & strNo & ".pdf"
    '        Dim PDFFileName As String = System.Windows.Forms.Application.StartupPath & "\Report_PDF\" & FormName & "_" & strNo & ".pdf"

    '        Me.ShowErrorMessage_Display("Mail - Create PDF for Attachment ..")


    '        CrExportOptions = CryRpt.ExportOptions
    '        With CrExportOptions
    '            .ExportDestinationType = ExportDestinationType.DiskFile
    '            .ExportFormatType = ExportFormatType.PortableDocFormat
    '            .DestinationOptions = CrDiskFileDestinationOptions
    '            .FormatOptions = CrFormatTypeOptions
    '        End With
    '        CryRpt.Export()
    '        releaseObject(CryRpt)
    '        GC.Collect()
    '        If ToSendEmails(PDFFileName, WhsCode, cardcode, FormName, FormCount) = False Then
    '            Return False
    '        End If
    '        Return True

    '        '  Next
    '        ' Else
    '        'jUtility.ShowWarningMessage("Email Id not found this Contact person...,")
    '        'Exit Sub
    '        'End If
    '        ' objUtility.ShowMessage("e-Mail has sent Successfully")
    '    Catch ex As Exception
    '        objclsSBO.SBO_Appln.MessageBox(ex.ToString)
    '        Return False

    '    End Try
    'End Function

    'Private Function ToSendEmails(ByVal PDFFileName As String, ByVal whscode As String, ByVal cardcode As String, ByVal FormName As String, ByVal FormCount As String)
    '    Try





    '        Dim mail As New MailMessage()


    '        Dim MailDetails As SAPbobsCOM.Recordset
    '        MailDetails = objclsSBO.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
    '        MailDetails.DoQuery("select * from [@TI_SL_EMAIL]  where Code='" & FormCount & "'")
    '        If MailDetails.RecordCount = 0 Then Exit Function

    '        Me.ShowErrorMessage_Display("Mail - Adding To Address ..")
    '        Dim SmtpServer As New SmtpClient(MailDetails.Fields.Item("U_HostName").Value)
    '        mail.Subject = MailDetails.Fields.Item("U_Subject").Value

    '        Dim CustomerName As String = getSingleValue("select Cardname  from OCRD where cardcode='" & cardcode.ToString.Trim & "'")
    '        Dim WhsName As String = getSingleValue("SELECT T0.Whsname FROM [dbo].[OWHS]  T0 INNER JOIN OCRY T1 ON T0.[Country] = T1.[Code] WHERE T0.[WhsCode] ='" & whscode & "'")
    '        Dim Address As String = getSingleValue("select concat(T0.[Street],' ',T0.[StreetNo],' ' ,T0.[City],' ',T1.[Name],'-', T0.[ZipCode])'Address' FROM [dbo].[OWHS]  T0 INNER JOIN OCRY T1 ON T0.[Country] = T1.[Code] WHERE T0.[WhsCode] ='" & whscode & "'")
    '        Dim Fax As String = getSingleValue("select T0.address2'fax' FROM [dbo].[OWHS]  T0 INNER JOIN OCRY T1 ON T0.[Country] = T1.[Code] WHERE T0.[WhsCode] ='" & whscode & "'")
    '        Dim Phone As String = getSingleValue("select  T0.address3'phone'  FROM [dbo].[OWHS]  T0 INNER JOIN OCRY T1 ON T0.[Country] = T1.[Code] WHERE T0.[WhsCode] ='" & whscode & "'")



    '        Dim BodyContent As String = "Dear " & CustomerName & " ,"
    '        BodyContent = BodyContent & "<br>"
    '        BodyContent = BodyContent & "<br> &nbsp&nbsp&nbsp&nbsp&nbsp&nbsp "
    '        BodyContent = BodyContent & MailDetails.Fields.Item("U_Body").Value
    '        BodyContent = BodyContent & "<br>"
    '        BodyContent = BodyContent & "<br>"
    '        BodyContent = BodyContent & "<br>"

    '        BodyContent = BodyContent & "Thanks "
    '        BodyContent = BodyContent & "<br><br><br><br><br>"







    '        BodyContent = BodyContent & "<style type='text/css'> p.MsoNormal { margin: 0px !important; } .sigbop { color: #000000; font-family: Arial; font-size: 12px; } .sigbop td { vertical-align: middle; } .sigbop a, .sigbop a:visited { text-decoration: none; color: #000000; }</style><div summary='sigbop-signature' style='text-size-adjust: none;'><table width='290' cellspacing='0' cellpadding='0' border='0' title='b7c2fe0f-4040-4b64-b907-15d92d04067f' style='display: none;'><tbody><tr><td></td></tr></tbody></table><table width='290' cellspacing='0' cellpadding='0' border='0' title='sigbop-signature' owner='b7c2fe0f-4040-4b64-b907-15d92d04067f' class='sigbop' id='preview'><tbody><tr><td><table border='0' cellspacing='0' cellpadding='0'><tr><td style='width:290px' ><a href='https://www.sigbop.com/Signature/GetClick.ashx?ClickTypeID=10&UserID=b7c2fe0f-4040-4b64-b907-15d92d04067f&TrackingID=00000000-0000-0000-0000-000000000000&SignatureID=8db60b9d-536b-4a73-a511-32c48f5bc9ec&IconID=55e0e806-9d20-43cb-9c28-724ebe28e176' target='_blank' ><img height='126' width='290' alt='Logo' src='https://images.sigbop.com/Signature/GetImage.ashx?ImageID=466b9d70-59e2-4d8f-bd88-dffa24c5dc42' style='display:block; border:0;' ></a></td></tr></table></td></tr><tr><td style='padding-bottom: 2px;'><table width='290' cellspacing='0' cellpadding='0' border='0' style='padding-bottom: 2px;'><tbody><tr><td align='left' style='vertical-align: top;'><div Style='Font-Family:Arial;Font-Size:12px;Line-height:Normal;Color:#000000;margin:0;'><span style='font-size: 110%;'><strong> " + WhsName + "</strong></span><br /><span style='font-size: 90%;'><strong>StorKom</strong></span><br /><div style='line-height: 95%;'><a style='Font-Family:Arial; color:#000000; text-decoration: none;' href='https://www.google.com/maps?q=block,address+county' target='_blank'><span style='font-size: 80%'>" + Address + "&mdash; </span><span style='font-size: 80%'></span></a></div></div><table style='width:100%;' border='0' cellspacing='0' cellpadding='0'><tr><td align='left' style='padding-right:3px;width:1px;'><a href='https://www.sigbop.com/Signature/GetClick.ashx?ClickTypeID=8&UserID=b7c2fe0f-4040-4b64-b907-15d92d04067f&TrackingID=00000000-0000-0000-0000-000000000000&SignatureID=8db60b9d-536b-4a73-a511-32c48f5bc9ec&IconID=e5e76747-6451-44db-85b4-9e4a986b20ff' target='_blank' ><img alt='Phone' height='9' width='17' src='https://images.sigbop.com/Signature/GetImage.ashx?ImageID=336c7aa2-0fc9-4442-82a3-c4a362a4e1c2' style='border:none; display:block;' ></a></td><td align='left' style='padding-right:10px;'><a href='https://www.sigbop.com/Signature/GetClick.ashx?ClickTypeID=8&UserID=b7c2fe0f-4040-4b64-b907-15d92d04067f&TrackingID=00000000-0000-0000-0000-000000000000&SignatureID=8db60b9d-536b-4a73-a511-32c48f5bc9ec&IconID=e5e76747-6451-44db-85b4-9e4a986b20ff' target='_blank' ><span style='font-family:Arial;color:Black;font-size:12px;text-decoration:none;' > " + Phone + "</span></a></td></tr><tr><td align='left' style='padding-right:3px;width:1px;'><a href='https://www.sigbop.com/Signature/GetClick.ashx?ClickTypeID=8&UserID=b7c2fe0f-4040-4b64-b907-15d92d04067f&TrackingID=00000000-0000-0000-0000-000000000000&SignatureID=8db60b9d-536b-4a73-a511-32c48f5bc9ec&IconID=c2d8a8cc-d5ca-4eb6-91c8-abbfa28f4568' target='_blank' ><img alt='Phone' height='9' width='17' src='https://images.sigbop.com/Signature/GetImage.ashx?ImageID=342e5406-cb90-4d46-9698-bfdddbd31ad5' style='border:none; display:block;' ></a></td><td align='left' style='padding-right:10px;'><a href='https://www.sigbop.com/Signature/GetClick.ashx?ClickTypeID=8&UserID=b7c2fe0f-4040-4b64-b907-15d92d04067f&TrackingID=00000000-0000-0000-0000-000000000000&SignatureID=8db60b9d-536b-4a73-a511-32c48f5bc9ec&IconID=c2d8a8cc-d5ca-4eb6-91c8-abbfa28f4568' target='_blank' ><span style='font-family:Arial;color:Black;font-size:12px;text-decoration:none;' >" + Fax + "</span></a></td></tr></table></td><td align='left' style='vertical-align: middle;'><table border='0' cellspacing='0' cellpadding='0'><tr><td style='width:24px;padding-bottom:5px;padding-right:5px;'><a href='https://www.sigbop.com/Signature/GetClick.ashx?ClickTypeID=7&UserID=b7c2fe0f-4040-4b64-b907-15d92d04067f&TrackingID=00000000-0000-0000-0000-000000000000&SignatureID=8db60b9d-536b-4a73-a511-32c48f5bc9ec&IconID=1477b08d-2dd8-4a71-bd2a-176db8654d6b' target='_blank' ><img alt='Email' height='24' width='24' src='https://images.sigbop.com/Signature/GetImage.ashx?ImageID=e075d988-cfa6-4481-ab20-5f30366cdfc4' style='border:none; display:block;'></a></td><td style='width:24px;padding-bottom:5px;padding-right:5px;'><a href='https://www.sigbop.com/Signature/GetClick.ashx?ClickTypeID=4&UserID=b7c2fe0f-4040-4b64-b907-15d92d04067f&TrackingID=00000000-0000-0000-0000-000000000000&SignatureID=8db60b9d-536b-4a73-a511-32c48f5bc9ec&IconID=1c70dbe1-4b13-426e-bbe1-5de1847ecc43' target='_blank' ><img alt='Website' height='24' width='24' src='https://images.sigbop.com/Signature/GetImage.ashx?ImageID=9f818faa-2729-47b0-bf0b-0adfe9a0fbce' style='border:none; display:block;' ></a></td><td style='width:24px;padding-bottom:5px;padding-right:5px;'><a href='https://www.sigbop.com/Signature/GetClick.ashx?ClickTypeID=5&UserID=b7c2fe0f-4040-4b64-b907-15d92d04067f&TrackingID=00000000-0000-0000-0000-000000000000&SignatureID=8db60b9d-536b-4a73-a511-32c48f5bc9ec&IconID=28a55cc7-8d88-4456-898b-35e3c6267fd4' target='_blank' ><img alt='Social' height='24' width='24' src='https://images.sigbop.com/Signature/GetImage.ashx?ImageID=d66a062b-de8a-4a83-a5a4-758b180509f4' style='border:none; display:block;'></a></td></tr></table></td></tr></tbody></table></td></tr><tr><td style='padding-bottom: 5px; padding-top: 5px;'><table width='290' cellspacing='0' cellpadding='0' border='0'><tbody><tr><td valign='middle' style='height: 1px;'><img alt='' width='290' height='1' src='https://www.sigbop.com/images/enterprise/companybranding/clearfieldpharmacy/line.png' style='display: block; border-width: 0px; border-style: solid;' /></td></tr></tbody></table></td></tr><tr><td><table width='290' cellspacing='0' cellpadding='0' border='0'><tbody><tr><td align='left'></td></tr></tbody></table></td></tr><tr><td></td></tr><tr><td></td></tr></tbody></table></a>"
    '        mail.IsBodyHtml = True
    '        mail.Body = BodyContent ' MailDetails.Fields.Item("U_Body").Value
    '        mail.From = New MailAddress(MailDetails.Fields.Item("U_FrmMailId").Value)

    '        Dim TomailId As String = getSingleValue("select E_Mail  from OCRD where cardcode='" & cardcode.ToString.Trim & "'")
    '        If TomailId = "" Then
    '            Me.ShowErrorMessage_Display("To Mail Address Should not be Empty...")
    '            Exit Function
    '        End If
    '        Dim CC1 As String = getSingleValue("select U_Email1  from OWHS where WhsCode='" & whscode.ToString.Trim & "'")
    '        Dim CC2 As String = getSingleValue("select U_Email2  from OWHS where WhsCode='" & whscode.ToString.Trim & "'")

    '        mail.[To].Add(TomailId)
    '        mail.[To].Add(MailDetails.Fields.Item("U_FrmMailId").Value)

    '        Me.ShowErrorMessage_Display("Mail - Adding To CC ..")
    '        If CC1 <> "" Then mail.CC.Add(CC1)
    '        If CC2 <> "" Then mail.CC.Add(CC2)



    '        Me.ShowErrorMessage_Display("Mail - Start Attachment ..")
    '        mail.Attachments.Add(New Attachment(PDFFileName))

    '        SmtpServer.Timeout = 200000

    '        SmtpServer.Port = 25
    '        SmtpServer.EnableSsl = True
    '        SmtpServer.UseDefaultCredentials = False
    '        SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network
    '        SmtpServer.Credentials = New System.Net.NetworkCredential(MailDetails.Fields.Item("U_FrmMailId").Value.ToString.Trim, MailDetails.Fields.Item("U_PassWord").Value.ToString.Trim)
    '        Me.ShowErrorMessage_Display("Mail - Completed Attachment ..")

    '        Me.ShowErrorMessage_Display("Mail Sending.....")
    '        SmtpServer.Send(mail)

    '        mail.Dispose()
    '        'releaseObject(mail)
    '        ' releaseObject(SmtpClient)
    '        ToDeleteFile_pdf(PDFFileName)

    '        Me.ShowErrorMessage_Display("Successfully Sent the mail...")
    '        GC.Collect()
    '        Return True
    '    Catch ex As Exception
    '        objclsSBO.SBO_Appln.MessageBox(ex.ToString)
    '        Me.ShowErrorMessage_Display("Not Successfully Sent the mail..." & ex.Message)
    '        Return False
    '    End Try
    'End Function

    Private Sub ToDeleteFile_pdf(ByVal FileName As String)
        Dim XLNewpath As String
        'Dim OrigFilPath As String
        'Dim TextSheetname As String
        XLNewpath = FileName
        Dim di As New IO.DirectoryInfo(XLNewpath)
        Try
            'For Each Directory In New IO.DirectoryInfo(XLNewpath).GetDirectories
            '    Directory.Delete(True)
            'Next
            If System.IO.File.Exists(FileName) = True Then
                System.IO.File.Delete(FileName)
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Sub setEdittextCFL(ByVal oForm As SAPbouiCOM.Form, ByVal UId As String, ByVal strCFL_ID As String, ByVal strCFL_Obj As String, ByVal strCFL_Alies As String)
        Try

            Dim oCFL As SAPbouiCOM.ChooseFromListCreationParams
            oCFL = objclsSBO.SBO_Appln.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_ChooseFromListCreationParams)
            oCFL.UniqueID = strCFL_ID
            oCFL.ObjectType = strCFL_Obj
            oForm.ChooseFromLists.Add(oCFL)

            Dim txt As SAPbouiCOM.EditText = oForm.Items.Item(UId).Specific
            txt.ChooseFromListUID = strCFL_ID
            txt.ChooseFromListAlias = strCFL_Alies

        Catch ex As Exception
            objclsSBO.SBO_Appln.StatusBar.SetText("Set EditText CFL Method Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        Finally
        End Try
    End Sub

    Public Function GetDateTimeValue(ByVal DateString As String) As DateTime
        Dim objBridge As SAPbobsCOM.SBObob
        objBridge = objclsSBO.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge)
        Return objBridge.Format_StringToDate(DateString).Fields.Item(0).Value
    End Function

    Public Function FormatHoursAndMinutes(totalMinutes As Integer) As String
        Dim span As TimeSpan = TimeSpan.FromMinutes(totalMinutes)
        Return String.Format("{0:00}", CInt(Math.Floor(span.TotalHours)))
    End Function

    Function LoadComboBoxSeries(ByVal oComboBox As SAPbouiCOM.ComboBox, ByVal UDOID As String) As Boolean
        Try
            oComboBox.ValidValues.LoadSeries(UDOID, SAPbouiCOM.BoSeriesMode.sf_Add)
            oComboBox.Select(0, SAPbouiCOM.BoSearchKey.psk_Index)
        Catch ex As Exception
            '   objUtility.ShowErrorMessage("LoadComboBoxSeries Function Failed:" & ex.Message)
            objclsSBO.SBO_Appln.StatusBar.SetText("LoadComboBoxSeries Function Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)

            Return False
        Finally
        End Try
        Return True
    End Function

    Function GetCodeGeneration(ByVal TableName As String) As Integer
        Try
            Dim rsetCode As SAPbobsCOM.Recordset = objclsSBO.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            Dim strCode As String = "Select ISNULL(Max(ISNULL(DocEntry,0)),0) + 1 Code From " & Trim(TableName) & ""
            rsetCode.DoQuery(strCode)
            Return CInt(rsetCode.Fields.Item("Code").Value)
        Catch ex As Exception
            objclsSBO.SBO_Appln.StatusBar.SetText("GetCodeGeneration Function Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            Return True
        Finally
        End Try
    End Function

    Public Sub write_log(ByVal status As String)
        Dim fs As FileStream
        Dim objWriter As System.IO.StreamWriter
        Dim chatlog As String
        Try

            Dim di As DirectoryInfo = New DirectoryInfo("C:\Kits\Common\QC\QCQueryCheckerLog_" & Today.ToString("yyyyMMdd") & "")
            If di.Exists Then
            Else
                di.Create()
            End If
            chatlog = "C:\Kits\Common\QC\QCQueryCheckerLog_" & time & ".txt"
            If File.Exists(chatlog) Then
            Else
                fs = New FileStream(chatlog, FileMode.Create, FileAccess.Write)
                fs.Close()
            End If
            objWriter = New System.IO.StreamWriter(chatlog, True)
            If status <> "" Then objWriter.WriteLine(vbNewLine + Now & " : " & status)
            objWriter.Close()
        Catch ex As Exception
            objclsSBO.SBO_Appln.StatusBar.SetText("WriteLog Function Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None)
        End Try
    End Sub
#End Region

End Class
