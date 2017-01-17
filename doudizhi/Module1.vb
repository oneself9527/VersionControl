Module
    Sub Main()
        '斗地主项目， 测试全部成功
        第一题()
        Dim a As String = "153,131,174,201,241,144"
        Console.WriteLine(innext(a))
        第二题()
        Dim b As String = "153,131,174,154,132,181"
        Console.WriteLine(delete(b))
        第三题()
        Dim a As String = "131,133,172"
        Dim b As String = "131,132,133,166"
        Console.WriteLine(contain(a, b))
        第四题()
        Dim c As String = "152,153,161,154"
        Console.WriteLine(c)
        第五题()
        Dim a As String = "191,192,193,172"
        Dim b As String = "143,164,162,161"
        Console.WriteLine(max(a, b))
        第六题()
        Dim arr() As String = delelopCard()
        第七题(需要和第六题一起测试)
        Dim num As Integer = minCard(arr(0), arr(1), arr(2), arr(3))
        Console.WriteLine(num)
        第八题()
        tThread.Start() '调用产生用户信息的方法
        第四题()
        Dim str As String = "131,132,133,141,142"
        Dim str2 As String = "144,151,153,154,162,163,181,182,183"
        Dim list As ArrayList = checkStr(str, str2)
        For i = 0 To list.Count - 1
            Console.WriteLine(list.Item(i))
        Next
        Dim ar As String = LostCard("12,45,23", "12")
        Dim qiangge As String = "qianggehaiyoushui lihaile "
        Console.WriteLine(ar)
    End Sub

    Function ranNum() As String
        Dim i, k As Integer
        Dim arr(9) As Integer '声明一个长度为10的数组
        Dim flag(9) As Integer '声明一个记录数字出现几次的数组
        Dim str As String = ""
        For i = 0 To arr.Length - 1 '给arr数组十个数随机赋值，并将值对应的 记录数组的元素 加一
            Randomize()
            arr(i) = Int(Rnd() * 9)
            flag(arr(i)) = flag(arr(i)) + 1
            'Console.WriteLine(arr(i))
        Next
        'For k = 0 To flag.Length - 1
        '    Console.WriteLine(flag(k))
        'Next
        For k = 0 To flag.Length - 1 '循环判断记录次数的数组，若等于一的就将对应数字相加，最后得出字符串
            If flag(k) = 1 Then
                str = str & k
            End If

            Return str
    End Function

    '附加题  第四题
    Function checkStr(ByVal v_card As String, ByVal v_card2 As String) As ArrayList
        Dim arr() As String = Split(v_card, ",")
        Dim brr() As String = Split(v_card2, ",")
        Dim list As ArrayList = New ArrayList '用来添加 都是 三张的集合
        Dim list2 As ArrayList = New ArrayList '用来添加都是 二张的集合
        Dim list3 As ArrayList = New ArrayList '用来添加 最后大于 参数1 的牌型
        Dim i, k, count As Integer
        Dim str As String = ""
        Dim str2 As String = ""
        Dim mrr() As Integer = {13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 26} '用来对应出现牌的数字 的数组
        Dim flag(13) As Integer  '声明一个记录数字出现几次的数组

        For i = 0 To brr.Length - 1 '给给定牌 数组元素循环输出，并将值与之对应的相等的  记录数组的元素  加一
            For k = 0 To mrr.Length - 1
                If Left(brr(i), 2) = mrr(k) Then
                    flag(k) = flag(k) + 1
                End If
            Next
        Next

        For i = 0 To brr.Length - 1 '对出现3次的数字，相对应的元素 连成字符串 ，并记录连接3个相等元素， 并将连接后的相同牌作为一个字符串 添加到list中
            For k = 0 To flag.Length - 1
                If flag(k) = 3 Then
                    If mrr(k) = Left(brr(i), 2) Then
                        str = str & brr(i) & ","
                        count = count + 1
                        If count = 3 Then
                            list.Add(Left(str, str.Length - 1))
                            str = ""
                            count = 0
                        End If
                    End If
                ElseIf flag(k) = 2 Then '对出现2次的数字，相对应的元素 连成字符串 ，并记录连接2个相等元素， 并将连接后的相同牌作为一个字符串 添加到list2中
                    If mrr(k) = Left(brr(i), 2) Then
                        str2 = str2 & brr(i) & ","
                        count = count + 1
                        If count = 2 Then
                            list2.Add(Left(str2, str2.Length - 1))
                            str2 = ""
                            count = 0
                        End If
                    End If
                End If
            Next
        Next

        If flag.Contains(2) Then '判断若是有两张牌大小相等的元素，则将其与三张牌list集合中的每个元素组合成字符串，并与参数一进行比较，用函数max，凡是为true的元素都添加到list3集合中
            For i = 0 To list.Count - 1
                For k = 0 To list2.Count - 1
                    str = list.Item(i) & "," & list2.Item(k)
                    If max(str, v_card) = True Then
                        list3.Add(str)
                        str = ""
                    End If
                Next
            Next
        End If
        If flag.Contains(2) = False AndAlso list.Count > 1 Then '判断若是没有两张牌大小相等的元素，再进行判断，若是三张牌的list集合长度大于1，则可以将三张牌中每个元素、加上最小元素的前两张，作为参数2与参数1比较，为true的添加到list3集合中
            For i = 0 To list.Count - 1
                str = list.Item(i) & "," & Left(list.Item(0), list.Item(0).ToString.Length - 4)
                If max(str, v_card) = True Then
                    list3.Add(str)
                    str = ""
                End If
            Next
            list3.RemoveAt(0) '返回的list3集合中，第一个元素删除，因为其是第一个元素加上第一个元素的前两张牌
        End If

        For k = 0 To brr.Length - 1
            For i = 0 To brr.Length - 1
                str = str & brr(i) & ","
                str2 = Left(str, str.Length - 1)
                If Split(str2, ",").Length = 5 Then '判断若是字符串等于5了，就加入集合中
                    list.Add(str2)
                    Console.WriteLine(str)
                    Console.WriteLine(str2)
                    i = i - 1
                    str = ""
                    str2 = ""
                    'Continue For
                End If
            Next
            'Next
            For j = 0 To list.Count - 1 '循环遍历输出得到的不同牌组合
                If type(list.Item(i)) = 7 Then '用判断类型函数对每个元素进行判断，如果元素类型为7（即三代二）
                    If max(list.Item(i), v_card) = True Then '再用比较函数max，将此元素与 参数一 进行比较，若大于为true， 并将大于的元素 加入新的集合中
                        list2.Add(list.Item(i))
                    End If
                End If
            Next
            ''然后将list集合循环输出，看看有几个元素符合三代二的，符合的再与参数一比较，符合又大于的就返回
            Return list3

    End Function

    Function LostCard(ByVal v_card As String, ByVal v_card2 As String) As String '减去出过牌的函数，参数一减去参数二
        Dim arr() As String = Split(v_card, ",")
        Dim brr() As String = Split(v_card2, ",")
        Dim i, k As Integer
        Dim _str As String = ""
        For i = 0 To arr.Length - 1
            For k = 0 To brr.Length - 1
                If arr(i) = brr(k) Then
                    arr(i) = "" '
                End If
            Next
        Next
        For k = 0 To arr.Length - 1
            _str = _str & arr(i) & ","
        Next
        Return Left(_str, _str.Length - 1)
    End Function
End Module

