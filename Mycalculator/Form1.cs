using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;

namespace Mycalculator
{
    public partial class Form1 : Form
    {
        private Stack<Char> stack;
        //private String preStr;
        //private String dealStr;
        public Form1()
        {
            InitializeComponent();
            stack = new Stack<Char>();
        }

        private void ExpressionStrIn(object sender, EventArgs e)
        {
            String str = ((Button)sender).Text;
            expression.Text = expression.Text + str;
        }

        private void Back(object sender, EventArgs e)
        {
            expression.Text = expression.Text.Substring(0, expression.Text.Length - 1);
        }

        private void ClearAll(object sender, EventArgs e)
        {
            expression.Text = "";
            result.Text = "";
        }

        private void CalculateExpression(object sender, EventArgs e)
        {
            /*String pattern = "[+,-,*,/]";
            String s = expression.Text.Split('e')[0];*/
            //String pattern = "[\"\"|\\D]-\\d+\\.?\\d+";
            //-5+8-2+15/(-5)+1

            char[] arr = expression.Text.ToCharArray();
            Boolean b1 = expression.Text.StartsWith(")");
            Boolean b2 = expression.Text.StartsWith(".");
            Boolean b3 = expression.Text.StartsWith("+");
            Boolean b4 = expression.Text.StartsWith("*");
            Boolean b5 = expression.Text.StartsWith("/");
            Boolean b6 = expression.Text.EndsWith("(");
            Boolean b7 = expression.Text.EndsWith("+");
            Boolean b8 = expression.Text.EndsWith("-");
            Boolean b9 = expression.Text.EndsWith("*");
            Boolean b10 = expression.Text.EndsWith("/");
            Boolean b11 = expression.Text.EndsWith(".");
            if (b1 || b2 || b3 || b4 || b5 || b6 ||
                b7 || b8 || b9 || b10 || b11)
            {
                result.Text = "不合法的表达式,请输入正确的表达式进行计算";
                expression.Text = "";
                return;
            }

            //进行括号匹配校验
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Equals('('))
                {
                    stack.Push(arr[i]);
                }
                if (arr[i].Equals(')'))
                {
                    //这个判断是为了判断第一个出现的括号字符是不是')'，如果是将其进栈然后直接中断循环,因为第一个括号字符如果是')'那么这个表达式一定是错误的
                    if (stack.Count == 0)
                    {
                        stack.Push(arr[i]);
                        break;
                    }
                    if (stack.Count != 0)
                    {
                        stack.Pop();
                    }
                }
            }
            if (stack.Count != 0)
            {
                result.Text = "括号不匹配,请输入正确的表达式进行计算";
                expression.Text = "";
                return;
            }

            string preDealStr = getPreDealStr();

