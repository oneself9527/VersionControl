Imports ToolModule
Imports System.Collections.Concurrent
''' <summary>
''' 与登入服务器交互的模块
''' </summary>
Public Class ToLoginServerClass
    ''' <summary>
    ''' 客户端SOCKET对象，用于连接登入服务器
    ''' </summary>
    Public WithEvents ClientSocket As ClientSocketClass

    ''' <summary>
    ''' 构造方法，同时开启连接登入服务器
    ''' </summary>
    ''' <param name="_ServerIP"></param>
    ''' <param name="_ServerPort"></param>
    Public Sub New(ByVal _ServerIP As String, ByVal _ServerPort As Integer)
        ClientSocket = New ClientSocketClass(_ServerIP, _ServerPort)
        ClientSocket.StartConnect()
    End Sub

    ''' <summary>
    ''' 销毁本类
    ''' </summary>
    Public Sub Dispose()
        ClientSocket.Dispose()
    End Sub
    ''' <summary>
    ''' 连接登入服务器成功时触发
    ''' </summary>
    Private Sub ConnectedEvent() Handles ClientSocket.ConnectedEvent
        '每次与登入服务器连接成功后，首先发送 游戏标记
        ClientSocket.SendAppData("LR>" & ManageXML.TagName)
    End Sub

    ''' <summary>
    ''' 接收数据触发
    ''' </summary>
    ''' <param name="_Msgstr"></param>
    Private Sub ReceiveEvent(ByVal _Msgstr As String) Handles ClientSocket.ReceiveAppEvent
        Dim Data1() As String = _Msgstr.Split(">")
        Dim Data() As String = Data1(4).Split("?")
        Select Case Data(0)
            Case "RZ" '玩家要进入本游戏，登入服务器先通知游戏服务器 数据格式 RZ>校验码, 座位号，帐号,积分
                Dim Tmp() As String = Data(1).Split(",")
                Dim user As UserClass
                '返回登录服务器 RZ>用户校验码
                ClientSocket.SendAppData("RZ>" & Data(1) & "," & ManageXML.TagName)
                '如果集合中已经有此对象，那么直接使用此对象，特别是断线重连逻辑使用
                If UserList.ContainsKey(Tmp(0)) = False Then
                    user = New UserClass()
                    user.CheckCode = Tmp(0)
                    user.MachineNumber = Tmp(1)
                    user.Account = Tmp(2)
                    user.Integral = Tmp(3)
                    user.Socket = Nothing
                    UserList.TryAdd(user.CheckCode, user)
                    '新用户连入
                    UesrNewConnEvent(user)
                Else
                    user = UserList(Tmp(0))
                    user.CheckCode = Tmp(0)
                    user.MachineNumber = Tmp(1)
                    user.Account = Tmp(2)
                    user.Integral = Tmp(3)
                    user.Socket = Nothing
                    user.OnLine = True
                    '断线重连逻辑
                    UesReconnEvent(user)
                End If

            Case "TC" '玩家离开本游戏，登入服务器通知游戏服务器 数据格式 TC>校验码
                Dim CheckCode As String = Data(1)
                If UserList.ContainsKey(CheckCode) = True Then
                    Dim user As UserClass = UserList(CheckCode)
                    UserDisconnEvent(user)
                    user.Dispose() '销毁内部对象
                    UserList.TryRemove(CheckCode, user) '在集合中删除
                End If
        End Select
    End Sub
    ''' <summary>
    ''' 掉线触发
    ''' </summary>
    Private Sub LeaveEvent() Handles ClientSocket.LeaveEvent

    End Sub


End Class
