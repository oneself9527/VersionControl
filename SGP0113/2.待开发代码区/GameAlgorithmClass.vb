
Imports System.IO
Imports System.Array
Imports System.Text
Imports ToolModule

Public Class GameAlgorithmClass

#Region "--计算中奖信息--"
    ''' <summary>
    ''' 各种奖项的赔率
    ''' </summary>
    ''' <remarks>分别记录：1樱桃，2樱桃，3樱桃连线，4橘子连线，5苹果连线，6葡萄连线，7西瓜连线，8乱BAR连线，9小BAR连线，10中BAR连线，11大BAR连线，12乱7连线，13蓝7连线，14红7连线，15天下连线，16水果盘，17ALL樱桃，18ALL橘子，19ALL苹果，20ALL葡萄，21ALL西瓜，22ALL乱BAR，23ALL小BAR，24ALL中BAR，25ALL大BAR，26ALL乱7，27ALL蓝7，28ALL红7，29、2天下，30、3天下/31、4天下/32、5天下/33、6天下/34、7天下/35、8天下/36、ALL天下/37、2蓝7/38、3蓝7/39、4蓝7/40、5蓝7/41、6蓝7/42、7蓝7/43、8蓝7/44、2红7/45、3红7/46、4红7/47、5红7/48、6红7/49、7红7/50、8红7</remarks>
    Public iAwardOdds() As Integer = {0, 2, 5, 10, 12, 14, 18, 20, 15, 30, 50, 80, 50, 100, 150, 200, 20, 40, 50, 60, 70, 80, 60, 80, 100, 150, 100, 200, 300, 500, 5, 10, 15, 20, 30, 40, 50, 10, 15, 20, 25, 40, 50, 60, 20, 30, 40, 50, 60, 70, 80}   '共50个元素
    ''' <summary>
    ''' 每个奖项占用格子的数量
    ''' </summary> 
    ''' <remarks></remarks>
    Public iCellNum() As Integer = {0, 1, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 2, 3, 4, 5, 6, 7, 8, 2, 3, 4, 5, 6, 7, 8, 2, 3, 4, 5, 6, 7, 8}
    ''' <summary>
    ''' 每个奖项对应的图片，1红7 2蓝7 3大RAB 4中RAB 5小RAB 6西瓜 7葡萄 8苹果 9橘子 10樱桃 11天下
    ''' </summary>
    ''' <remarks></remarks>
    Public sPrizePicture() As String = {0, 10, 10, 10, 9, 8, 7, 6, "3,4,5", 5, 4, 3, "1,2", 2, 1, 11, "6,7,8,9,10", 10, 9, 8, 7, 6, "3,4,5", 5, 4, 3, "1,2", 2, 1, 11, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 11, 11, 11, 11, 11, 11, 11}
    ''' <summary>
    ''' '8条连线的排列
    ''' </summary>
    ''' <remarks></remarks>
    Public sLineRank() As String = {"4,5,6", "1,2,3", "7,8,9", "7,5,3", "1,5,9", "1,4,7", "2,5,8", "3,6,9"}
    ''' <summary>
    '''   '一个樱桃的位置
    ''' </summary>
    ''' <remarks></remarks>
    Public sOneCherrySeat() As String = {"2", "3", "4", "7"}
    ''' <summary>
    '''  '两个樱桃的位置
    ''' </summary>
    ''' <remarks></remarks>
    Public sTwoCherrySeat() As String = {"2,5", "3,6", "7,5"}
    ''' <summary>
    '''  '9个格子所有位置
    ''' </summary>
    ''' <remarks></remarks>
    Public sAllSeat() As String = {"1", "2", "3", "4", "5", "6", "7", "8", "9"}
    ''' <summary>
    '''  '混B的所有排列组合
    ''' </summary>
    ''' <remarks></remarks>
    Public sMixBarRank() As String = {"3,3,4", "3,3,5", "3,4,3", "3,4,4", "3,4,5", "3,5,3", "3,5,4", "3,5,5", "4,3,3", "4,3,4", "4,3,5", "4,4,3", "4,4,5", "4,5,3", "4,5,4", "4,5,5", "5,3,3", "5,3,4", "5,3,5", "5,4,3", "5,4,4", "5,4,5", "5,5,3", "5,5,4"}
    ''' <summary>
    '''  '混7的所有排列组合
    ''' </summary>
    ''' <remarks></remarks>
    Public sMix7Rank() As String = {"1,2,1", "1,1,2", "2,1,2", "2,2,1", "2,1,1", "1,2,2"}
    ''' <summary>
    ''' '其他连线的排列组合
    ''' </summary>
    ''' <remarks></remarks>
    Public sOtherRank() As String = {"00000", "1111", "2222", "10,10,10", "9,9,9", "8,8,8", "7,7,7", "6,6,6", "hbhbhb", "5,5,5", "4,4,4", "3,3,3", "h7h7h7", "2,2,2", "1,1,1", "11,11,11"}
    ''' <summary>
    '''   'all赔率所对应的位置
    ''' </summary>
    ''' <remarks></remarks>
    Public iOddsSeat() As Integer = {0, 28, 27, 25, 24, 23, 21, 20, 19, 18, 17, 29}
    ''' <summary>
    '''  '蓝7个数赔率所对应的位置
    ''' </summary>
    ''' <remarks></remarks>
    Public iBlue7OddsSeat() As Integer = {0, 0, 30, 31, 32, 33, 34, 35, 36}
    ''' <summary>
    '''  红7个数赔率所对应的位置
    ''' </summary>
    ''' <remarks></remarks>
    Public iRed7OddsSeat() As Integer = {0, 0, 37, 38, 39, 40, 41, 42, 43}
    ''' <summary>
    ''' 天下个数赔率所对应的位置
    ''' </summary>
    ''' <remarks></remarks>
    Public iWorldOddsSeat() As Integer = {0, 0, 44, 45, 46, 47, 48, 49, 50}
    ''' <summary>
    ''' 比倍时 随机生成的没有比过倍的牌
    ''' </summary>
    ''' <remarks></remarks>
    Private sNotCompareCards(GameMachineCount) As String
    ''' <summary>
    ''' 比倍时 sNotCompareCards的前六张牌，当sNotCompareCards中小于6时取全部
    ''' </summary>
    ''' <remarks></remarks>
    Private sTheFirstSixCards(GameMachineCount) As String
    ''' <summary>
    ''' 继承数据库类的方法 记录错误
    ''' </summary>
    Public RecordError As ToConfigDBclass
    ''' <summary>
    ''' 用户必赢时的逻辑
    ''' </summary>
    ''' <param name="_OddsList">保存中的什么奖项的集合，是全盘其一、还是疯狂送等等</param>
    ''' <param name="_userinfo0">用户</param>
    ''' <remarks></remarks>
    Public Sub MustWin(ByVal _OddsList As List(Of Double), ByVal _userinfo0 As UserClass)
        '保存必中概率之和
        Dim allgl As Double = 0
        '必中概率集合
        Dim bzgl As IList(Of Double) = New List(Of Double)
        For i As Integer = 1 To _OddsList.Count - 1
            '如果该用户的中奖几率不等于0
            If _OddsList(i) <> 0 Then
                '必中概率之和 = 必中概率之和 + 该机台中奖几率
                allgl += CDbl(1 / _OddsList(i))
                '把该必中概率之和 加入到 必中概率集合中
                bzgl.Add(allgl)
            Else
                bzgl.Add(0)
            End If
        Next
        '获取所中的奖项
        Randomize()
        '临时变量  随机大小=随机数*必中概率之和 
        Dim rndx As Double = RandomDouble(1) * allgl
        '循环从必中概率集合中找出 大于随机大小概率的、赋值给 控制按钮
        For i As Integer = 0 To bzgl.Count - 1
            '如果随机大小的概率 小于必中概率集合中的某一元素
            If rndx < bzgl(i) Then
                '控制中3个连线的按钮 = i+1
                PushButton = i + 1
                Exit For
            End If
        Next
        '获取中奖数据，九个图片的代号  （玩家分区，押注分数，分区参数）
        _userinfo0.WinData = WinData(_userinfo0.MachineNumber, _userinfo0.BetNumeber, Subregion)
        '判断该玩家是否中全盘
        IfOverAll(_userinfo0)
    End Sub

    ''' <summary>
    ''' 用户必输的逻辑
    ''' </summary>
    ''' <returns>返回未中奖的字符串</returns>
    ''' <remarks></remarks>
    Public Function MustLost()
        Dim user3 As String = String.Empty
        Dim num As Integer = 0
        '获取未中奖的字符串
        user3 = NotWinStr() & "%"
        '将未中奖的字符串拆分成数组
        Dim xz() As String = user3.Split("%")
        '如果未中奖数组的1元素不等于空，就一直执行内部操作
        While xz(1) <> ""
            '执行一次就记录一次
            num += 1
            '避免出现死循环
            If num > 8 Then
                '   GameSocket.errorjl("MustLost必输111" & user3)
                '返回必输的 九个位置图片
                Return "2,8,1,8,10,5,4,6,7%"
            End If
            ' GameSocket.errorjl("MustLost必输" & user3)
        End While
        '返回未中奖的字符串
        Return user3
    End Function
    ''' <summary>
    ''' 疯狂送中奖 强制中全盘的函数
    ''' </summary>
    ''' <param name="_WinType">1/2/3 中奖的类型 1红七 2蓝七 3天下</param>
    ''' <returns>返回数据（疯狂送全盘奖的参数,下注分数,排除的奖项）</returns>
    ''' <remarks></remarks>
    Public Function CrazySendOverAll(ByVal _WinType As Integer) As String
        Dim stra As List(Of String) = New List(Of String)
        '判断参数zjlx，
        Select Case _WinType
            'all红7
            Case 1
                '声明一个，【奖项所占格子数，奖项序号，赔率】
                Dim hh7() As String = {"9,28,300"}
                '把参数数组 加入到stra集合中
                stra.AddRange(hh7)
                'all蓝7
            Case 2
                Dim ll7() As String = {"9,27,200"}
                stra.AddRange(ll7)
                'all天下
            Case 3
                Dim ttx() As String = {"9,29,500"}
                stra.AddRange(ttx)
        End Select
        '获取疯狂送全盘奖的参数
        Dim tj As String = stra(RandomInt(0, stra.Count))
        Dim lista As List(Of String) = New List(Of String)
        '将参数加入临时变量lista集合中
        lista.Add(tj)
        '声明一个存储排除其他奖项的字符串
        Dim pc As String = ""
        '判断是什么全盘奖
        Select Case _WinType
            Case 1 '红7奖项
                pc = 2
            Case 2 '蓝7
                pc = 1
        End Select
        '过滤生成最终数据（疯狂送全盘奖的参数,下注分数,排除的奖项）
        Return FilterData(lista, 8, pc)
    End Function
    ''' <summary>
    ''' 普通中奖返回数据
    ''' </summary>
    ''' <param name="_GameSubarea">分区号</param>
    ''' <param name="_GameBill">下注数量</param>
    ''' <param name="_SubareaParameter">分区参数</param>
    ''' <returns>返回数据（中奖内容，下注分数，""）</returns>
    ''' <remarks></remarks>
    Public Function WinData(ByVal _GameSubarea As Integer, ByVal _GameBill As Integer, ByVal _SubareaParameter(,) As String) As String
        '记录中奖内容：奖项所占格子数，奖项序号，赔率
        Dim zjnr As New List(Of String)
        '是否有强制中奖
        Select Case PushButton
            Case 0
                Dim listjl As List(Of String) = New List(Of String)
                listjl.Add(10)
                '循环获得该玩家分区的所有的奖项赔率
                For i As Integer = 6 To 55
                    listjl.Add(_SubareaParameter(_GameSubarea - 1, i))
                Next
                '连线中奖
                Dim bool As Boolean = False
                For i As Integer = 1 To 15
                    If bool AndAlso i < 4 Then Continue For
                    '如果获取的随机数等于1 （0-所有奖项的赔率）
                    If RandomInt(0, listjl(i)) = 1 Then
                        '往新集合中加入  奖项所占格子数，奖项序号，赔率
                        zjnr.Add(iCellNum(i) & "," & i & "," & iAwardOdds(i))
                        '通过该变量确定中了一个或两个樱桃时则不再中其他连线
                        If i < 4 Then bool = True
                    End If
                Next
                '必须下注大于等于8注才有可能中ALL奖和多红蓝7天下奖
                If _GameBill >= 8 Then
                    For i As Integer = 16 To 50
                        '如果获取的随机数等于1（0-所有奖项的赔率）
                        If RandomInt(0, listjl(i)) = 1 Then
                            '往新集合中加入  奖项所占格子数，奖项序号，赔率
                            zjnr.Add(iCellNum(i) & "," & i & "," & iAwardOdds(i))
                        End If
                    Next
                End If
                '判断是否有中奖
                If zjnr.Count <> 0 Then
                    '有中奖后把大奖提前
                    For i As Integer = 0 To zjnr.Count - 1
                        For j As Integer = i + 1 To zjnr.Count - 1
                            '如果第一个中奖元素的，所占格子数小于第二个中奖元素所占格子数
                            If CInt(Microsoft.VisualBasic.Left(zjnr(i), 1)) < CInt(Microsoft.VisualBasic.Left(zjnr(j), 1)) Then
                                '把前后两个中奖元素调换位置
                                Dim temp As String = zjnr(i)
                                zjnr(i) = zjnr(j)
                                zjnr(j) = temp
                            End If
                        Next
                    Next
                    '并把多余奖项删除()
                    Dim wzsl As Integer = 9
                    For i As Integer = 0 To zjnr.Count - 1
                        '如果中奖元素的所占格子数量小于等于9个
                        If wzsl >= CInt(Microsoft.VisualBasic.Left(zjnr(i), 1)) Then
                            '把位置数量重新赋值 = 9 -中奖元素的格子数量
                            wzsl = wzsl - CInt(Microsoft.VisualBasic.Left(zjnr(i), 1))
                        Else '否则把此中奖元素清空
                            zjnr(i) = ""
                        End If
                    Next
                End If
            Case Else
                zjnr = New List(Of String)
                '记录中奖内容：奖项所占格子数，奖项序号，奖项赔率
                Dim str As String = iCellNum(PushButton) & "," & PushButton & "," & iAwardOdds(PushButton)
                zjnr.Add(str)
        End Select
        '过滤生成最终数据（中奖内容，下注分数，""）
        Return FilterData(zjnr, _GameBill, "")
    End Function
    ''' <summary>
    ''' 过滤生成最终数据
    ''' </summary>
    ''' <param name="_AwardParameters">奖项所占格子数，奖项序号，赔率</param>
    ''' <param name="_GameBill">下注分数</param>
    ''' <param name="_Remove">排除的奖项</param>
    ''' <returns>返回数据 msg ： 九个位置图片代号 ， 中奖图片赔率所对应的位置|10+中奖图片的个数|中奖图片个数的赔率 "%" </returns>
    ''' <remarks></remarks>
    Function FilterData(ByVal _AwardParameters As List(Of String), ByVal _GameBill As Integer, ByVal _Remove As String) As String
        '格子数组
        Dim _sCell() As String = {"", "", "", "", "", "", "", "", "", ""}
        '如果传入的参数zjnr集合长度为0
        If _AwardParameters.Count = 0 Then
            '就返回一个未中奖的字符串
            Return NotWinStr() & "%"
        Else
            '如果传入zjnr集合有元素，就循环所有元素，存入到临时变量aa数组中
            For i As Integer = 0 To _AwardParameters.Count - 1
                '如果元素不为空，就将集合中元素拆分存入到aa数组中
                If _AwardParameters(i) <> "" Then
                    Dim aa() As String = _AwardParameters(i).Split(",")
                    '数组内添加字符串
                    _sCell = ArrayAddStr(_sCell, aa, _GameBill)
                End If
            Next
        End If
        '循环9个格
        For i As Integer = 1 To 9
            '如果某个格子为空字符串
            If _sCell(i) = "" Then
                '声明一个保存字符串
                Dim bc As String = ""
                '循环8条连线
                For j As Integer = 0 To 7
                    '判断连线是否包含当前格子
                    If sLineRank(j).Contains(i) Then
                        '如果中奖连线包含此格子，就将这条连线的三个位置拆分成数组，存入到lf数组
                        Dim lf() As String = sLineRank(j).Split(",")
                        Dim ls(3) As Integer
                        '3个B的判断
                        For k As Integer = 0 To 2
                            '判断中奖的三个位置上对应的图片是什么（大B、中B、小B）、【九个位置数组循环（中奖的三个位置）】 是哪个B，就将对应的数量加1
                            Select Case _sCell(lf(k))
                                Case "3"
                                    ls(0) += 1
                                Case "4"
                                    ls(1) += 1
                                Case "5"
                                    ls(2) += 1
                            End Select
                        Next
                        '符合三个B的判断排除掉 / 如果 中B + 小B的数量 =2、或者 大B + 中B的数量 = 2 。就将排除字符串赋值3,4,5
                        If (ls(1) + ls(2) = 2) OrElse (ls(1) + ls(0) = 2) OrElse (ls(0) + ls(2)) Then bc = bc & ",3,4,5"
                    End If
                Next
                '水果篮子的判断
                Dim iInterimNum As Integer = 0
                '循环九个位置，如果有位置为空,就将临时数量加1
                For j As Integer = 1 To 9
                    If _sCell(j) = "" Then
                        iInterimNum += 1
                    End If
                Next
                '当是最后一个空格子的时候
                If iInterimNum = 1 Then
                    '把9个格子的数组转化字符串
                    Dim str As String = ArrayChangeStr(_sCell)
                    '把樱桃替换为A,方便判断
                    str = str.Replace("10", "a")
                    '判断是否全是水果
                    If str.Contains("1") = False AndAlso str.Contains("2") = False AndAlso str.Contains("3") = False AndAlso str.Contains("4") = False AndAlso str.Contains("5") = False Then
                        '如果全是水果,随即生成的格子不能是水果
                        bc = bc & ",6,7,8,9,10"
                    End If
                End If
                '产生随机数
                Select Case i
                    Case 1, 2, 3, 4, 7
                        '获取的随机数图片，存入到该位置上
                        _sCell(i) = ProduceRandom(ArrayChangeStr(_sCell) & ",10" & bc & "," & _Remove)
                    Case Else
                        _sCell(i) = ProduceRandom(ArrayChangeStr(_sCell) & bc & "," & _Remove)
                End Select
            End If
        Next
        Dim listz As List(Of String) = New List(Of String)
        '混B混7天下的判断
        Dim bbc As String = Mix7MixBarWorld(_sCell)
        ' bbc -> 40|15|25"  中奖图片赔率所对应的位置|10+中奖图片的个数|中奖图片个数的赔率
        If bbc <> "" Then
            '将混B混7天下的参数存入到集合listz
            listz.Add(bbc)
            '如果得到的是九个格子混7
            If bbc.Split("%")(0).Split("|")(1) = "18" Then
                For i As Integer = 0 To 7
                    '8条连线的排列 拆分成数组aa， 三个元素，对应3个位置
                    Dim aa() As String = sLineRank(i).Split(",")
                    '传入的参数（第一条连线的第一个位置的图片代号 + 第二个位置图片代号 + 第三个位置的图片代号，记录是第几条中奖线，false）
                    '返回值bb =  中奖图片赔率所对应的位置|第几条中奖线|该奖项的赔率          
                    Dim bb As String = OnLineIfWin(_sCell(aa(0)) + "," + _sCell(aa(1)) + "," + _sCell(aa(2)), i + 1, False)
                    '如果 bb数组的第一个元素不等于0，即奖项的概率不等于0，就把该字符串加入listz集合
                    If bb.Split("|")(0) <> "0" Then
                        listz.Add(bb)
                    End If
                Next
            Else
                '如果得到的是九个格子混B，水果篮
                If bbc = (16 & "|19|" & iAwardOdds(16)) OrElse bbc.Split("%")(0).Split("|")(1) <> "19" Then
                    For i As Integer = 0 To 7
                        ' '8条连线的排列 拆分成数组aa， 三个元素，对应3个位置
                        Dim aa() As String = sLineRank(i).Split(",")
                        '传入的参数（第一条连线的第一个位置的图片代号 + 第二个位置图片代号 + 第三个位置的图片代号，记录是第几条中奖线，false）
                        '返回值bb = 中奖图片赔率所对应的位置|第几条中奖线|该奖项的赔率
                        Dim bb As String = OnLineIfWin(_sCell(aa(0)) + "," + _sCell(aa(1)) + "," + _sCell(aa(2)), i + 1, True)
                        '如果 bb数组的第一个元素不等于0，即奖项的概率不等于0，就把该字符串加入listz集合
                        If bb.Split("|")(0) <> "0" Then
                            listz.Add(bb)
                        End If
                    Next
                End If
            End If
        Else '如果不是混B混7天下
            For i As Integer = 0 To 7
                '8条连线的排列 拆分成数组aa， 三个元素，对应3个位置
                Dim aa() As String = sLineRank(i).Split(",")
                '传入的参数（第一条连线的第一个位置的图片代号 + 第二个位置图片代号 + 第三个位置的图片代号，记录是第几条中奖线，false）
                '返回值bb =  中奖图片赔率所对应的位置|第几条中奖线|该奖项的赔率          
                Dim bb As String = OnLineIfWin(_sCell(aa(0)) + "," + _sCell(aa(1)) + "," + _sCell(aa(2)), i + 1, True)
                '如果 bb数组的第一个元素不等于0，即奖项的概率不等于0，就把该字符串加入listz集合
                If bb.Split("|")(0) <> "0" Then
                    listz.Add(bb)
                End If
            Next
        End If
        '将九个位置图片代号存入msg字符串
        Dim msg As String = ArrayChangeStr(_sCell)
        '循环listz数组中的元素  中奖图片赔率所对应的位置|10+中奖图片的个数|中奖图片个数的赔率
        For i As Integer = 0 To listz.Count - 1
            '如果下注分数大于等于8
            If _GameBill >= 8 Then
                'msg重新赋值 = 九个位置图片代号 ， 中奖图片赔率所对应的位置|10+中奖图片的个数|中奖图片个数的赔率
                msg = msg & "%" & listz(i)
            Else '如果下注分数小于8
                '如果下注的分数 大于等于 10+中奖图片的个数
                If listz(i).Split("|")(1) <= _GameBill Then
                    'msg重新赋值 = 九个位置图片代号 ， 中奖图片赔率所对应的位置|10+中奖图片的个数|中奖图片个数的赔率
                    msg = msg & "%" & listz(i)
                End If
            End If
        Next
        Return msg & "%"
    End Function
    ''' <summary>
    ''' 疯狂送中奖返回数据 
    ''' </summary>
    ''' <param name="_CanWin">0或1 0是不可以中奖ALL 1是可以中奖ALL</param>
    ''' <param name="_WinType">1/2/3 中奖的类型 1红七 2蓝七 3天下</param>
    ''' <param name="_SeveralCell">1-9 要中几个</param>
    ''' <returns> '返回数据：中几个，什么奖，倍数，8，排除的奖项</returns>
    ''' <remarks></remarks>
    Public Function CrazySendData(ByVal _CanWin As Integer, ByVal _WinType As Integer, ByVal _SeveralCell As Integer) As String
        '对应红7奖项的倍数
        Dim _iRed7Multiple As Integer = 0
        '蓝7奖项的倍数
        Dim _iBlue7Multiple As Integer = 0
        '天下奖项的倍数
        Dim _iWorldMultiple As Integer = 0
        Dim stra As List(Of String) = New List(Of String)
        ' 根据中了几个图片，来给定不同的奖项倍数
        Select Case _SeveralCell
            Case 2
                _iRed7Multiple = 10
                _iBlue7Multiple = 5
                _iWorldMultiple = 20
            Case 3
                _iRed7Multiple = 15
                _iBlue7Multiple = 10
                _iWorldMultiple = 30
            Case 4
                _iRed7Multiple = 20
                _iBlue7Multiple = 15
                _iWorldMultiple = 40
            Case 5
                _iRed7Multiple = 25
                _iBlue7Multiple = 20
                _iWorldMultiple = 50
            Case 6
                _iRed7Multiple = 40
                _iBlue7Multiple = 30
                _iWorldMultiple = 60
            Case 7
                _iRed7Multiple = 50
                _iBlue7Multiple = 40
                _iWorldMultiple = 70
            Case 8
                _iRed7Multiple = 60
                _iBlue7Multiple = 50
                _iWorldMultiple = 80
                '如果是中了9个图片，全盘奖
            Case 9
                '判断是红7，还是蓝7、天下
                Select Case _WinType
                    Case 1 'all红7
                        Dim hh7() As String = {"2,37,10", "3,38,15", "4,39,20", "5,40,25", "6,41,40", "7,42,50", "8,43,60", "9,28,300"}
                        '将指定集合的元素添加到 stra集合的末尾。
                        stra.AddRange(hh7)
                    Case 2 'all蓝7
                        Dim ll7() As String = {"2,30,5", "3,31,10", "4,32,15", "5,33,20", "6,34,30", "7,35,40", "8,36,50", "9,27,200"}
                        stra.AddRange(ll7)
                    Case 3 'all天下
                        Dim ttx() As String = {"2,44,20", "3,45,30", "4,46,40", "5,47,50", "6,48,60", "7,49,70", "8,50,80", "9,29,500"}
                        stra.AddRange(ttx)
                End Select
        End Select
        '声明红7、蓝7、天下的集合（ 图片中几个，什么奖，倍数）
        Dim h7() As String = {_SeveralCell & "," & _SeveralCell + 35 & "," & _iRed7Multiple}
        Dim l7() As String = {_SeveralCell & "," & _SeveralCell + 28 & "," & _iBlue7Multiple}
        Dim tx() As String = {_SeveralCell & "," & _SeveralCell + 42 & "," & _iWorldMultiple}
        '参数kezj 0是不可以中奖ALL 1是可以中奖ALL
        If _CanWin = 0 Then
            '如果不是全盘奖，判断中奖的类型 1红七 2蓝七 3天下
            Select Case _WinType
                Case 1 '红73-8
                    '将指定集合的元素添加到 stra集合的末尾。 图片中几个, 什么奖, 倍数
                    stra.AddRange(h7)
                    If stra.Contains("9,28,300") Then
                        '包含全盘则将其移除
                        stra.Remove("9,28,300")
                    End If
                Case 2 '蓝73-8
                    stra.AddRange(l7)
                    If stra.Contains("9,27,200") Then
                        stra.Remove("9,27,200")
                    End If
                Case 3 '天下3-8
                    stra.AddRange(tx)
                    If stra.Contains("9,29,500") Then
                        stra.Remove("9,29,500")
                    End If
            End Select
        End If
        ' 将所得到奖项（中几个，什么奖，倍数）添加临时变量lista集合
        Dim tj As String = stra(RandomInt(0, stra.Count))
        Dim lista As List(Of String) = New List(Of String)
        lista.Add(tj)
        Dim pc As String = ""
        '判断中的红7还是蓝7
        Select Case _WinType
            Case 1 '红7奖项
                pc = 2
            Case 2 '蓝7奖项
                pc = 1
        End Select
        '返回中几个，什么奖，倍数，8，排除的奖项
        Return FilterData(lista, 8, pc)
    End Function
    ''' <summary>
    '''  未中奖的字符串生成
    ''' </summary>
    ''' <returns>返回str：未中奖的九个位置上的图片代号</returns>
    ''' <remarks></remarks>
    Public Function NotWinStr() As String
        '声明的_lLocationImage用来存入 未中奖的九个位置图片字符串
        Dim _lLocationImage As New List(Of String)
        _lLocationImage.Add(99)
        Dim l7_b As Boolean = False
        Dim h7_b As Boolean = False
        Dim tian_b As Boolean = False
        Dim xb As Integer = 0
        ' 用来存储不中奖的位置
        Dim bd() As String = {"", "", "", "1,2", "", "", "4,5", "1,4,5,3", "2,5", "1,5,3,6,7,8"}
        '用来设置未出现过对应位置图片的开关
        Dim bda() As Boolean = {False, False, False, False, False, False, False, False, False, False}
        '用来保存不是水果图片的数组
        Dim bdb() As String = {"1", "2", "11", "3", "4", "5"}
        '用来保存全是水果图片的数组
        Dim bdc() As String = {"6", "7", "8", "9", "10"}
        '循环九次，每次得到一个位置上的图片
        For i As Integer = 1 To 9
            '用来存储图片的代号
            Dim stra As String = ""
            '如果出现蓝7，就在字符串中存入蓝7的图案代号，并把开关改为true
            If h7_b Then stra = stra + ",1" : bda(0) = True
            '如果出现红，就在字符串中存入红7的图案代号，并把开关改为true
            If l7_b Then stra = stra + ",2" : bda(1) = True
            '如果出现天下，就在字符串中存入天下的图案代号，并把开关改为true
            If tian_b Then stra = stra + ",11" : bda(2) = True
            '如果小B出现的次数=2  stra=stra + ",3,4,5" （小B，中B，大B）并把对应的开关都改为true
            If xb = 2 Then stra = stra + ",3,4,5" : bda(3) = True : bda(4) = True : bda(5) = True
            '如果是下边的这几次循环，就在stra后边加上10 的图片代号
            Select Case CInt(i)
                Case 1, 2, 3, 4, 7
                    stra = stra + ",10"
            End Select
            ' 如果循环的位置字符串不为空
            If bd(i) <> "" Then
                '就将此位置字符串拆分成数组
                Dim bb() As String = bd(i).Split(",")
                '循环将位置字符串中元素 对应的_lLocationImage集合中的图案代号 得出来，加进stra中
                For j As Integer = 0 To bb.Length - 1
                    stra = stra + "," + _lLocationImage.Item(CInt(bb(j))) 'Item 获取或设置位于指定索引处的元素。
                Next
            End If
            '随机出来的图案代号是，从上边stra变量里得到的所有的图案代号中随机出来的
            stra = ProduceRandom(stra)
            '随机出一个图案代号  是哪个图片就给哪个图片数量加一
            Select Case stra
                Case "1"
                    h7_b = True
                Case "2"
                    l7_b = True
                Case "3", "4", "5"
                    xb += 1
                Case "11"
                    tian_b = True
            End Select
            '把最后随机出来的图片代号加入集合_lLocationImage中
            _lLocationImage.Add(stra)
            '当循环到第九次
            If i = 9 Then
                For y As Integer = 1 To 9
                    '如果这五个图片不包含，_lLocationImage中得到的九个位置上的图片就退出循环（第一个不包含就直接退出了）
                    If bdc.Contains(_lLocationImage(y)) = False Then
                        Exit For
                    End If
                    '如果前八个图片是都 6.7.8.9.10 都是水果
                    If y = 9 Then
                        For x As Integer = 0 To 5
                            If bda(x) = False Then
                                _lLocationImage(RandomInt(1, 10)) = bdb(x)
                                Exit For
                            End If
                        Next
                    End If
                Next
            End If
        Next

        '声明一个临时变量用来储存未中奖字符串 
        Dim str As String = ""
        '循环将最后得到的九个位置上的图片代号，依次存入str字符串中返回
        For i As Integer = 1 To _lLocationImage.Count - 1
            If str = "" Then
                str = _lLocationImage.Item(i)
            Else
                str = str & "," & _lLocationImage.Item(i)
            End If
        Next
        Return str
    End Function
    ''' <summary>
    ''' 产生一个随机数(不包括括号内的)
    ''' </summary>
    ''' <param name="_str">该随机数不包括的数字</param>
    ''' <returns>返回一个随机数</returns>
    ''' <remarks></remarks>
    Function ProduceRandom(ByVal _str As String) As String
        Dim i As String
        Dim bb() As String = _str.Split(",")
        Dim aa As New List(Of String)
        For j As Integer = 1 To 11
            aa.Add(j)
        Next
        For j As Integer = 0 To bb.Length - 1
            aa.Remove(bb(j))
        Next
        i = RandomInt(0, aa.Count - 1)
        Return aa(i)
    End Function
    ''' <summary>
    ''' 数组转字符串
    ''' </summary>
    ''' <param name="_sList">一个数组：九个位置上的图片代号</param>
    ''' <returns>九个位置图片代号的字符串</returns>
    ''' <remarks></remarks>
    Function ArrayChangeStr(ByVal _sList() As String) As String
        Dim str As String = ""
        '循环将数组中的每一个元素相加，然后赋值给str返回
        For i As Integer = 0 To _sList.Length - 1
            If str = "" Then
                str = _sList(i)
            Else
                str = str & "," & _sList(i)
            End If
        Next
        Return str
    End Function
    ''' <summary>
    ''' 数组内添加字符串
    ''' </summary>
    ''' <param name="_str">字符串容器</param>
    ''' <param name="_sParameter">奖项所占格子数，奖项序号，赔率</param>
    ''' <param name="_bottom">押注倍数</param>
    ''' <returns>返回参数_str容器数组，里边所对应的奖项位置上已经赋值中奖图片代号</returns>
    ''' <remarks></remarks>
    Function ArrayAddStr(ByVal _str() As String, ByVal _sParameter() As String, ByVal _bottom As Integer) As String()
        '获取奖项的图片代号
        Dim aa() As String = sPrizePicture(_sParameter(1)).Split(",")
        '声明一个新的集合
        Dim list As List(Of String) = New List(Of String)
        '用来判断八条中奖线的三个位置是否都为空
        Dim cc() As Boolean = {False, False, False, False, False, False, False, False}
        '循环判断八条中奖线的三个位置是否全为空，若为空就把该条中奖线对应的cc数组改为flase，即不是该条线中奖
        For i As Integer = 0 To sLineRank.Length - 1
            '获取该中奖线的位置字符串，拆分成数组
            Dim dd() As String = sLineRank(i).Split(",")
            '判断如果三个位置都为空，对应的cc数组状态就为false，反之改为true
            If Str(dd(0)) = "" AndAlso Str(dd(1)) = "" AndAlso Str(dd(2)) = "" Then
                cc(i) = False
            Else
                cc(i) = True
            End If
        Next
        Dim lb As Integer
        '判断传入的参数中，奖项序号如果小于16，就不是全盘奖，根据对应奖项进入相应代码块
        If CInt(_sParameter(1)) < 16 Then
            '--------------------下列每个方法块都是同理，最终都是将得到的奖项位置赋值 对应中奖的图案代号，然后加入list集合中，参考 Case2的注释------------------------
            Select Case CInt(_sParameter(1))
                '单个樱桃
                Case 1
                    list = DeleteNonEmptyCell(_str, sOneCherrySeat)
                    If list.Count = 0 Then Exit Select
                    _str(list(RandomInt(0, list.Count))) = "10"
                    '两个樱桃
                Case 2
                    'list集合 = 奖项的位置集合
                    list = DeleteNonEmptyCell(_str, sTwoCherrySeat)
                    '如果奖项位置的集合为空，就退出select
                    If list.Count = 0 Then Exit Select
                    '根据集合的长度，随机出一个数，得到该奖项位置的字符串，并拆分成数组
                    Dim ls() As String = list(RandomInt(0, list.Count)).Split(",")
                    '循环将得到的奖项位置字符串容器_str中，赋值对应中奖的图案代号
                    For i As Integer = 0 To ls.Length - 1
                        _str(ls(i)) = "10"
                    Next
                    '三个樱桃
                Case 3
                    Dim bb11() As String = sLineRank.Clone
                    bb11(0) = ""
                    bb11(1) = ""
                    bb11(2) = ""
                    list = DeleteNonEmptyCell(_str, bb11)
                    If list.Count = 0 Then Exit Select
                    Dim ls() As String = list(RandomInt(0, list.Count)).Split(",")
                    For i As Integer = 0 To ls.Length - 1
                        _str(ls(i)) = "10"
                    Next
                    '混合B
                Case 8
                    Dim sbb() As String = sMixBarRank(RandomInt(0, sMixBarRank.Count)).Split(",")
                    list = DeleteNonEmptyCell(_str, sLineRank)
                    If list.Count = 0 Then Exit Select
                    Dim ls() As String = list(RandomInt(0, list.Count)).Split(",")
                    For i As Integer = 0 To ls.Length - 1
                        _str(ls(i)) = sbb(i)
                    Next
                    '混合7
                Case 12
                    Dim sbb() As String = sMix7Rank(RandomInt(0, sMix7Rank.Count)).Split(",")
                    list = DeleteNonEmptyCell(_str, sLineRank)
                    If list.Count = 0 Then Exit Select
                    Dim ls() As String = list(RandomInt(0, list.Count)).Split(",")
                    For i As Integer = 0 To ls.Length - 1
                        _str(ls(i)) = sbb(i)
                    Next
                    '其他
                Case Else
                    list = DeleteNonEmptyCell(_str, sLineRank)
                    If list.Count = 0 Then Exit Select
                    Dim ls() As String
                    If _bottom > 8 Then
                        _bottom = 8
                    End If
                    Try
                        ls = list(RandomInt(0, _bottom)).Split(",")
                        For i As Integer = 0 To ls.Length - 1
                            _str(ls(i)) = aa(RandomInt(0, aa.Count))
                        Next
                    Catch ex As Exception
                    End Try
            End Select
        Else
            '如果是中的全盘奖，下列全盘奖的方法块都是同理，都是最终将bb数组中的中奖位置代号随机赋值给参数_str字符串容器的九个位置上，即字符串容器_str是九个图片代号， 参考杂B全盘奖的注释
            Select Case CInt(_sParameter(1))
                'ALL奖
                Case 17, 18, 19, 20, 21, 23, 24, 25, 27, 28, 29
                    For i As Integer = 1 To 9
                        lb = aa(RandomInt(0, aa.Count))
                        _str(i) = lb
                    Next
                    'ALL混合7
                Case 26
                    Dim bb() As String = {"", "1", "2"}
                    For i As Integer = 1 To 9
                        lb = aa(RandomInt(0, aa.Count))
                        _str(i) = lb
                    Next
                    For ii As Integer = 1 To 2
                        Dim b() As String = {"", "", "", "", ""}
                        For i As Integer = 1 To 9
                            Select Case Str(i)
                                Case "1"
                                    b(1) = i & b(1)
                                Case "2"
                                    b(2) = i & b(2)
                            End Select
                        Next
                        For i As Integer = 1 To 2
                            If b(i) = "" Then
                                For j As Integer = 1 To 2
                                    If b(j).Split(bb(j)).Length >= 2 Then
                                        _str(b(j).Split(bb(j))(RandomInt(0, b(j).Split(bb(j)).Length))) = bb(i)
                                        Continue For
                                    End If
                                Next
                            End If
                        Next
                        Exit For
                    Next
                    'ALL混合b
                Case 22 '如果是杂B全盘奖
                    '声明一个数组，
                    Dim bb() As String = {"", "3", "4", "5"}
                    '循环9次，每次都随机出一个0到aa长度之间的数，并将随机数赋值给对应的 参数_str字符串容器
                    For i As Integer = 1 To 9
                        lb = aa(RandomInt(0, aa.Count))
                        _str(i) = lb
                    Next
                    For ii As Integer = 1 To 3
                        '声明一个空字符串数组
                        Dim b() As String = {"", "", "", "", ""}
                        '循环9次，每一次都判断参数_str字符串容器对应位置上的 值等于几，进入相应代码块。将该次循环的i和对应b数组中的字符串重新赋值给b数组
                        For i As Integer = 1 To 9
                            Select Case Str(i)
                                Case "3"
                                    b(1) = i & b(1)
                                Case "4"
                                    b(2) = i & b(2)
                                Case "5"
                                    b(3) = i & b(3)
                            End Select
                        Next
                        '循环3次判断，如果b数组中还有元素为空
                        For i As Integer = 1 To 3
                            If b(i) = "" Then
                                '再循环3次
                                For j As Integer = 1 To 3
                                    '再判断按照bb数组的元素拆分b数组，如果长度减1还大于等于2
                                    If b(j).Split(bb(j)).Length - 1 >= 2 Then
                                        '将bb数组中的中奖位置代号随机赋值给参数_str字符串容器的九个位置
                                        _str(b(j).Split(bb(j))(RandomInt(0, b(j).Split(bb(j)).Length))) = bb(i)
                                        '退出for循环
                                        Continue For
                                    End If
                                Next
                            End If
                        Next
                        Exit For
                    Next
                    '其他奖项
                Case Else
                    Dim k As Integer = 0
                    Dim kk As String = ""
                    Select Case CInt(_sParameter(1))
                        '所有"天下"
                        Case 44 To 50
                            kk = "11"
                            '所有"蓝7"
                        Case 30 To 36
                            kk = "2"
                            '所有"红7"
                        Case 37 To 43
                            kk = "1"
                    End Select
                    If kk <> "" Then
                        For i As Integer = 1 To 9
                            If Str(i) = kk Then k += 1
                        Next
                        If k >= CInt(_sParameter(0)) Then
                            Exit Select
                        End If
                    End If
                    list = DeleteNonEmptyCell(_str, sAllSeat)
                    If list.Count = 0 Then Exit Select
                    For i As Integer = 1 To _sParameter(0)
                        Dim j As Integer = RandomInt(0, list.Count)
                        _str(list(j)) = aa(RandomInt(0, aa.Count))
                        list.RemoveAt(j)
                    Next
            End Select
        End If
        '返回参数_str容器数组，里边所对应的奖项位置上已经赋值中奖图片代号
        Return _str
    End Function
    ''' <summary>
    '''  去掉非空的格子
    ''' </summary>
    ''' <param name="stra">字符串容器：九个位置，全为空</param>
    ''' <param name="strb">是奖项的位置数组</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function DeleteNonEmptyCell(ByVal stra() As String, ByVal strb() As String) As List(Of String)
        Dim list As List(Of String) = New List(Of String)
        For i As Integer = 0 To strb.Length - 1
            If strb(i) <> "" Then
                '将参数strb奖项的位置数组中的元素，位置字符串 拆分成数组aa， aa中每一个元素就是一个位置
                Dim aa() As String = strb(i).Split(",")
                '循环判断aa元素中的位置，对应字符串容器的位置 是否为空，不为空就退出函数
                For j As Integer = 0 To aa.Length - 1
                    If stra(aa(j)) <> "" Then
                        Exit For
                    End If
                    '如果得到的aa数组的长度减1  = 循环的j，就把该次循环的i所对应的奖项位置字符串加入list集合中
                    If j = aa.Length - 1 Then
                        list.Add(strb(i))
                    End If
                Next
            End If
        Next
        '奖项的位置集合
        Return list
    End Function
    ''' <summary>
    ''' 8条连线是否中奖的判断
    ''' </summary>
    ''' <param name="_str"> 第一条连线的第一个位置的图片代号 + 第二个位置图片代号 + 第三个位置的图片代号   </param>
    ''' <param name="_Num">'记录是第几条中奖线</param>
    ''' <param name="_Mix7">用来判断是不是混7</param>
    ''' <returns>'返回值 = 中奖图片赔率所对应的位置|第几条中奖线|该奖项的赔率</returns>
    ''' <remarks></remarks>
    Function OnLineIfWin(ByVal _str As String, ByVal _Num As Integer, ByVal _Mix7 As Boolean) As String
        '方便观看
        ' Dim bb() As String = {"1,2,3", "1,4,7", "1,5,9", "4,5,6", "2,5,8", "7,5,3", "7,8,9", "9,6,3"}
        ' Dim n1() As String = {"红7", "蓝7", "大B", "中B", "小B", "西瓜", "葡萄", "苹果", "橘子", "樱桃", "天下"}
        '返回中奖的序号
        Dim ii As String = "0"
        '如果混B的所有排列组合包含此中奖线中，对应的三个图片代号 ii等于8
        If sMixBarRank.Contains(_str) Then ii = 8
        '如果是混7
        If _Mix7 Then
            '如果 混7的所有排列组合包含此中奖线三个位置对应的图片代号  ii=18
            If sMix7Rank.Contains(_str) Then ii = 12
        End If
        '如果 其他连线的排列组合包含此中奖线三个位置对应的图片代号
        If sOtherRank.Contains(_str) Then
            '如果其他连线的排列组合中有一个等于此中奖线三个位置对应的图片代号
            For i As Integer = 0 To sOtherRank.Length - 1
                'ii= 其他连线的排列组合相对应的位置数值 并退出for循环
                If sOtherRank(i) = _str Then
                    ii = i
                    Exit For
                End If
            Next
        End If
        If ii = "0" Then
            '如果这三个位置上包含樱桃的图片
            If _str.Contains("10") Then
                '将此三个位置图片拆分成数组
                Dim yt() As String = _str.Split(",")
                '如果中奖线第一个位置和第二个位置都是樱桃的图片  ii=2
                If yt(0) = "10" AndAlso yt(1) = "10" Then
                    ii = 2
                Else '如果中奖线第一个位置是樱桃的图片 ii=1
                    If yt(0) = "10" Then ii = 1
                End If
            End If
        End If
        ' 返回 ii = 中奖图片赔率所对应的位置|第几条中奖线|该奖项的赔率
        ii = ii & "|" & _Num & "|" & iAwardOdds(ii)
        Return ii
    End Function
    ''' <summary>
    ''' 混B混7天下的判断
    ''' </summary>
    ''' <param name="_str">九个位置的图片代号数组</param>
    ''' <returns> '返回值 = 天下个数赔率所对应的位置|10+天下的个数|天下个数的赔率</returns>
    ''' <remarks></remarks>
    Function Mix7MixBarWorld(ByVal _str() As String) As String
        Dim ii As String = ""
        ' {"红7", "蓝7", "大B", "中B", "小B", "西瓜", "葡萄", "苹果", "橘子", "樱桃", "天下"} 对应下行数组
        '用来记录每个图片出现的次数
        Dim strint() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        '循环九个格子的图片，记录每个图片出现的次数
        For i As Integer = 1 To 9
            strint(_str(i)) += 1
        Next
        '判断哪个图片出现过九次，
        For i As Integer = 1 To 11
            If strint(i) = 9 Then
                '全盘奖 、返回：该全盘奖赔率数组中对应的位置|19|该全盘奖的二批率
                Return iOddsSeat(i) & "|19|" & iAwardOdds(iOddsSeat(i))
            End If
        Next
        '如果三个B的数量加一起等于9个，返回：该全盘奖赔率数组中对应的位置|19|混B全盘奖的赔率
        If (strint(3) + strint(4) + strint(5)) = 9 Then Return 22 & "|19|" & iAwardOdds(22)
        '如果两种7的数量加一起等于9个，返回：该全盘奖赔率数组中对应的位置|18|混7全盘奖的赔率
        If strint(1) <> 0 AndAlso strint(2) <> 0 AndAlso (strint(1) + strint(2) = 9) Then Return 26 & "|18|" & iAwardOdds(26)
        '如果全是水果图片数量加一起等于9个，返回：该全盘奖赔率数组中对应的位置|19|水果盘全盘奖的赔率
        If (strint(6) + strint(7) + strint(8) + strint(9) + strint(10)) = 9 Then Return 16 & "|19|" & iAwardOdds(16)
        '如果红7的数量大于等于2
        If strint(1) >= 2 Then
            '并且ii为空
            If ii = "" Then
                'ii=红7个数赔率所对应的位置|10+红7的个数|红7个数的赔率/以下同此
                ii = iRed7OddsSeat(strint(1)) & "|" & 10 + strint(1) & "|" & iAwardOdds(iRed7OddsSeat(strint(1)))
            Else
                ii = ii & "%" & iRed7OddsSeat(strint(1)) & "|" & 10 + strint(1) & "|" & iAwardOdds(iRed7OddsSeat(strint(1)))
            End If
        End If
        '如果蓝7的数量大于等于2
        If strint(2) >= 2 Then
            '并且ii为空
            If ii = "" Then
                'ii=蓝7个数赔率所对应的位置|10+蓝7的个数|蓝7个数的赔率
                ii = iBlue7OddsSeat(strint(2)) & "|" & 10 + strint(2) & "|" & iAwardOdds(iBlue7OddsSeat(strint(2)))
            Else
                ii = ii & "%" & iBlue7OddsSeat(strint(2)) & "|" & 10 + strint(2) & "|" & iAwardOdds(iBlue7OddsSeat(strint(2)))
            End If
        End If
        '如果天下的数量大于等于2
        If strint(11) >= 2 Then
            '并且ii为空
            If ii = "" Then
                'ii=天下个数赔率所对应的位置|10+天下的个数|天下个数的赔率
                ii = iWorldOddsSeat(strint(11)) & "|" & 10 + strint(11) & "|" & iAwardOdds(iWorldOddsSeat(strint(11)))
            Else
                ii = ii & "%" & iWorldOddsSeat(strint(11)) & "|" & 10 + strint(11) & "|" & iAwardOdds(iWorldOddsSeat(strint(11)))
            End If
        End If
        Return ii
    End Function
    ''' <summary>
    ''' 初始化比倍信息
    ''' </summary>
    ''' <param name="_ThanCard">分区号</param>
    ''' <remarks></remarks>
    Public Sub InitialiseInfo(ByVal _ThanCard As Integer)
        '随机顺序的52张牌
        Dim ZiPaiList As List(Of Integer) = New List(Of Integer)
        '存52张牌
        Dim ZiPaiTemp As List(Of Integer) = New List(Of Integer)
        For i As Int16 = 0 To 51
            ZiPaiTemp.Add(i)
        Next
        '没比过的牌
        Dim yaof As String = ""
        '要返回的字符串
        Dim yaof6 As String = ""
        Dim randomValue As Integer
        Try
            '打乱52张牌的顺序
            For ii As Integer = 0 To 51
                randomValue = RandomInt(0, ZiPaiTemp.Count)
                ZiPaiList.Add(ZiPaiTemp.Item(randomValue))
                ZiPaiTemp.RemoveAt(randomValue)
            Next
        Catch ex As Exception
            RecordError.WriteGameServerErr("SGP", "GameAlgorithmClass->执行bibeiq方法时错误-> 初始化比倍信息 错误:", ex)
        End Try
        ' 将右边已有的字符串，或许是空 即比过的牌拆分成数组
        Dim leng() As String = Split(RightCardStr(_ThanCard), ",")
        '循环比过的牌 所有元素
        For iu As Integer = 0 To leng.Length - 1
            '如果比过的牌 不为空
            If leng(iu) <> Nothing Then
                For ij As Integer = 0 To 51 - iu
                    '如果存储52张牌的变量中，该位置的牌值等于 比过牌的数值
                    If CInt(ZiPaiList(ij)) + 1 = CInt(leng(iu)) Then
                        '将右边显示的已经比过的牌从ZiPaiList中移除
                        ZiPaiList.RemoveAt(ij)
                        Exit For
                    End If
                Next
            End If
        Next
        '将未比过的牌的集合转化为数组
        For ij As Integer = 0 To ZiPaiList.Count - 1
            ' 将没比过的牌依次存入 yaof字符串中
            If yaof = "" Then
                yaof = ZiPaiList(ij) + 1
            Else
                yaof = yaof & "," & (ZiPaiList(ij) + 1)
            End If
        Next
        '获取未比过的牌的前6个
        For ik As Integer = 0 To 5
            If yaof6 <> "" Then
                yaof6 = yaof6 & "," & (ZiPaiList(ik) + 1)
            Else
                yaof6 = ZiPaiList(ik) + 1
            End If
        Next
        '比倍时 随机生成的没有比过倍的牌 = 没比过的牌字符串yaof
        sNotCompareCards(_ThanCard) = yaof & "," '代替左侧字符串
        ' 比倍时 sNotCompareCards的前六张牌，当sNotCompareCards中小于6时取全部
        sTheFirstSixCards(_ThanCard) = yaof6
        '只要从 没比过的牌中 取出来前六张牌的变量不为空，（不足六张时，有多少取多少）
        If sTheFirstSixCards(_ThanCard) <> "" Then
            '将取出来的六张牌拆分成数组
            Dim jixuc() As String = sTheFirstSixCards(_ThanCard).Split(",")
            '将取出来的牌的张数赋值给 sTheFirstSixCards中牌的张数
            CardsNum(_ThanCard) = jixuc.Length
            '如果sTheFirstSixCards中牌的张数大于6张
            If CardsNum(_ThanCard) > 6 Then
                '就赋值sTheFirstSixCards为6张
                CardsNum(_ThanCard) = 6
            End If
        End If
    End Sub

    ''' <summary>
    ''' 返回比倍结果，并将相关左右牌数据进行更新
    ''' </summary>
    ''' <param name="_iMaxMin">1/大，2/小</param>
    ''' <param name="_userinfo0">该用户</param>
    ''' <param name="_iIfLosing">是否必输,1不是必输 ,2是必输</param>
    ''' <returns>0和，1赢，2输</returns>
    ''' <remarks></remarks>
    Function UpdateCardData(ByVal _iMaxMin As Integer, ByVal _userinfo0 As UserClass, ByVal _iIfLosing As Integer) As Integer
        Dim return0 As Integer
        Dim x As Integer = _userinfo0.MachineNumber
        ' 比倍时 随机生成的没有比过倍的牌
        Dim zuo1() As String = sNotCompareCards(x).Split(",")
        '要显示的牌的大小（1-13）
        Dim _iCardMaxMin As Integer
        For ihg As Integer = 0 To zuo1.Length - 1
            If zuo1(ihg) <> Nothing Then
                ' 比倍要显示的牌
                ShowCards(x) = CInt(zuo1(ihg))
                '要显示的牌的大小
                _iCardMaxMin = CInt(zuo1(ihg))
                Exit For
            End If
        Next
        '如果牌大于13
        If _iCardMaxMin > 13 Then
            '将两个数字相除并且仅返回余数。、就把该牌取13的余数
            _iCardMaxMin = _iCardMaxMin Mod 13
            '如果牌等于0，就赋值13
            If _iCardMaxMin = 0 Then
                _iCardMaxMin = 13
            End If
        End If
        ' 如果参数_iIfLosing = 1正常产生，不是必输
        If _iIfLosing = 1 Then
            '判断取余后的 牌的值在什么范围
            Select Case _iMaxMin
                '1押大
                Case 1
                    Select Case _iCardMaxMin
                        Case 7
                            return0 = 0 '和
                        Case 1 To 6
                            return0 = 2 '输
                        Case 8 To 13
                            return0 = 1 '赢
                    End Select
                Case 2 '2押小 
                    Select Case _iCardMaxMin
                        Case 7
                            return0 = 0 '和
                        Case 1 To 6
                            return0 = 1 '赢
                        Case 8 To 13
                            return0 = 2 '输
                    End Select
            End Select
        Else '必输
            Select Case _iMaxMin
                Case 1 '1押大
                    Select Case _iCardMaxMin
                        Case 7 To 13
                            CompareResults(_userinfo0, 1) '转为输
                    End Select
                Case 2 '2押小 
                    Select Case _iCardMaxMin
                        Case 1 To 7
                            CompareResults(_userinfo0, 2) '转为输
                    End Select
            End Select
            return0 = 2 '输
        End If
        '重新整理
        ' 比倍时的右边显示.6个以内 = 右边已有的字符串，或许是空 即比过的牌
        RightShowSix(x) = RightCardStr(x)
        ' 如果为空 ，把zuo11显示的牌加入到右边中、否则就给他加上
        If RightCardStr(x) = "" Then
            RightCardStr(x) = ShowCards(x)
        Else
            RightCardStr(x) = ShowCards(x) & "," & RightCardStr(x) '右+1
        End If
        '把右边的牌拆分
        Dim jian() As String = RightCardStr(x).Split(",")
        '右边要显示的牌，取最新的6张
        If jian.Length > 6 Then
            RightCardStr(x) = ""
            For yj As Integer = 0 To 5
                If RightCardStr(x) = "" Then
                    RightCardStr(x) = jian(yj)
                Else
                    RightCardStr(x) = RightCardStr(x) & "," & jian(yj)
                End If
            Next
        End If
        '将比倍中左边的牌删除1张(该张是中间显示出的牌)
        sNotCompareCards(x) = Replace("," & sNotCompareCards(x), "," & ShowCards(x) & ",", ",")
        '清空比被中左边显示的6张牌
        sTheFirstSixCards(x) = ""
        ' sTheFirstSixCards中牌的张数
        CardsNum(x) = 0
        '对右边显示的6张牌进行重新付值
        For Each i As String In sNotCompareCards(x).Split(",")
            If i <> "" Then
                sTheFirstSixCards(x) &= "," & i '服务器记录的实际牌
            End If
        Next
        '删除多余
        sTheFirstSixCards(x) = Right(sTheFirstSixCards(x), Len(sTheFirstSixCards(x)) - 1)
        CardsNum(x) = sTheFirstSixCards(x).Split(",").Length - 1
        If CardsNum(x) > 6 Then
            CardsNum(x) = 6
        End If
        '当比倍赢的次数超过52张牌限制，则需要重新对右边的牌进行付值，值为客户端显示的最后6张 
        If sNotCompareCards(x) = "" Then RightCardStr(x) = RightShowSix(x)
        Return return0
    End Function
    ''' <summary>
    ''' 后加的比倍结果,让玩家必比必输，押大给小，押小给大
    ''' </summary>
    ''' <param name="userinfo0"></param>
    ''' <param name="ria"></param>
    ''' <remarks></remarks>
    Sub CompareResults(ByVal userinfo0 As UserClass, ByVal ria As Integer)
        Dim xindea As String = "1,2,3,4,5,6,14,"
        Dim xindeb As String = "39,47,48,49,50,51,52,"
        Dim xinde As String = ""
        If ria = 1 Then
            xinde = xindea
        Else
            xinde = xindeb
        End If
        If RightCardStr(userinfo0.MachineNumber) <> "" Then
            Dim pqw() As String = RightCardStr(userinfo0.MachineNumber).Split(",")
            '只取6个值
            For yji As Integer = 0 To pqw.Length - 1
                If xinde <> "" Then
                    Dim pqs() As String = xinde.Split(",")
                    For yjj As Integer = 0 To pqs.Length - 1
                        If pqw(yji) = pqs(yjj) Then
                            xinde = Replace(xinde, pqs(yjj) & ",", "")
                        End If
                    Next
                End If
            Next
        End If
        ShowCards(userinfo0.MachineNumber) = xinde.Split(",")(0)
    End Sub
#End Region
End Class