            String expStr = preProcess(preDealStr);
            //返回的错误信息格式是：-1,错误信息;所以要对返回信息进行切割提取具体有用信息
            if ("-1".Equals(expStr.Split(',')[0]))
            {
                result.Text = expStr.Split(',')[1];
            }
            else
            {
                String[] ExpressionArr = expStr.Split('#');
                Stack<String> intStack = transfer2PostOrderExpression(ExpressionArr);
                if ("-1".Equals(intStack.Peek().Split(',')[0]))
                {
                    result.Text = intStack.Peek().Split(',')[1];
                }
                else
                {
                    Stack<Double> calculateResult = concreteCalculation(intStack);
                    if (Double.IsNaN(calculateResult.Peek()))
                    {
                        result.Text = "除数不能为0";
                    }
                    else
                    {
                        result.Text = calculateResult.Peek().ToString();
                    }
                }
            }
            

        }

        /*主要是获取是否有负数的匹配项，有则处理，没有则返回原来的字符串*/
        private string getPreDealStr()
        {
            String tempStr = expression.Text;

            if (tempStr.StartsWith("-"))
            {
                tempStr = " " + tempStr;
            }

            String pattern = "(\\(|\\s){1}?-\\d+(\\.\\d+)?";
            Regex regex = new Regex(pattern);
            MatchCollection matchCollection = Regex.Matches(tempStr, pattern);

            if (matchCollection.Count==0)
            {
                return tempStr;
            }
            
            String temp2 = "";
            String temp3 = "";
            int originLenth = tempStr.Length;
            //因为暂时没想太多边界问题 所以暂时写了3倍空间 可能会造成内存浪费
            String[] spiltArr = new String[2 * tempStr.Length];
            int count = 0;
            foreach (Match match in matchCollection)
            {
                int index = tempStr.IndexOf(match.Value);
                String insertValue = "";
                temp2 = tempStr.Substring(0, index);
                int startIndex = index + match.Length;
                int tempLength = tempStr.Length - match.Length - temp2.Length;
                temp3 = tempStr.Substring(startIndex, tempLength);
                if (match.Value.StartsWith(" "))
                {
                    insertValue = "#" + match.Value.Trim() + "#";
                }
                else
                {
                    insertValue = match.Value.Substring(0, 1) + "#" + match.Value.Substring(1, match.Length - 1) + "#";
                }
                spiltArr[count] = temp2;
                spiltArr[count + 1] = insertValue;

                tempStr = temp3;
                count = count + 2;
            }
            if (temp3.Length != 0)
            {
                spiltArr[count] = temp3;
            }
            String preDealStr = "";
            for (int i = 0; i < spiltArr.Length; i++)
            {
                preDealStr = preDealStr + spiltArr[i];
            }

            return preDealStr;
        }

        private static Stack<Double> concreteCalculation(Stack<String> intStack)
        {
            Stack<Double> result = new Stack<Double>();
            String[] tempArr=intStack.ToArray();
            for (int i=tempArr.Length-1;i>=0;i--)
            {
                String string2 = tempArr[i];
                String pattern1 = "(-)?\\d+";
                String pattern2 = "(-)?\\d+\\.\\d+";
                Regex reg1 = new Regex(pattern1);
                Regex reg2 = new Regex(pattern2);
                Boolean b1 = reg1.IsMatch(string2);
                Boolean b2 = reg2.IsMatch(string2);
                if (b1 || b2)
                {
                    Double temp = Double.Parse(string2);
                    result.Push(temp);
                }
                if ("*".Equals(string2) || "/".Equals(string2) || "+".Equals(string2) || "-".Equals(string2))
                {
                    // 取栈顶的两位数 注意取出来的数据的运算顺序 应该是第二个取出来的是左运算数，第一个是右运算数
                    double d1 = result.Pop();
                    double d2 = result.Pop();
                    double d3 = 0.0;
                    if ("*".Equals(string2))
                    {
                        d3 = d2 * d1;
                    }
                    if ("/".Equals(string2))
                    {
                        if (d1 == 0)
                        {
                            result.Clear();
                            result.Push(Double.NaN);
                            break;
                        }
                        d3 = d2 / d1;
                    }
                    if ("+".Equals(string2))
                    {
                        d3 = d2 + d1;
                    }
                    if ("-".Equals(string2))
                    {
                        d3 = d2 - d1;
                    }
                    result.Push(d3);
                }
            }
            return result;
        }

        private static Stack<String> transfer2PostOrderExpression(String[] expressionArr)
        {
            Stack<String> symbolStack = new Stack<String>();
            Stack<String> intStack = new Stack<String>();
            foreach (String str in expressionArr)
            {
                
                if ("(".Equals(str))
                {
                    symbolStack.Push(str);
                }
                else
                {
                    String pattern1 = "(-)?\\d+";
                    String pattern2 = "(-)?\\d+\\.\\d+";
                    Regex reg1 = new Regex(pattern1);
                    Regex reg2 = new Regex(pattern2);
                    if (reg1.IsMatch(str) || reg2.IsMatch(str))
                    {
                        if (reg2.IsMatch(str))
                        {
                            if (str.StartsWith("0"))
                            {
                                if (str.Split('.')[0].Length > 1)
                                {
                                    intStack.Clear();
                                    intStack.Push("-1,数字格式错误");
                                    break;
                                }
                            }
                            else if (str.StartsWith("-"))
                            {
                                if (str.Split('.')[0].Length > 2)
                                {
                                    char[] validateArr = str.Split('.')[0].ToCharArray();
                                    if (validateArr[1] == '0')
                                    {
                                        intStack.Clear();
                                        intStack.Push("-1,数字格式错误");
                                        break;
                                    }
                                }
                            }
                        }
                        intStack.Push(str);
                        continue;
                    }

                    if (")".Equals(str))
                    {
                        while (!"(".Equals(symbolStack.Peek()))
                        {
                            String strTemp = symbolStack.Pop();
                            intStack.Push(strTemp);
                        }
                        // 这个主要是把底部的“（”也取出来
                        symbolStack.Pop();
                    }
                    else
                    {
                        // 如果顶部是‘(’字符 那么 直接入栈
                        if (!(symbolStack.Count == 0) && "(".Equals(symbolStack.Peek()))
                        {
                            symbolStack.Push(str);
                            continue;
                        }
                        //如果是其他字符 ，如果栈非空且栈顶部不是‘(’字符 那么就对该字符与字符栈的栈顶数据进行循环比较 如果栈顶的字符比较级大于等于该字符
                        //那么栈顶数据出栈进入数字栈 否则该字符直接进字符栈,一般情况下字符优先级应该是*/+-，但这里注意如果栈顶是-，而字符时+的情况 这个时候-号的优先级比+号高
                        //因为如果不是这样(a+b)-c+d这个表达式根据一般优先级会变成 最后结果会是(a+b)-(c+d),所以arr2[3][2]应该设为1

                        Boolean isAlreadyIn = false;
                        while (!(symbolStack.Count == 0) && !"(".Equals(symbolStack.Peek()))
                        {

                            int compareResult = compare(symbolStack.Peek(), str);
                            if (compareResult != -1)
                            {
                                if (compareResult == 1)
                                {
                                    String temp = symbolStack.Pop();
                                    intStack.Push(temp);
                                }
                                else
                                {
                                    symbolStack.Push(str);
                                    isAlreadyIn = true;
                                    break;
                                }
                            }
                            else
                            {
                                intStack.Clear();
                                intStack.Push("-1,要比较符号不是计算符号");
                                return intStack;
                            }
                        }
                        // 字符栈比较进栈有两种方式 一是比较完之后将该字符进栈 二是栈顶比字符比较级小 所以将字符直接进栈
                        // 这里就是为了防止第二种情况在字符已经进栈的情况下再进一次栈
                        if (!isAlreadyIn)
                        {
                            symbolStack.Push(str);
                        }
                    }

                }
            }
            if (!(symbolStack.Count == 0))
            {
                intStack.Push(symbolStack.Pop());
            }
            return intStack;
        }

        private static String preProcess(String expression)
        {
            StringBuilder processStr = new StringBuilder();
            char[] tempArr = expression.ToCharArray();
            Boolean isNegativeNumberRange = false;
            int countInNegativeRange = 0;
            for (int i = 0; i < tempArr.Length; i++)
            {
                if (tempArr[i]=='（'||tempArr[i] == '）')
                {
                    String errStr = "-1,不能出现中文括号";
                    return errStr;
                }
                //主要是为了将在#号里面的负数原样输入而不用将负数的负号进行处理
                if (tempArr[i] == '#')
                {
                    if (countInNegativeRange % 2 == 0)
                    {
                        isNegativeNumberRange = true;
                    }
                    else
                    {
                        isNegativeNumberRange = false;
                    }
                    countInNegativeRange++;
                }
                if (isNegativeNumberRange)
                {
                    if (i > 0 && processStr.ToString().EndsWith("#") && tempArr[i] == '#')
                    {
                        continue;
                    }
                    processStr.Append(tempArr[i]);
                    continue;
                }

                if (tempArr[i] == '(' || tempArr[i] == ')' || tempArr[i] == '*' || tempArr[i] == '/' || tempArr[i] == '+' || tempArr[i] == '-')
                {
                    if (processStr.ToString().EndsWith("#"))
                    {
                        processStr.Append(tempArr[i]).Append("#");
                    }
                    else
                    {
                        processStr.Append("#").Append(tempArr[i]).Append("#");
                    }
                }
                else
                {
                    processStr.Append(tempArr[i]);
                }
            }
            //主要是为了如果第一个字符是（或者是-号的情况去掉第一个字符的# 还有最后一个字符也一样避免后面split出来会有空的情况
            String returnStr = processStr.ToString();
            if (returnStr.StartsWith("#")&& returnStr.EndsWith("#"))
            {
                return returnStr.Substring(1, returnStr.Length-2);
            }
            else if (returnStr.StartsWith("#"))
            {
                return returnStr.Substring(1);
            }
            else if (returnStr.EndsWith("#"))
            {
                return returnStr.Substring(0,processStr.Length-1);
            }
            return returnStr;
        }

        private static int compare(String peek, String symbol)
        {
            int temp1 = -1, temp2 = -1;
            //一般情况下字符优先级应该是*/+-，但这里注意如果栈顶是-，而字符时+的情况 这个时候-号的优先级比+号高 
            //因为如果不是这样(a+b)-c+d这个表达式根据一般优先级会变成 最后结果会是(a+b)-(c+d),所以arr2[3][2]应该设为1
            int[,] arr2 = new int[,] { { 1, 1, 1, 1 }, { 1, 1, 1, 1 }, { 0, 0, 1, 1 }, { 0, 0, 1, 1 } };

            switch (peek)
            {
                case "*": temp1 = 0; break;
                case "/": temp1 = 1; break;
                case "+": temp1 = 2; break;
                case "-": temp1 = 3; break;
            }

            switch (symbol)
            {
                case "*": temp2 = 0; break;
                case "/": temp2 = 1; break;
                case "+": temp2 = 2; break;
                case "-": temp2 = 3; break;
            }

            if (temp1 == -1 || temp2 == -1)
            {
                return -1;
            }
            return arr2[temp1,temp2];
        }
    }
}
