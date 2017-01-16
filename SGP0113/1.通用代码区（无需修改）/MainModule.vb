Imports System.Collections.Concurrent
Imports ToolModule

''' <summary>
''' 游戏服务器：主逻辑模块
''' </summary>
Module MainModule

    ''' <summary>
    ''' 读取XML类对象
    ''' </summary>
    Public ManageXML As ToXMLClass
    ''' <summary>
    ''' 读取数据库对象
    ''' </summary>
    Public ManageDB As ToConfigDBclass
    ''' <summary>
    ''' 
    ''' </summary>
    Public ManageDB2 As ToDatabankClass
    ''' <summary>
    ''' 管理APP连接的对象
    ''' </summary>
    Public ManageClient As ToListenClientClass
    ''' <summary>
    ''' 管理连接游戏服务器的对象
    ''' </summary>
    Public ManageServer As ToLoginServerClass
    ''' <summary>
    ''' 当前程序状态
    ''' </summary>
    Public State As Boolean = False

    ''' <summary>
    ''' 是否维护状态 0非 1是
    ''' </summary>
    Public Maintenance As Integer
    ''' <summary>
    ''' 所有玩家集合Key记录校验码
    ''' </summary>
    Public UserList As New ConcurrentDictionary(Of String, UserClass)

    ''' <summary>
    ''' 主线程
    ''' </summary>
    Private MainTh As Threading.Thread

    ''' <summary>
    ''' 主线程启动
    ''' </summary>
    Public Sub MainStart()
        If MainTh IsNot Nothing Then MainTh.Abort()
        MainTh = New Threading.Thread(AddressOf StartTheGameServer)
        MainTh.Start()
    End Sub
    ''' <summary>
    ''' 启动游戏服务器
    ''' </summary>
    Private Sub StartTheGameServer()

        Reset() '重置
        '实例化读取XML对象 实例化同时会读取XML
        ManageXML = New ToXMLClass
        If ManageXML.TagName = "" Then MsgBox("游戏服务器XML配置文件讀取出錯，請管理員查看配置文件格式！") ： Exit Sub
        '实例化读取数据库对象，实例化同时会读取数据库部分信息
        ManageDB = New ToConfigDBclass(ManageXML.ConfigDatabase)
        If ManageDB.DBconnect = False Then MsgBox("游戏服务器讀取數據庫出錯，請管理員查看配置文件內容是否正確！") ： Exit Sub
        ManageDB2 = New ToDatabankClass(ManageXML.ConfigDatabase)

        Dim Switch As Integer = 1
        While True
            '读取游戏服务器信息
            Dim Data As ArrayList = ManageDB.LoadGameServerConfigInfo(ManageXML.TagName)
            If Data IsNot Nothing Then
                Dim ListenIP As String = Data(0)
                Dim ListenPort As Integer = Data(1)
                Dim ListenMaxCount As Integer = Data(2)
                Switch = Data(3)
                Dim Maintenance As Integer = Data(4)

                '如果游戏服务器没有启动，并且数据库设置为开启状态，那么启动
                If State = False AndAlso Switch = 1 Then
                    '实例化监听客户端的管理对象，实例化同时会启动监听
                    ManageClient = New ToListenClientClass(ListenIP, ListenPort, ListenMaxCount)
                    '获取连接登录服务器信息
                    Dim LoginServerConnectInfo As ArrayList = ManageDB.LoadLoginServerConfig()
                    If LoginServerConnectInfo IsNot Nothing Then
                        Dim LoginServerListenIP As String = LoginServerConnectInfo(0)
                        Dim LoginServerListenPort As Integer = LoginServerConnectInfo(1)
                        '实例化连接登入服务器，同时开始连接
                        ManageServer = New ToLoginServerClass(LoginServerListenIP, LoginServerListenPort)
                    End If
                    '设置为启动状态
                    State = True
                    '设置数据库存储的状态
                    ManageDB.WriteGameServerState(ManageXML.TagName, 0, "已啟動")
                    '初始化游戏资源
                    LogicModule.ItemInitialise()
                End If
                '当游戏服务器为启动状态，数据库设置为关闭，那么关闭游戏服务器
                If State = True AndAlso Switch = 0 Then
                    If ManageClient IsNot Nothing Then ManageClient.Dispose() : ManageClient = Nothing
                    If ManageServer IsNot Nothing Then ManageServer.Dispose() : ManageServer = Nothing
                    '设置为关闭状态
                    State = False
                    '设置数据库存储的状态
                    ManageDB.WriteGameServerState(ManageXML.TagName, 0, "已关闭")
                End If

            End If

            '向数据库写入游戏服务器状态，更新连线时间、连线人数、与服务器连接情况
            If State = True AndAlso Switch = 1 Then ManageDB.WriteGameServerState(ManageXML.TagName, UserList.Count, "已啟動")
            Threading.Thread.Sleep(3000)
        End While

    End Sub

    ''' <summary>
    ''' 重置本程序
    ''' </summary>
    Private Sub Reset()
        State = False
        ManageXML = Nothing ： ManageDB = Nothing
        If ManageClient IsNot Nothing Then ManageClient.Dispose() : ManageClient = Nothing
        If ManageServer IsNot Nothing Then ManageServer.Dispose() : ManageServer = Nothing
    End Sub


End Module
