
Module SubMain

#Region "Declaration"
    'Private objMain As ClsSBO
    'Public strPDFFolderPath, strServerName, strDBName, strSQLUserName, strSQLPassword As String
    'Public blnChoose As Boolean
    'Public blnCFL As Boolean
    'Public blnGL As Boolean
    'Public intTypeRow As Integer
   
    'Local Variable
    Public DataBaseType As String = "SQL"

    'Public EwbSession As EWBSession = New EWBSession

#End Region

#Region "Main Method"
    '*****************************************************************************
    'Type               : Procedure   
    'Name               : main
    'Parameter          : 
    'Return Value       : 
    'Author             : Mathi QL Sivaprakash
    'Created Date       : 
    'Last Modified By   : 
    'Modified Date      : 
    'Purpose            : Create Instance to MainClass and Initialize Applicaiton 
    '******************************************************************************
    Public Sub Main()
        Dim objBP As clsMain
        objBP = New clsMain
        If (objBP.Initialise()) Then
            objBP.objSBOAPI.SBO_Appln.StatusBar.SetText("BA Price Automation Add-on connected successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
            System.Windows.Forms.Application.Run()
        Else
            objBP.objSBOAPI.SBO_Appln.StatusBar.SetText("Error in Connection", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End If

    End Sub
#End Region

#Region "Close Application"

    Public Sub CloseApp()
        System.Windows.Forms.Application.Exit()
    End Sub

#End Region

End Module
