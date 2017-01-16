Imports ToolModule
Imports System.Collections.Concurrent
Imports System.Threading
''' <summary>
''' 与客户端交互的模块
''' </summary>
Public Class ToListenClientClass
    ''' <summary>
    ''' 监听APP连入的服务器SOCKET
    ''' </summary>
    Private WithEvents ListenServer As ServerSocketClass

    ''' <summary>
    ''' 构造方法
    ''' </summary>
    Public Sub New(ByVal _ListenIP As String, ByVal _ListenPort As Integer, ByVal _ListenCount As Integer)
        ListenServer = New ServerSocketClass(_ListenIP, _ListenPort, _ListenCount)
        ListenServer.StartListen()
        '未操作控制线程
        NoActiveTh.Start()
        '心跳验证线程
        HeartTh.Start()
    End Sub
    ''' <summary>
    ''' 销毁本类
    ''' </summary>
    Public Sub Dispose()
        ListenServer.Dispose()
        UserList.Clear()
    End Sub


    ''' <summary>
    ''' 当有接收应用层数据自动触发
    ''' </summary>
    ''' <param name="_Msgstr"></param>
    ''' <param name="_SocketUser"></param>
    Public Sub UesrReceiveAppEvent(ByVal _Msgstr As String, ByVal _SocketUser As ServerUserClass) Handles ListenServer.UesrReceiveAppEvent
        '数据格式 (校验码 周+小时+分+秒+毫秒)
        '用途心跳 CH>游戏服务器标记>校验码>信息序号
        '转发数据 ZF>游戏服务器标记>校验码>信息序号>信息标头?内容

        '拆分接收数据
        Dim Data() As String = _Msgstr.Split(">")
        Dim CheckCode As String = Data(2)

        Dim User As UserClass

        '为了测试方便，直接验证成功
        If UserList.ContainsKey(CheckCode) = False Then
            User = New UserClass
            User.CheckCode = CheckCode
            User.Integral = 10000
            User.MachineNumber = 1
            User.Socket = _SocketUser
            User.Account = "AY-61"
            UserList.TryAdd(User.CheckCode, User)
            UesrNewConnEvent(User)
        Else
            User = UserList(CheckCode)
            If User.Socket IsNot _SocketUser Then User.Socket = _SocketUser
        End If
        User.HeartTimes = 0
        '根据数据格式，做相应处理
        Select Case Data(0)
            Case "CH" '心跳无需验证
                _SocketUser.SendAppData(_Msgstr) '返回心跳数据
            Case "ZF"
                User.ActiveTime = Now '记录活跃时间
                UesrReceiveEvent(Data(4).Split("?")(0), Data(4).Split("?")(1), User)
        End Select
    End Sub
    ''' <summary>
    ''' 心跳验证线程
    ''' </summary>
    Private HeartTh As Thread = New Thread(AddressOf HeartTest)
    ''' <summary>
    ''' 心跳测试掉线
    ''' </summary>
    Private Sub HeartTest()
        While True
            Try
                '遍历用户字典
                For Each user As UserClass In UserList.Values
                    '验证次数+1
                    user.HeartTimes += 1
                    '15s没有心跳触发且用户处于连接状态
                    If user.HeartTimes > 15 AndAlso user.OnLine = True Then
                        user.OnLine = False
                    End If
                Next
            Catch ex As Exception

            End Try
            '1秒一次
            Threading.Thread.Sleep(1000)
        End While
    End Sub
    ''' <summary>
    ''' 长时间未操作控制线程
    ''' </summary>
    Private NoActiveTh As Thread = New Thread(AddressOf NoActiveTh1)
    ''' <summary>
    ''' 长时间未操作方法 5分钟踢掉
    ''' </summary>
    Private Sub NoActiveTh1()
        '限定时长
        Dim outtime As Integer = 300
        While True
            Try
                '遍历用户字典
                For Each user As UserClass In UserList.Values
                    '用户处于连接状态
                    If user.OnLine = True Then
                        '获取时间差
                        Dim spTime As TimeSpan = Now - user.ActiveTime
                        '时间超时
                        If spTime.TotalSeconds > outtime Then
                            '向登陆服务器发送提出请求
                            ManageServer.ClientSocket.SendAppData("TC>" & user.CheckCode)
                            user.SendData("OUT")
                            '销毁socket
                            user.Dispose()
                            '将该用户删除
                            UserList.TryRemove(user.CheckCode, user)
                        End If
                        ''如果延迟退出时间大于 用户最大不操作的时间+16秒，踢出该玩家
                        'If spTime.TotalSeconds > (outtime + 16) Then
                        '    '向登陆服务器发送提出请求
                        '    ManageServer.ClientSocket.SendAppData("TC>" & user.CheckCode)
                        '    '向该用户发送OUT 退出标识
                        '    user.SendData("<OUT>")
                        '    UserDisconnEvent(user)
                        '    '销毁用户类
                        '    user.Dispose()
                        '    '从字典中删除玩家
                        '    UserList.TryRemove(user.CheckCode, user)
                        '    '如果延迟退出的时间 小于 用户最大不操作的时间+5秒，给客户端该玩家发送NN? 客户端弹出是否继续游戏提示框
                        'ElseIf spTime.TotalSeconds < (outtime + 5) Then
                        '    user.SendData("NN?SGP")
                        'End If
                    End If
                Next
            Catch ex As Exception

            End Try
            '10秒一次
            Threading.Thread.Sleep(10000)
        End While
    End Sub
End Class
