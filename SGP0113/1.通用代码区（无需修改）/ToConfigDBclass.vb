Imports ToolModule
Imports System.Data.SqlClient
''' <summary>
''' 向数据库交互的模块
''' </summary>
Public Class ToConfigDBclass

    ''' <summary>
    ''' 数据库连接状态
    ''' </summary>
    Public DBconnect As Boolean
    ''' <summary>
    ''' 数据库控制对象
    ''' </summary>
    Private ConfigDB As SQLServerControlClass
    ''' <summary>
    ''' 构造方法，
    ''' </summary>
    ''' <param name="_ConfigDatabase"></param>
    Public Sub New(ByVal _ConfigDatabase As String)
        '实例化配置数据库对象
        ConfigDB = New SQLServerControlClass(_ConfigDatabase)
        '检测记录连接数据库是否正常
        If ConfigDB.ExecParamSQL(2, "select 1") IsNot Nothing Then DBconnect = True
    End Sub

    ''' <summary>
    ''' 获取游戏服务器监听信息
    ''' </summary>
    ''' <param name="_TagName">游戏表示</param>
    ''' <returns></returns>
    Public Function LoadGameServerConfigInfo(ByVal _TagName As String) As ArrayList
        Dim ReValue As New ArrayList
        '获取游戏服务器 监听的IP和端口等相关信息
        Dim GameServerTableData As DataTable = ConfigDB.ExecParamSQL(1, "select * from T_EGC_GameServerData with (NOLOCK) where _vTag='" & _TagName & "'")
        If GameServerTableData IsNot Nothing Then
            If GameServerTableData.Rows.Count > 0 Then
                Dim Data As DataRow = GameServerTableData.Rows(0)
                ReValue.Add(Data("_vListenIP"))
                ReValue.Add(Data("_iListenPort"))
                ReValue.Add(Data("_iListenMaxCount"))
                ReValue.Add(Data("_iSwitch"))
                ReValue.Add(Data("_iMaintenance"))
            End If
            GameServerTableData.Clear() : GameServerTableData.Dispose()
        End If
        If ReValue.Count = 0 Then
            Return Nothing
        Else
            Return ReValue
        End If
    End Function

    ''' <summary>
    ''' 读取登入服务器监听IP和端口
    ''' </summary>
    ''' <returns></returns>
    Public Function LoadLoginServerConfig() As ArrayList
        Dim ReValue As New ArrayList
        '获取登入服务器 监听的IP和端口等相关信息
        Dim GameServerTableData As DataTable = ConfigDB.ExecParamSQL(1, "select * from T_EGC_GameServerData with (NOLOCK) where _vTag='DR'")
        If GameServerTableData IsNot Nothing Then
            If GameServerTableData.Rows.Count > 0 Then
                Dim Data As DataRow = GameServerTableData.Rows(0)
                ReValue.Add(Data("_vListenIP"))
                ReValue.Add(Data("_iListenPort"))
            End If
            GameServerTableData.Clear() : GameServerTableData.Dispose()
        End If
        If ReValue.Count = 0 Then
            Return Nothing
        Else
            Return ReValue
        End If
    End Function

    ''' <summary>
    ''' 读取其他数据库连接字符串（帐号、注单、调阅 暂时不用）
    ''' </summary>
    ''' <param name="_TagName"></param>
    ''' <returns></returns>
    Public Function LoadGOtherDBConnstr(ByVal _TagName As String) As ArrayList
        '暂时用补上代码空着
        Return Nothing
    End Function

    ''' <summary>
    ''' 记录游戏服务器状态状态
    ''' </summary>
    Public Sub WriteGameServerState(ByVal _TagName As String, ByVal _Count As Integer, ByVal _ConnectState As String)
        ConfigDB.ExecParamSQL(3， "update T_EGC_GameServerData set _dCheckActiveTime=getdate(),_iOnlineCount=" & _Count & ",_nConnectState=N'" & _ConnectState & "' where _vTag='" & _TagName & "'")
    End Sub

    ''' <summary>
    ''' 写入游戏服务器运行日记
    ''' </summary>
    ''' <param name="_TagName">游戏标记</param>
    ''' <param name="_Content">内容</param>
    Public Sub WriteGameServerLog(ByVal _TagName As String, ByVal _Content As String)
        'If WriteExcessCheck(_TagName, _Content) = True Then Exit Sub '检测存储过量机制
        Dim SqlParams() As SqlParameter = New SqlParameter() {New SqlParameter("@_vTag", _TagName), New SqlParameter("@_nContent", _Content)}
        Dim ReValue As Integer = ConfigDB.ExecParamSQL(3, "insert into T_EGC_GameServerLog (_vTag,_nContent)valuse(@_vTag,@_nContent)", SqlParams)
        '判断SQL执行情况，ReValue = 0 执行失败，那么记入到文本中
        If ReValue = 0 Then LogWrite(_Content, _TagName)
    End Sub

    ''' <summary>
    ''' 写入游戏服务器错误记录
    ''' </summary>
    ''' <param name="_TagName">游戏标记</param>
    ''' <param name="_Content">内容</param>
    ''' <param name="_EX">异常对象</param>
    Public Sub WriteGameServerErr(ByVal _TagName As String, ByVal _Content As String, Optional ByVal _EX As Exception = Nothing)
        '如果带入有异常对象，那么重组内容数据
        If _EX IsNot Nothing Then _Content &= "<BR>" & _EX.Message & "<BR>" & _EX.StackTrace
        'If WriteExcessCheck(_TagName, _Content) = True Then Exit Sub '检测存储过量机制
        Dim SqlParams() As SqlParameter = New SqlParameter() {New SqlParameter("@_vTag", _TagName), New SqlParameter("@_nContent", _Content)}
        Dim ReValue As Integer = ConfigDB.ExecParamSQL(3, "insert into T_EGC_GameServerErr (_vTag,_nContent)valuse(@_vTag,@_nContent)", SqlParams)
        '判断SQL执行情况，ReValue = 0 执行失败，那么记入到文本中
        If ReValue = 0 Then LogWrite(_Content, _TagName)
    End Sub

#Region "#机制：写入过量检测#"
    ''' <summary>
    ''' 累计数量
    ''' </summary>
    Private TotalCount As Integer
    ''' <summary>
    ''' 当前时间
    ''' </summary>
    Private PresentMinute As Integer
    ''' <summary>
    ''' 机制：写入过量检测
    ''' 当调用WriteGameServerLog或WriteGameServerErr 1分钟内调用超过30次，将超出的记录到文件中
    ''' </summary>
    Private Function WriteExcessCheck(ByVal _TagName As Integer, ByVal _Content As String) As Boolean
        If Now.Minute <> PresentMinute Then
            PresentMinute = Now.Minute : TotalCount = 0
            Return False
        Else
            TotalCount += 1
            If TotalCount > 30 Then
                Return True
            Else
                Return False
            End If
        End If
    End Function
#End Region

End Class
