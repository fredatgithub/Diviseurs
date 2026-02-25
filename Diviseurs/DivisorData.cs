using System;
using System.Collections.Generic;
using System.Linq;

namespace Diviseurs
{
  public class DivisorData
  {
    public int Number { get; set; }
    public string Divisors { get; set; }
    public int DivisorCount { get; set; }
    public bool IsPrime { get; set; }

    public static List<DivisorData> CalculateDivisors(int maxNumber, Action<int> progressCallback = null)
    {
      var result = new List<DivisorData>();

      for (int i = 1; i <= maxNumber; i++)
      {
        var divisors = GetDivisors(i);
        var divisorList = divisors.OrderByDescending(d => d).ToList();

        result.Add(new DivisorData
        {
          Number = i,
          Divisors = string.Join(", ", divisorList),
          DivisorCount = divisors.Count,
          IsPrime = divisors.Count == 2
        });

        // Notifier de la progression
        progressCallback?.Invoke(i);
      }

      return result;
    }

    private static List<int> GetDivisors(int number)
    {
      var divisors = new List<int>();

      for (int i = 1; i <= number; i++)
      {
        if (number % i == 0)
        {
          divisors.Add(i);
        }
      }

      return divisors;
    }
  }
}
