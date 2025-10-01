using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDBms
{
    internal class CommandManager
    {
        /// <summary>
        /// 숫자로 입력을 받을 수 있게 하는 함수. 숫자 입력 범위를 검사할 수 있고, 정상적인 입력시까지 계속 입력을 받게 한다.
        /// </summary>
        /// <param name="menu">화면에 출력할 사용자 지시사항</param>
        /// <param name="limit">입력 범위 조건. 0이면 범위 무시</param>
        /// <returns></returns>
        public int GetSelectNumber(string menu, int limit = 0)
        {
            bool isExit = false;
            int select = 0;
            while (!isExit)
            {
                Console.Write(menu);

                string input = Console.ReadLine();
                if (int.TryParse(input, out select))
                {
                    if (limit == 0)
                        isExit = true;
                    else if (select > 0 && select <= limit)
                        isExit = true;
                    else
                        Console.WriteLine("입력이 잘못되었습니다. 확인하고 다시 입력하세요...");
                }
            }
            return select;
        }
        /// <summary>
        /// 문자열을 입력받는 함수, 특정 문자열을 지정하여 조건을 감지하게 함.
        /// </summary>
        /// <param name="menu">화면에 출력할 사용자 지시사항</param>
        /// <param name="condition">지정된 문자. 안 넣으면 조건을 검사하지 않음</param>
        /// <returns>입력 받는 문자열으로 empty일 경우 조건에 걸린것임.</returns>
        public string GetSelectText(string menu, string condition = "")
        {
            Console.Write(menu);
            string input = Console.ReadLine();

            if(input.Length != 0)
            {
                if (input == condition)
                    input = string.Empty;
            }
            return input;
        }

    }
}
