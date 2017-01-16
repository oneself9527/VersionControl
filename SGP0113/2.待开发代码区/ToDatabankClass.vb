Imports ToolModule
Imports System.Data.SqlClient
''' <summary>
''' 对数据库进行操作的函数集合类
''' </summary>
Public Class ToDatabankClass
    ''' <summary>
    ''' 配置数据库控制对象
    ''' </summary>
    Private ConfigDB As SQLServerControlClass

    ''' <summary>
    ''' 构造方法
    ''' </summary>
    ''' <param name="_ConfigDatabase"></param>
    Public Sub New(ByVal _ConfigDatabase As String)
        ConfigDB = New SQLServerControlClass(_ConfigDatabase)
    End Sub
    ''' <summary>
    ''' 用来读取所有分区参数信息的 连接数据库字符串
    ''' </summary>
    Dim AllZoneStr As New SQLServerControlClass("server=127.0.0.1,1433;uid=testacc;pwd=100200;database=play_data;MultipleActiveResultSets=true;Connection Timeout=30;Pooling=true;Max Pool Size=512;Min Pool Size=1;")
    ' Public ZoneInfo As New SQLServerControlClass(AllZoneStr)
#Region "###  根据游戏不同在下方添加数据库操作过程或函数 ###"
    '可以参考  ToConfigDBclass 函数进行开发
    ''' <summary>
    ''' 新NEW一个XML类
    ''' </summary>
    Public XMLInfo As New ToXMLClass
    ''' <summary>
    ''' 获取游戏标记
    ''' </summary>
    Public GameTab As String = XMLInfo.TagName
    ''' <summary>
    ''' 查询 本游戏的所有机台参数 信息
    ''' </summary>
    ''' <returns></returns>
    Public Function SelectNewDate() As DataTable
        Dim sql As String = "select * from T_EGC__" & GameTab & " with (NOLOCK)"
        Return ConfigDB.ExecParamSQL(1, sql)
    End Function
    ''' <summary>
    ''' 查询数据库中的
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>(ALL天下,ALL红7,ALL蓝7,ALL大B,ALL中B,ALL杂7,天天天,红777,蓝777,小BAR，樱桃连线，水果盘，小Bar)</remarks>
    Public Function SelectWinningOfSgp(ByVal _fq As Integer) As String
        Dim sql As String = "select T_EGC__SGP from  _nNumberQP  with (NOLOCK) where _iNumber=@fq"
        Dim sqlparams() As SqlClient.SqlParameter = New SqlParameter() {New SqlParameter("@fq", _fq)}
        Return ConfigDB.ExecParamSQL(2， sql, sqlparams)
    End Function
    ''' <summary>
    ''' 更新数据库中的f_Winning字段
    ''' </summary>
    ''' <param name="_fq">分区</param>
    ''' <param name="_cftemp">数据值</param>
    ''' <remarks></remarks>
    Public Sub UpdateWinningOfSgp(ByVal _cftemp As String, ByVal _fq As Integer)
        Dim sql As String = "update T_EGC__SGP set _nNumberQP=@_nNumberQP where f_number=@fq"
        Dim sqlparams() As SqlClient.SqlParameter = New SqlParameter() {New SqlParameter("@f_Winning", _cftemp), New SqlParameter("@fq", _fq)}
        ConfigDB.ExecParamSQL(3, sql, sqlparams)
    End Sub
    ''' <summary>
    ''' 修改 游戏状态信息
    ''' </summary>
    ''' <param name="_mess">状态信息</param>
    ''' <param name="_account">帐号</param>
    ''' <param name="_fq">分区</param>
    ''' <remarks></remarks>
    Public Sub UpdateByGameMess(ByVal _mess As String, ByVal _account As String, ByVal _fq As Integer)
        Dim sql As String = "update  T_EGC__SGP set _nSeatStatus=@_nSeatStatus,_vRecordUsers=@_vRecordUsers where _iNumber=@_iNumber"
        Dim sqlparams() As SqlParameter = New SqlParameter() {
                New SqlParameter("@f_gameMess", _mess),
                New SqlParameter("@f_useracc", _account),
                New SqlParameter("@f_number", _fq)
        }
        ConfigDB.ExecParamSQL(3, sql, sqlparams)
    End Sub
    ''' <summary>
    ''' 更新水果盘彩金
    ''' </summary>
    ''' <param name="_Bule">蓝7全盘</param>
    ''' <param name="_Red7">红7全盘</param>
    ''' <param name="_TX">天下全盘</param>
    ''' <remarks></remarks>
    Public Sub UpdateMoneyOfSgp(ByVal _Bule As Double, ByVal _Red7 As Double, ByVal _TX As Double)
        Dim sql As String = "update T_EGC__SGP set _fWholePrizeBlue7=@_fWholePrizeBlue7,_fWholePrizeRed7=@_fWholePrizeRed7 ,_fWholePrizeTX=@_fWholePrizeTX"
        Dim sqlparams() As SqlClient.SqlParameter = New SqlParameter() {
            New SqlParameter("@_fWholePrizeBlue7", _Bule), New SqlParameter("@_fWholePrizeRed7", _Red7), New SqlParameter("@_fWholePrizeTX", _TX)}
        ConfigDB.ExecParamSQL(3, sql, sqlparams)
    End Sub
    ''' <summary>
    ''' 修改 总押和总赢 次数
    ''' </summary>
    ''' <param name="_zy">总押</param>
    ''' <param name="_zying">总赢</param>
    ''' <param name="_fq">分区</param>
    ''' <remarks></remarks>
    Public Sub UpdateZyaAndZyingCount(ByVal _zy As Integer, ByVal _zying As Integer, ByVal _fq As Integer)
        Dim sql As String = "update T_EGC__SGP set _iTotalBettingNum=@_iTotalBettingNum,_iTotalWinNum=@_iTotalWinNum where _iNumber=@_iNumber"
        Dim sqlparams() As SqlParameter = New SqlParameter() {
                New SqlParameter("@_iTotalBettingNum", _zy),
                New SqlParameter("@_iTotalWinNum", _zying),
                New SqlParameter("@_iNumber", _fq)
        }
        ConfigDB.ExecParamSQL(3， sql, sqlparams)
    End Sub

    ''' <summary>
    ''' 记录调阅演示数据
    ''' </summary>
    ''' <param name="_BillId">演示数据ID</param>
    ''' <param name="_Participants">玩家帐号</param>
    ''' <param name="_Content">演示內容</param>
    Public Sub ToGameNote(ByVal _BillId As String, ByVal _Participants As String, ByVal _Content As String)
        Dim _SQLStr As String = "insert into   此处填写调阅表名 ([f_BillId],[f_Participants],[f_Content],[f_Time]) values(N'" & _BillId & "',N'" & _Participants & "',N'" & _Content & "','" & Now() & "')"
        ' AsynExecParamSQL(_SQLStr, PublicModule.SQLServerConnStr_DY)
    End Sub
#End Region

End Class
