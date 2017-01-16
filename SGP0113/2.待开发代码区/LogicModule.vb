Imports System.Text
Imports System.IO
Imports System.Data.SqlClient
Imports System.Threading
Imports System.Array
Imports System.Collections.Concurrent
Imports ToolModule
''' <summary>
''' 逻辑代码模块
''' </summary>
Module LogicModule
    '#############开发必读'#############
    '发送数据方法 userclass.senddata(数据)
    '记录ERR方法  ManageDB.WriteGameServerLog()
    '记录LOG方法  ManageDB.WriteGameServerErr()
    '调阅演示数据暂时不记录到数据库中
    '###################################

#Region "逻辑变量声明，注意声明为Private，仅限本模块中使用，尽量减少公共变量的使用"
    ''' <summary>
    ''' 游戏开放的机器数量（机台数量）默认80
    ''' </summary>
    Public GameMachineCount As Integer = 80
    ''' <summary>
    ''' 表示遊戲時間，如果在這一時間段沒有任何操作，我們將採取措施  原来g_gametime
    ''' </summary>
    Public PersistGameTime(GameMachineCount) As Integer
    ''' <summary>
    ''' 座位号
    ''' </summary>
    Public SeatNumber As Integer
    ''' <summary>
    ''' 算法实体类  Note number
    ''' </summary>
    ''' <remarks></remarks>
    Public AlgorithmClass(GameMachineCount) As GameAlgorithmClass
    ''' <summary>
    ''' 数据库类
    ''' </summary>
    ''' <remarks></remarks>
    Public DatabaseClass As DatabaseInfoClass
    ''' <summary>
    ''' 即时送的随机 1-5中的一个
    ''' </summary>
    ''' <remarks></remarks>
    Public ImmediatelySendRnd(GameMachineCount) As Integer
    ''' <summary>
    ''' 越南分站
    ''' </summary>
    Public VietnamStation As String() = {"5"}
    ''' <summary>
    ''' 同步锁
    ''' </summary>
    Public Synchrolock As New Object
    ''' <summary>
    ''' 各分区总押次数 设置即时连线送和连线送结束后为其赋值，其余时间不变
    ''' </summary>
    ''' <remarks></remarks>
    Public TotalBetNum(GameMachineCount) As Integer
    ''' <summary>
    ''' 各分区总押次数 每押一次该值加1
    ''' </summary>
    ''' <remarks></remarks>
    Public BetNum(GameMachineCount) As Integer
    ''' <summary>
    ''' 當前中全盤積分的記錄 格式为：帐号，中奖积分，中奖类型（十个初始值的顺序chushi）,第几分区，日期  recoBetNumrd(0)表示所有分区内最近中的5个大奖，record(x)表示个分区中的大奖
    ''' </summary>
    ''' <remarks></remarks>
    Public OverallRecord(GameMachineCount + 1) As String
    ''' <summary>
    ''' 蓝7奖金 对应参数设置中的当前蓝7全盘积分 
    ''' </summary>
    ''' <remarks></remarks>
    Public Blue7Money As Double
    ''' <summary>
    ''' 红7奖金 对应参数设置中的当前红7全盘积分
    ''' </summary>
    ''' <remarks></remarks>
    Public Red7Money As Double
    ''' <summary>
    ''' 天下奖金 对应参数设置中的当前天下全盘积分
    ''' </summary>
    ''' <remarks></remarks>
    Public WorldMoney As Double
    ''' <summary>
    ''' 对应樱桃连线以上的奖项是否开放 1开放 0不开放
    ''' </summary>
    ''' <remarks></remarks>
    Public CherryIfOpen(GameMachineCount) As Integer
    ''' <summary>
    ''' 蓝7 奖金抽分   游戏押注分数乘以百分之多少累计为蓝7全盘积分
    ''' </summary>
    ''' <remarks></remarks>
    Public Blue7Bonus(GameMachineCount) As Double
    ''' <summary>
    ''' 红7 奖金抽分   游戏押注分数乘以百分之多少累计为红7全盘积分
    ''' </summary>
    ''' <remarks></remarks>
    Public Red7Bonus(GameMachineCount) As Double
    ''' <summary>OverallRecord
    ''' 天下 奖金抽分   游戏押注分数乘以百分之多少累计为天下全盘积分
    ''' </summary>
    ''' <remarks></remarks>
    Public WorldBonus(GameMachineCount) As Double
    ''' <summary>
    ''' 是否开放蓝7全盘 0不可中，1可中 
    ''' </summary>
    ''' <remarks></remarks>
    Public IfOpenBlue7All(GameMachineCount) As Double
    ''' <summary>
    ''' 是否开放红7全盘 0不可中，1可中
    ''' </summary>
    ''' <remarks></remarks>
    Public IfOpenRed7All(GameMachineCount) As Double
    ''' <summary>
    ''' 是否开放天下全盘 0不可中，1可中
    ''' </summary>
    ''' <remarks></remarks>
    Public IfOpenWorldAll(GameMachineCount) As Double
    ''' <summary>
    ''' 强制中奖的设置   1蓝7、2红7、3天下、4樱桃连线、5水果盘、6小B连线、7樱桃全盘、8铆钉全盘、9苹果全盘、10葡萄全盘、11西瓜全盘、12杂B全盘、13小B全盘、14中B全盘、15大B全盘、16杂7全盘
    ''' </summary>
    ''' <remarks></remarks>
    Public ForceWin(GameMachineCount) As String
    ''' <summary>
    ''' 疯狂送奖项是否开放-是否开放樱桃连线 0不开放，1开放
    ''' </summary>
    ''' <remarks></remarks>
    Public IfOpenCrazySendCherry(GameMachineCount) As Double
    ''' <summary>
    ''' 疯狂送奖项是否开放-是否开放小B连线 0不开放，1开放
    ''' </summary>
    ''' <remarks></remarks>
    Public IfOpenCrazySendBar(GameMachineCount) As Double
    ''' <summary>
    ''' 疯狂送奖项是否开放-是否开放水果盘连线 0不开放，1开放
    ''' </summary>
    ''' <remarks></remarks>
    Public IfOpenCrazySendFruitDish(GameMachineCount) As Double
    ''' <summary>
    '''设置自动VIP的字符串 '10,6,3,2|10,6,3,2|10,6,3,2|10,6,3,2|0'   VIP设置部分几分|几位|几进|几出|自动模式(选中则为1) 
    ''' </summary>
    ''' <remarks></remarks>
    Public VipParameter As String
    ''' <summary>
    '''对应f_users 设置会员限制 被设置的用户将不能中连线以上的奖项
    ''' </summary>
    ''' <remarks></remarks>
    Public UsersLimit As String
    ''' <summary>
    ''' 蓝7全盘 积分起始分数
    ''' </summary>
    ''' <remarks></remarks>
    Public Blue7StartIntegral As Double
    ''' <summary>
    ''' 红7全盘 积分起始分数
    ''' </summary>
    ''' <remarks></remarks>
    Public Red7StartIntegral As Double
    ''' <summary>
    ''' 天下全盘 积分起始分数
    ''' </summary>
    ''' <remarks></remarks>
    Public WorldStartIntegral As Double
    ''' <summary>
    ''' 蓝7全盘 积分最高分数限制
    ''' </summary>
    ''' <remarks></remarks>
    Public Blue7MaxIntegral As Double
    ''' <summary>
    ''' 红7全盘 积分最高分数限制
    ''' </summary>
    ''' <remarks></remarks>
    Public Red7MaxIntegral As Double
    ''' <summary>
    ''' 天下全盘 积分最高分数限制
    ''' </summary>
    ''' <remarks></remarks>
    Public WorldMaxIntegral As Double
    ''' <summary>
    ''' 疯狂送 遊戲輸贏百分比0.6 ，疯狂送中，中奖的比例 该值越大中奖率越大
    ''' </summary>
    ''' <remarks></remarks>
    Public CrazySendPercentage(GameMachineCount) As Double
    ''' <summary>
    ''' 各人樱桃连线的次数
    ''' </summary>
    ''' <remarks></remarks>
    Public PersonalCherryNum(GameMachineCount) As Integer
    ''' <summary>
    ''' 个人水果盘的次数
    ''' </summary>
    ''' <remarks></remarks>
    Public PersonalFruitDishNum(GameMachineCount) As Integer
    ''' <summary>
    ''' 个人小B的次数
    ''' </summary>
    ''' <remarks></remarks>
    Public PersonalSmallBarNum(GameMachineCount) As Integer
    ''' <summary>
    ''' 本局所中的红7全盘彩金
    ''' </summary>
    ''' <remarks></remarks>
    Public BureauRed7Bonus(GameMachineCount) As Double
    ''' <summary>
    ''' 本局所中的蓝7全盘彩金
    ''' </summary>
    ''' <remarks></remarks>
    Public BureauBlue7Bonus(GameMachineCount) As Double
    ''' <summary>
    ''' 本局所中的天下全盘彩金
    ''' </summary>
    ''' <remarks></remarks>
    Public BureauWorldBonus(GameMachineCount) As Double
    ''' <summary>
    ''' sTheFirstSixCards中牌的张数
    ''' </summary>
    ''' <remarks></remarks>
    Public CardsNum(GameMachineCount) As Integer
    ''' <summary>
    ''' 比倍最后一张
    ''' </summary>
    ''' <remarks></remarks>
    Public LastOne(GameMachineCount) As Boolean
    ''' <summary>
    ''' 比倍的初始分
    ''' </summary>
    ''' <remarks></remarks>
    Public CompareInitialPoints(GameMachineCount) As Double
    ''' <summary>
    ''' 右边已有的字符串，或许是空 即比过的牌
    ''' </summary>
    ''' <remarks></remarks>
    Public RightCardStr(GameMachineCount) As String
    ''' <summary>
    ''' 比倍时的右边显示.6个以内
    ''' </summary>
    ''' <remarks></remarks>
    Public RightShowSix(GameMachineCount) As String
    ''' <summary>
    ''' 比倍要显示的牌
    ''' </summary>
    ''' <remarks></remarks>
    Public ShowCards(GameMachineCount) As Integer
    ''' <summary>
    ''' 控制中3个连线的按钮
    ''' </summary>
    ''' <remarks></remarks>
    Public PushButton As Integer
    ''' <summary>
    ''' 各人樱桃连线的次数
    ''' </summary>
    ''' <remarks></remarks>
    Public PersonalCherryNumMax(GameMachineCount) As Integer
    ''' <summary>
    ''' 即时送翻倍的变量 ，true表示可以即时送
    ''' </summary>
    ''' <remarks></remarks>
    Public ImmediatelySend(GameMachineCount) As Boolean
    ''' <summary>
    ''' 即时送左边随机 （櫻桃.铆丁.蘋果.葡萄.西瓜.小BAR.中BAR.大BAR）
    ''' </summary>
    ''' <remarks></remarks>
    Public ImmediatelySendLeft(GameMachineCount) As Integer
    ''' <summary>
    ''' 即时送右边随机
    ''' </summary>
    ''' <remarks></remarks>
    Public ImmediatelySendRight(GameMachineCount) As Integer
    ''' <summary>
    ''' 即时送左边随机
    ''' </summary>
    ''' <remarks></remarks>
    Public ImmediatelySendLeftRnd(GameMachineCount) As Integer
    ''' <summary>
    ''' 是否是即时连线送
    ''' </summary>
    ''' <remarks></remarks>
    Public IfImmediatelyLineSend(GameMachineCount) As Boolean
    ''' <summary>
    ''' 存储即时连线送的第几次
    ''' </summary>
    ''' <remarks></remarks>
    Public ImmediatelySendNum(GameMachineCount) As Integer
    ''' <summary>
    ''' 即时送右边随机
    ''' </summary>
    ''' <remarks></remarks>
    Public ImmediatelySendRightRnd(GameMachineCount) As Integer
    ''' <summary>
    ''' 用来判断是否中了九宫格的樱桃
    ''' </summary>
    ''' <remarks></remarks>
    Public IfJGGcherry(GameMachineCount) As Integer
    ''' <summary>
    ''' 用来判断是否中了九宫格的水果盘
    ''' </summary>
    ''' <remarks></remarks>
    Public IfJGGfruit(GameMachineCount) As Integer
    ''' <summary>
    ''' 用来判断是否中了九宫格的小B
    ''' </summary>
    ''' <remarks></remarks>
    Public IfJGGSmallBar(GameMachineCount) As Integer
    ''' <summary>
    ''' 个人水果盘的次数
    ''' </summary>
    ''' <remarks></remarks>
    Public PersonalFruitDishNumMax(GameMachineCount) As Integer
    ''' <summary>
    ''' 个人小B的次数
    ''' </summary>
    ''' <remarks></remarks>
    Public PersonalSmallBarNumMax(GameMachineCount) As Integer
    ''' <summary>
    ''' 比倍次数
    ''' </summary>
    ''' <remarks></remarks>
    Public CompareNum(GameMachineCount) As Integer
    ''' <summary>
    ''' 最后一次操作的时间
    ''' </summary>
    ''' <remarks></remarks>
    Public LastOperationTime(GameMachineCount) As Date
    ''' <summary>
    ''' 分区参数信息
    ''' </summary>
    ''' <remarks></remarks>
    Public Subregion(GameMachineCount + 1, 59) As String
    ''' <summary>
    ''' 继承数据库类的方法 记录错误
    ''' </summary>
    Public RecordError As New ToConfigDBclass(ManageXML.ConfigDatabase)
    ''' <summary>
    ''' 客户端SOCKET对象，用于连接登入服务器
    ''' </summary>
    Private WithEvents ClientSocket As ClientSocketClass
    ''' <summary>
    ''' 声明一个 调用数据库的类
    ''' </summary>
    Public ToDataBank As New ToDatabankClass(ManageXML.ConfigDatabase)
    ''' <summary>
    ''' 表示AP启动后是否有人下注
    ''' </summary>
    ''' <remarks></remarks>
    Public IfSomeOneBet(GameMachineCount) As Boolean
    ''' <summary>
    ''' 用来保存每个玩家发送的启动命令
    ''' </summary>
    ''' <remarks></remarks>
    Private NoteSingle(GameMachineCount) As String
    ''' <summary>
    ''' 当前积分减去下注积分
    ''' </summary>
    ''' <remarks></remarks>
    Public ResidueScore(GameMachineCount) As Double
    ''' <summary>
    ''' 各分区底分 Subregion(x-1,3)
    ''' </summary>
    ''' <remarks></remarks>
    Public EndPoints(GameMachineCount) As Integer
    ''' <summary>
    ''' 当局是否中了强制奖项
    ''' </summary>
    ''' <remarks></remarks>
    Public NowIfForceWin(GameMachineCount) As String
    ''' <summary>
    '''  房间比例
    ''' </summary>
    ''' <remarks></remarks>
    Public RoomRatio(GameMachineCount) As Double
    ''' <summary>
    ''' 是否有会员限制，0没有 1限制模式  2低于当前模式
    ''' </summary>
    ''' <remarks></remarks>
    Public IfUserLimit(GameMachineCount) As Integer
    ''' <summary>
    ''' 给演示用的初始分
    ''' </summary>
    ''' <remarks></remarks>
    Public DemoInitialPoints(GameMachineCount) As Integer
    ''' <summary>
    ''' 记载疯狂送的总赢分数(针对修改记录的处理)
    ''' </summary>
    ''' <remarks></remarks>
    Public CrazySendWinScore(GameMachineCount) As Double
    ''' <summary>
    ''' 是否需要记载疯狂送分数(针对修改记录的处理)   -----------------------这个变量有的位置需要加上  对照一下
    ''' </summary>
    ''' <remarks></remarks>
    Public IfRecordCrazySendScore(GameMachineCount) As Boolean
    ''' <summary>
    ''' 触发疯狂送时的连线得分
    ''' </summary>
    ''' <remarks></remarks>
    Public CrazySendScore(GameMachineCount) As Double
    ''' <summary>
    ''' 記錄步驟记录游戏演示信息 
    ''' </summary>
    ''' <remarks></remarks>
    Public RecordGameMessage(GameMachineCount) As StringBuilder '格式：(底分$下注分数$user_3*蓝7奖金|红7奖金|天下奖金|RoomRatio|$区号^樱桃连线次数^水果盘次数^小B连线次数^随机送水果^随机送倍数^(1^)JIE|账号,赢得分数,剩余分数,剩余分数|积分验证#user_3$&isfks&是否是疯狂送~ 牌|左边张数|右边的牌|中奖分数|比倍分数|第几次比倍|$)
    ''' <summary>
    ''' 是否疯狂送 0：不是疯狂送   1：樱桃(蓝7)  2：水果盘(红7)  3：小BAR(天下)
    ''' </summary>
    ''' <remarks></remarks>
    Public IfCrazySend(GameMachineCount) As Integer
    ''' <summary>
    ''' 我的疯狂送
    ''' </summary>
    ''' <remarks></remarks>
    Public MyCrazySend(GameMachineCount) As Boolean
    ''' <summary>
    ''' 我的疯狂送额度
    ''' </summary>
    ''' <remarks></remarks>
    Public MyCrazySendLimit(GameMachineCount) As Double
    ''' <summary>
    ''' 我的疯狂送分数
    ''' </summary>
    ''' <remarks></remarks>
    Public MyCrazySendScore(GameMachineCount) As Double
    ''' <summary>
    ''' 记录疯狂送(免费游戏)是否贏夠五次
    ''' </summary>
    ''' <remarks></remarks>
    Public IfFreeFiveNum(GameMachineCount) As Integer
    ''' <summary>
    ''' 是否已结算 0未结算 1已发送结算但未返回 2已返回
    ''' </summary>
    ''' <remarks></remarks>
    Public IfSettleAccounts(GameMachineCount) As Integer
    ''' <summary>
    ''' true表示比倍中断线，此时待中心服务器结算后再退出
    ''' </summary>
    ''' <remarks></remarks>
    Public CompareBreakLine(GameMachineCount) As Boolean
    ''' <summary>
    ''' true表示疯狂送中断线，此时待中心服务器结算后再退出
    ''' </summary>
    ''' <remarks></remarks>
    Public CrazySendBreakLine(GameMachineCount) As Boolean
    ''' <summary>
    ''' 是否可以比倍
    ''' </summary>
    ''' <remarks></remarks>
    Public IfCanCompare(GameMachineCount) As Boolean
    ''' <summary>
    ''' 是否已经开始比倍
    ''' </summary>
    ''' <remarks></remarks>
    Public IfSatrtCompare(GameMachineCount) As Boolean
    ''' <summary>
    ''' 比倍后的分
    ''' </summary>
    ''' <remarks></remarks>
    Public ComparedScore(GameMachineCount) As Integer
    ''' <summary>
    ''' 即时送翻倍 true表示中了即时连线送
    ''' </summary>
    ''' <remarks></remarks>
    Public ImmediatelySendDouble(GameMachineCount) As Boolean
    ''' <summary>
    ''' 向中心服务器发送的信息
    ''' </summary>
    ''' <remarks></remarks>
    Public ToCenterSendMessage(GameMachineCount) As String
    ''' <summary>
    ''' 项目版本号(每次版本更新修改)
    ''' </summary>
    Public Const ItemVersion As String = "Version.20161201"
    Public made As String = "you are stupid rubshie"
#End Region

