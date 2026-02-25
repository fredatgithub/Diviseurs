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
    public bool HasTwinPrime { get; set; }

    public static List<DivisorData> CalculateDivisors(int maxNumber, Action<int> progressCallback = null)
    {
      var result = new List<DivisorData>();

      for (int i = 1; i <= maxNumber; i++)
      {
        var divisors = GetDivisors(i);
        var divisorList = divisors.OrderByDescending(d => d).ToList();
        var isPrime = divisors.Count == 2;

        result.Add(new DivisorData
        {
          Number = i,
          Divisors = string.Join(", ", divisorList),
          DivisorCount = divisors.Count,
          IsPrime = isPrime,
          HasTwinPrime = isPrime && HasTwinPrimeNumber(i, maxNumber)
        });

        // Notifier de la progression
        progressCallback?.Invoke(i);
      }

      return result;
    }

    private static bool HasTwinPrimeNumber(int number, int maxNumber)
    {
      // Vérifier si number+2 est premier et dans la plage
      var twinNumber = number + 2;
      if (twinNumber > maxNumber)
        return false;

      var twinDivisors = GetDivisors(twinNumber);
      return twinDivisors.Count == 2;
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
