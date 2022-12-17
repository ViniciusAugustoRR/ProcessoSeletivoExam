using System;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace RHManager.Util
{
    public class CustomConverter
    {
        public static DateTime StringToTime(string stringTime)
        {
            return DateTime.ParseExact(stringTime, "HH:mm:ss",
                                                            CultureInfo.InvariantCulture);
        }

        /* Função para verificar a quantia de dias de semana num mês de um determinado ano*/

        public static int GetWeekDaysOfMonth(int mes, int ano)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(ano, mes))
                    .Select(day => new DateTime(ano, mes, day))
                    .Where(dt => dt.DayOfWeek != DayOfWeek.Sunday &&
                                 dt.DayOfWeek != DayOfWeek.Saturday)
                    .ToList().Count;

        }


        
    }

}
