Imports ToolModule
''' <summary>
''' 游戏服务器 用户类
''' </summary>
Public Class UserClass

#Region "### 禁止修改代码，如果使用过程中不方便，在反馈给孙哥 ###"
    ''' <summary>
    ''' 校验码
    ''' </summary>
    Public CheckCode As String
    ''' <summary>
    ''' 机台号
    ''' </summary>
    Public MachineNumber As Integer
    ''' <summary>
    ''' 游戏帐号
    ''' </summary>
    Public Account As String
    ''' <summary>
    ''' 游戏积分
    ''' </summary>
    Public Integral As Double
    ''' <summary>
    ''' 用户是否处于连接状态
    ''' </summary>
    Public OnLine As Boolean
    ''' <summary>
    ''' 用户心跳的验证次数
    ''' </summary>
    Public HeartTimes As Integer
    ''' <summary>
    ''' 活跃时间
    ''' </summary>
    Public ActiveTime As DateTime = Now
    ''' <summary>
    ''' 用于通讯的SOCKET
    ''' </summary>
    Public Socket As ServerUserClass
    ''' <summary>
    ''' 发送数据
    ''' </summary>
    ''' <param name="_MsgStr"></param>
    Public Sub SendData(ByVal _MsgStr As String)
        '按格式重组数据
        Socket.SendAppData("ZF>" & ManageXML.TagName & ">" & CheckCode & ">>" & _MsgStr)
        ' Socket.SendAppData(_MsgStr)
    End Sub
    ''' <summary>
    ''' 如果类中有使用非托管对象，那么当本类销毁的时候，在销毁过程中加入该对象
    ''' </summary>
    Public Sub Dispose()
        Socket = Nothing
    End Sub
#End Region

#Region "###  根据游戏不同在下方添加所需变量 ###"
    ''' <summary>
    ''' 押注分数
    ''' </summary>
    Public BetNumeber As Integer
    ''' <summary>
    ''' 用来存储中奖数据(九个图片的代号  （玩家分区，押注分数，分区参数）)
    ''' </summary>
    Public WinData As String
    ''' <summary>
    ''' 是否中全盘 1.红七2.蓝七，3.天下
    ''' </summary>
    Public IfOverAll As Integer
    ''' <summary>
    ''' 玩家是否续连
    ''' </summary>
    Public ContinueEven As Boolean
    ''' <summary>
    ''' 断线时的数据信息
    ''' </summary>
    ''' <remarks></remarks>
    Public BreakMessage As String
    ''' <summary>
    ''' 断线时比倍信息
    ''' </summary>
    ''' <remarks></remarks>
    Public CompareMessage As String
    ''' <summary>
    ''' 是否断线
    ''' </summary>
    ''' <remarks></remarks>
    Public IfBreak As Boolean
    ''' <summary>
    ''' 为0时表示疯狂送开始，但还未进入,-1表示不是疯狂送，其他表示疯狂送第几次
    ''' </summary>
    ''' <remarks></remarks>
    Public IfCrazySendStart As String = "-1"
    ''' <summary>
    ''' true表示疯狂送中即时送变暗
    ''' </summary>
    ''' <remarks></remarks>
    Public ImmediatelySendClose As Boolean
    ''' <summary>
    ''' 为1时表示中即时送后重新生成了即时送，重连后要显示新的
    ''' </summary>
    ''' <remarks></remarks>
    Public NewImmediatelySend As String = "0"
    ''' <summary>
    ''' 点得分之后的剩余分数以及比倍输后的剩余分数
    ''' </summary>
    ''' <remarks></remarks>
    Public ResidualFraction As String
    ''' <summary>
    ''' 疯狂送中奖分数
    ''' </summary>
    ''' <remarks></remarks>
    Public CrazySendWinScore As Double
    ''' <summary>
    ''' 中全盘奖时的分数
    ''' </summary>
    ''' <remarks></remarks>
    Public AllAwardsScores As Double
    ''' <summary>
    ''' 下注分数清零
    ''' </summary>
    ''' <remarks></remarks>
    Public BetReset As Integer
    ''' <summary>
    ''' 玩家注单号
    ''' </summary>
    ''' <remarks></remarks>
    Public NoteNum As Object
    ''' <summary>
    ''' 中了什么奖，和几注
    ''' </summary>
    Public AwardAndNum(2) As String
    ''' <summary>
    ''' 用来保存中奖分数
    ''' </summary>
    Public winScore As Integer
    ''' <summary>
    ''' 游戏状态
    ''' </summary>
    Public GameState As String
    ''' <summary>
    ''' 用来判断玩家是否玩过
    ''' </summary>
    Public IsPlay As Boolean = False

    Public Sub Clear()
        IfCrazySendStart = "-1"
        ImmediatelySendClose = False
        NewImmediatelySend = "0"
        CompareMessage = ""
        ResidualFraction = "0"
        AllAwardsScores = "0"
        BetReset = 0
    End Sub
#End Region

End Class
