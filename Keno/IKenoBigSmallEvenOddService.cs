using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Keno
{
    public interface IKenoBigSmallEvenOddService
    {
        void RunGame();
    }

    public class KenoBigSmallEvenOddService : IKenoBigSmallEvenOddService
    {
        private readonly ILogger<KenoBigSmallEvenOddService> _log;
        private readonly IConfiguration _config;

        public KenoBigSmallEvenOddService(ILogger<KenoBigSmallEvenOddService> log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }


        public void RunGame()
        {
            double total_win_times = 0;
            double total_lose_times = 0;

            long fund = 1000000;
            _log.LogWarning($"Fund: {fund:C0}");

            var nbrOfWins = 0;

            long highestWinAmount = 0;
            long round = 0;

            while (true)
            {
                round++;

                var maskEvenOdd = new List<KeyValuePair<EvenOddTypes, long>>();
                maskEvenOdd.Add(new KeyValuePair<EvenOddTypes, long>(EvenOddTypes.Even, 10000));
                maskEvenOdd.Add(new KeyValuePair<EvenOddTypes, long>(EvenOddTypes.Even, 10000));
                maskEvenOdd.Add(new KeyValuePair<EvenOddTypes, long>(EvenOddTypes.Odd, 10000));

                var maskBigSmall = new List<KeyValuePair<BigSmallTypes, long>>();
                maskBigSmall.Add(new KeyValuePair<BigSmallTypes, long>(BigSmallTypes.Big, 10000));
                maskBigSmall.Add(new KeyValuePair<BigSmallTypes, long>(BigSmallTypes.Big, 10000));
                maskBigSmall.Add(new KeyValuePair<BigSmallTypes, long>(BigSmallTypes.Small, 10000));

                var playCost = maskEvenOdd.Sum(x => x.Value) + maskBigSmall.Sum(x => x.Value);
                _log.LogDebug($"Play cost: {playCost:C0}");

                fund -= playCost;

                if (fund <= 0)
                {
                    _log.LogWarning($"Out of money at round {round}");
                    break;
                }

                var prizeMoney = GameRound(maskEvenOdd, maskBigSmall);

                if (prizeMoney > 0)
                {
                    nbrOfWins++;
                }

                if (prizeMoney != playCost)
                {
                    if (prizeMoney > playCost)
                    {
                        total_win_times++;
                    }
                    else
                    {
                        total_lose_times++;
                    }
                }                

                fund += prizeMoney;
                _log.LogDebug("Cash " + fund.ToString("C0"));

                if (fund > highestWinAmount)
                {
                    highestWinAmount = fund;
                }

                _log.LogDebug("---------------");
            }

            _log.LogWarning($"Cash: {fund:C0}");
            _log.LogWarning($"Nbr of wins: {nbrOfWins}");
            _log.LogWarning($"Highest cash: {highestWinAmount:C0}");
            _log.LogWarning($"Win rate {Math.Round(total_win_times * 100 / total_lose_times, 2)}%");
        }

        private long GameRound(List<KeyValuePair<EvenOddTypes, long>> maskEvenOdd, List<KeyValuePair<BigSmallTypes, long>> maskBigSmall)
        {
            long prizeMoney = 0;

            var outputNumbers = new List<int>();
            for (int i = 0; i < 20; i++)
            {
                var number = (int)Get64BitRandom(1, 80);
                outputNumbers.Add(number);
            }

            _log.LogDebug(string.Join(", ", outputNumbers));

            var countOfEvens = outputNumbers.Count(x => x % 2 == 0);
            var countOfOdd = outputNumbers.Count(x => x % 2 != 0);

            var is15Even = countOfEvens >= 15;
            var is15Odd = countOfOdd >= 15;

            var is1314Even = countOfEvens == 13 || countOfEvens == 14;
            var is1314Odd = countOfOdd == 13 || countOfOdd == 14;

            if (is15Even || is15Odd)
            {
                foreach (var item in maskEvenOdd)
                {
                    if (item.Key == EvenOddTypes.Even && is15Even)
                    {
                        _log.LogDebug($"You won ( >= 15 even numbers)");
                        prizeMoney += 210000;
                    }

                    if (item.Key == EvenOddTypes.Odd && is15Odd)
                    {
                        _log.LogDebug($"You won ( >= 15 odd numbers )");
                        prizeMoney += 210000;
                    }
                }
            }
            else
            {
                if (is1314Even || is1314Odd)
                {
                    foreach (var item in maskEvenOdd)
                    {
                        if (item.Key == EvenOddTypes.Even && is1314Even)
                        {
                            _log.LogDebug($"You won 13/14 even");
                            prizeMoney += 40000;
                        }

                        if (item.Key == EvenOddTypes.Odd && is1314Odd)
                        {
                            _log.LogDebug($"You won 13 /14 odd");
                            prizeMoney += 40000;
                        }
                    }
                }
                else
                {
                    _log.LogDebug("Fail even/odd");
                }
            }

            ///////////////
            var isBig = outputNumbers.Count(x => x >= 41) >= 13;
            var isSmall = outputNumbers.Count(x => x < 41) >= 13;

            if (isBig || isSmall)
            {
                foreach (var item in maskBigSmall)
                {
                    if (item.Key == BigSmallTypes.Big && isBig)
                    {
                        _log.LogDebug($"Won big numbers");
                        prizeMoney += 56000;
                    }

                    if (item.Key == BigSmallTypes.Small && isSmall)
                    {
                        _log.LogDebug($"Won small numbers");
                        prizeMoney += 56000;
                    }
                }
            }
            else
            {
                _log.LogDebug("Fail big/small");
            }


            _log.LogDebug("-----");

            _log.LogDebug($"Even: {(is15Even || is1314Even ? "YES" : "NO")}");
            _log.LogDebug($"Odd: {(is15Odd || is1314Odd ? "YES" : "NO")}");
            _log.LogDebug($"Big: {(isBig ? "YES" : "NO")}");
            _log.LogDebug($"Small: {(isSmall ? "YES" : "NO")}");

            _log.LogDebug("Prize money: " + prizeMoney.ToString("C0"));
            return prizeMoney;
        }


        private static readonly Random rnd = new Random();
        private static ulong Get64BitRandom(ulong minValue, ulong maxValue)
        {
            // Get a random array of 8 bytes. 
            // As an option, you could also use the cryptography namespace stuff to generate a random byte[8]
            byte[] buffer = new byte[sizeof(ulong)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0) % (maxValue - minValue + 1) + minValue;
        }

        enum EvenOddTypes
        {
            Even = 0,
            Odd = 1
        }

        enum BigSmallTypes
        {
            Big = 0,
            Small = 1
        }
    }
}
