

Public Class DatabaseInfoClass
    ''' <summary>
    ''' 座位号
    ''' </summary>
    Public SeatNumber As Integer
    ''' <summary>
    ''' 各分区总赢次数
    ''' </summary>
    ''' <remarks></remarks>
    Public TotalWinNum(GameMachineCount) As Integer
    ''' <summary>
    ''' 游戏输赢百分比，用于 总赢积分/总押注积分>百分比 必输...
    ''' </summary>
    ''' <remarks></remarks>
    Public Percentage(GameMachineCount) As Double
    ''' <summary>
    ''' 各分区总押注积分
    ''' </summary>
    ''' <remarks></remarks>
    Public TotalBet(GameMachineCount) As Double
    ''' <summary>
    ''' 各分区总赢积分
    ''' </summary>
    ''' <remarks></remarks>
    Public TotalWin(GameMachineCount) As Double

    ''' <summary>
    ''' 查询数据库所有机台参数信息，并给变量赋值
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SelectAspDbThread() 'Optional ByVal _boo As Boolean = True
        '返回本游戏所有字段的参数信息表
        Dim _AllInfo As DataTable = ToDataBank.SelectNewDate()
        '循环获取每个字段的信息 赋值给变量 来使用
        For i As Integer = 0 To _AllInfo.Rows.Count - 1
            Dim ii As Integer = i + 1
            'If _boo = False Then
            '获取个分区总押次数
            BetNum(ii) = CDbl(_AllInfo.Rows(i).Item("_iTotalBettingNum"))
                '获取各分区总赢次数
                TotalWinNum(ii) = CDbl(_AllInfo.Rows(i).Item("_iTotalWinNum"))

                '获取當前中全盤積分的記錄
                OverallRecord(ii) = _AllInfo.Rows(i).Item("_nOverallScoreRecord")
                '获取蓝7，红7，天下彩金
                Blue7Money = CDbl(_AllInfo.Rows(i).Item("_fWholePrizeBlue7"))
                Red7Money = CDbl(_AllInfo.Rows(i).Item("_fWholePrizeRed7"))
                WorldMoney = CDbl(_AllInfo.Rows(i).Item("_fWholePrizeTX"))
            ' End If
            Try
                '如果游戏输赢百分比 不等于数据库中获取的百分比
                If Percentage(ii) <> CDbl(_AllInfo.Rows(i).Item("_fLoseWinRatio")) Then
                    '添加或修改 中奖记录 信息
                End If
                '总计赌注
                TotalBet(ii) = CDbl(_AllInfo.Rows(i).Item("_fTotalWinBetting"))
                '各分区总赢积分
                TotalWin(ii) = CDbl(_AllInfo.Rows(i).Item("_fTotalWinIntegral"))
                '对应樱桃连线以上的奖项是否开放 1开放 0不开放
                CherryIfOpen(ii) = CInt(_AllInfo.Rows(i).Item("_tWhetheOpenYT"))
                ' 游戏输赢百分比，用于 总赢积分/总押注积分>百分比 必输...
                Percentage(ii) = CDbl(_AllInfo.Rows(i).Item("_fLoseWinRatio"))
                '获取蓝7，红7，天下奖金抽分
                Blue7Bonus(ii) = CDbl(_AllInfo.Rows(i).Item("_iTotalIntegraBlue7"))
                Red7Bonus(ii) = CDbl(_AllInfo.Rows(i).Item("_fTotalIntegraRed7"))
                WorldBonus(ii) = CDbl(_AllInfo.Rows(i).Item("_fTotalIntegraTX"))
                '获取是否开放蓝7，红7，天下、的状态
                IfOpenBlue7All(ii) = CDbl(_AllInfo.Rows(i).Item("_iWhethOpenBlue7"))
                IfOpenRed7All(ii) = CDbl(_AllInfo.Rows(i).Item("_iWhethOpenRed7"))
                IfOpenWorldAll(ii) = CDbl(_AllInfo.Rows(i).Item("_iWhethOpenTX"))
                '获取强制中奖信息
                ForceWin(ii) = _AllInfo.Rows(i).Item("_nCompelWinInfo")
                '获取是否疯狂送樱桃，水果盘，小B
                IfOpenCrazySendCherry(ii) = CDbl(_AllInfo.Rows(i).Item("_iWhethOpenYT"))
                IfOpenCrazySendBar(ii) = CDbl(_AllInfo.Rows(i).Item("_iWhethOpenXb"))
                IfOpenCrazySendFruitDish(ii) = CDbl(_AllInfo.Rows(i).Item("_iWhethOpenSGL"))
                '更新VIP
                VipParameter = _AllInfo.Rows(i).Item("_nAutoUpdateVIP")
                '获取数据库中vip参数，并开始更新vip
                '-----------------------------------------------------*****--------------------------
                ' StartVIP(CStr(_AllInfo.Rows(i).Item("_nKzvip")))
                '获取设置会员限制参数信息
                UsersLimit = _AllInfo.Rows(i).Item("_nLimitUsersRecord")
                '获取蓝7，红7，天下的积分起始分数
                Blue7StartIntegral = CDbl(_AllInfo.Rows(i).Item("_fMinNumBlue7"))
                Red7StartIntegral = CDbl(_AllInfo.Rows(i).Item("_fMinNumRed7"))
                WorldStartIntegral = CDbl(_AllInfo.Rows(i).Item("_fMinNumTX"))
                ' '获取蓝7，红7，天下的积分最高分数限制
                Blue7MaxIntegral = CDbl(_AllInfo.Rows(i).Item("_fMaxNumBlue7"))
                Red7MaxIntegral = CDbl(_AllInfo.Rows(i).Item("_fMaxNumRed7"))
                WorldMaxIntegral = CDbl(_AllInfo.Rows(i).Item("_fMaxNumTX"))
                '疯狂送 遊戲輸贏百分比0.6 ，疯狂送中，中奖的比例 该值越大中奖率越大
                CrazySendPercentage(ii) = CDbl(_AllInfo.Rows(i).Item("_fFreeWinChance"))
                Dim cftemp() As String = _AllInfo.Rows(i).Item("_nNumberQP").Split(",")
                '如果获取的参数是 数值，就分别赋值给樱桃连线次数，水果盘连线次数，小B连线次数
                If IsNumeric(cftemp(10)) Then PersonalCherryNum(ii) = cftemp(10)
                If IsNumeric(cftemp(11)) Then PersonalFruitDishNum(ii) = cftemp(11)
                If IsNumeric(cftemp(12)) Then PersonalSmallBarNum(ii) = cftemp(12)
            Catch ex As Exception
                RecordError.WriteGameServerErr("SGP", "GameLogicModule->UpdateBackgroundParameter方法-> 更新输赢百分比时发生错误", ex)
            End Try
        Next
    End Sub

    '''' <summary>
    '''' 清空强制中奖变量，即设置强制中奖后只能强制一次 后台设置强制中奖时，中奖后更新账号及中奖信息
    '''' </summary>
    '''' <param name="v_iZone">分区</param>
    '''' <param name="v_douScore">得分</param>
    '''' <remarks></remarks>
    'Public Sub UpdateQiang(ByVal v_iZone As Integer, ByVal v_douScore As Double)
    '    Try
    '        '获取当局是否中了强制奖项的状态
    '        Dim qiang0 As Integer = CInt(sNowIfForceWin(v_iZone))
    '        If GameSocket.MainLocation.GameLocation(v_iZone, 1, 1) IsNot Nothing AndAlso GameSocket.MainLocation.GameLocation(v_iZone, 1, 1) <> "" Then
    '            '根据座位获取该用户的信息
    '            Dim user0 As TCPNetSocket.Userinfo = GetUserInfo(v_iZone)
    '            If user0 IsNot Nothing Then
    '                '判断中的是蓝7，红7，天下 哪个全盘彩金
    '                Select Case qiang0
    '                    Case 1 '调用数据库修改 all蓝7 中奖记录信息
    '                        InsertOrUpdateByNode(user0.UserAccounts, " 得分:" & v_douScore & " 連線獎金:" & GetTwoString(BureauBlue7Bonus(v_iZone)), "%設置強制中獎%", v_iZone)
    '                    Case 2 '调用数据库修改 all红7 中奖记录信息   
    '                        InsertOrUpdateByNode(user0.UserAccounts, " 得分:" & v_douScore & " 連線獎金:" & GetTwoString(BureauRed7Bonus(v_iZone)), "%設置強制中獎%", v_iZone)
    '                    Case 3 '调用数据库修改 all天下 中奖记录信息   
    '                        InsertOrUpdateByNode(user0.UserAccounts, " 得分:" & v_douScore & " 連線獎金:" & GetTwoString(BureauWorldBonus(v_iZone)), "%設置強制中獎%", v_iZone)
    '                    Case Else
    '                        InsertOrUpdateByNode(user0.UserAccounts, " 得分:" & v_douScore, "%設置強制中獎%", v_iZone)
    '                End Select
    '            End If
    '        End If
    '    Catch ex As Exception
    '        ToNoteItemError("GameLogicModule->执行UpdateQiang方法时错误->清空强制中奖变量时 错误:", ex.Message, ex)
    '    End Try
    'End Sub
End Class
