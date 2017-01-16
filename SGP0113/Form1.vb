Imports ToolModule

Public Class Form1

    Private WithEvents ListenServer As ServerSocketClass
    Private WithEvents Client As ClientSocketClass
    Public WithEvents Server As ToListenClientClass
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        MainStart()
    End Sub

    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        End
    End Sub

    '接收核心层数据触发
    'Public Sub ReceiveCoreEvent(ByVal _Msgstr As String, ByVal _SocketUser As ServerUserClass) Handles Server.UesrReceiveAppEvent
    '    Console.WriteLine("客户端接收： " & _Msgstr)
    '    AddlistStr("接收:" & _Msgstr)
    'End Sub
    '///////////////////////////////////////////////////委托显示’
    Sub AddlistStr(ByVal str As String)
        Me.Invoke(New AddlistStr0(AddressOf AddlistStr1), str)
    End Sub
    Delegate Sub AddlistStr0(ByVal str As String)
    Sub AddlistStr1(ByVal str As String)
        ListBox1.Items.Add(str)
        ListBox1.SelectedIndex = ListBox1.Items.Count - 1

    End Sub
End Class
