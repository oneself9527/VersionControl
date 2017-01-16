Imports System.Xml
''' <summary>
''' 读取XML配置信息
''' </summary>
Public Class ToXMLClass
    Private XMLFileUrl As String = "Config.xml"
    ''' <summary>
    ''' 游戏名称
    ''' </summary>
    Public TagName As String = ""
    ''' <summary>
    ''' 游戏配置数据库连接字符串
    ''' </summary>
    Public ConfigDatabase As String = ""
    ''' <summary>
    ''' 构造方法，执行后会自动读取XML文件
    ''' </summary>
    Public Sub New()
        XMLconfigRead()
    End Sub
    ''' <summary>
    ''' XML设置档信息读取，并保存到相关变量中
    ''' </summary>
    Private Sub XMLconfigRead()
        '建立读取XML文件的对象
        Dim XMLdata As XmlDocument = New XmlDocument()
        Dim XMLread As XmlTextReader = New XmlTextReader(XMLFileUrl)
        Try
            XMLdata.Load(XMLread)
            Dim XMLlist As XmlNodeList = XMLdata.ChildNodes(0).ChildNodes
            '将XML中数据保存到对应变量中
            For i As Integer = 0 To XMLlist.Count - 1
                Select Case XMLlist.Item(i).Name
                    Case "Tag" : TagName = XMLlist.Item(i).InnerText
                    Case "ConnectString" : ConfigDatabase = XMLlist.Item(i).InnerText
                End Select
            Next
        Catch ex As Exception
        Finally
            XMLread.Close()
        End Try
    End Sub
End Class