#Region "线程"
    ''' <summary>
    ''' //3秒线程
    ''' </summary>
    ''' <remarks></remarks>
    Private Update As Threading.Thread = New Threading.Thread(AddressOf UpdateParameter)
    ''' <summary>
    ''' 向登录服务器发送当前在机台的用户和累计蓝7，红7，天下彩金的线程
    ''' </summary>
    Private AccumulateJackpotThread As Threading.Thread = New Threading.Thread(AddressOf SendHandselMessage)
    ''' <summary>
    ''' 更新后台设置页参数
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UpdateParameter()
        ''  开启每3秒更新后台设置页参数线程()
        'For Each item As DatabaseInfoClass In DatabaseClass
        '    '如果获取的参数信息不为空
        '    If item IsNot Nothing Then
        '        '循环获取每个字段数据，重新给变量赋值
        '        item.SelectAspDbThread(False)
        '    End If
        'Next
        '更新后台参数
        While True
            Try
                Thread.Sleep(3000)
                '更新后台参数，获取每个字段数据，重新给变量赋值
                DatabaseClass.SelectAspDbThread()
                '清理不再线用户
                ' XTBUPdatecs()
                '很久不操作清理用户
                LongTimeNoOperate()
            Catch ex As Exception
                RecordError.WriteGameServerErr("SGP", "GameLogicModule->UPdatecs方法->更新后台设置页参数 错误：", ex)
            End Try
        End While
    End Sub
    ''' <summary>
    ''' 获取并发送彩金信息
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SendHandselMessage()
        While True
            Try
                '彩金自动累加逻辑 '有人下注时就开始自动累加
                If IfSomeOneBet.Contains(True) Then
                    '分别获取蓝7，红7，天下的彩金，存储临时变量中 
                    Dim Blue7 As Double = Blue7Money
                    Dim Red7 As Double = Red7Money
                    Dim World As Double = WorldMoney
                    '用来判断彩金是否是最大值
                    Dim IfMax1 As Integer = 0
                    Dim IfMax2 As Integer = 0
                    Dim IfMax3 As Integer = 0
                    '如果获取的蓝7彩金大于等于蓝7全盘 积分起始分数
                    If Blue7 >= Blue7StartIntegral Then
                        '就将蓝7彩金重新赋值
                        Blue7 = Blue7 + 80 * Blue7Bonus(1)
                        '如果蓝7彩金大于蓝7全盘 积分起始分数
                        If Blue7 > Blue7MaxIntegral Then
                            '就将蓝7全盘积分起始分数 赋值给蓝7彩金
                            Blue7 = Blue7MaxIntegral
                            '彩金是否是最大值改为1
                            IfMax1 = 1
                        End If
                    Else  '如果获取的蓝7彩金小于等于蓝7全盘 积分起始分数
                        '就将蓝7全盘积分起始分数 赋值给蓝7彩金，并且加上蓝7奖金抽粪
                        Blue7 = Blue7StartIntegral
                        Blue7 = Blue7 + 80 * Blue7Bonus(1)
                    End If
                    '如果获取的红7彩金大于等于红7全盘 积分起始分数
                    If Red7 >= Red7StartIntegral Then
                        '就将红7彩金重新赋值
                        Red7 = Red7 + 80 * Red7Bonus(1)
                        '如果红7彩金大于红7全盘 积分起始分数
                        If Red7 > Red7MaxIntegral Then
                            '就将红7全盘积分起始分数 赋值给红7彩金
                            Red7 = Red7MaxIntegral
                            '彩金是否是最大值改为1
                            IfMax2 = 1
                        End If
                    Else '如果获取的红7彩金小于等于红7全盘 积分起始分数
                        '就将红7全盘积分起始分数 赋值给红彩金，并且加上红7奖金抽粪
                        Red7 = Red7StartIntegral
                        Red7 = Red7 + 80 * Red7Bonus(1)
                    End If
                    '如果获取的天下彩金大于等于天下全盘 积分起始分数
                    If World >= WorldStartIntegral Then
                        '就将天下彩金重新赋值
                        World = World + 80 * WorldBonus(1)
                        '如果天下彩金大于天下全盘 积分起始分数
                        If World > WorldMaxIntegral Then
                            '就将天下全盘积分起始分数 赋值给天下彩金
                            World = WorldMaxIntegral
                            '彩金是否是最大值改为1
                            IfMax3 = 1
                        End If
                    Else '如果获取的天下彩金小于等于天下全盘 积分起始分数
                        '就将天下全盘积分起始分数 赋值给天下彩金，并且加上天下奖金抽粪
                        World = WorldStartIntegral
                        World = World + 80 * WorldBonus(1)
                    End If
                    '将获取的蓝7，红7，天下彩金分别赋值给  对应参数设置中的当前蓝7全盘积分、红7全盘积分、天下全盘积分
                    Blue7Money = Blue7
                    Red7Money = Red7
                    WorldMoney = World
                    '向所有分区发送彩金信息  
                    ' SendToAll("XS?" & Blue7Money & "/" & Red7Money & "/" & WorldMoney & "/" & IfMax1 & "/" & IfMax2 & "/" & IfMax3)
                    SendToAll("XS?" & Blue7Money & "," & Red7Money & "," & WorldMoney & "," & IfMax1 & "," & IfMax2 & "," & IfMax3)
                    '更新数据库中水果盘的所有彩金
                    ToDataBank.UpdateMoneyOfSgp(Blue7Money, Red7Money, WorldMoney)
                End If
            Catch ex As Exception
                RecordError.WriteGameServerErr("SGP", "GameLogicModule->SendHandselMessage方法->计算彩金发生错误", ex)
            End Try
            Threading.Thread.Sleep(5000)
        End While
    End Sub
#End Region

#Region "被动触发过程，内部每个过程都非常重要程序员负责填写"
    ''' <summary>
    ''' 过程特点：UI线程、被动触发
    ''' 触发条件：服务器程序点击【启动】按钮后，自动触发本过程
    ''' 作用说明：主要用于变量初始化或自定义线程、过程等
    ''' </summary>
    Public Sub ItemInitialise()
        '  新new 数据库信息类 算法类
        For i As Integer = 0 To 80
            AlgorithmClass(i) = New GameAlgorithmClass
        Next
        DatabaseClass = New DatabaseInfoClass
        '所有的参数现在从数据库中最后的字段读取

        MachineParameters()

        Try

            AccumulateJackpotThread.Start()
            Update.Start()
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->ItemInitialise方法->启动后变量初始化错误", ex)
        End Try
    End Sub
    ''' <summary>
    '''  用户连入事件（新用户）
    ''' </summary>
    Public Sub UesrNewConnEvent(ByRef _User As UserClass)

    End Sub
    ''' <summary>
    ''' 向所有人发送信息
    ''' </summary>
    Public Sub SendToAll(ByVal _Str As String)
        '向所有分区发送彩金信息
        For Each item As UserClass In UserList.Values
            item.SendData(_Str)
        Next
    End Sub
    ''' <summary>
    ''' 用户连入事件（旧用户重连）
    ''' </summary>
    Public Sub UesReconnEvent(ByRef _User As UserClass)
        _User.SendData("DXQ?")
        '断线重连的逻辑方法
        BreakLineReconnection(_User)
        '玩家断线重连，都得先登入，然后登入服务器给我们发送该玩家的检验码和名称， 框架会判断该用户是否重连， 重连的话就会触发本事件，进行断线重连相关操作
        '只需要给客户端发送重连的一些数据就OK， 不会在接收客户端发过来的DXCL了
    End Sub

    ''' <summary>
    ''' 用户断线事件
    ''' </summary>
    ''' <param name="_User"></param>
    Public Sub UserDisconnEvent(ByRef _User As UserClass)
        '内容程序员自由添加
        Dim SeatNum As Integer = _User.MachineNumber
        _User.IfBreak = True
        '掉线后，将玩家游戏状态改为false
        ' booUserIfInGame = False
        Try
            '比倍结算中 不退出
            If IfSatrtCompare(SeatNum) AndAlso CompareBreakLine(SeatNum) Then
                Exit Sub
            End If
            '疯狂送离线结算中 不退出
            If IfCrazySend(SeatNum) <> 0 AndAlso IfFreeFiveNum(SeatNum) > 0 AndAlso CrazySendBreakLine(SeatNum) Then
                Exit Sub
            End If
            'true表示比倍中断线，此时待中心服务器结算后再退出
            CompareBreakLine(SeatNum) = False
            'true表示疯狂送中断线，此时待中心服务器结算后再退出
            CrazySendBreakLine(SeatNum) = False
            '当用户处于比倍中时断线
            If IfSatrtCompare(SeatNum) Then
                'true表示比倍中断线，此时待中心服务器结算后再退出
                CompareBreakLine(SeatNum) = True
                Dim bili As String = IIf(RoomRatio(SeatNum) <= 1, "(" & 1 / RoomRatio(SeatNum) & ":1)", "(1:" & RoomRatio(SeatNum) & ")")
                '如果是越南分站 向中心服务器发送结算数据
                If VietnamStation.Contains(_User.Account.Split("-")(1)) Then
                    ToCenterServerSendData("SGP|" & SeatNum & "|" & 1 & "|1|10|" & "水果盤 " & SeatNum & "區<BR>" & bili & "<BR>" & "比倍輸##Bass Trái Cây   Khu " & SeatNum & "<BR>(" & 1 / RoomRatio(SeatNum) & ":1)<BR>" & "Nhân đôi thua|" & "" & "|" & _User.Account & "," & (-CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)))
                Else
                    ToCenterServerSendData("SGP|" & SeatNum & "|" & 1 & "|1|10|" & "水果盤 " & SeatNum & "區<BR>" & bili & "<BR>" & "比倍輸|" & "" & "|" & _User.Account & "," & (-CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)))
                End If
                Exit Sub
            End If
            '疯狂送中断线时，将未完成的疯狂送执行完
            If IfCrazySend(SeatNum) <> 0 AndAlso IfFreeFiveNum(SeatNum) > 0 Then 'AndAlso v_User.UserCamouflage = False
                'true表示疯狂送中断线，此时待中心服务器结算后再退出
                CrazySendBreakLine(SeatNum) = True
                '疯狂送离线结算
                CrazySendOffLineSettle(_User)
            Else
                '退出时初始化相关变量
                OutClearData(_User, True)
            End If

            ''断线时执行结算     ----------------------------------------------
            ClearingScore(_User)

        Catch ex As Exception
            OutClearData(_User, True)
            RecordError.WriteGameServerErr("GameLogicModule->UserDisconnEvent->用户断线错误", ex.Message, ex)
        End Try

    End Sub

    ''' <summary>
    ''' 用户接收数据事件
    ''' </summary>
    ''' <param name="_Tab">数据标头</param>
    ''' <param name="_Data">数据内容</param>
    ''' <param name="_User">用户</param>
    Public Sub UesrReceiveEvent(ByVal _Tab As String, ByVal _Data As String, ByRef _User As UserClass)
        '内容程序员自由添加        
        '如果数据不等于XTB，并且不等于error，将延迟退出时间改为now
        Dim SeatNum As Integer = _User.MachineNumber
        If _Tab <> "OK" AndAlso _Tab <> "error" Then LastOperationTime(SeatNum) = Now
        '如果玩家已经断线，又没有发送DXCL，不可以进行任何操作，退出方法
        If _User.IfBreak AndAlso _Tab <> "DXCL" Then
            Exit Sub
        End If
        '根据拆分的不同指令 进行不同操作
        Select Case _Tab
            Case "OK"
                '玩家是否在游戏中、状态改为true
                'booUserIfInGame = True
                '如果该座玩家 个人樱桃连线的次数 大于等于3次 、 就更新数据库中 樱桃的数据
                If PersonalCherryNum(SeatNum) >= 3 Then
                    PersonalCherryNum(SeatNum) = 0
                    UpdateOverAllNum(SeatNum, 10)
                End If
                '如果个人水果盘的次数大于等于3次 ，就更新数据库中水果盘的数据
                If PersonalFruitDishNum(SeatNum) >= 3 Then
                    PersonalFruitDishNum(SeatNum) = 0
                    UpdateOverAllNum(SeatNum, 11)
                End If
                ' 如果个人小B的次数大于等于3次，就更新数据库中小B的数据
                If PersonalSmallBarNum(SeatNum) >= 3 Then
                    PersonalSmallBarNum(SeatNum) = 0
                    UpdateOverAllNum(SeatNum, 12)
                End If
                '用来判断是否中了九宫格的荔枝 赋值为0
                IfJGGcherry(SeatNum) = 0
                '用来判断是否中了九宫格的水果 赋值为0
                IfJGGfruit(SeatNum) = 0
                ' 用来判断是否中了九宫格的小B 赋值为0
                IfJGGSmallBar(SeatNum) = 0
                '各分区底分 Subregion(SeatNum-1,3)
                EndPoints(SeatNum) = CInt(Subregion(SeatNum - 1, 3))
                '即时送的随机 1-5中的一个
                ImmediatelySendRnd(SeatNum) = RandomInt(1, 6)
                '各分区总押次数 设置连线送和连线送结束后为其赋值，其余时间不变 /BetNum各分区总押次数 每押一次该值加1
                TotalBetNum(SeatNum) = BetNum(SeatNum)
                '临时将房间比例改一下 ，用来测试
                RoomRatio(SeatNum) = 10
                '现在的数据发送格式 
                'OK?账号/玩家积分/各人樱桃连线的次数,个人水果盘的次数,个人小B的次数/当前红7积分，蓝7积分，天下积分/房间号/房间比例/房间总倍数 
                _User.SendData("OK?" & _User.Account & "/" & _User.Integral & "/" & PersonalCherryNum(SeatNum) & "," & PersonalFruitDishNum(SeatNum) & "," & PersonalSmallBarNum(SeatNum) & "/" & Red7Money & "," & Blue7Money & "," & WorldMoney & "/" & _User.MachineNumber & "/" & RoomRatio(SeatNum) & "/" & EndPoints(SeatNum)) 'Subregion(SeatNum - 1, 59)            
                ' 判断是否是被限制账号,限制会员樱桃连线以上的奖项不开放 / 为0不限制
                IfUserLimit(SeatNum) = IfRestrictedAccount(_User.Account)
                '更新后台设置页的游戏消息
                ShowGameMessage("用戶進入遊戲", 0, _User)
                ' 触发疯狂送时的连线得分
                CrazySendScore(SeatNum) = 0
                ' 记载疯狂送的总赢分数(针对修改记录的处理)
                CrazySendWinScore(SeatNum) = 0
                _User.ContinueEven = False
                ''在线方式
                'iOnlineType(SeatNum) = _sData(1)
                ''客户端系统讯息
                'If _sData.Count > 2 Then
                '    sSystemInfo(SeatNum) = _sData(2).Replace(",", ".")
                '    If (sSystemInfo(SeatNum).Length > 50) Then
                '        sSystemInfo(SeatNum) = sSystemInfo(SeatNum).Substring(0, 50)
                '    End If
                'End If        
            Case "XSCJ"
                '显示彩金  发送XS|蓝7彩金|红7彩金|天下彩金
                _User.SendData("XS?" & Blue7Money & "/" & Red7Money & "/" & WorldMoney)
            Case "QD"
                SyncLock Synchrolock
                    Try
                        '点击启动后，结算为0，如果等于1 ，结算还未返回
                        If IfSettleAccounts(SeatNum) = 1 Then
                            _User.SendData("CSH")
                            Exit Sub
                        End If
                        '如果该分区=1 就说明在公测中
                        If CInt(Subregion(SeatNum - 1, 59)) = 1 Then '公测中
                            '如果下注不在0-80范围就退出
                            If CInt(_Data) <= 0 OrElse CInt(_Data) > 80 Then Exit Sub

                        Else '正式环境
                            If CInt(_Data) <= 0 OrElse CInt(_Data) > 80 OrElse CInt(_Data) * EndPoints(SeatNum) > _User.Integral * RoomRatio(SeatNum) Then Exit Sub
                            If _User.Integral * RoomRatio(SeatNum) >= 8 * EndPoints(SeatNum) AndAlso CInt(_Data) < 8 Then Exit Sub
                            '当前积分小于8*底分,并且压注不等于最多可压注数
                            If _User.Integral * RoomRatio(SeatNum) < 8 * EndPoints(SeatNum) AndAlso CInt(_Data) <> _User.Integral * RoomRatio(SeatNum) / EndPoints(SeatNum) Then Exit Sub
                        End If
                        '下边是给初始化
                        '是否比倍最后一张改为false
                        LastOne(SeatNum) = False
                        '各分区总押次数 每押一次该值加1
                        BetNum(SeatNum) += 1
                        '压注(下注分数)
                        _User.BetNumeber = _Data
                        '先将記錄步驟记录游戏演示信息初始化为Nothing
                        RecordGameMessage(SeatNum) = Nothing
                        RecordGameMessage(SeatNum) = New StringBuilder
                        '往游戏演示信息加入各分区底分、
                        RecordGameMessage(SeatNum).Append(EndPoints(SeatNum) & "$")
                        RecordGameMessage(SeatNum).Append(CInt(_Data) & "$")
                        'true表示比倍中断线，此时待中心服务器结算后再退出
                        CompareBreakLine(SeatNum) = False
                        'true表示疯狂送中断线，此时待中心服务器结算后再退出
                        CrazySendBreakLine(SeatNum) = False
                        '是否可以比倍
                        IfCanCompare(SeatNum) = False
                        '是否已经开始比倍
                        IfSatrtCompare(SeatNum) = False
                        '比倍后的分
                        ComparedScore(SeatNum) = 0
                        '即时送翻倍 true表示中了即时连线送
                        ImmediatelySendDouble(SeatNum) = False
                        '表示AP启动后是否有人下注
                        IfSomeOneBet(SeatNum) = True
                        '当局是否中了强制奖项
                        NowIfForceWin(SeatNum) = ""
                        '是否已结算 0未结算 1已发送结算但未返回 2已返回
                        IfSettleAccounts(SeatNum) = 0
                        '是否中全盘 1.红七2.蓝七，3.天下
                        _User.IfOverAll = 0
                        ''中了什么奖几注
                        _User.AwardAndNum(0) = ""
                        _User.AwardAndNum(1) = ""
                        '用户玩过游戏了,在线程里用来判断玩家未操作时间，是在玩过还是没玩过游戏的状态
                        If _User.IsPlay = False Then
                            ManageServer.ClientSocket.SendAppData("YW?" & _User.CheckCode)
                            _User.IsPlay = True
                        End If
                        '如果个人樱桃连线的次数大于等于3此，就赋值为0，并更新数据库
                        If PersonalCherryNum(SeatNum) >= 3 Then
                            PersonalCherryNum(SeatNum) = 0
                            UpdateOverAllNum(SeatNum, 10)
                        End If
                        '如果个人水果盘的次数大于等于3此，就赋值为0，并更新数据库
                        If PersonalFruitDishNum(SeatNum) >= 3 Then
                            PersonalFruitDishNum(SeatNum) = 0
                            UpdateOverAllNum(SeatNum, 11)
                        End If
                        '如果个人小B的次数大于等于3此，就赋值为0，并更新数据库
                        If PersonalSmallBarNum(SeatNum) >= 3 Then
                            PersonalSmallBarNum(SeatNum) = 0
                            UpdateOverAllNum(SeatNum, 12)
                        End If
                        '将用户掉线信息清空
                        _User.Clear()
                        '判断是否为疯狂中， 0：不是疯狂送   1：樱桃(蓝7)  2：水果盘(红7)  3：小BAR(天下)
                        If IfCrazySend(SeatNum) = 0 Then
                            '将疯狂送中奖分数归零
                            _User.CrazySendWinScore = 0
                        End If
                        '表示即时连线送的时候 遇到疯狂送
                        If ImmediatelySend(SeatNum) AndAlso IfCrazySend(SeatNum) <> 0 Then
                            _User.SendData("TS") 'jsstop
                            ' 各分区总押次数 设置连线送和连线送结束后为其赋值，其余时间不变
                            TotalBetNum(SeatNum) = TotalBetNum(SeatNum) + 1
                            '用户断线及时送 改为true
                            _User.ImmediatelySendClose = True
                        End If
                        '处于即时送中时显示即时送，通常作用于疯狂送结束后
                        If ImmediatelySend(SeatNum) AndAlso IfCrazySend(SeatNum) = 0 AndAlso TotalBetNum(SeatNum) + 5 <> BetNum(SeatNum) Then
                            _User.SendData("KS?" & ImmediatelySendLeft(SeatNum) & "/" & ImmediatelySendRight(SeatNum)) 'jsplay
                        End If
                        '即时连线送逻辑   Subregion(SeatNum - 1, 57) = 150 * 250 * 300 * 400 * 500 / 多少把以后即时连线
                        Dim dush As Integer = Subregion(SeatNum - 1, 57).Split("*")(ImmediatelySendRnd(SeatNum) - 1)
                        If dush > 0 Then
                            '相当于游戏开始dush局之后开始即时送    
                            If TotalBetNum(SeatNum) + dush = BetNum(SeatNum) AndAlso ImmediatelySend(SeatNum) = False Then
                                '即时送开始
                                StartInstantDelivery(_User)
                                ' 即时送翻倍的变量 ，true表示可以即时送
                                ImmediatelySend(SeatNum) = True
                                '各分区总押次数 设置连线送和连线送结束后为其赋值，其余时间不变
                                TotalBetNum(SeatNum) = BetNum(SeatNum)
                            End If
                            '5局之后即时送结束
                            If ImmediatelySend(SeatNum) = True Then 'TotalBetNum(SeatNum) + 5 = BetNum(SeatNum) AndAlso
                                ' 即时送翻倍的变量 ，true表示可以即时送
                                ImmediatelySend(SeatNum) = False
                                '各分区总押次数 设置连线送和连线送结束后为其赋值，其余时间不变
                                TotalBetNum(SeatNum) = BetNum(SeatNum)
                                '即时送左边随机 （櫻桃.铆丁.蘋果.葡萄.西瓜.小BAR.中BAR.大BAR）
                                ImmediatelySendLeft(SeatNum) = 0
                                '即时送右边随机
                                ImmediatelySendRight(SeatNum) = 0
                                '即时送结束
                                _User.SendData("JSW")
                                ' 即时送的随机 1-5中的一个
                                ImmediatelySendRnd(SeatNum) = RandomInt(1, 6)
                            End If
                        End If
                        ' 即时送翻倍的变量 ，true表示可以即时送
                        If ImmediatelySend(SeatNum) = False Then
                            _User.SendData("TS") 'jsstop
                        End If
                        '账号限制信息 是否有会员限制，0没有 1限制模式  2低于当前模式
                        IfUserLimit(SeatNum) = IfRestrictedAccount(_User.Account)
                        ' 是否疯狂送 0：不是疯狂送   1：樱桃(蓝7)  2：水果盘(红7)  3：小BAR(天下)
                        If IfCrazySend(SeatNum) <> 0 Then
                            ShowGameMessage("用戶正在遊戲中", IfCrazySend(SeatNum), _User)
                        End If
                        ToCenterSendMessage(SeatNum) = JudgmentLine(_User, _Data)
                        GC.Collect()
                    Catch ex As Exception
                        RecordError.WriteGameServerErr("GameLogicModule->ReceiveFlashData方法->QDYX错误", ex.Message, ex)
                    End Try
                End SyncLock
            Case "DUANOK" '断线重连后进入游戏              
                _User.SendData("YS?" & PersonalCherryNum(SeatNum) & "," & PersonalFruitDishNum(SeatNum) & "," & PersonalSmallBarNum(SeatNum) & "," & SeatNum & "/" & Subregion(SeatNum - 1, 59) & "/" & IfCrazySend(SeatNum) & "/" & 150 & "/" & ItemVersion & "/1/" & _User.Integral * RoomRatio(SeatNum) & "/" & EndPoints(SeatNum))
                _User.SendData("XS?" & Blue7Money & "/" & Red7Money & "/" & WorldMoney)
                _User.GameState = "0"
            Case "QXZD" '取消自动游戏 ----------------------------------- 这个应该没必要， 客户端自己决定取消就好了
                _User.SendData("QXZD?")
            Case "JIESUAN"
                ClearingScore(_User)
            Case "TC" '游戏中退出机台              
                '退出时初始化相关变量
                OutClearData(_User, False)
                '发送CLOSE、 XS|蓝7彩金|红7彩金|天下彩金
                _User.SendData("TCJT") 'CLOSE
                _User.SendData("XS?" & Blue7Money & "/" & Red7Money & "/" & WorldMoney)
            Case "QL"
                '下注分数=1
                _User.BetReset = 1
            Case "LeftFen" '点得分时，作用于断线重连
                If IfSatrtCompare(SeatNum) = False Then
                    ComparedScore(SeatNum) = 0
                    IfCanCompare(SeatNum) = False
                    '将疯狂送中奖分数归零
                    _User.CrazySendWinScore = 0
                    _User.ResidualFraction = _Data
                    '将中全盘奖分数归零
                    _User.AllAwardsScores = 0
                    _User.SendData("DF?" & _Data)
                End If
            Case "WYB" '申请我要比倍  WYB?2100  后边是分
                SyncLock Synchrolock
                    Try
                        If IfCanCompare(SeatNum) = True Then
                            DemoInitialPoints(SeatNum) = CompareInitialPoints(SeatNum)
                            '求出左边字符串和报送位置
                            AlgorithmClass(SeatNum).InitialiseInfo(SeatNum)
                            If Trim(CompareInitialPoints(SeatNum)).IndexOf(".") <> -1 Then
                                '记录比倍信息，用于断线重连 
                                _User.CompareMessage = "AB?" & CardsNum(SeatNum) & "/" & RightCardStr(SeatNum) & "/" & CompareInitialPoints(SeatNum).ToString().Substring(0, CompareInitialPoints(SeatNum).ToString().IndexOf(".")) & "$" & CompareInitialPoints(SeatNum)
                                ' _User.SendData("AB?" & CardsNum(SeatNum) & "/" & RightCardStr(SeatNum) & "/" & CompareInitialPoints(SeatNum).ToString().Substring(0, CompareInitialPoints(SeatNum).ToString().IndexOf("."))) '可以按比倍|左侧|右侧的牌|保送的牌| 
                                _User.SendData("AB?" & CardsNum(SeatNum) & "," & RightCardStr(SeatNum) & "|" & CompareInitialPoints(SeatNum).ToString())
                            Else
                                _User.CompareMessage = "AB?" & CardsNum(SeatNum) & "/" & RightCardStr(SeatNum) & "/" & CompareInitialPoints(SeatNum).ToString() & "$" & CompareInitialPoints(SeatNum)
                                _User.SendData("AB?" & CardsNum(SeatNum) & "/" & RightCardStr(SeatNum) & "/" & CompareInitialPoints(SeatNum).ToString()) '可以按比倍|左侧|右侧的牌|保送的牌|
                                ' _User.SendData("AB?15,15,15|" & CompareInitialPoints(SeatNum).ToString)
                            End If
                            IfSatrtCompare(SeatNum) = True
                        End If
                    Catch ex As Exception
                        RecordError.WriteGameServerErr("GameLogicModule->ReceiveFlashData方法->WYB错误", ex.Message, ex)
                    End Try
                End SyncLock
            Case "WYY" '押大    WYL?2     1大2小               
                SyncLock Synchrolock
                    Try
                        '是否可以比倍
                        If IfCanCompare(SeatNum) = True Then
                            '比倍次数自身加一
                            CompareNum(SeatNum) += 1
                            '如果比倍后的分=0、 就将比倍的初始分赋值给比倍后的分
                            If ComparedScore(SeatNum) = 0 Then ComparedScore(SeatNum) = CompareInitialPoints(SeatNum)
                            Dim bigor As Integer '比倍结果
                            '如果比倍后的分
                            If ComparedScore(SeatNum) <= Subregion(SeatNum - 1, 58) Then
                                bigor = 1 '可赢可输
                            Else
                                bigor = 2 '必输
                            End If
                            ' _sData(1)--->1大2小
                            Select Case AlgorithmClass(SeatNum).UpdateCardData(_Data, _User, bigor)
                                            '比倍平局
                                Case 0
                                    ' 给演示用的初始分 =  比倍的初始分  
                                    DemoInitialPoints(SeatNum) = CompareInitialPoints(SeatNum)
                                    _User.CompareMessage = "YL?" & ShowCards(SeatNum) & "/" & CardsNum(SeatNum) & "/" & RightShowSix(SeatNum) & "/" & ComparedScore(SeatNum) & "/" & CompareNum(SeatNum) & "$" & CompareInitialPoints(SeatNum)
                                    '押大了(平)
                                    _User.SendData("YL?" & ShowCards(SeatNum) & "/" & CardsNum(SeatNum) & "/" & RightShowSix(SeatNum) & "/" & ComparedScore(SeatNum) & "/" & CompareNum(SeatNum))
                                                '比倍赢
                                Case 1
                                    '判断是否是第一次比倍赢，并将比倍结果重新计算保存.
                                    If ComparedScore(SeatNum) = 0 Then
                                        ComparedScore(SeatNum) = CInt(CompareInitialPoints(SeatNum)) * 2
                                    Else
                                        ComparedScore(SeatNum) = ComparedScore(SeatNum) * 2
                                    End If
                                    '、将比倍的初始分赋值给演示的初始分
                                    DemoInitialPoints(SeatNum) = CompareInitialPoints(SeatNum)
                                    '用户断线时比倍信息 =比倍要显示的牌/牌的张数/比倍时的右边显示.6个以内/比倍后分/比倍的次数/比倍的初始分/
                                    _User.CompareMessage = "YL?" & ShowCards(SeatNum) & "/" & CardsNum(SeatNum) & "/" & RightShowSix(SeatNum) & "/" & ComparedScore(SeatNum) & "/" & CompareNum(SeatNum) & "$" & CompareInitialPoints(SeatNum)
                                    '将用户断线时比倍信息发送给玩家
                                    _User.SendData("YL?" & ShowCards(SeatNum) & "/" & CardsNum(SeatNum) & "/" & RightShowSix(SeatNum) & "/" & ComparedScore(SeatNum) & "/" & CompareNum(SeatNum))                          '比倍输
                                Case 2
                                    '记录本局比倍输
                                    LastOne(SeatNum) = True
                                    '记录是否可以比倍
                                    IfCanCompare(SeatNum) = False
                                    'BNB?比倍要显示的牌/牌的张数/比倍时的右边显示.6个以内/比倍后分/比倍的次数/比倍的初始分
                                    _User.SendData("BNB?" & ShowCards(SeatNum) & "/" & CardsNum(SeatNum) & "/" & RightShowSix(SeatNum) & "/" & ComparedScore(SeatNum) & "/" & CompareNum(SeatNum))
                                    Dim bili As String = IIf(RoomRatio(SeatNum) <= 1, "(" & 1 / RoomRatio(SeatNum) & ":1)", "(1:" & RoomRatio(SeatNum) & ")")
                                    '如果是越南分站
                                    If VietnamStation.Contains(_User.Account.Split("-")(1)) Then
                                        '向中心服务器发送结算数据 、押小输了
                                        ToCenterServerSendData("SGP|" & SeatNum & "|" & 1 & "|1|10|" & "水果盤 " & SeatNum & "區<BR>" & bili & "<BR>" & "比倍輸##Bass Trái Cây   Khu " & SeatNum & "<BR>(" & 1 / RoomRatio(SeatNum) & ":1)<BR>" & "Nhân đôi thua|" & "" & "|" & _User.Account & "," & (-CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)))
                                    Else
                                        '向中心服务器发送结算数据 、押小输了
                                        ToCenterServerSendData("SGP|" & SeatNum & "|" & 1 & "|1|10|" & "水果盤 " & SeatNum & "區<BR>" & bili & "<BR>" & "比倍輸|" & "" & "|" & _User.Account & "," & (-CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)))
                                    End If
                                    '将比倍后的分归零
                                    ComparedScore(SeatNum) = 0
                                    '将比倍的初始分赋值给给演示用的初始分
                                    DemoInitialPoints(SeatNum) = CompareInitialPoints(SeatNum)
                                    '将比倍的初始分归零
                                    CompareInitialPoints(SeatNum) = 0
                                    '是否已经开始比倍状态改为false
                                    IfSatrtCompare(SeatNum) = False
                                    '清空退出比倍信息
                                    _User.CompareMessage = ""
                                    '中全盘奖分数归零
                                    _User.AllAwardsScores = 0
                            End Select
                        End If
                    Catch ex As Exception
                        RecordError.WriteGameServerErr("GameLogicModule->ReceiveFlashData方法->WYL错误", ex.Message, ex)
                    End Try
                End SyncLock
            Case "ZGL" '如果有中即时连线送，重新设置即时送参数
                SyncLock Synchrolock(SeatNum)
                    '如果有中即时连线送
                    If ImmediatelySendDouble(SeatNum) Then
                        '开始即时送
                        StartInstantDelivery(_User)
                        '为1时表示中即时送后重新生成了即时送，重连后要显示新的
                        _User.NewImmediatelySend = "1"
                    End If
                End SyncLock
                            '赢后退出比倍   BBL|6400  6400为中奖后的份数
            Case "BBL"
                SyncLock Synchrolock
                    '如果比了而且没比输
                    If CompareNum(SeatNum) <> 0 AndAlso IfCanCompare(SeatNum) = True Then
                        IfSatrtCompare(SeatNum) = False
                        '记录是否可以比倍
                        IfCanCompare(SeatNum) = False
                        '清空退出比倍信息
                        _User.CompareMessage = ""
                        '发送比倍后的分
                        _User.SendData("CQB?" & ComparedScore(SeatNum))
                        Dim bili As String = IIf(RoomRatio(SeatNum) <= 1, "(" & 1 / RoomRatio(SeatNum) & ":1)", "(1:" & RoomRatio(SeatNum) & ")")
                        '如果是越南分站，就向中心服务器发送结算数据包括（用户|比倍后的分-比倍初始分/剩余额度倍数倍|用户IP|在线方式|系统信息等）
                        If VietnamStation.Contains(_User.Account.Split("-")(1)) Then
                            ToCenterServerSendData("SGP|" & SeatNum & "|" & 1 & "|1|10|" & "水果盤 " & SeatNum & "區<BR>" & bili & "<BR>" & "比倍贏##Bass Trái Cây   Khu " & SeatNum & "<BR>(" & 1 / RoomRatio(SeatNum) & ":1)<BR>" & "Nhân đôi thắng|" & "" & "|" & _User.Account & "," & ((ComparedScore(SeatNum) - CompareInitialPoints(SeatNum)) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)))
                        Else
                            ToCenterServerSendData("SGP|" & SeatNum & "|" & 1 & "|1||0|" & "水果盤 " & SeatNum & "區<BR>" & bili & "<BR>" & "比倍贏|" & "" & "|" & _User.Account & "," & ((ComparedScore(SeatNum) - CompareInitialPoints(SeatNum)) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)) & "," & (CompareInitialPoints(SeatNum) / RoomRatio(SeatNum)))
                        End If
                    End If
                End SyncLock
            Case "TS" '判断是否比倍输过，如果没有输则将左右牌重新发侄客户端  TS|
                SyncLock Synchrolock
                    '如果不是比倍的最后一张、并且可以比倍，就向该玩家发送：牌的张数|比过的牌|53
                    If LastOne(SeatNum) = False AndAlso IfCanCompare(SeatNum) Then
                        _User.SendData("TS?" & CardsNum(SeatNum) & "/" & RightCardStr(SeatNum) & "/" & 53)
                    End If
                End SyncLock
            Case "DXCL"
                '断线重连的逻辑方法
                BreakLineReconnection(_User)
            Case "CY" '长时间没操作倒计时中点击继续按钮
                '将玩家最后的操作时间改为Now
                LastOperationTime(SeatNum) = Now
                '玩家续连状态改为true
                '_User.xulian = True
        End Select

    End Sub


#End Region

#Region "###  根据游戏不同在下方添加游戏过程或函数 ###"
    ''' <summary>
    ''' 向中心服务器发送结算数据
    ''' </summary>
    ''' <param name="_Str">
    ''' 数据格式为：游戏标记|分区号|桌子号|实际参与游戏中人数|自定义数据|记录数据|用户1，得分,ip|用户2，得分,ip|用户3，得分....</param>
    ''' 如：ZYJZ|77|1|1|5|趙雲救主 77區br>(1:20)br>投注30條線，每線投注：100分，總投注：3000分br>第3條線,撥浪鼓圖案,得分:2000 /font>br>本局除了Jackpot共得分：2000||AW61-1,-50,127.0.0.1,150,150,1
    ''' <remarks>注：此方法发送后，中心服务器处理后，会返回结果信息.</remarks>
    Private Sub ToCenterServerSendData(ByVal _Str As String)
        '发送给中心服务器结算数据

    End Sub
    ''' <summary>
    ''' 判断中奖线
    ''' </summary>
    ''' <param name="_User">玩家用户</param>
    ''' <param name="_BetNum">玩家的 下注倍数</param>
    ''' <returns> 返回该玩家的中奖参数信息</returns>
    ''' <remarks></remarks>
    Public Function JudgmentLine(ByVal _User As UserClass, ByVal _BetNum As String)
        '获取玩家分区号
        Dim SeatNum As Integer = _User.MachineNumber
        Dim strSend As String = String.Empty
        Try
            '判断是不是疯狂送 0：不是疯狂送   1：樱桃(蓝7)  2：水果盘(红7)  3：小BAR(天下)
            Select Case IfCrazySend(SeatNum)
                Case 0
                    Dim _listRecordParameter As New List(Of Double)
                    _listRecordParameter.Add(10)
                    '如果不是疯狂送，就把该分区的所有参数信息都存入到临时变量新集合_listRecordParameter
                    For i As Integer = 6 To 55
                        ' 分区
                        _listRecordParameter.Add(Subregion(SeatNum - 1, i))
                    Next
                    '是否开放蓝7、红7、ts全盘
                    If IfOpenBlue7All(SeatNum) = 0 Then _listRecordParameter(27) = 0
                    If IfOpenRed7All(SeatNum) = 0 Then _listRecordParameter(28) = 0
                    If IfOpenWorldAll(SeatNum) = 0 Then _listRecordParameter(29) = 0
                    ShowGameMessage("用戶正在遊戲中", 0, _User)
                    '如果后台设置强制中奖
                    If ForceWin(SeatNum) <> "" AndAlso CInt(ForceWin(SeatNum)) > 0 Then
                        '将强制中的奖项存入到  当局是否强制中奖全局变量中
                        NowIfForceWin(SeatNum) = ForceWin(SeatNum)
                        '判断强制中的奖项，来控制中3个连线的按钮
                        Select Case CInt(ForceWin(SeatNum))
                            Case 1 '蓝七
                                PushButton = 27
                            Case 2 '红七
                                PushButton = 28
                            Case 3 '天下
                                PushButton = 29
                            Case 4 '樱桃连线
                                PushButton = 3
                            Case 5 '水果盘
                                PushButton = 16
                            Case 6 '小BAR连线
                                PushButton = 9
                            Case 7 To 16 '樱桃全盘、铆钉全盘、苹果全盘、葡萄全盘、西瓜全盘、杂B全盘、小B全盘、中B全盘、大B全盘、杂7全盘
                                PushButton = CInt(ForceWin(SeatNum)) + 10
                        End Select
                        '获取中奖线，9个位置上的图片代号
                        _User.WinData = AlgorithmClass(SeatNum).WinData(SeatNum, _User.BetNumeber, Subregion)
                        '判断该玩家是否中全盘
                        IfOverAll(_User)
                        '强制中奖的设置信息清空
                        ForceWin(SeatNum) = ""
                        Exit Select
                    End If
                    '总赢积分/总押注积分
                    Dim MyPercent As Double = DatabaseClass.TotalWin(SeatNum) / DatabaseClass.TotalBet(SeatNum)
                    '当前输赢百分比
                    Dim Percentage As Double = Math.Floor(MyPercent * 10000) / 10000.0
                    '游戏输赢百分比
                    Dim GamePercentage As Double = Math.Floor(DatabaseClass.Percentage(SeatNum) * 10000) / 10000.0
                    '后台设置的 疯狂送是否开放樱桃、小B、水果盘的代码
                    If IfOpenCrazySendCherry(SeatNum) = 0 Then _listRecordParameter(3) = 0
                    If IfOpenCrazySendBar(SeatNum) = 0 Then _listRecordParameter(9) = 0
                    If IfOpenCrazySendFruitDish(SeatNum) = 0 Then _listRecordParameter(16) = 0
                    '限制会员 可中一个、两个樱桃 
                    If CherryIfOpen(SeatNum) = 0 OrElse IfUserLimit(SeatNum) = 1 Then
                        '获取一个0-3的随机数
                        Dim xiannumber As Integer = RandomInt(0, 3)
                        '如果随机数等于0 未中奖
                        If xiannumber = 0 Then
                            _User.WinData = AlgorithmClass(SeatNum).MustLost()
                        Else '中一个或两个樱桃
                            '将随机数赋值给 控制中3个连线的按钮
                            PushButton = xiannumber
                            '获取中奖线，9个位置上的图片代号
                            _User.WinData = AlgorithmClass(SeatNum).WinData(SeatNum, _User.BetNumeber, Subregion)
                            '判断该玩家是否中全盘
                            IfOverAll(_User)
                        End If
                        Exit Select
                    Else '如果会员限制低于当前模式
                        If IfUserLimit(SeatNum) = 2 Then
                            '如果当前输赢百分比大于游戏输赢百分比
                            If Percentage > GamePercentage Then
                                '获取一个随机数
                                Dim RanNum As Integer = RandomInt(1, Subregion(SeatNum - 1, 5) + 1)
                                '必输的时候不让中疯狂送，将疯狂送状态归零
                                _listRecordParameter(3) = 0
                                _listRecordParameter(9) = 0
                                _listRecordParameter(16) = 0
                                '如果随机数大于1，并且游戏状态小于等于8
                                If RanNum > 1 AndAlso CInt(_User.GameState) <= 8 Then
                                    '执行用户必输的逻辑
                                    _User.WinData = AlgorithmClass(SeatNum).MustLost()
                                    '更新玩家的游戏状态
                                    _User.GameState = Trim(CInt(_User.GameState) + 1)
                                Else  '连续8次未中奖之后必赢  
                                    '执行用户必赢时的逻辑
                                    AlgorithmClass(SeatNum).MustWin(_listRecordParameter, _User)
                                    '将游戏状态赋值为0
                                    _User.GameState = "0"
                                End If
                                Exit Select
                            End If
                        End If
                    End If
                    '如果当前输赢百分比小于游戏输赢百分，此时比可以赢
                    If Percentage < GamePercentage Then
                        '获取随机数
                        Dim RanNum As Integer = RandomInt(1, RandomInt(5, 8) + 1)
                        '如果随机数大于1，必赢
                        If RanNum > 1 Then
                            '执行用户必赢时的逻辑
                            AlgorithmClass(SeatNum).MustWin(_listRecordParameter, _User)
                        Else '执行用户必输的逻辑
                            _User.WinData = AlgorithmClass(SeatNum).MustLost()
                        End If
                        '如果当前输赢百分比大于游戏输赢百分，此时比可以赢
                    ElseIf Percentage > GamePercentage Then
                        '获取随机数
                        Dim RanNum As Integer = RandomInt(1, Subregion(SeatNum - 1, 5) + 1)
                        '必输的时候不让中疯狂送，将疯狂送状态归零
                        _listRecordParameter(3) = 0
                        _listRecordParameter(9) = 0
                        _listRecordParameter(16) = 0
                        '如果随机大于1，并且游戏状态大于等于8
                        If RanNum > 1 AndAlso CInt(_User.GameState) <= 8 Then
                            '执行用户必输的逻辑
                            _User.WinData = AlgorithmClass(SeatNum).MustLost()
                            _User.GameState = Trim(CInt(_User.GameState) + 1)
                        Else  '连续8次未中奖之后必赢 、执行用户必赢时的逻辑                                      
                            AlgorithmClass(SeatNum).MustWin(_listRecordParameter, _User)
                            _User.GameState = "0"
                        End If
                        '如果当前输赢百分比等于游戏输赢百分，此时可赢可输
                    ElseIf Percentage = GamePercentage Then
                        '获取随机数
                        Dim Ran As Integer = RandomInt(1, 5)
                        '如果随机数大于2'必输
                        If Ran > 2 Then
                            '执行用户必输的逻辑
                            _User.WinData = AlgorithmClass(SeatNum).MustLost()
                        Else '否则必赢，执行用户必赢时的逻辑
                            AlgorithmClass(SeatNum).MustWin(_listRecordParameter, _User)
                        End If
                    End If
                Case 1, 2, 3 '此时疯狂送
                    '疯狂送时中全盘
                    CrazySendOverall(_User, IfCrazySend(SeatNum))
                    '如果后台设置强制中奖
                    If ForceWin(SeatNum) <> "" AndAlso Int(ForceWin(SeatNum)) > 0 Then
                        '将强制中的奖项存入到  当局是否强制中奖全局变量中
                        NowIfForceWin(SeatNum) = ForceWin(SeatNum)
                        '再将强制中奖参数清空
                        ForceWin(SeatNum) = ""
                    End If
            End Select
            '当前积分减去下注积分=用户当前积分*倍数-底分
            ResidueScore(SeatNum) = _User.Integral * RoomRatio(SeatNum) - CInt(_BetNum) * EndPoints(SeatNum)
            '記錄步驟记录游戏演示信息
            RecordGameMessage(SeatNum).Append("%" & _User.WinData & "*")
            '把最后的连线奖金求出来 ,user,蓝，红，天下
            OnLineBonus(_User, Blue7Money, Red7Money, WorldMoney)
            '記錄步驟记录游戏演示信息后边 再加上 蓝7,、红7、天下、剩余分数的数据
            RecordGameMessage(SeatNum).Append(Blue7Money & "|" & Red7Money & "|" & WorldMoney & "|" & RoomRatio(SeatNum) & "|$")
            '向中心服务器发送该玩家中奖的参数信息
            Dim ToCenterSend As String = ToCenterSendWinInfo(_User)
            '是否中全盘 1.红七2.蓝七，3.天下
            If _User.IfOverAll = 0 Then
                strSend = ToCenterSend
            Else '中了红7、蓝7、天下全盘
                Dim strarray1() As String = ToCenterSend.Split("|")
                '全盘彩金奖项中奖信息
                Dim sss As String = OverallWinMessage(_User)
                '将全盘彩金奖项中奖信息拆分成数组
                Dim strarray2() As String = sss.Split("|")
                '判断是不是越南站，都重新赋值该玩家的中奖参数信息
                If VietnamStation.Contains(_User.Account.Split("-")(1)) Then
                    strarray1(5) = strarray1(5).Split("##")(0) & strarray2(5).Split("&")(0).Substring(4) & ":" & strarray2(7).Split(",_User")(1) & "##" & strarray1(5).Split("##")(2) & strarray2(5).Split("&")(1).Substring(4) & ":" & strarray2(7).Split(",")(1).Replace(".", ",")
                    strarray1(7) = strarray1(7).Split(",")(0) & "," & (CDbl(strarray1(7).Split(",")(1)) + CDbl(strarray2(7).Split(",")(1))).ToString() & "," & strarray1(7).Split(",")(2) & "," & strarray1(7).Split(",")(3) & "," & strarray1(7).Split(",")(4)
                Else
                    strarray1(5) = strarray1(5) & strarray2(5).Split("&")(0).Substring(4) & ":" & strarray2(7).Split(",")(1)
                    strarray1(7) = strarray1(7).Split(",")(0) & "," & (CDbl(strarray1(7).Split(",")(1)) + CDbl(strarray2(7).Split(",")(1))).ToString() & "," & strarray1(7).Split(",")(2) & "," & strarray1(7).Split(",")(3) & "," & strarray1(7).Split(",")(4)
                End If
                Dim s2 As String = String.Empty
                '循环将玩家的中奖参数信息都存入到变量s2中
                For i As Integer = 0 To strarray1.Length - 1
                    s2 = s2 & strarray1(i) & "|"
                Next
                s2 = s2.Trim("|")
                strSend = s2
            End If
            '将该玩家的比倍初始分 重新赋值       
            CompareInitialPoints(SeatNum) = ToCenterSend.Split("|")(7).Split(",")(1) * RoomRatio(SeatNum) + EndPoints(SeatNum) * _User.BetNumeber
            '获取输赢分数
            Dim fen = strSend.Split("|")(7).Split(",")(1)
            '疯狂送时统计五把疯狂送的得分
            If IfCrazySend(SeatNum) <> 0 Then
                '疯狂送中将分数 = 自身 +  输赢分数*剩余额度倍
                _User.CrazySendWinScore = _User.CrazySendWinScore + fen * RoomRatio(SeatNum)
            End If
            '  _User.SendData("SHOW?SFTA?0|1:10,2:10,3:10,4:10,5:10,6:10,7:10,8:10,9:10|521")
            '新加逻辑 在客户端点击“转”“开牌”“启动”的逻辑处理方法内添加逻辑“向中心服务器发送扣分请求”
            ' 发送的字符串 = A0|游戏标记|用户帐号|下注真实额度（总投注/房间比例）|用户IP|分区号,0,0|设备类型（没有，默认1）|客户端系统信息（没有写空）|分区号,1,1|下注说明
            If IfCrazySend(SeatNum) = 0 Then
                NoteSingle(SeatNum) = "SHOW?#" & Replace(_User.WinData, "|", "/") & "#" & fen

                Dim _sStr As String = "A0|" & "SGP" & "|" & _User.Account & "|" & CInt(_BetNum) / RoomRatio(SeatNum) & "|" & "|" & SeatNum & ",0,0" & "|" & "|" & SeatNum & ",1,1" & "|" & "下注说明"
                'ToCenterServerSendData(_sStr)
                SimulationCenterServer(_sStr, _User)
            Else
                '发送得分数据（SHOW|#中奖字符串#得分）
                _User.SendData("SHOW?#" & Replace(_User.WinData, "|", "/") & "#" & fen)
            End If
            '是否中了红7、蓝7、天下全盘
            Select Case CInt(_User.IfOverAll)
                Case 1
                    _User.SendData("ZLL?" & 1 & "/" & BureauRed7Bonus(SeatNum) * RoomRatio(SeatNum) & "/" & Red7Money)
                    '中全盘奖分数=比倍初始分+本局所中全盘彩金*剩余额度倍
                    _User.AllAwardsScores = CompareInitialPoints(SeatNum) + BureauRed7Bonus(SeatNum) * RoomRatio(SeatNum)
                Case 2
                    _User.SendData("ZLL?" & 2 & "/" & BureauBlue7Bonus(SeatNum) * RoomRatio(SeatNum) & "/" & Blue7Money)
                    '中全盘奖分数=比倍初始分+本局所中全盘彩金*剩余额度倍
                    _User.AllAwardsScores = CompareInitialPoints(SeatNum) + BureauBlue7Bonus(SeatNum) * RoomRatio(SeatNum)
                Case 3
                    _User.SendData("ZLL?" & 3 & "/" & BureauWorldBonus(SeatNum) * RoomRatio(SeatNum) & "/" & WorldMoney)
                    '中全盘奖分数=比倍初始分+本局所中全盘彩金*剩余额度倍
                    _User.AllAwardsScores = CompareInitialPoints(SeatNum) + BureauWorldBonus(SeatNum) * RoomRatio(SeatNum)
            End Select
            '获取个人樱桃次数
            PersonalCherryNumMax(SeatNum) = PersonalCherryNum(SeatNum)
            '获取个人水果盘次数
            PersonalFruitDishNumMax(SeatNum) = PersonalFruitDishNum(SeatNum)
            '获取个人小B次数
            PersonalSmallBarNumMax(SeatNum) = PersonalSmallBarNum(SeatNum)
            '用来判断是否中了九宫格的樱桃
            If IfJGGcherry(SeatNum) = 1 Then
                '更新数据库中参数
                UpdateOverAllNum(SeatNum, 10)
                '向改玩家发送ZL|各人樱桃连线的次数|获取个人水果盘次数|获取个人小B次数
                _User.SendData("ZL?" & PersonalCherryNum(SeatNum) & "," & PersonalFruitDishNum(SeatNum) & "," & PersonalSmallBarNum(SeatNum))
                '积累3次。开始疯狂送
                If PersonalCherryNum(SeatNum) = 3 Then
                    '将是否疯狂送状态改为0
                    _User.IfCrazySendStart = "0"
                    _User.SendData("GL?" & PersonalCherryNum(SeatNum) & "," & PersonalFruitDishNum(SeatNum) & "," & PersonalSmallBarNum(SeatNum))
                    '疯狂送状态改为樱桃疯狂送
                    IfCrazySend(SeatNum) = 1
                End If
                '将判断是否中了九宫格樱桃的状态改为 0
                IfJGGcherry(SeatNum) = 0
            End If
            '如果是中的全盘水果，
            If IfJGGfruit(SeatNum) = 1 Then
                '就更新数据水果盘
                UpdateOverAllNum(SeatNum, 11)
                _User.SendData("ZL?" & PersonalCherryNum(SeatNum) & "," & PersonalFruitDishNum(SeatNum) & "," & PersonalSmallBarNum(SeatNum))
                '如果水果盘中了3次
                If PersonalFruitDishNum(SeatNum) = 3 Then
                    '是否疯狂送状态改为0
                    _User.IfCrazySendStart = "0"
                    _User.SendData("GL?" & PersonalCherryNum(SeatNum) & "," & PersonalFruitDishNum(SeatNum) & "," & PersonalSmallBarNum(SeatNum))
                    '疯狂送状态改为水果盘
                    IfCrazySend(SeatNum) = 2
                End If
                '将判断是否中了九宫格的水果盘 归零
                IfJGGfruit(SeatNum) = 0
            End If
            '如果中小B连线
            If IfJGGSmallBar(SeatNum) = 1 Then
                '更新数据疯狂送小B
                UpdateOverAllNum(SeatNum, 9)
                UpdateOverAllNum(SeatNum, 12)
                _User.SendData("ZL?" & PersonalCherryNum(SeatNum) & "," & PersonalFruitDishNum(SeatNum) & "," & PersonalSmallBarNum(SeatNum))
                '如果小B中了三次
                If PersonalSmallBarNum(SeatNum) = 3 Then
                    '就将疯狂送状态改为0
                    _User.IfCrazySendStart = "0"
                    _User.SendData("GL?" & PersonalCherryNum(SeatNum) & "," & PersonalFruitDishNum(SeatNum) & "," & PersonalSmallBarNum(SeatNum))
                    '开始疯狂送天下
                    IfCrazySend(SeatNum) = 3
                End If
                '用来判断是否中了九宫格的小B 状态归零
                IfJGGSmallBar(SeatNum) = 0
            End If
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->JudgmentLine方法->判断中奖线发生错误", ex)
        End Try
        '返回该玩家的中奖参数信息
        Return strSend
    End Function
    ''' <summary>
    ''' 结算
    ''' </summary>
    ''' <param name="_UserInfo">玩家</param>
    ''' <remarks></remarks>
    Public Sub ClearingScore(ByVal _UserInfo As UserClass)
        '获取玩家分区
        Dim SeatNum As Integer = _UserInfo.MachineNumber
        Try
            '是否已结算 0未结算 1已发送结算但未返回 2已返回 、并且向中心服务器发送的信息不为空
            If IfSettleAccounts(SeatNum) = 0 AndAlso ToCenterSendMessage(SeatNum) <> "" AndAlso ToCenterSendMessage(SeatNum) IsNot Nothing Then
                '将结算状态改为已发送结算但未返回
                IfSettleAccounts(SeatNum) = 1
                '声明标记和注单号
                Dim sXZMark1 As String = ""
                Dim sXZMark2 As String = ""
                If _UserInfo.NoteNum <> -1 Then
                    sXZMark1 = "A1|"
                    sXZMark2 = _UserInfo.NoteNum
                Else
                    sXZMark1 = ""
                    ' sXZMark2 = _UserInfo.UserIP.Split(":")(0)
                End If
                '数据内容例子："A1|SGP|4|1|1|0|水果盤 4區<BR>(10:1)<BR>3個蘋果連線10注 <BR>||AJ61-1,600,161030144758480,800,800,1,"
                '               A1|SGP|分区号|座位号|1|中奖类别(0正常的，4疯狂送，1红7全盘，2蓝7全盘，3天下全盘，10比倍)|汉字中将格式|“”|账号，赢得分数，注单号，下注分数，下注分数 '160929新添加的逻辑：根据数据，拼接字符串
                ToCenterSendMessage(SeatNum) = sXZMark1 & "SGP" & "|" & ToCenterSendMessage(SeatNum).Split("|")(1) & "|" & ToCenterSendMessage(SeatNum).Split("|")(2) & "|" & ToCenterSendMessage(SeatNum).Split("|")(3) & "|" & ToCenterSendMessage(SeatNum).Split("|")(4) & "|" & ToCenterSendMessage(SeatNum).Split("|")(5) & "|" & ToCenterSendMessage(SeatNum).Split("|")(6) & "|" & ToCenterSendMessage(SeatNum).Split("|")(7).Split(",")(0) & "," & ToCenterSendMessage(SeatNum).Split("|")(7).Split(",")(1) & "," & sXZMark2 & "," & ToCenterSendMessage(SeatNum).Split("|")(7).Split(",")(3) & "," & ToCenterSendMessage(SeatNum).Split("|")(7).Split(",")(4) & "," & ToCenterSendMessage(SeatNum).Split("|")(7).Split(",")(5) & "," & ToCenterSendMessage(SeatNum).Split("|")(7).Split(",")(6)
                '向中心服务器发数据           
                ' ToCenterServerSendData(ToCenterSendMessage(SeatNum))
                SimulationCenterServer(ToCenterSendMessage(SeatNum), _UserInfo)
                '将向中心服务器发送的信息清空              
                ToCenterSendMessage(SeatNum) = ""
            End If
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->ClearingScore方法->向中心服务器发送结算数据发生错误", ex)
        End Try
    End Sub
    ''' <summary>
    ''' 断线重连逻辑
    ''' </summary>
    ''' <param name="_User">玩家</param>
    ''' <remarks></remarks>
    Public Sub BreakLineReconnection(ByVal _User As UserClass)
        Try
            Dim SeatNum As Integer = _User.MachineNumber
            '延迟退出时间计时改为now

            '最后一次操作的时间改为now
            LastOperationTime(SeatNum) = Now
            '是否断线 改为false
            _User.IfBreak = False
            '玩家是否在游戏中、状态改为true
            'booUserIfInGame = True           
            '断无线网点比倍再点得分，偶尔IfCanCompare会被清为false 
            If _User.CompareMessage <> "" Then
                '是否可以比倍改为true
                IfCanCompare(SeatNum) = True
            End If
            '中奖分数
            Dim WinScore As Integer = 0
            '疯狂送中////_User.User_dx
            If _User.IfCrazySendStart > 0 Then
                '中将分数=等于疯狂送中将分数
                WinScore = _User.CrazySendWinScore
            Else
                '可以比倍时显示初始分
                If IfCanCompare(SeatNum) Then
                    WinScore = CompareInitialPoints(SeatNum)
                Else '不可比倍时 表示已经比倍过，显示比倍后的分，点得分后 此值清0
                    WinScore = ComparedScore(SeatNum)
                End If
                '如果用户中全盘奖时的分数大于0
                If _User.AllAwardsScores > 0 Then
                    '把中全盘奖时的分数赋值给临时变量WinScore中奖分数
                    WinScore = _User.AllAwardsScores
                End If
            End If
            '重连向客户端发送的页面数据信息
            Dim ReconnectionInfo As String = WinScore & "/" & _User.IfCrazySendStart & "/" & IfCanCompare(SeatNum) & "/" & _User.ResidualFraction & "/" & _User.NewImmediatelySend & "~" & _User.BreakMessage & _User.CompareMessage
            '发送 dxcl|重连向客户端发送的页面数据信息~下注分数清零
            _User.SendData("dxcl?" & ReconnectionInfo & "~" & _User.BetReset)
            If _User.NewImmediatelySend = "1" Then
                _User.SendData("JSS?" & _User.IfCrazySendStart & "/" & ImmediatelySendLeft(SeatNum) & "/" & ImmediatelySendRight(SeatNum))
            End If
            '更新后台设置页的游戏消息
            ShowGameMessage("用户重連成功", 0, _User)
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->ReceiveFlashData方法->DXCL错误", ex)
        End Try
    End Sub
    ''' <summary>
    ''' 疯狂送离线结算
    ''' </summary>
    ''' <param name="_User">玩家信息</param>
    ''' <remarks>在疯狂送中断线固定时间内没有重连成功时，自动执行此方法执行完剩余次数的疯狂送</remarks>
    Public Sub CrazySendOffLineSettle(ByVal _User As UserClass)
        Try
            '获取分区号
            Dim SeatNum As Integer = _User.MachineNumber
            '是否中全盘 1.红七2.蓝七，3.天下
            _User.IfOverAll = 0
            '中了什么奖，几注 清空
            _User.AwardAndNum(0) = ""
            _User.AwardAndNum(1) = ""
            '各人樱桃连线的次数大于等于3次
            If PersonalCherryNum(SeatNum) >= 3 Then
                '将樱桃连线次数归零，并更新数据库
                PersonalCherryNum(SeatNum) = 0
                UpdateOverAllNum(SeatNum, 10)
            End If
            '如果个人水果盘的次数大于等于3次
            If PersonalFruitDishNum(SeatNum) >= 3 Then
                '将个人水果盘的次数归零，并更新数据库
                PersonalFruitDishNum(SeatNum) = 0
                UpdateOverAllNum(SeatNum, 11)
            End If
            '如果个人小B的次数大于等于3次
            If PersonalSmallBarNum(SeatNum) >= 3 Then
                '将个人小B的次数归零，并更新数据库
                PersonalSmallBarNum(SeatNum) = 0
                UpdateOverAllNum(SeatNum, 12)
            End If
            '将当局是否中了强制奖项 清空
            NowIfForceWin(SeatNum) = ""
            '即时送翻倍 true表示中了即时连线送
            ImmediatelySendDouble(SeatNum) = False
            ' 各分区总押次数 每押一次该值加1
            BetNum(SeatNum) += 1
            '記錄步驟记录游戏演示信息 清空，重新new一个
            RecordGameMessage(SeatNum) = Nothing
            RecordGameMessage(SeatNum) = New StringBuilder
            '記錄步驟记录游戏演示信息 （个分区底分） 
            RecordGameMessage(SeatNum).Append(EndPoints(SeatNum) & "$")
            '記錄步驟记录游戏演示信息 （玩家下注分） 
            RecordGameMessage(SeatNum).Append(CInt(_User.BetNumeber) & "$")
            '疯狂送时中全盘
            CrazySendOverall(_User, IfCrazySend(SeatNum))
            '如果强制中奖
            If ForceWin(SeatNum) <> "" AndAlso Int(ForceWin(SeatNum)) > 0 Then
                '将强制的奖项 赋值给 当局强制中奖
                NowIfForceWin(SeatNum) = ForceWin(SeatNum)
                '强制奖项变量清空
                ForceWin(SeatNum) = ""
            End If
            '表示即时连线送的时候 遇到疯狂送
            If ImmediatelySend(SeatNum) AndAlso IfCrazySend(SeatNum) <> 0 Then
                _User.SendData("jsstop")
                '得到各区的总押次数
                TotalBetNum(SeatNum) = TotalBetNum(SeatNum) + 1
                '疯狂送中 即时送变暗
                _User.ImmediatelySendClose = True
            End If
            '当前积分减去下注积分
            ResidueScore(SeatNum) = _User.Integral * RoomRatio(SeatNum) - CInt(_User.BetNumeber) * EndPoints(SeatNum)
            '記錄步驟记录游戏演示信息（中奖线信息）
            RecordGameMessage(SeatNum).Append("%" & _User.WinData & "*")
            '向中心服务器发送ZHONG的方法
            Dim strSendCenter As String = ToCenterSendWinInfo(_User)
            '如果不等于0，是疯狂送
            If IfCrazySend(SeatNum) <> 0 Then
                Dim ddff As Double
                Dim tempss As Double
                tempss = CDbl(strSendCenter.Split("|")(7).Split(",")(1))
                ddff = tempss * RoomRatio(SeatNum)
                '疯狂送中奖分数
                _User.CrazySendWinScore = _User.CrazySendWinScore + ddff
            End If
            '判断连线奖金，把最后的连线奖金求出来 ,user,蓝，红，天下
            OnLineBonus(_User, Blue7Money, Red7Money, WorldMoney)
            '記錄步驟记录游戏演示信息蓝，红，天下，剩余分数
            RecordGameMessage(SeatNum).Append(Blue7Money & "|" & Red7Money & "|" & WorldMoney & "|" & RoomRatio(SeatNum) & "|$")
            '得到各人樱桃连线的次数
            PersonalCherryNumMax(SeatNum) = PersonalCherryNum(SeatNum)
            '得到个人水果盘的次数
            PersonalFruitDishNumMax(SeatNum) = PersonalFruitDishNum(SeatNum)
            '得到个人小B的次数
            PersonalSmallBarNumMax(SeatNum) = PersonalSmallBarNum(SeatNum)
            Dim strSend As String = String.Empty
            '如果没有中全盘 
            If _User.IfOverAll = 0 Then
                '存储向中心服务器发送的中奖信息
                Dim Center() As String = strSendCenter.Split("|")
                '越南站，将向中心服务器发送的中奖信息变成字符串存入到临时变量 strSend
                If VietnamStation.Contains(_User.Account.Split("-")(1)) Then
                    Dim CenterSplit() As String = Center(5).Split("##")
                    strSend = Center(0) & "|" & Center(1) & "|" & Center(2) & "|" & Center(3) & "|" & Center(4) & "|" & CenterSplit(0) & "用戶斷線 系統完成##" & CenterSplit(2) & "Người dùng bị ngắt kết nối   Hệ thống hoàn thành|" & Center(6) & "|" & Center(7)
                Else
                    strSend = Center(0) & "|" & Center(1) & "|" & Center(2) & "|" & Center(3) & "|" & Center(4) & "|" & Center(5) & "用戶斷線 系統完成|" & Center(6) & "|" & Center(7)
                End If
            Else '中了红7、蓝7、天下全盘
                Dim WinInfo() As String = strSendCenter.Split("|")
                'ReserveInfo存储全盘彩金奖项中奖信息
                Dim ReserveInfo As String = OverallWinMessage(_User)
                '将全盘彩金奖项中奖信息拆分成数组
                Dim WinInfoSplit() As String = ReserveInfo.Split("|")
                '越南站 向中心服务器发送的全盘中奖信息，变成字符串重新赋值给'向中心服务器发送ZHONG的方法
                If VietnamStation.Contains(_User.Account.Split("-")(1)) Then
                    WinInfo(5) = WinInfo(5).Split("##")(0) & WinInfoSplit(5).Split("&")(0).Substring(4) & ":" & WinInfoSplit(7).Split(",")(1) & "<BR>用戶斷線 系統完成##" & WinInfo(5).Split("##")(2) & WinInfoSplit(5).Split("&")(1).Substring(4) & ":" & WinInfoSplit(7).Split(",")(1).Replace(".", ",") & "<BR>Người dùng bị ngắt kết nối   Hệ thống hoàn thành"
                    WinInfo(7) = WinInfo(7).Split(",")(0) & "," & (CDbl(WinInfo(7).Split(",")(1)) + CDbl(WinInfoSplit(7).Split(",")(1))).ToString() & "," & WinInfo(7).Split(",")(2) & "," & WinInfo(7).Split(",")(3) & "," & WinInfo(7).Split(",")(4)
                Else
                    WinInfo(5) = WinInfo(5) & WinInfoSplit(5).Split("&")(0).Substring(4) & ":" & WinInfoSplit(7).Split(",")(1) & "<BR>用戶斷線 系統完成"
                    WinInfo(7) = WinInfo(7).Split(",")(0) & "," & (CDbl(WinInfo(7).Split(",")(1)) + CDbl(WinInfoSplit(7).Split(",")(1))).ToString() & "," & WinInfo(7).Split(",")(2) & "," & WinInfo(7).Split(",")(3) & "," & WinInfo(7).Split(",")(4)
                End If
                Dim TempInfo As String = String.Empty
                ' 将向中心服务器发送ZHONG的方法，中奖信息循环存入字符串TempInfo中
                For i As Integer = 0 To WinInfo.Length - 1
                    TempInfo = TempInfo & WinInfo(i) & "|"
                Next
                TempInfo = TempInfo.Trim("|")
                '把TempInfo的中奖信息循环存入字符串strSend中
                strSend = TempInfo
            End If
            '最后向中心服务器发送结算数据
            ToCenterServerSendData(strSend)
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->CrazySendOffLineSettle方法->疯狂送离线结算时发生错误", ex)
        End Try
    End Sub
    ''' <summary>
    ''' 向中心服务器发送ZHONG的方法
    ''' </summary>
    ''' <param name="_User">玩家信息</param>
    ''' <returns>'返回数据-> SGP|分区号|座位号|1|中奖类别(0正常的，4疯狂送，1红7全盘，2蓝7全盘，3天下全盘，10比倍)|汉字中将格式|“”|账号，赢得分数，注单号，下注分数，下注分数</returns>
    ''' <remarks></remarks>
    Function ToCenterSendWinInfo(ByVal _User As UserClass) As String
        ' SGP|分区号|座位号|1|中奖类别(0正常的，4疯狂送，1红7全盘，2蓝7全盘，3天下全盘，10比倍)|汉字中将格式|“”|账号，赢得分数，ip地址，下注分数，下注分数
        _User.winScore = 0
        Dim SeatNum As Integer = _User.MachineNumber
        '四个元素 6,5,9,7,1,8,10,4,2     1|3|2     1|4|2   ""
        Dim SeatInfo() As String = _User.WinData.Split("%")
        '获取当前   日-月
        Dim TimeNow As String = Now.Month & "-" & Now.Day
        '如果中奖线为空就未中奖
        If SeatInfo(1) = "" Then
            '疯狂送结束
            _User.SendData("BSH")
            Dim Ratio As String = IIf(RoomRatio(SeatNum) <= 1, "(" & 1 / RoomRatio(SeatNum) & ":1)", "(1:" & RoomRatio(SeatNum) & ")")
            '判断是否越南分站 发送 SGP|分区号|座位号|1|中奖类别(0正常的，4疯狂送，1红7全盘，2蓝7全盘，3天下全盘，10比倍)|汉字中将格式|“”|账号，赢得分数，ip地址，下注分数，下注分数
            If VietnamStation.Contains(_User.Account.Split("-")(1)) Then
                Return "SGP|" & SeatNum & "|" & 1 & "|1|0|" & "水果盤 " & SeatNum & "區<BR>" & Ratio & "<BR>" & "未中獎.##Bass Trái Cây   Khu " & SeatNum & "<BR>(" & 1 / RoomRatio(SeatNum) & ":1)<BR>" & "Chưa trúng thưởng.|" & "" & "|" & _User.Account & "," & (-EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum)) & "," & (EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum)) & "," & (EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum))
            Else
                Return "SGP|" & SeatNum & "|" & 1 & "|1|0|" & "水果盤 " & SeatNum & "區<BR>" & Ratio & "<BR>" & "未中獎.|" & "" & "|" & _User.Account & "," & (-EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum)) & "," & (EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum)) & "," & (EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum))
            End If
            '是否可以比倍状态 改为false
            IfCanCompare(SeatNum) = False
        Else '如果有连线
            IfCanCompare(SeatNum) = True
            For i As Integer = 1 To SeatInfo.Length - 2
                '中的线是几注，总共是80.每条线最多10注
                Dim jibei As Integer
                '中的是1-15哪个
                If CInt(SeatInfo(i).Split("|")(0)) <= 15 Then
                    If _User.BetNumeber Mod 8 >= CInt(SeatInfo(i).Split("|")(1)) Then
                        jibei = _User.BetNumeber \ 8 + 1
                    Else
                        jibei = _User.BetNumeber \ 8
                    End If
                    'user,中的什么奖，几注
                    WinStr(_User, CInt(SeatInfo(i).Split("|")(0)), jibei)
                    '本局得分
                    Dim ResultScore As Integer
                    '如果是即时送
                    If ImmediatelySendDouble(SeatNum) Then
                        Select Case ImmediatelySendLeft(SeatNum)
                            ' 即时送左边随机 （1櫻桃.2铆丁.3蘋果.4葡萄.5西瓜.6小BAR.7中BAR.8大BAR）
                            Case 1, 2, 3, 4, 5
                                If CInt(SeatInfo(i).Split("|")(0)) = ImmediatelySendLeft(SeatNum) + 2 Then
                                    ResultScore = SeatInfo(i).Split("|")(2) * jibei * EndPoints(SeatNum) * (ImmediatelySendRight(SeatNum) + 1)
                                Else
                                    ResultScore = SeatInfo(i).Split("|")(2) * jibei * EndPoints(SeatNum)
                                End If
                            Case 6, 7, 8
                                If CInt(SeatInfo(i).Split("|")(0)) = ImmediatelySendLeft(SeatNum) + 3 Then
                                    ResultScore = SeatInfo(i).Split("|")(2) * jibei * EndPoints(SeatNum) * (ImmediatelySendRight(SeatNum) + 1)
                                Else
                                    ResultScore = SeatInfo(i).Split("|")(2) * jibei * EndPoints(SeatNum)
                                End If
                        End Select
                    Else
                        '本局得分 = 倍数*分区底分
                        ResultScore = SeatInfo(i).Split("|")(2) * jibei * EndPoints(SeatNum)
                    End If
                    '成连线的分 = 自身 + 本局得分
                    _User.winScore += ResultScore
                    Select Case CInt(SeatInfo(i).Split("|")(0))
                        '大B连线、蓝七连线、红七连线、天下连线
                        Case 11, 13, 14, 15
                            ' 當前中全盤積分的記錄 格式为：帐号，中奖积分，中奖类型（十个初始值的顺序chushi）,第几分区，日期  record(0)表示所有分区内最近中的5个大奖，record(SeatNum)表示个分区中的大奖
                            OverallRecord(SeatNum) = _User.Account & "," & ResultScore & "," & SeatInfo(i).Split("|")(0) & "," & SeatNum & "," & TimeNow & "|" & OverallRecord(SeatNum)
                            OverallRecord(0) = _User.Account & "," & ResultScore & "," & SeatInfo(i).Split("|")(0) & "," & SeatNum & "," & TimeNow & "|" & OverallRecord(0)
                            'OverallRecord(SeatNum) = RollWinInformation(SeatNum, _User)
                    End Select
                Else ' 中的7的倍数*压了几注*每注多少分
                    jibei = _User.BetNumeber
                    '本局得分
                    Dim ResultScore As Integer = SeatInfo(i).Split("|")(2) * _User.BetNumeber * EndPoints(SeatNum)
                    _User.winScore += ResultScore
                    'user,中的什么奖，几注
                    WinStr(_User, CInt(SeatInfo(i).Split("|")(0)), jibei)
                    Select Case CInt(SeatInfo(i).Split("|")(0))
                        Case 17 To 26 '櫻桃全盤、柳丁全盤、蘋果全盤、葡萄全盤、西瓜全盤、雜BAR全盤、小BAR全盤、All中BAR、All大BAR、All雜七
                            OverallRecord(SeatNum) = _User.Account & "," & ResultScore & "," & SeatInfo(i).Split("|")(0) & "," & SeatNum & "," & TimeNow & "|" & OverallRecord(SeatNum)
                            OverallRecord(0) = _User.Account & "," & ResultScore & "," & SeatInfo(i).Split("|")(0) & "," & SeatNum & "," & TimeNow & "|" & OverallRecord(0)
                            ' OverallRecord(SeatNum) = RollWinInformation(SeatNum, _User)
                    End Select
                End If
            Next
            Dim NoteSingle As String
            '算出比例
            Dim Ratio As String = IIf(RoomRatio(SeatNum) <= 1, "(" & 1 / RoomRatio(SeatNum) & ":1)", "(1:" & RoomRatio(SeatNum) & ")")
            If VietnamStation.Contains(_User.Account.Split("-")(1)) Then
                '判断是不是越南分站，分别发送 水果盘|分区|比例|中奖文字描述|剩余分数
                NoteSingle = "水果盤 " & SeatNum & "區<BR>" & Ratio & "<BR>" & WinTextDescription(_User.AwardAndNum(0)) & "##" & "Bass trái cây    Khu " & SeatNum & "<BR>(" & 1 / RoomRatio(SeatNum) & ":1)<BR>" & WinTextDescription(_User.AwardAndNum(1))
            Else
                NoteSingle = "水果盤 " & SeatNum & "區<BR>" & Ratio & "<BR>" & WinTextDescription(_User.AwardAndNum(0))
            End If
            '如果押的分大于等于赢的分
            If EndPoints(SeatNum) * _User.BetNumeber - _User.winScore >= 0 Then
                '疯狂送结束
                _User.SendData("BSH")
                '返回数据-> SGP|分区号|座位号|1|中奖类别(0正常的，4疯狂送，1红7全盘，2蓝7全盘，3天下全盘，10比倍)|汉字中将格式|“”|账号，赢得分数，ip地址，下注分数，下注分数
                Return "SGP|" & SeatNum & "|" & 1 & "|1|0|" & NoteSingle & "|" & "" & "|" & _User.Account & "," & ((_User.winScore - EndPoints(SeatNum) * _User.BetNumeber) / RoomRatio(SeatNum)) & "," & (EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum)) & "," & (EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum))
            Else '如果押的分小于赢的分
                If IfCrazySend(SeatNum) = 0 Then
                    '疯狂送结束
                    _User.SendData("BSH")
                    '返回数据-> SGP|分区号|座位号|1|中奖类别(0正常的，4疯狂送，1红7全盘，2蓝7全盘，3天下全盘，10比倍)|汉字中将格式|“”|账号，赢得分数，ip地址，下注分数，下注分数
                    Return "SGP|" & SeatNum & "|" & 1 & "|1|0|" & NoteSingle & "|" & "" & "|" & _User.Account & "," & ((_User.winScore - EndPoints(SeatNum) * _User.BetNumeber) / RoomRatio(SeatNum)) & "," & (EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum)) & "," & (EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum))
                Else '如果是即时送
                    If ImmediatelySendDouble(SeatNum) Then
                    Else '不是即时送. 疯狂送进行中
                        _User.SendData("JXZ?" & IfCrazySend(SeatNum))
                        '返回数据-> SGP|分区号|座位号|1|中奖类别(0正常的，4疯狂送，1红7全盘，2蓝7全盘，3天下全盘，10比倍)|汉字中将格式|“”|账号，赢得分数，ip地址，下注分数，下注分数
                        Return "SGP|" & SeatNum & "|" & 1 & "|1|4|" & NoteSingle & "|" & "" & "|" & _User.Account & "," & (_User.winScore / RoomRatio(SeatNum)) & "," & (EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum)) & "," & (EndPoints(SeatNum) * _User.BetNumeber / RoomRatio(SeatNum))
                    End If
                End If
            End If
        End If
        '如果中奖线不存在就返回空字符串
        Return ""
    End Function
    ''' <summary>
    ''' 中奖文字描述
    ''' </summary>
    ''' <param name="_WinMessage">保存中奖信息的，包括什么类型奖项，以及多少注</param>
    ''' <returns>返回的也是中奖信息 "1個櫻桃連線10注 1個櫻桃連線10注"</returns>
    ''' <remarks></remarks>
    Public Function WinTextDescription(ByVal _WinMessage As String) As String
        Dim MessageSplit() As String = _WinMessage.Split("|")
        '如果中奖信息拆分成数组就只有一个元素，直接将其返回
        If MessageSplit.Length = 1 Then
            Return MessageSplit(0)
        End If
        Dim Result As String = String.Empty
        Dim i As Integer
        For i = 0 To MessageSplit.Length - 1 Step 2
            If i + 1 <= MessageSplit.Length - 1 Then
                Result = Result & MessageSplit(i) & " " & MessageSplit(i + 1) & "<BR>"
            Else
                Result = Result & MessageSplit(i)
            End If
        Next
        Return Result
    End Function
    ''' <summary>
    ''' 中奖字符串，获得userinfo0.User_5  汉字记录中奖格式为 1個荔枝連線几注
    ''' </summary>
    '''  <param name="_User">玩家信息</param>
    ''' <param name="_Awards">中的什么奖</param>
    '''  <param name="_Multiple">压的几注</param>
    ''' <returns>返回字符串，中奖格式为  "1個荔枝連線" jb  "注"</returns>
    ''' <remarks></remarks>
    Function WinStr(ByVal _User As UserClass, ByVal _Awards As Integer, ByVal _Multiple As Integer)
        Dim ResultStr As String = ""
        Dim SeatNum As Integer = _User.MachineNumber
        '判断是中的什么奖项
        Select Case _Awards
            Case 1
                '获得_User.AwardAndNum  汉字记录中奖格式为 1個荔枝連線几注
                _User.AwardAndNum(0) &= "1個櫻桃連線" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "Liên kết 1 hình Anh Đào " & _Multiple & " đơn " & "|"
            Case 2
                _User.AwardAndNum(0) &= "2個櫻桃連線" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "Liên kết 2 hình Anh Đào " & _Multiple & " đơn " & "|"
            Case 3
                '如果下注分数*底分大于等于 该分区的分数额度
                If _User.BetNumeber * EndPoints(SeatNum) >= Subregion(SeatNum - 1, 56) Then
                    '如果玩家中奖字符串不包含九宫格的樱桃
                    If _User.WinData.Contains("%16") = False Then
                        '判断是否中了九宫格的樱桃的状态改为1
                        IfJGGcherry(SeatNum) = 1
                    End If
                End If
                '櫻桃.橘子.蘋果.葡萄.西瓜.小BAR.中BAR.大BAR第二個畫面有8種2倍~9倍 ，并且是即时送
                If ImmediatelySendLeft(SeatNum) = 1 AndAlso ImmediatelySend(SeatNum) Then
                    '即时连线送状态改为true
                    ImmediatelySendDouble(SeatNum) = True
                    '获得_User.AwardAndNum  汉字记录中奖格式为 1個荔枝連線几注
                    _User.AwardAndNum(0) &= "3個櫻桃連線" & _Multiple & "注" & (ImmediatelySendRight(SeatNum) + 1) & "倍" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình Anh Đào " & _Multiple & " đơn đền " & (ImmediatelySendRight(SeatNum) + 1) & " lần " & "|"
                Else
                    _User.AwardAndNum(0) &= "3個櫻桃連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình Anh Đào " & _Multiple & " đơn " & "|"
                End If
            Case 4
                If ImmediatelySendLeft(SeatNum) = 2 AndAlso ImmediatelySend(SeatNum) Then
                    ImmediatelySendDouble(SeatNum) = True
                    _User.AwardAndNum(0) &= "3個柳丁連線" & _Multiple & "注" & (ImmediatelySendRight(SeatNum) + 1) & "倍" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình Trái Cam " & _Multiple & " đơn đền " & (ImmediatelySendRight(SeatNum) + 1) & " lần " & "|"
                Else
                    _User.AwardAndNum(0) &= "3個柳丁連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình Trái Cam " & _Multiple & " đơn " & "|"
                End If
            Case 5
                If ImmediatelySendLeft(SeatNum) = 3 AndAlso ImmediatelySend(SeatNum) Then
                    ImmediatelySendDouble(SeatNum) = True
                    _User.AwardAndNum(0) &= "3個蘋果連線" & _Multiple & "注" & (ImmediatelySendRight(SeatNum) + 1) & "倍" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình Trái Táo " & _Multiple & " đơn đền " & (ImmediatelySendRight(SeatNum) + 1) & " lần " & "|"
                Else
                    _User.AwardAndNum(0) &= "3個蘋果連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình Trái Táo " & _Multiple & " đơn " & "|"
                End If
            Case 6
                If ImmediatelySendLeft(SeatNum) = 4 AndAlso ImmediatelySend(SeatNum) Then
                    ImmediatelySendDouble(SeatNum) = True
                    _User.AwardAndNum(0) &= "3個葡萄連線" & _Multiple & "注" & (ImmediatelySendRight(SeatNum) + 1) & "倍" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình Trái Nho " & _Multiple & " đơn đền " & (ImmediatelySendRight(SeatNum) + 1) & " lần " & "|"
                Else
                    _User.AwardAndNum(0) &= "3個葡萄連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình Trái Nho " & _Multiple & " đơn " & "|"
                End If
            Case 7
                If ImmediatelySendLeft(SeatNum) = 5 AndAlso ImmediatelySend(SeatNum) Then
                    ImmediatelySendDouble(SeatNum) = True
                    _User.AwardAndNum(0) &= "3個西瓜連線" & _Multiple & "注" & (ImmediatelySendRight(SeatNum) + 1) & "倍" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình Dưa Hấu " & _Multiple & " đơn đền " & (ImmediatelySendRight(SeatNum) + 1) & " lần " & "|"
                Else
                    _User.AwardAndNum(0) &= "3個西瓜連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình Dưa Hấu " & _Multiple & " đơn " & "|"
                End If
            Case 8
                _User.AwardAndNum(0) &= "3個混BAR連線" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "Liên kết 3 hình BAR hỗn hợp " & _Multiple & " đơn " & "|"
            Case 9
                If _User.BetNumeber * EndPoints(SeatNum) >= Subregion(SeatNum - 1, 56) Then
                    IfJGGSmallBar(SeatNum) = 1
                End If
                If ImmediatelySendLeft(SeatNum) = 6 AndAlso ImmediatelySend(SeatNum) Then
                    ImmediatelySendDouble(SeatNum) = True
                    _User.AwardAndNum(0) &= "3個小BAR連線" & _Multiple & "注" & (ImmediatelySendRight(SeatNum) + 1) & "倍" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình BAR nhỏ " & _Multiple & " đơn đền " & (ImmediatelySendRight(SeatNum) + 1) & " lần " & "|"
                Else
                    _User.AwardAndNum(0) &= "3個小BAR連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình BAR nhỏ " & _Multiple & " đơn " & "|"
                End If
            Case 10
                If ImmediatelySendLeft(SeatNum) = 7 AndAlso ImmediatelySend(SeatNum) Then
                    ImmediatelySendDouble(SeatNum) = True
                    _User.AwardAndNum(0) &= "3個中BAR連線" & _Multiple & "注" & (ImmediatelySendRight(SeatNum) + 1) & "倍" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình BAR vừa " & _Multiple & " đơn đền " & (ImmediatelySendRight(SeatNum) + 1) & " lần " & "|"
                Else
                    _User.AwardAndNum(0) &= "3個中BAR連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình BAR vừa " & _Multiple & " đơn " & "|"
                End If
            Case 11
                If ImmediatelySendLeft(SeatNum) = 8 AndAlso ImmediatelySend(SeatNum) Then
                    ImmediatelySendDouble(SeatNum) = True
                    _User.AwardAndNum(0) &= "3個大BAR連線" & _Multiple & "注" & (ImmediatelySendRight(SeatNum) + 1) & "倍" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình BAR lớn " & _Multiple & " đơn đền " & (ImmediatelySendRight(SeatNum) + 1) & " lần " & "|"
                Else
                    _User.AwardAndNum(0) &= "3個大BAR連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình BAR lớn " & _Multiple & " đơn " & "|"
                End If
            Case 12
                _User.AwardAndNum(0) &= "3個混七連線" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "Liên kết 3 hình số 7 hỗn hợp " & _Multiple & " đơn " & "|"
            Case 13
                If IfCrazySend(SeatNum) = 1 Then
                    _User.AwardAndNum(0) &= "瘋狂送中3個藍七連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Tặng thêm trúng liên kết 3 hình số 7 màu xanh " & _Multiple & " đơn " & "|"
                Else
                    _User.AwardAndNum(0) &= "3個藍七連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình số 7 màu xanh " & _Multiple & " đơn " & "|"
                End If
                UpdateOverAllNum(SeatNum, 8)
            Case 14
                If IfCrazySend(SeatNum) = 2 Then
                    _User.AwardAndNum(0) &= "瘋狂送中3個紅七連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Tặng thêm trúng liên kết 3 hình số 7 màu đỏ " & _Multiple & " đơn " & "|"
                Else
                    _User.AwardAndNum(0) &= "3個紅七連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình số 7 màu đỏ " & _Multiple & " đơn " & "|"
                End If
                UpdateOverAllNum(SeatNum, 7)
            Case 15
                If IfCrazySend(SeatNum) = 3 Then
                    _User.AwardAndNum(0) &= "瘋狂送中3個TS連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Tặng thêm trúng liên kết 3 hình TS " & _Multiple & " đơn " & "|"
                Else
                    _User.AwardAndNum(0) &= "3個TS連線" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Liên kết 3 hình TS " & _Multiple & " đơn " & "|"
                End If
                UpdateOverAllNum(SeatNum, 6)
            Case 16
                _User.AwardAndNum(0) &= "水果盤" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "Bass Trái Cây " & _Multiple & " đơn " & "|"
                If _User.BetNumeber * EndPoints(SeatNum) >= Subregion(SeatNum - 1, 56) Then
                    IfJGGfruit(SeatNum) = 1
                End If
            Case 17
                _User.AwardAndNum(0) &= "9個櫻桃" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "9 hình Anh Đào " & _Multiple & " đơn " & "|"
            Case 18
                _User.AwardAndNum(0) &= "9個柳丁" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "9 hình Trái Cam " & _Multiple & " đơn " & "|"
            Case 19
                _User.AwardAndNum(0) &= "9個蘋果" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "9 hình Trái Táo " & _Multiple & " đơn " & "|"
            Case 20
                _User.AwardAndNum(0) &= "9個葡萄" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "9 hình Trái Nho " & _Multiple & " đơn " & "|"
            Case 21
                _User.AwardAndNum(0) &= "9個西瓜" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "9 hình Dưa Hấu " & _Multiple & " đơn " & "|"
            Case 22
                _User.AwardAndNum(0) &= "9個混BAR" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "9 hình BAR hỗn hợp " & _Multiple & " đơn " & "|"
            Case 23
                _User.AwardAndNum(0) &= "9個小BAR" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "9 hình BAR nhỏ " & _Multiple & " đơn " & "|"
            Case 24
                _User.AwardAndNum(0) &= "9個中BAR" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "9 hình BAR vừa " & _Multiple & " đơn " & "|"
                UpdateOverAllNum(SeatNum, 4)
            Case 25
                _User.AwardAndNum(0) &= "9個大BAR" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "9 hình BAR lớn " & _Multiple & " đơn " & "|"
                UpdateOverAllNum(SeatNum, 3)
            Case 26
                _User.AwardAndNum(0) &= "9個混七" & _Multiple & "注" & "|"
                _User.AwardAndNum(1) &= "9 hình số 7 hỗn hợp " & _Multiple & " đơn " & "|"
                UpdateOverAllNum(SeatNum, 5)
            Case 27
                If IfCrazySend(SeatNum) = 1 Then
                    _User.AwardAndNum(0) &= "瘋狂送中9個藍七" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Tặng thêm trúng 9 hình số 7 màu xanh " & _Multiple & " đơn " & "|"
                Else
                    _User.AwardAndNum(0) &= "9個藍七" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "9 hình số 7 màu xanh " & _Multiple & " đơn " & "|"
                End If
                UpdateOverAllNum(SeatNum, 2)
            Case 28
                If IfCrazySend(SeatNum) = 2 Then
                    _User.AwardAndNum(0) &= "瘋狂送中9個紅七" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Tặng thêm trúng 9 hình số 7 màu đỏ " & _Multiple & " đơn " & "|"
                Else
                    _User.AwardAndNum(0) &= "9個紅七" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "9 hình số 7 màu đỏ " & _Multiple & " đơn " & "|"
                End If
                UpdateOverAllNum(SeatNum, 1)
            Case 29
                If IfCrazySend(SeatNum) = 3 Then
                    _User.AwardAndNum(0) &= "瘋狂送中9個TS" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Tặng thêm trúng 9 hình TS " & _Multiple & " đơn " & "|"
                Else
                    _User.AwardAndNum(0) &= "9個TS" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "9 hình TS " & _Multiple & " đơn " & "|"
                End If
                UpdateOverAllNum(SeatNum, 0)
            Case 30, 31, 32, 33, 34, 35, 36
                Dim num As Integer = _Awards - 28
                If IfCrazySend(SeatNum) = 1 Then
                    _User.AwardAndNum(0) &= "瘋狂送中" & num & "個藍七" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Tặng thêm trúng " & num & " hình số 7 màu xanh " & _Multiple & " đơn " & "|"
                Else
                    _User.AwardAndNum(0) &= num & "個藍七" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= num & " hình số 7 màu xanh " & _Multiple & " đơn " & "|"
                End If
            Case 37, 38, 39, 40, 41, 42, 43
                Dim num As Integer = _Awards - 35
                If IfCrazySend(SeatNum) = 2 Then
                    _User.AwardAndNum(0) &= "瘋狂送中" & num & "個紅七" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Tặng thêm trúng " & num & " hình số 7 màu đỏ " & _Multiple & " đơn " & "|"
                Else
                    _User.AwardAndNum(0) &= num & "個紅七" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= num & " hình số 7 màu đỏ " & _Multiple & " đơn " & "|"
                End If
            Case 44, 45, 46, 47, 48, 49, 50
                Dim num As Integer = _Awards - 42
                If IfCrazySend(SeatNum) = 3 Then
                    _User.AwardAndNum(0) &= "瘋狂送中" & num & "個TS" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= "Tặng thêm trúng " & num & " hình TS " & _Multiple & " đơn " & "|"
                Else
                    _User.AwardAndNum(0) &= num & "個TS" & _Multiple & "注" & "|"
                    _User.AwardAndNum(1) &= num & " hình TS " & _Multiple & " đơn " & "|"
                End If
        End Select
        '汉字记录中奖格式为 "1個荔枝連線" & _Multiple & "注"
        ResultStr = _User.AwardAndNum(0) & "#" & _User.AwardAndNum(1)
        Return ResultStr
    End Function
    ''' <summary>
    ''' 疯狂送时中全盘
    ''' </summary>
    ''' <param name="_User">玩家信息</param>
    ''' <param name="_CrazySendType">疯狂送类型，1蓝7，2红7，3天下</param>
    ''' <remarks></remarks>
    Private Sub CrazySendOverall(ByVal _User As UserClass, ByVal _CrazySendType As Integer)
        '获取玩家座位
        Dim SeatNum As Integer = _User.MachineNumber
        '判断疯狂送类型
        Select Case _CrazySendType
            Case 1 '蓝七
                Dim TempBlue As Integer = 0
                '如果强制中蓝7奖
                If ForceWin(SeatNum) = "1" Then
                    '疯狂送中奖 强制中全盘的信息
                    _User.WinData = AlgorithmClass(SeatNum).CrazySendOverAll(2)
                    '如果获取的中奖线是蓝7全盘
                    If _User.WinData.IndexOf("2,2,2,2,2,2,2,2,2") >= 0 Then
                        '全盘蓝七
                        _User.IfOverAll = 2
                    End If
                Else
                    '可以中ALL
                    TempBlue = 1
                    '获取一个随机数
                    Dim RndNum As Integer = RandomInt(1, 1001)
                    '如果 随机数/1000 小于等于游戏百分比 、并且可中蓝7全盘
                    If CDbl(RndNum / 1000) <= CDbl(CrazySendPercentage(SeatNum)) AndAlso IfOpenBlue7All(SeatNum) = 1 Then
                        '获取中奖线，9个位置上的图片代号
                        _User.WinData = AlgorithmClass(SeatNum).CrazySendData(1, 2, 9)
                        '如果获取的中奖线是蓝7全盘
                        If _User.WinData.IndexOf("2,2,2,2,2,2,2,2,2") >= 0 Then
                            '全盘蓝七
                            _User.IfOverAll = 2
                        End If
                    Else
                        '改为不可以种的状态
                        TempBlue = 0
                    End If
                    If TempBlue = 0 Then
                        '疯狂送奖项的WinData
                        CrazySendWinData(2, _User)
                    End If
                End If
            Case 2 '紅七all
                Dim TempRed As Integer = 0
                '如果强制全盘红7
                If ForceWin(SeatNum) = "2" Then
                    '获取中奖线，9个位置上的图片代号
                    _User.WinData = AlgorithmClass(SeatNum).CrazySendOverAll(1)
                    '如果获取的中奖线是红7全盘
                    If _User.WinData.IndexOf("1,1,1,1,1,1,1,1,1") >= 0 Then
                        '全盘红七
                        _User.IfOverAll = 1
                    End If
                Else
                    '可以中ALL
                    TempRed = 1
                    '获取随机数
                    Dim RndNum As Integer = RandomInt(1, 1001)
                    '如果 随机数/1000 小于等于游戏百分比 、并且可中红7全盘
                    If CDbl(RndNum / 1000) <= CDbl(CrazySendPercentage(SeatNum)) AndAlso IfOpenRed7All(SeatNum) = 1 Then
                        '获取中奖线，9个位置上的图片代号
                        _User.WinData = AlgorithmClass(SeatNum).CrazySendData(1, 1, 9)
                        '如果获取的中奖线是红7全盘
                        If _User.WinData.IndexOf("1,1,1,1,1,1,1,1,1") >= 0 Then
                            _User.IfOverAll = 1 '全盘红七
                        End If
                    Else
                        ''可以中全盘状态改为0
                        TempRed = 0
                    End If
                    If TempRed = 0 Then
                        '疯狂送奖项的WinData
                        CrazySendWinData(1, _User)
                    End If
                End If
            Case 3 ' 如果强制中奖天下全盘
                Dim TempWorld As Integer = 0
                If ForceWin(SeatNum) = "3" Then
                    '获取中奖线，9个位置上的图片代号
                    _User.WinData = AlgorithmClass(SeatNum).CrazySendOverAll(3)
                    '如果获取的中奖线是天下全盘
                    If _User.WinData.IndexOf("11,11,11,11,11,11,11,11,11") >= 0 Then
                        '全盘天下
                        _User.IfOverAll = 3
                    End If
                Else
                    '可以中全盘状态改为1
                    TempWorld = 1
                    '获取随机数
                    Dim RndNum As Integer = RandomInt(1, 1001)
                    '如果 随机数/1000 小于等于游戏百分比 、并且可中天下全盘
                    If CDbl(RndNum / 1000) <= CDbl(CrazySendPercentage(SeatNum)) AndAlso IfOpenWorldAll(SeatNum) = 1 Then
                        '产生至少2个天下
                        _User.WinData = AlgorithmClass(SeatNum).CrazySendData(1, 3, 9)
                        '如果获取的中奖线是天下全盘
                        If _User.WinData.IndexOf("11,11,11,11,11,11,11,11,11") >= 0 Then
                            '全盘天下
                            _User.IfOverAll = 3
                        End If
                    Else ''可以中全盘状态改为0
                        TempWorld = 0
                    End If
                    If TempWorld = 0 Then
                        '疯狂送奖项的WinData
                        CrazySendWinData(3, _User)
                    End If
                End If
        End Select
    End Sub
    ''' <summary>
    ''' '全盘彩金奖项中奖信息
    ''' </summary>
    ''' <param name="_User">玩家信息</param>
    ''' <returns>返回中奖信息  SGP|分区|桌子|1|1|玩家名称|本局获得红7全盘彩金|玩家IP参数，0,0，在线方式，系统信息</returns>
    ''' <remarks></remarks>
    Function OverallWinMessage(ByVal _User As UserClass) As String
        Dim ResultStr As String = ""
        '获取分区
        Dim SeatNum As Integer = _User.MachineNumber
        '获取月份-日期
        Dim TimeNow As String = Now.Month & "-" & Now.Day
        '判断中的是蓝7，红7，天下 哪个全盘彩金
        Select Case _User.IfOverAll
            Case 1
                '连线奖金判断，额度和玩家的分之间转化
                OnlineBonusJudge(_User, BureauRed7Bonus(SeatNum), Red7Money, 1)
                '获取中奖信息SGP|分区|桌子|1|1|玩家名称|本局获得红7全盘彩金|玩家IP参数，0,0，在线方式，系统信息
                ResultStr = "SGP|" & SeatNum & "|" & _User.MachineNumber & "|1|1|" & "<BR>中红七連線獎金&<BR>Trúng điểm thưởng liên kết hình số 7 màu đỏ|" & "" & "|" & _User.Account & "," & GetTwoString(BureauRed7Bonus(SeatNum)) & "," & 0 & "," & 0
                '當前中全盤積分的記錄 格式为：帐号，中奖积分，中奖类型（十个初始值的顺序chushi）,第几分区，日期  record(0)表示所有分区内最近中的5个大奖，record(SeatNum)表示个分区中的大奖
                OverallRecord(SeatNum) = _User.Account & "," & GetTwoString(BureauRed7Bonus(SeatNum)) & "," & "1" & "," & SeatNum & "," & TimeNow & "|" & OverallRecord(SeatNum)
                OverallRecord(0) = _User.Account & "," & GetTwoString(BureauRed7Bonus(SeatNum)) & "," & "1" & "," & SeatNum & "," & TimeNow & "|" & OverallRecord(0)
            Case 2
                '连线奖金判断，额度和玩家的分之间转化
                OnlineBonusJudge(_User, BureauBlue7Bonus(SeatNum), Blue7Money, 2)
                '获取中奖信息SGP|分区|桌子|1|1|玩家名称|本局获得蓝7全盘彩金|玩家IP参数，0,0，在线方式，系统信息
                ResultStr = "SGP|" & SeatNum & "|" & _User.MachineNumber & "|1|2|" & "<BR>中蓝七連線獎金&<BR>Trúng điểm thưởng liên kết hình số 7 màu xanh|" & "" & "|" & _User.Account & "," & GetTwoString(BureauBlue7Bonus(SeatNum)) & "," & 0 & "," & 0
                '當前中全盤積分的記錄 格式为：帐号，中奖积分，中奖类型（十个初始值的顺序chushi）,第几分区，日期  record(0)表示所有分区内最近中的5个大奖，record(SeatNum)表示个分区中的大奖
                OverallRecord(SeatNum) = _User.Account & "," & GetTwoString(BureauBlue7Bonus(SeatNum)) & "," & "2" & "," & SeatNum & "," & TimeNow & "|" & OverallRecord(SeatNum)
                OverallRecord(0) = _User.Account & "," & GetTwoString(BureauBlue7Bonus(SeatNum)) & "," & "2" & "," & SeatNum & "," & TimeNow & "|" & OverallRecord(0)
            Case 3
                '连线奖金判断，额度和玩家的分之间转化
                OnlineBonusJudge(_User, BureauWorldBonus(SeatNum), WorldMoney, 3)
                '获取中奖信息SGP|分区|桌子|1|1|玩家名称|本局获得红7全盘彩金|玩家IP参数，0,0，在线方式，系统信息
                ResultStr = "SGP|" & SeatNum & "|" & _User.MachineNumber & "|1|3|" & "<BR>中TS連線獎金&<BR>Trúng điểm thưởng liên kết hình TS|" & "" & "|" & _User.Account & "," & GetTwoString(BureauWorldBonus(SeatNum)) & "," & 0 & "," & 0
                '當前中全盤積分的記錄 格式为：帐号，中奖积分，中奖类型（十个初始值的顺序chushi）,第几分区，日期  record(0)表示所有分区内最近中的5个大奖，record(SeatNum)表示个分区中的大奖
                OverallRecord(SeatNum) = _User.Account & "," & GetTwoString(BureauWorldBonus(SeatNum)) & "," & "3" & "," & SeatNum & "," & TimeNow & "|" & OverallRecord(SeatNum)
                OverallRecord(0) = _User.Account & "," & GetTwoString(BureauWorldBonus(SeatNum)) & "," & "3" & "," & SeatNum & "," & TimeNow & "|" & OverallRecord(0)
        End Select
        '返回玩家中全盘奖的信息
        Return ResultStr
    End Function
    ''' <summary>
    ''' 判断连线奖金，把最后的连线奖金求出来
    ''' </summary>
    ''' <param name="_User">玩家信息</param>
    ''' <param name="_BlueIntegral">蓝7积分</param>
    ''' <param name="_RedIntegral">红7积分</param>
    ''' <param name="_WorldIntegral">天下积分</param>
    ''' <remarks></remarks>
    Sub OnLineBonus(ByVal _User As UserClass, ByRef _BlueIntegral As Double, ByRef _RedIntegral As Double, ByRef _WorldIntegral As Double)
        '获取座位号
        Dim SeatNum As Integer = _User.MachineNumber
        '彩金是否是最大值
        Dim IfMax1 As Integer = 0
        Dim IfMax2 As Integer = 0
        Dim IfMax3 As Integer = 0
        '如果当前连线蓝7奖金大于等于蓝7全盘积分起始分数
        If _BlueIntegral >= Blue7StartIntegral Then
            '在不是疯狂送的情况下重新给蓝7奖金赋值
            If IfCrazySend(SeatNum) = 0 Then
                _BlueIntegral = _BlueIntegral + 555 ' + _User.BetNumeber * EndPoints(SeatNum) * Blue7Bonus(SeatNum) / RoomRatio(SeatNum)            
            End If
            '如果大于蓝7全盘积分起始分数，则等于最高限制分数
            If _BlueIntegral > Blue7MaxIntegral Then
                _BlueIntegral = Blue7MaxIntegral
                IfMax1 = 1
            End If
        Else '如果当前连线奖金小于起始分数
            _BlueIntegral = Blue7StartIntegral
            '在不是疯狂送的情况下重新给蓝7奖金赋值
            If IfCrazySend(SeatNum) = 0 Then
                _BlueIntegral = _BlueIntegral + 555 '+ _User.BetNumeber * EndPoints(SeatNum) * Blue7Bonus(SeatNum) / RoomRatio(SeatNum)
            End If
        End If
        '如果当前连线红7奖金大于等于红7全盘积分起始分数
        If _RedIntegral >= Red7StartIntegral Then
            '在不是疯狂送的情况下重新给红7奖金赋值
            If IfCrazySend(SeatNum) = 0 Then
                _RedIntegral = _RedIntegral + 500 '+ _User.BetNumeber * EndPoints(SeatNum) * Red7Bonus(SeatNum) / RoomRatio(SeatNum)
            End If
            '如果大于红7全盘积分起始分数，则等于最高限制分数
            If _RedIntegral > Red7MaxIntegral Then
                _RedIntegral = Red7MaxIntegral
                IfMax2 = 1
            End If
        Else '如果当前连线奖金小于起始分数
            _RedIntegral = Red7StartIntegral
            '在不是疯狂送的情况下重新给蓝7奖金赋值
            If IfCrazySend(SeatNum) = 0 Then
                _RedIntegral = _RedIntegral + 500 '+ _User.BetNumeber * EndPoints(SeatNum) * Red7Bonus(SeatNum) / RoomRatio(SeatNum)
            End If
        End If
        '如果当前连线天下奖金大于等于，天下全盘积分起始分数
        If _WorldIntegral >= WorldStartIntegral Then
            '在不是疯狂送的情况下重新给天下奖金赋值
            If IfCrazySend(SeatNum) = 0 Then
                _WorldIntegral = _WorldIntegral + 5555 ' + _User.BetNumeber * EndPoints(SeatNum) * WorldBonus(SeatNum) / RoomRatio(SeatNum)
            End If
            '如果大于天下全盘积分起始分数，则等于最高限制分数
            If _WorldIntegral > WorldMaxIntegral Then
                _WorldIntegral = WorldMaxIntegral
                IfMax3 = 1
            End If
        Else '如果当前连线奖金小于起始分数
            _WorldIntegral = WorldStartIntegral
            '在不是疯狂送的情况下重新给天下奖金赋值
            If IfCrazySend(SeatNum) = 0 Then
                _WorldIntegral = _WorldIntegral + 5555 ' + _User.BetNumeber * EndPoints(SeatNum) * WorldBonus(SeatNum) / RoomRatio(SeatNum)
            End If
        End If
        '如果没有中全盘奖，就发送蓝7，红7，天下的彩金、和是否最大值的状态标记  
        If _User.IfOverAll = 0 Then
            _User.SendData("XS?" & Blue7Money & "/" & Red7Money & "/" & WorldMoney & "/" & IfMax1 & "/" & IfMax2 & "/" & IfMax3)
        End If
    End Sub
    ''' <summary>
    ''' 连线奖金判断，额度和玩家的分之间转化
    ''' </summary>
    ''' <param name="_User">玩家信息</param>
    ''' <param name="_OverallBonus">本局所中的全盘彩金</param>
    ''' <param name="_CurrentBonus">当前彩金池奖金</param>
    '''<param name="_Type">全盘奖的类型、1红 2蓝 3天下</param>
    ''' <remarks></remarks>
    Sub OnlineBonusJudge(ByVal _User As UserClass, ByRef _OverallBonus As Double, ByRef _CurrentBonus As Double, ByVal _Type As Integer)
        '根据下注分数，分不同层次得到 本局所中的全盘彩金
        Select Case _User.BetNumeber
            Case 8 To 15
                _OverallBonus = _CurrentBonus * 1 / 10
            Case 16 To 23
                _OverallBonus = _CurrentBonus * 2 / 10
            Case 24 To 31
                _OverallBonus = _CurrentBonus * 3 / 10
            Case 32 To 39
                _OverallBonus = _CurrentBonus * 4 / 10
            Case 40 To 47
                _OverallBonus = _CurrentBonus * 5 / 10
            Case 48 To 55
                _OverallBonus = _CurrentBonus * 6 / 10
            Case 56 To 63
                _OverallBonus = _CurrentBonus * 7 / 10
            Case 64 To 71
                _OverallBonus = _CurrentBonus * 8 / 10
            Case 72 To 79
                _OverallBonus = _CurrentBonus * 9 / 10
            Case 80
                _OverallBonus = _CurrentBonus
        End Select
        '将当前彩金减去所中全盘彩金  重新赋值给当前彩金
        _CurrentBonus = _CurrentBonus - _OverallBonus
        '判断是什么类型的全盘奖
        Select Case _Type
            Case 1
                '如果本局所中的红7全盘彩金小于等于 红7全盘起始分数、就把起始分数赋值给本局所中彩金
                If _CurrentBonus <= Red7StartIntegral Then
                    _CurrentBonus = Red7StartIntegral
                End If
            Case 2
                '如果本局所中的蓝7全盘彩金小于等于 蓝7全盘起始分数、就把起始分数赋值给本局所中彩金
                If _CurrentBonus <= Blue7StartIntegral Then
                    _CurrentBonus = Blue7StartIntegral
                End If
            Case 3
                '如果本局所中的天下全盘彩金小于等于 天下全盘起始分数、就把起始分数赋值给本局所中彩金
                If _CurrentBonus <= WorldStartIntegral Then
                    _CurrentBonus = WorldStartIntegral
                End If
        End Select
    End Sub
    ''' <summary>
    ''' 字符串转换成double
    ''' </summary>
    ''' <param name="_WinMessage">中奖参数信息</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetTwoString(ByVal _WinMessage As String) As Double
        Return Math.Floor(CDbl(_WinMessage) * 100) / 100
    End Function
    ''' <summary>
    ''' 疯狂送奖项的WinData
    ''' </summary>
    ''' <param name="_Awards">判断是何奖项，1红、2蓝、3天下</param>
    ''' <param name="_User">玩家信息</param>
    ''' <remarks></remarks>
    Sub CrazySendWinData(ByVal _Awards As Integer, ByVal _User As UserClass)
        '获取随机数
        Dim RndNum As Integer = RandomInt(1, 1001)
        '获取分区
        Dim SeatNum As Integer = _User.MachineNumber
        '如果 随机数/1000 小于等于 该分区的游戏百分比
        If CDbl(RndNum / 1000) <= CDbl(CrazySendPercentage(_User.MachineNumber)) Then
            '获取5-9的随机数
            Dim Num As Integer = RandomInt(5, 9)
            '产生5-8个藍七.红七，天下
            _User.WinData = AlgorithmClass(SeatNum).CrazySendData(0, _Awards, Num)
        Else
            '获取2-5的随机数
            Dim Num As Integer = RandomInt(2, 5)
            '产生2-4个藍七.红七，天下
            _User.WinData = AlgorithmClass(SeatNum).CrazySendData(0, _Awards, Num)
        End If
    End Sub
    ''' <summary>
    ''' 获得疯狂送奖项信息
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetCrazySendInfo() As String
        Try
            '用来保存个人中奖次数
            Dim PersonalWinNumber As String = ""
            '循环获取每个座位的  各人樱桃连线的次数/个人水果盘的次数/个人小B的次数
            For i As Integer = 1 To GameMachineCount
                If PersonalWinNumber = "" Then
                    PersonalWinNumber = PersonalCherryNum(i) & "|" & PersonalFruitDishNum(i) & "|" & PersonalSmallBarNum(i)
                Else
                    PersonalWinNumber = PersonalWinNumber & "|" & PersonalCherryNum(i) & "|" & PersonalFruitDishNum(i) & "|" & PersonalSmallBarNum(i)
                End If
            Next
            '返回80个座位，每个人的樱桃连线的次数|个人水果盘的次数|个人小B的次数
            Return PersonalWinNumber
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->执行GetCrazySendInfo方法时错误-> 获得疯狂送奖项信息 错误:", ex)
        End Try
        Return ""
    End Function
    ''' <summary>
    ''' 退出时初始化相关变量
    ''' </summary>
    ''' <param name="_User">玩家信息</param>
    '''  <param name="_Type">判断玩家是否离线，true为离线退出游戏，false是正常退出游戏</param>
    ''' <remarks></remarks>
    Public Sub OutClearData(ByVal _User As UserClass, ByVal _Type As Boolean)
        Try
            Dim x As Integer = _User.MachineNumber
            _User.IsPlay = False
            IfSatrtCompare(x) = False
            CompareNum(x) = 0
            IfFreeFiveNum(x) = 0
            IfCrazySend(x) = 0
            ComparedScore(x) = 0
            CompareInitialPoints(x) = 0
            ImmediatelySend(x) = False
            ImmediatelySendLeft(x) = 0
            ImmediatelySendRight(x) = 0
            ImmediatelySendLeftRnd(x) = 0
            ImmediatelySendRightRnd(x) = 0
            IfSettleAccounts(x) = -1

            If PersonalCherryNum(x) >= 3 Then
                PersonalCherryNum(x) = 0
                UpdateOverAllNum(x, 10)
            End If
            If PersonalFruitDishNum(x) >= 3 Then
                PersonalFruitDishNum(x) = 0
                UpdateOverAllNum(x, 11)
            End If
            If PersonalSmallBarNum(x) >= 3 Then
                PersonalSmallBarNum(x) = 0
                UpdateOverAllNum(x, 12)
            End If
            ToCenterSendMessage(x) = ""
            IfRecordCrazySendScore(x) = False
            CrazySendWinScore(x) = 0
            CrazySendScore(x) = 0
            IfSomeOneBet(x) = False
            ShowGameMessage("機台無人", 0, _User)
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->OutClearData方法->退出时初始化相关变量发生错误", ex)
        End Try
    End Sub

    ''' <summary>
    ''' 判断是否中全盘
    ''' </summary>
    ''' <param name="_User">玩家信息</param>
    ''' <remarks></remarks>
    Sub IfOverAll(ByVal _User As UserClass)
        '全盘红七
        If _User.WinData.IndexOf("1,1,1,1,1,1,1,1,1") >= 0 Then
            _User.IfOverAll = 1
            ' ToAllUserSendData2("全盤紅")
        End If
        '全盘蓝七
        If _User.WinData.IndexOf("2,2,2,2,2,2,2,2,2") >= 0 Then
            _User.IfOverAll = 2
            '  ToAllUserSendData2("全盤藍")
        End If
        '全盘天下
        If _User.WinData.IndexOf("11,11,11,11,11,11,11,11,11") >= 0 Then
            _User.IfOverAll = 3
            '  ToAllUserSendData2("全盤TS")
        End If
    End Sub
    ''' <summary>
    ''' 很久不操作时踢出用户
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub LongTimeNoOperate()
        '设置临时变量，在这个时间内未操作的用户被踢出
        Dim OutTime As Integer = 300
        Dim TempTime As Integer
        'For Each item As UserClass In UserList.Values

        'Next
        Try
                '根据座位号获取用户
                For Each item As UserClass In UserList.Values
                    Thread.Sleep(3000)
                '把判定用户未操作时间值，赋值给临时变量TempTime
                '如果玩家开始过游戏，就判断他最后操作时间是否超过5分钟，超过就踢出
                '如果玩家未开始过游戏
                If item.IsPlay = True Then
                    TempTime = OutTime
                Else
                    TempTime = 120
                End If
                ' 延迟退出时间 = 当前时间 - 最后一次操作时间
                Dim DelaySpanTime As TimeSpan = Now - LastOperationTime(item.MachineNumber)
                '如果延迟退出时间大于 用户最大不操作的时间
                If DelaySpanTime.TotalSeconds > TempTime Then
                    '如果延迟退出时间大于 用户最大不操作的时间+16
                    If DelaySpanTime.TotalSeconds > (TempTime + 16) Then
                        '最后一次操作时间 重新赋值为now
                        LastOperationTime(item.MachineNumber) = Now
                        '向该用户发送OUT 退出标识
                        item.SendData("OUT")
                        UserDisconnEvent(item)
                        '销毁用户类
                        item.Dispose()
                        '给登入服务器发送GS>SGP>检验码>T，从字典里踢出该玩家

                        '从字典中删除玩家
                        UserList.TryRemove(item.CheckCode, item)
                        item.IfBreak = False
                        '如果延迟退出的时间 小于 用户最大不操作的时间
                    ElseIf DelaySpanTime.TotalSeconds < (TempTime + 5) Then
                        item.SendData("NN?SGP")

                    End If
                End If
                ' 用现在的时间 减去用户的活跃时间
                Dim ActiveTime As TimeSpan = Now - item.ActiveTime
                '如果超过5秒，说明已经和客户端断线，进行相应处理
                If ActiveTime.TotalSeconds > 5 Then

                End If
            Next
            Catch ex As Exception
                RecordError.WriteGameServerErr("SGP", "GameLogicModule->LongTimeNoOperate->很久不操作时踢出用户", ex)
            End Try

    End Sub

    ''' <summary>
    ''' 作用说明：主要用于机台游戏参数初始化赋值内存变量
    ''' </summary>
    ''' <param name="_Message">查询数据库返回的分区信息</param>
    Public Sub MachineParameters(ByVal _Message As String)
        '内容程序员自由添加

        Try
            ' 用来临时存储各分区的所有参数信息
            Dim _sZoneMessage(GameMachineCount + 1, 59) As String
            Dim fqd() As String = Split(_Message, "$$")
            Dim aa() As String = fqd(1).Split(",")
            Dim bb As Integer = aa.Length
            '循环赋值给所有机台游戏参数、初始化赋值内存变量
            For i As Integer = 0 To fqd.Length - 2
                Dim qfq() As String = Split(fqd(i), "/")
                For j As Integer = 0 To 59
                    _sZoneMessage(CInt(qfq(0)) - 1, j) = fqd(i).Split(",")(j)
                Next
                'LongKickUser(i) = CInt(_sZoneMessage(i, 4))
            Next
            '将临时存储的游戏参数 赋值给公共变量Subregion
            Subregion = _sZoneMessage
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->MachineParameters方法->登入服务器返回信息后触发错误", ex)
        End Try
        'For Each a As String In Split(_Message, "$$")
        '    If a <> "" Then
        '        Try
        '            '拆分当前数据
        '            Dim _TempData() As String = a.Split(",")
        '            '求出当前数据机台号
        '            Dim SeatNum As Integer = CInt(_TempData(0).Split("/")(0))
        '            '如果此数据大于最大机台号，那么退出
        '            If SeatNum > GameMachineCount Then Exit For
        '            '记录玩家不玩时保留座位的时间
        '            If PersistGameTime(0) = 0 Then PersistGameTime(0) = _TempData(4)
        '            PersistGameTime(SeatNum) = _TempData(4)
        '        Catch ex As Exception
        '            RecordError.WriteGameServerErr("SGP", "GameLogicModule->MachineParameters方法->登入服务器返回信息后触发错误", ex)
        '        End Try
        '    End If
        'Next
    End Sub
    ''' <summary>
    ''' 判断是否是被限制账号,限制会员樱桃连线以上的奖项不开放
    ''' </summary>
    ''' <param name="_UserAccounts">用户账号，用来判断该用户是否被限制账号</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function IfRestrictedAccount(ByVal _UserAccounts As String) As Integer
        '声明一个存储 账号是否被限制的 变量
        Dim IfLimit As Integer = 0
        Try
            Dim UserInfo() As String
            '获取设置会员限制的参数 被设置的用户将不能中连线以上的奖项
            UserInfo = UsersLimit.Split("|")
            Dim i As Integer
            Dim UserName() As String
            '循环将被限制的会员存入到UserName中
            For i = 0 To UserInfo.Length - 1
                UserName = UserInfo(i).Split(",")
                '如果获取的会员 大写后等于传进来的参数
                If UserName(0).ToUpper() = _UserAccounts.ToUpper() Then
                    '获取是否被限制的状态信息，0没有 1限制模式  2低于当前模式
                    IfLimit = CInt(UserName(4))
                    Return IfLimit
                End If
            Next
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->IfRestrictedAccount方法->判断账号是否是被限制时发生错误", ex)
        End Try
        Return IfLimit
    End Function
    ''' <summary>
    ''' 更新后台设置页的游戏消息
    ''' </summary>
    ''' <param name="_StateMessage">描述用户状态信息字符串</param>
    ''' <param name="_IfCrazySend">是否是疯狂送， 1：樱桃(蓝7)  2：水果盘(红7)  3：小BAR(天下)</param>
    ''' <param name="_User">玩家信息</param>
    ''' <remarks></remarks>
    Public Sub ShowGameMessage(ByVal _StateMessage As String, ByVal _IfCrazySend As Integer, ByVal _User As UserClass)
        Try
            '获取玩家座位号
            Dim SeatNum As Integer = _User.MachineNumber
            Dim Message As String = ""
            '如果参数_StateMessage，游戏状态str是 机台无人
            If _StateMessage.Equals("機台無人") Then
                '就将该机台无人状态赋值给 临时变量Message
                Message = _StateMessage
            Else
                '设置疯狂送临时变量字符串
                Dim CrazySendStr As String = ""
                '如果不等于0，说明在疯狂送状态
                If _IfCrazySend <> 0 Then
                    CrazySendStr = "機臺瘋狂送中"
                End If
                '是否有会员限制，0没有 1限制模式  2低于当前模式
                If IfUserLimit(SeatNum) = 0 Then
                    '如果没有会员限制、 存入"機臺瘋狂送中"、 會員帳號、玩家名称
                    Message = _StateMessage & "<br><span style=""font-size:45px; color:blue;"">" & CrazySendStr & " 會員帳號:" & _User.Account & "</span>"
                ElseIf IfUserLimit(SeatNum) = 1 Then
                    '如果有会员限制  存入、限制會員、玩家名称
                    Message = _StateMessage & "<br><span style=""font-size:45px; color:red;"">" & CrazySendStr & " 限制會員:" & _User.Account & " 正在該台</span>"
                Else
                    '如果 低于当前模式  存入"低於當前模式"、玩家名称
                    Message = _StateMessage & "<br><span style=""font-size:42px; color:red;"">" & CrazySendStr & " 低於當前模式:" & _User.Account & " 正在該台</span>"
                End If
            End If
            '如果变量Message为空，说明不符合上面的判断，没有给其赋值
            If Message <> "" Then
                '修改 游戏状态信息
                ToDataBank.UpdateByGameMess(Message, _User.Account, SeatNum)
            End If
        Catch ex As Exception
            RecordError.WriteGameServerErr("GameLogicModule->执行ShowGameMessage方法时错误-> 更新后台设置页的游戏消息 错误:", ex.Message, ex)
        End Try
    End Sub
    ''' <summary>
    ''' 更新数据库中的f_Winning(ALL天下,ALL红7,ALL蓝7,ALL大B,ALL中B,ALL杂7,天天天,红777,蓝777,小BAR，樱桃连线，水果盘，小Bar)
    ''' </summary>
    ''' <param name="_Zone">区号</param>
    ''' <param name="_Number">f_Winning数据中的第几个</param>
    ''' <remarks></remarks>
    Public Sub UpdateOverAllNum(ByVal _Zone As Integer, ByVal _Number As Integer)
        Try
            '防止同步
            SyncLock Synchrolock(_Zone)
                '获取数据库中的f_Winning字段
                Dim TempStr() As String = ToDataBank.SelectWinningOfSgp(_Zone).Split(",")
                TempStr(_Number) = Trim(CInt(TempStr(_Number)) + 1)
                '如果是樱桃连线、水果盘、小B连线其中一个，并且樱桃连线、水果盘、小B连线次数等于4了 （连线次数最大值为3）、就将连线次数清零
                If _Number >= 10 AndAlso _Number <= 12 AndAlso TempStr(_Number) = 4 Then
                    TempStr(_Number) = "0"
                End If
                '更新数据库中的f_Winning字段
                ToDataBank.UpdateWinningOfSgp(Join(TempStr, ","), _Zone)
                Select Case _Number
                    Case 10 '重新赋值樱桃连线次数
                        PersonalCherryNum(_Zone) = CInt(TempStr(_Number))
                        PersonalCherryNumMax(_Zone) = PersonalCherryNum(_Zone)
                    Case 11 '重新赋值水果盘次数
                        PersonalFruitDishNum(_Zone) = CInt(TempStr(_Number))
                        PersonalFruitDishNumMax(_Zone) = PersonalFruitDishNum(_Zone)
                    Case 12 '重新赋值小B连线次数
                        PersonalSmallBarNum(_Zone) = CInt(TempStr(_Number))
                        PersonalSmallBarNumMax(_Zone) = PersonalSmallBarNum(_Zone)
                End Select
            End SyncLock
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->UpdateOverAllNum方法-> 更新数据库中的f_Winning时发生错误", ex)
        End Try
    End Sub
    ''' <summary>
    ''' 作用说明：模拟中心服务器
    ''' 接收 服务器向中心服务器发送的数据，然后模拟中心服务器给  服务器返回一个数据，来保证正常进行游戏
    ''' 需要根据此数据，提示FLASH端输赢情况，以及状态
    ''' </summary>
    ''' <param name="_Value">中心服务器返回计算得分结果的数据</param>
    Public Sub SimulationCenterServer(ByVal _Value As String, ByVal _User As UserClass)
        Try
            Dim _sStr() As String = _Value.Split("|")
            '_Value的值可能是  扣分标记= A0|游戏标记|用户帐号|下注真实额度（总投注/房间比例）|用户IP|分区号,0,0|设备类型（没有，默认1）|客户端系统信息（没有写空）|分区号,1,1|下注说明
            '  返回0A0时 向单个玩家发送
            '"SHOW|#2,10,7,1,5,9,8,6,11%1|7|2%#-60"
            If _sStr(0) = "A0" Then
                Dim x As Integer = _sStr(5).Split(",")(0)
                '中心服务器返回的数据格式--"0A0|W611-1|80|5.000|87072.400|111021103706849|41,1,1|"
                'Return "0A0|" & _sStr(2) & _sStr(3) & "|" & 5.0 & ResidueScore(x) = _uUser.UserCurrentlyIntegral * RoomRatio(x) - CInt(_sStr(3)) * EndPoints(x) & sXZMark2 & "|" & _sStr(8) & "|"
                '让游戏界面开始转
                _User.NoteNum = RandomInt(10000000, 999999999)
                '让游戏界面开始转
                _User.SendData(NoteSingle(x))
            Else
                ' '结算标记 向中心服务器发送的       ："A1|SGP|4|1|1|0|水果盤 4區<BR>(10:1)<BR>3個蘋果連線10注 <BR>||AJ61-1,600,161030144758480,800,800,1,"
                '                                       A1|SGP|分区号|座位号|1|中奖类别(0正常的，4疯狂送，1红7全盘，2蓝7全盘，3天下全盘，10比倍)|汉字中将格式|“”|账号，赢得分数，注单号，下注分数，下注分数 '160929新添加的逻辑：根据数据，拼接字符串
                '中心服务器返回的结果
                '_sStr数组数据依次是：游戏标记|分区号|桌子号|实际参与游戏中人数|自定义数据( 自定义数据  0正常  4疯狂送  1红7连线奖金 2蓝7连线奖金 3天下连线奖金 10比倍)|主单号||账号,输赢分数,剩余分数,0,返利,剩余分数
                '_sStr数组内容例如  ： ""0SGP|41|1|1|0|111021103454738||W611-1,-60,87092.4,0,0,87092.4,0"
                '向客户端应该发送的数据   "JIE|W69-1,80,835640,83564|0"
                Dim x As Integer = _sStr(2)
                Dim data() As String = _sStr(8).Split(",")
                Dim _sSettlementData As String = "JIE?" & data(0) & "," & _User.BetNumeber & "," & _User.Integral * RoomRatio(x) + CInt(data(1)) * EndPoints(x) & "," & _User.Integral + CInt(data(1)) * EndPoints(x) & "|" & 0
                '向客户端发送本局的游戏信息
                _User.SendData(_sSettlementData)
                '記錄步驟记录游戏演示信息 
                RecordGameMessage(x).Append(x & "^" & PersonalCherryNumMax(x) & "^" & PersonalFruitDishNumMax(x) & "^" & PersonalSmallBarNumMax(x) & "^")
                '記錄步驟记录游戏演示信息 
                RecordGameMessage(x).Append(_sSettlementData & "$" & "&" & 0 & "&" & IfCrazySend(x) & "~")
                '用户断线时的数据信息=記錄步驟记录游戏演示信息
                _User.BreakMessage = RecordGameMessage(x).ToString
                '向数据库中保存演示数据
                'GameToSQLServerModule.ToGameNote(data(2), data(0), RecordGameMessage(x).ToString)
                '将是否已结算状态改为2 -> 0未结算 1已发送结算但未返回 2已返回
                IfSettleAccounts(x) = 2
                '总赢累加,每次加的是页面显示的中奖分数
                Dim _iAlwaysWinScore As Integer = CInt(data(1) * RoomRatio(x))
                '取本次押分
                Dim _iTheBet As Int16 = EndPoints(x) * _User.BetNumeber
                '当输赢结果大于负的本次押分时表示一定中奖，那么我们要在总赢中做累加       
                '疯狂送
                If CInt(_sStr(5)) = 4 Then
                    '修改 总押和总赢
                    ToDataBank.UpdateZyaAndZyingCount(_iTheBet, _iAlwaysWinScore, x)
                Else
                    ToDataBank.UpdateZyaAndZyingCount(_iTheBet, _iAlwaysWinScore + _iTheBet, x)
                End If
            End If
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->SimulationCenterServer方法->模拟中心服务器发送结算数据发生错误", ex)
        End Try
    End Sub
    ''' <summary>
    ''' 创始时间：2016-1-1 创始人： XXXX  
    ''' 描述：随机出即时连线送.即时送开始
    ''' 修改时间：2016-12-1 修改人：吴昊
    ''' 修改描述：
    ''' </summary>
    ''' <param name="_User">玩家信息</param>
    ''' <remarks></remarks>
    Sub StartInstantDelivery(ByVal _User As UserClass)
        '获取玩家分区
        Dim SeatNum As Integer = _User.MachineNumber
        '如果即时连线送是第五次，就退出即时送
        If ImmediatelySendNum(SeatNum) = 5 Then Exit Sub
        '随机即时送的水果
        ImmediatelySendLeft(SeatNum) = RandomInt(1, 9)
        '随机即时送的几倍
        ImmediatelySendRight(SeatNum) = RandomInt(1, 9)
        '即时送左边随机
        ImmediatelySendLeftRnd(SeatNum) = ImmediatelySendLeft(SeatNum)
        '即时送右边随机
        ImmediatelySendRightRnd(SeatNum) = ImmediatelySendRight(SeatNum)
        '即时送开始
        _User.SendData("JSK?" & ImmediatelySendLeft(SeatNum) & "|" & ImmediatelySendRight(SeatNum))
        '如果不是即时送状态，就改为是即时送
        If IfImmediatelyLineSend(SeatNum) = False Then IfImmediatelyLineSend(SeatNum) = True
    End Sub
    ''' <summary>
    ''' 过程特点：线程、被动触发
    ''' 触发条件：中心服务器输赢结果和状态数据触发
    ''' 作用说明：接收到的数据是中心服务器返回的，需要根据此数据，提示FLASH端输赢情况，以及状态
    ''' </summary>
    ''' <param name="_Value">中心服务器返回计算得分结果的数据</param>
    Public Sub SettleAccountsInfo(ByVal _Value As String)
        '      _Value数据格式：0游戏标记|机台号|1|1|自定义数据|输赢单ID号||帐号-网站序号,输赢结果,剩余积分或状态序号,未知,未知,剩余积分,未知
        '例如：_Value数据内容： 0ZYJZ|78|1|1|5|180021140223172||Z680-1,-125,-10,0,0,-10,0
        Try

            '将接收的数据拆分成数组
            '_sAllData数组数据依次是：游戏标记|分区号|桌子号|实际参与游戏中人数|自定义数据( 自定义数据  0正常  4疯狂送  1红7连线奖金 2蓝7连线奖金 3天下连线奖金 10比倍)|主单号||账号,输赢分数,剩余分数,0,返利,剩余分数
            '_sAllData数组内容例如  ： "0SGP|41|1|1|0|111021103454738||W611-1,-60,87092.4,0,0,87092.4,0"
            Dim _sAllData() As String = _Value.Split("|")
            If _sAllData(0) = "0A0" Then
                Try
                    '获取座位号
                    Dim UserC As New UserClass
                    Dim x As Integer = CInt(_sAllData(6).Split(",")(0))
                    For Each item As UserClass In UserList.Values
                        If item.MachineNumber = x Then
                            UserC = item
                        End If
                    Next

                    If UserC.MachineNumber = x Then
                        '判断发送的数据是否合法，
                        If CInt(_sAllData(3)) = 5 Then
                            UserC.NoteNum = _sAllData(5)
                            '让游戏界面开始转
                            UserC.SendData(NoteSingle(x))
                            '需要断线重连
                            ' UserC.xulian = True                            
                        Else
                            '错误信息
                            UserC.NoteNum = "-1"
                            '不需要断线重连
                            ' UserC.xulian = False
                            '输的分赋值为0
                            Dim xzsk As Integer = 0
                            Dim xzskstr As String = ""
                            If CInt(_sAllData(3)) = 1 Then
                                xzsk = 2
                                xzskstr = "my-2"
                            Else
                                xzsk = 1
                                xzskstr = "dw"
                            End If
                            If xzskstr > 0 Then
                                '  UserC.UserCamouflage = True
                                ' UserC.xulian = False
                                'IsPaly(x) = False
                                UserC.SendData("CLOSE?" & xzskstr & "/" & xzsk)
                            End If
                            If UserC.IfBreak = True Then
                                '更改机台状态为无人
                                ShowGameMessage("機台無人", 0, UserC)
                                'UserC.UserStrat = False
                                UserC.SendData("<OUT>")
                                ' SetUserLeave(_User)
                            End If
                        End If
                    End If
                Catch ex As Exception
                    RecordError.WriteGameServerErr("SGP", "GameLogicModule->TCPclassNewUesrLogin ->中心服务器处理用户数据错误，错误：", ex)
                End Try
            Else
                '获取玩家的座位号
                Dim x As Integer = CInt(_sAllData(1))
                '_sPlayersScore（0）玩家名称、 _sPlayersScore（1）输赢结果、 _sPlayersScore（2）玩家当前积分、 _sPlayersScore（5）玩家当前积分   _sPlayersScore（6）固定一百（可能是100个机台上限）
                Dim _sPlayersScore() As String = _sAllData(7).Split(",")
                SyncLock Synchrolock(x)
                    '根据座位号获取用户
                    Dim UserC As New UserClass
                    For Each item As UserClass In UserList.Values
                        If item.MachineNumber = x Then
                            UserC = item
                        End If
                    Next
                    '声明一个保存入场资格的参数
                    Dim _iQualification As Integer
                    '是否维护
                    'If GameSocket.MainLocation.GameStop = False Then
                    '    Select Case _sPlayersScore(2)
                    '        '-1,-5,-10  账号限制    -2  入场资格不足     -3  超过总输上限     -4  服务器维护
                    '        Case "-1", "-2", "-3", "-4", "-5", "-10"
                    '            '用户伪装改为true
                    '            UserC.UserCamouflage = True
                    '            '用户续连状态改为false
                    '            UserC.xulian = False
                    '            _iQualification = CInt(_sPlayersScore(2))
                    '    End Select
                    'Else 'AP维护
                    '    _iQualification = -20
                    '    '用户伪装改为true
                    '    UserC.UserCamouflage = True
                    '    '用户续连状态改为false
                    '    UserC.xulian = False
                    'End If
                    '获取用户当前余额
                    UserC.Integral = _sPlayersScore(2)
                    '如果得分大于押分( 低分*押注分数/额度倍数)
                    If _sPlayersScore(1) > EndPoints(x) * UserC.BetNumeber / RoomRatio(x) Then
                        '总赢次数
                        DatabaseClass.TotalWinNum(x) += 1
                    End If
                    '修改 总押和总赢 次数
                    ToDataBank.UpdateZyaAndZyingCount(BetNum(x), DatabaseClass.TotalWinNum(x), x)
                    '自定义数据  0正常  4疯狂送  1红7连线奖金 2蓝7连线奖金 3天下连线奖金 10比倍
                    Select Case CInt(_sAllData(4))
                        Case 0, 4 '0中的是正常的 4是疯狂送的
                            Dim _sSettlementData As String = "JIE?" & _sPlayersScore(0) & "," & _sPlayersScore(1) & "," & _sPlayersScore(2) * RoomRatio(x) & "," & _sPlayersScore(5) & "/" & _iQualification
                            '向客户端发送本局的游戏信息
                            UserC.SendData(_sSettlementData)
                            '記錄步驟记录游戏演示信息 
                            RecordGameMessage(x).Append(x & "^" & PersonalCherryNumMax(x) & "^" & PersonalFruitDishNumMax(x) & "^" & PersonalSmallBarNumMax(x) & "^")
                            '如果可以即时送，并且各分区总押次数+5大于总押次数，并且退出疯狂送为true
                            If ImmediatelySend(x) AndAlso TotalBetNum(x) + 5 <> BetNum(x) AndAlso UserC.ImmediatelySendClose = False Then
                                '記錄步驟记录游戏演示信息 
                                RecordGameMessage(x).Append(ImmediatelySendLeft(x) & "^" & ImmediatelySendRight(x) & "^")
                            Else
                                RecordGameMessage(x).Append(ImmediatelySendLeftRnd(x) & "^" & ImmediatelySendRightRnd(x) & "^1^")
                            End If
                            Dim mybol As Boolean = False
                            If CInt(_sPlayersScore(5)) < 0 Then
                                _sPlayersScore(5) = "0"
                                mybol = True
                            End If
                            '是否疯狂送
                            Dim _IfCrazySend As Integer = 0
                            '如果是疯狂送，并且不是即时送
                            If IfCrazySend(x) <> 0 AndAlso ImmediatelySendDouble(x) = False AndAlso CInt(_sAllData(4)) = 4 Then
                                _IfCrazySend = 1
                            End If
                            '是否疯狂送
                            If _IfCrazySend = 0 Then
                                '我的疯狂送改为false
                                MyCrazySend(x) = False
                                '记录本局游戏信息( 玩家名称 ， 输赢分数 ， 当前积分减去下注积分， 用户当前积分)
                                _sSettlementData = "JIE|" & _sPlayersScore(0) & "," & _sPlayersScore(1) & "," & ResidueScore(x) & "," & _sPlayersScore(5) & "|" & _iQualification
                            Else
                                '如果我的疯狂送为false
                                If MyCrazySend(x) = False Then
                                    '记录本局游戏信息( 玩家名称 ， 输赢分数 ， 当前积分减去下注积分， 用户当前积分)
                                    _sSettlementData = "JIE|" & _sPlayersScore(0) & "," & _sPlayersScore(1) & "," & ResidueScore(x) & "," & _sPlayersScore(5) & "|" & _iQualification
                                    '将我的疯狂送改为true
                                    MyCrazySend(x) = True
                                    '当前积分减去下注积分赋值给 我的疯狂送额度
                                    MyCrazySendLimit(x) = ResidueScore(x)
                                    '我的疯狂送分数
                                    MyCrazySendScore(x) = _sPlayersScore(1)
                                Else
                                    '我的疯狂送分数
                                    MyCrazySendScore(x) += CDbl(_sPlayersScore(1))
                                    '记录本局游戏信息( 玩家名称 ， 疯狂送积分 ， 我的疯狂送额度， 用户当前积分)
                                    _sSettlementData = "JIE|" & _sPlayersScore(0) & "," & MyCrazySendScore(x) & "," & MyCrazySendLimit(x) & "," & _sPlayersScore(5) & "|" & _iQualification
                                End If
                            End If
                            If mybol Then
                                If _IfCrazySend = 0 Then
                                    '将我的疯狂送改为false
                                    MyCrazySend(x) = False
                                    '记录本局游戏信息( 玩家名称 ， 输赢分数 )
                                    _sSettlementData = "JIE|" & _sPlayersScore(0) & "," & _sPlayersScore(1) & "," & "0" & "," & "0" & "|" & _iQualification
                                Else
                                    '如果我的疯狂送为false
                                    If MyCrazySend(x) = False Then
                                        '记录本局游戏信息( 玩家名称 ， 输赢分数 )
                                        _sSettlementData = "JIE|" & _sPlayersScore(0) & "," & _sPlayersScore(1) & "," & "0" & "," & "0" & "|" & _iQualification
                                        '将我的疯狂送改为true
                                        MyCrazySend(x) = True
                                        '我的疯狂送额度改为0
                                        MyCrazySendLimit(x) = "0"
                                        '我的疯狂送分数
                                        MyCrazySendScore(x) = _sPlayersScore(1)
                                    Else
                                        '我的疯狂送分数
                                        MyCrazySendScore(x) += CDbl(_sPlayersScore(1))
                                        '记录本局游戏信息(玩家名称,我的疯狂送积分)
                                        _sSettlementData = "JIE|" & _sPlayersScore(0) & "," & MyCrazySendScore(x) & "," & "0" & "," & "0" & "|" & _iQualification
                                    End If
                                End If
                            End If
                            '記錄步驟记录游戏演示信息 
                            RecordGameMessage(x).Append(_sSettlementData & "$" & "&" & _IfCrazySend & "&" & IfCrazySend(x) & "~")
                            '用户断线时的数据信息=記錄步驟记录游戏演示信息
                            UserC.BreakMessage = RecordGameMessage(x).ToString
                            '向数据库中保存演示数据
                            '  GameToSQLServerModule.ToGameNote(_sAllData(5), _sAllData(7).Split(",")(0), RecordGameMessage(x).ToString)
                            '如果是强制中奖，更新中奖信息
                            If NowIfForceWin(x) <> "" AndAlso CInt(NowIfForceWin(x)) > 0 Then
                                '樱桃连线\水果盘\小BAR连线                    
                                If NowIfForceWin(x) = "4" AndAlso IfCrazySend(x) = 1 OrElse NowIfForceWin(x) = "5" AndAlso IfCrazySend(x) = 2 OrElse NowIfForceWin(x) = "6" AndAlso IfCrazySend(x) = 3 Then
                                    '是否需要记载疯狂送分数
                                    IfRecordCrazySendScore(x) = True
                                    '触发疯狂送时的连线得分
                                    CrazySendScore(x) = _sPlayersScore(1)
                                Else
                                    '是否需要记载疯狂送分数
                                    IfRecordCrazySendScore(x) = False
                                    '清空强制中奖变量
                                    'DatabaseClass(x).UpdateQiang(x, _sPlayersScore(1))
                                End If
                            End If
                            '看是否为疯狂送 1蓝7，2红7，3天下
                            Select Case IfCrazySend(x)
                                Case 1, 2, 3
                                    '记录是否贏夠五次
                                    IfFreeFiveNum(x) += 1
                                    '用户是否疯狂送=贏夠五次变量 - 1
                                    UserC.IfCrazySendStart = IfFreeFiveNum(x) - 1
                                    '如果赢够五次变量大于1
                                    If IfFreeFiveNum(x) > 1 Then
                                        '疯狂送的总赢分数 = 疯狂送的总赢分数 + 
                                        CrazySendWinScore(x) = CrazySendWinScore(x) + _sPlayersScore(1)
                                    End If
                                    '如果赢够五次变量等于6、'疯狂送结束
                                    If IfFreeFiveNum(x) = 6 Then
                                        '把状态改为不是疯狂送
                                        IfCrazySend(x) = 0
                                        '赢够五次变量归零
                                        IfFreeFiveNum(x) = 0
                                        '发送，樱桃、水果盘、小B连线次数
                                        UserC.SendData("WB?" & PersonalCherryNum(x) & "," & PersonalFruitDishNum(x) & "," & PersonalSmallBarNum(x))
                                        '如果需要记载疯狂送分数
                                        If IfRecordCrazySendScore(x) Then
                                            '需要记载疯狂送分数改为false
                                            IfRecordCrazySendScore(x) = False
                                            '更新强制中奖疯狂送的得分
                                            ' UpdateFKS(x, CrazySendScore(x), CrazySendWinScore(x))
                                            '触发疯狂送时的连线得分 归零
                                            CrazySendScore(x) = 0
                                        End If
                                        '记载疯狂送的总赢分数归零
                                        CrazySendWinScore(x) = 0
                                    End If
                            End Select
                            '总赢累加,每次加的是页面显示的中奖分数
                            Dim _iAlwaysWinScore As Integer = CInt(_sPlayersScore(1) * RoomRatio(x))
                            '取本次押分
                            Dim _iTheBet As Int16 = EndPoints(x) * UserC.BetNumeber
                            '当输赢结果大于负的本次押分时表示一定中奖，那么我们要在总赢中做累加       
                            '疯狂送
                            If CInt(_sAllData(4)) = 4 Then
                                '修改 总押和总赢
                                ToDataBank.UpdateZyaAndZyingCount(_iTheBet, _iAlwaysWinScore, x)
                            Else
                                ToDataBank.UpdateZyaAndZyingCount(_iTheBet, _iAlwaysWinScore + _iTheBet, x)
                            End If
                        Case 1 '中的红7连线奖金        ZLL|中的什么|中了多少|剩余的分数
                            UserC.SendData("ZLL?" & 1 & "/" & BureauRed7Bonus(x) * RoomRatio(x) & "/" & Red7Money)
                        Case 2 '中的是蓝7连线奖金
                            UserC.SendData("ZLL?" & 2 & "/" & BureauBlue7Bonus(x) * RoomRatio(x) & "/" & Blue7Money)
                        Case 3 '中的是天下奖金连线奖金
                            UserC.SendData("ZLL?" & 3 & "/" & BureauWorldBonus(x) * RoomRatio(x) & "/" & WorldMoney)
                        Case 10 '比倍的返回
                            '参数信息( 玩家名称 ， 输赢分数 ， 当前积分减去下注积分， 用户当前积分)
                            Dim cd As String = "JIEB?" & _sPlayersScore(0) & "," & _sPlayersScore(1) & "," & _sPlayersScore(2) * RoomRatio(x) & "," & _sPlayersScore(5) & "/" & _iQualification
                            If CInt(_sPlayersScore(1)) < 0 Then
                                '点得分之后的剩余分数以及比倍输后的剩余分数
                                UserC.ResidualFraction = _sPlayersScore(2) * RoomRatio(x)
                            End If
                            '向玩家发送游戏信息
                            UserC.SendData(cd)
                            '記錄步驟记录游戏演示信息
                            RecordGameMessage(x).Append(ShowCards(x) & "|" & CardsNum(x) & "|" & RightShowSix(x) & "|" & DemoInitialPoints(x) & "|" & ComparedScore(x) & "|" & CompareNum(x) & "|$" & _iQualification)
                            If CInt(_sPlayersScore(5)) < Subregion(x - 1, 2) Then
                                UserC.SendData("ZGBZ")
                            End If
                            If CInt(_sPlayersScore(5)) < 0 Then
                                '将記錄步驟记录游戏演示信息拆分成数组
                                Dim ss2() As String = RecordGameMessage(x).ToString().Split("|")
                                Dim ss3() As String = ss2(7).Split(",")
                                ss3(2) = "0"
                                ss3(3) = "0"
                                ss2(7) = ss3(0) & "," & ss3(1) & "," & ss3(2) & "," & ss3(3)
                                Dim str2 As String = String.Empty
                                For i As Integer = 0 To ss2.Length - 1
                                    If i <> ss2.Length - 1 Then
                                        str2 = str2 & ss2(i) & "|"
                                    Else
                                        str2 = str2 & ss2(i)
                                    End If
                                Next
                                '新new一个記錄步驟记录游戏演示信息
                                RecordGameMessage(x) = New StringBuilder()
                                RecordGameMessage(x).Append(str2)
                            End If
                            '向数据库中保存演示数据
                            'GameToSQLServerModule.ToGameNote(_sAllData(5), _sAllData(7).Split(",")(0), RecordGameMessage(x).ToString)
                            '总赢累加 当比倍时 不管是输或赢都要对 总赢进行处理
                            Dim lin As Integer = CInt(_sPlayersScore(1) * RoomRatio(x))
                            '修改 总押和总赢
                            ToDataBank.UpdateZyaAndZyingCount(0, lin, x)
                            '比倍次数归零
                            CompareNum(x) = 0
                    End Select
                    '将是否已结算状态改为2 -> 0未结算 1已发送结算但未返回 2已返回
                    IfSettleAccounts(x) = 2
                    '如果各人樱桃连线的次数大于等于三次，就归零，并更新数据库
                    If PersonalCherryNum(x) >= 3 Then
                        PersonalCherryNum(x) = 0
                        UpdateOverAllNum(x, 10)
                    End If
                    '如果各人水果盘连线的次数大于等于三次，就归零，并更新数据库
                    If PersonalFruitDishNum(x) >= 3 Then
                        PersonalFruitDishNum(x) = 0
                        UpdateOverAllNum(x, 11)
                    End If
                    '如果各人小B连线的次数大于等于三次，就归零，并更新数据库
                    If PersonalSmallBarNum(x) >= 3 Then
                        PersonalSmallBarNum(x) = 0
                        UpdateOverAllNum(x, 12)
                    End If
                    '自定义数据  0正常  4疯狂送  1红7连线奖金 2蓝7连线奖金 3天下连线奖金 10比倍
                    '如果不是比倍，并且也不是疯狂送
                    If CInt(_sAllData(4)) <> 10 AndAlso CInt(_sAllData(4)) <> 4 Then
                        '如果是即时连线送，就存储是第几次
                        If IfImmediatelyLineSend(x) Then ImmediatelySendNum(x) += 1
                        '当即时连线送第五次
                        If ImmediatelySendNum(x) = 5 Then
                            '即时连线送翻倍状态改为false（退出即时连线送）
                            ImmediatelySendDouble(x) = False
                            '即时连线送状态改为false （退出即时连线送）
                            IfImmediatelyLineSend(x) = False
                            '即时连线送次数归零
                            ImmediatelySendNum(x) = 0
                        End If
                    End If
                    '疯狂送离线结算中
                    If CrazySendBreakLine(x) Then
                        '疯狂送中断线时，将未完成的疯狂送执行完
                        If IfCrazySend(x) <> 0 AndAlso IfFreeFiveNum(x) > 0 Then
                            '疯狂送离线结算
                            CrazySendOffLineSettle(UserC)
                        Else
                            '退出时初始化相关变量
                            OutClearData(UserC, True)
                        End If
                        Exit Sub
                    End If
                    ' true表示比倍中断线，此时待中心服务器结算后再退出
                    If CompareBreakLine(x) Then
                        '将比倍中断状态改为false
                        CompareBreakLine(x) = False
                        '退出时初始化相关变量
                        OutClearData(UserC, True)
                    End If
                    '伪用户或玩家不在线
                    ' If _uUser.UserCamouflage Then
                    '  _uUser.UserStrat = False
                    '设置用户离线
                    'SetUserLeave(_uUser)
                    'End If
                End SyncLock
            End If
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameLogicModule->SettleAccountsInfo方法->接收中心服务器返回数据，根据此数据，提示FLASH端输赢情况，以及状态发生错误", ex)
        End Try
    End Sub
    '''' <summary>
    '''' 更新强制中奖疯狂送的得分
    '''' </summary>
    '''' <param name="v_iZone">分区</param>
    '''' <param name="v_douScore">触发疯狂的连线得分</param>
    '''' <param name="v_douTotalScore">疯狂送总得分</param>
    '''' <remarks></remarks>
    'Private Sub UpdateFKS(ByVal v_iZone As Integer, ByVal v_douScore As Double, ByVal v_douTotalScore As Double)
    '    Try
    '        If GameSocket.MainLocation.GameLocation(v_iZone, 1, 1) IsNot Nothing AndAlso GameSocket.MainLocation.GameLocation(v_iZone, 1, 1) <> "" Then
    '            '根据座位号获取用户
    '            Dim user0 As TCPNetSocket.Userinfo = GetUserInfo(v_iZone)
    '            If user0 IsNot Nothing Then
    '                '调用数据库修改 疯狂送总得分 
    '                InsertOrUpdateByNode(user0.UserAccounts, " 得分:" & v_douScore & " 瘋狂送得分:" & v_douTotalScore, "%設置強制中獎%", v_iZone, True)
    '            End If
    '        End If
    '    Catch ex As Exception
    '        RecordError.WriteGameServerErr("GameLogicModule->执行UpdateFKS方法时错误->更新强制中奖疯狂送的得分 错误:", ex.Message, ex)
    '    End Try
    'End Sub
#End Region


End Module
