using System;
using contest.submission.contract;
using System.Diagnostics;

// 
// Lösung für den Programmierwettbewerb der dotNetPro 2015/04
// Programiert von Christof Konstantinopoulos 
//
// e-Mail: christof@konstantinopoulos.de
//
// Der Quelltext ist nicht zur Veröffentlichung freigegeben und 
// darf nur für die Auswertung des Wettbewerbs verwendet werden.
//
// 21.7.2015

namespace contest.submission
{
  [Serializable]
  public class Solution : IDnp1504Solution
  {
      decimal lastEstimation = 0.0m;
      decimal nextEstimation = 0.0m;
      decimal max = decimal.MaxValue;
      decimal min = decimal.MinValue;
      int counter = 0;

      bool isNegative = false;
      bool isExponentFound = false;
      bool isRounded = false;

      int[] bits;
      int[] bitsMax;
      int[] bitsMin;
      byte expMax = 0;
      byte expMin = 28;
      byte exp = 0;
      byte nextExp = 0;

      int startIdx0000 = 0;
      int startIdx9999 = 0;

    public void Process(Rating rating)
    {
        if (rating == Rating.Exactly) Debug.WriteLine("After {0} steps the correct number {1} was found.", counter, lastEstimation);
        
        // Schritt 1
        if (rating == Rating.Start)
        {
            Debug.WriteLine(" Start : ");

            lastEstimation = 0.0m;
            max = decimal.MaxValue;
            min = decimal.MinValue;
            
            isNegative = false;
            isExponentFound = false;
            isRounded = false;

            expMax = 0;
            expMin = 28;
            exp = 0;
            nextExp = 0;

            counter = 0;
            nextEstimation = 0.0m;  // : auf 0.0 Prüfen, um das Vorzeichen zu ermitteln

            startIdx0000 = 0;
            startIdx9999 = 0;
        }

        if (counter == 1) // Erste Prüfung gegen 0 für das Vorzeichen
        {
            if (rating == Rating.ToHigh)
            {
                Debug.WriteLine("to High");
                isNegative = true;
                max = 0.0m;
                nextEstimation = decimal.MinusOne; // 2. Prüfung, ob kleiner oder größer 1
            }
            if (rating == Rating.ToLow)
            {
                Debug.WriteLine("to Low");
                min = 0.0m;
                isNegative = false;
                nextEstimation = decimal.One; // 2. Prüfung, ob kleiner oder größer 1
            }
        }

        if (counter == 2) // zweite Prüfung, ob die Zahl im Interval [-1..0] oder [0..1] liegt
        {
            if (isNegative)
            {
                if (rating == Rating.ToHigh)
                {
                    // ja, Zahl liegt im intervall [-1..0]
                    min = decimal.MinusOne;
                }
                if (rating == Rating.ToLow)
                {
                    // nein, Zahl liegt nicht im intervall [-1..0]
                    max = decimal.MinusOne;
                }
            }
            else
            {
                if (rating == Rating.ToHigh)
                {
                    // nein, Zahl liegt nicht im intervall [0..1]
                    min = decimal.One;
                }
                if (rating == Rating.ToLow)
                {
                    // ja, Zahl liegt im intervall [0..1]
                    max = decimal.One;
                }
            }

            bits = Decimal.GetBits(lastEstimation);
            bits[0] = bits[0] / 2;
            nextEstimation = new Decimal(bits);
        }

        if (counter > 2)
        {
            bits = Decimal.GetBits(lastEstimation);
            exp = Convert.ToByte((bits[3] >> 16) & 0x7F);
            var s1 = string.Format("{0,31} : {1,10:X8} => Scale = {2}", lastEstimation, bits[3], exp);
            nextExp = exp;
            
            if (rating == Rating.ToHigh)
            {
                Debug.WriteLine("to High");
                max = lastEstimation;
                nextEstimation = max - (max - min) / 2;                
             
                if (isNegative)
                {
                    expMin = exp;
                    nextExp = Convert.ToByte(expMin - (expMax - expMin) / -2); // durch -2 weil der Exponent von 10^0 bis 10^-28 läuft
                }
                else
                {
                    expMax = exp;
                    nextExp = Convert.ToByte(expMax + (expMax - expMin) / -2); // durch -2 weil der Exponent von 10^0 bis 10^-28 läuft
                }
            }
            if (rating == Rating.ToLow)
            {
                Debug.WriteLine("to Low");
                min = lastEstimation;
                nextEstimation = min + (max - min) / 2;               
                if (isNegative)
                {
                    expMax = exp;
                    nextExp = Convert.ToByte(expMax + (expMax - expMin) / -2); // durch -2 weil der Exponent von 10^0 bis 10^-28 läuft
                }
                else
                {
                    expMin = exp;
                    nextExp = Convert.ToByte(expMin - (expMax - expMin) / -2); // durch -2 weil der Exponent von 10^0 bis 10^-28 läuft
                }
            }


            if (!isExponentFound)
            {
                bits = Decimal.GetBits(nextEstimation);
                nextEstimation = new decimal(bits[0], bits[1], bits[2], isNegative, nextExp);
                if (exp == nextExp) isExponentFound = true;
            }

        }

        // Eingebaut, damit Min und Max gefunden werden
        if (counter > 5 && lastEstimation == nextEstimation && exp == 0)
        {
            if (isNegative && nextEstimation != Decimal.MinValue)
            {
                nextEstimation = nextEstimation - Decimal.One;
            }
            if (!isNegative && nextEstimation != Decimal.MaxValue)
            {
                nextEstimation = nextEstimation + Decimal.One;
            }
        }

        var diff = Math.Abs(lastEstimation - nextEstimation);

        //Prüfen, ob die Differenz eventuell knapp neben einer Zahl liegt
        var estStr = nextEstimation.ToString();

        if (estStr.Length > startIdx0000)
        {
            var idx0000 = estStr.IndexOf("000", startIdx0000);
            if (idx0000 > 0)
            {
                startIdx0000 = idx0000 + 1;
                estStr = estStr.Substring(0, idx0000);
                var est = Convert.ToDecimal(estStr);
                if (est < max && est > min)
                {
                    nextEstimation = est;
                }
            }
        }

        if (estStr.Length > startIdx9999)
        {
            var idx9999 = estStr.IndexOf("999", startIdx9999);
            if (idx9999 > 0 && idx9999 < 29)
            {
                startIdx9999 = idx9999 + 1;
                estStr = estStr.Substring(0, idx9999);
                estStr = estStr + "999";
                var est = Convert.ToDecimal(estStr);
                est = Math.Round(est, idx9999);
                if (est < max && est > min)
                {
                    nextEstimation = est;
                }
            }
        }


        bits = Decimal.GetBits(nextEstimation);
        var s2 = string.Format("{0,31} : {1,10:X8} => Scale = {2}", nextEstimation, bits[3], nextExp);


        lastEstimation = nextEstimation;
        Debug.Write(string.Format("{0:d3} : Exp={5:d2}, {4:X8},{3:X8},{2:X8},{1:X8}, diff={6:f28}: ", counter, bits[0], bits[1], bits[2], bits[3], nextExp, diff));
        SendResult(nextEstimation);
        counter++;
    }

    public event Action<decimal> SendResult;
  }
}
