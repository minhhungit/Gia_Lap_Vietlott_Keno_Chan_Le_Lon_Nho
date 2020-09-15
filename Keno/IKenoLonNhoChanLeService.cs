using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Keno
{
    public interface IKenoLonNhoChanLeService
    {
        void RunGame();
    }

    public class KenoLonNhoChanLeService : IKenoLonNhoChanLeService
    {
        private readonly ILogger<KenoLonNhoChanLeService> _log;
        private readonly IConfiguration _config;

        public KenoLonNhoChanLeService(ILogger<KenoLonNhoChanLeService> log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }


        public void RunGame()
        {
            double win_times = 0;
            double lose_times = 0;

            long tien_von = 1000000;
            _log.LogWarning($"Tien khoi dau: {tien_von:C0}");

            var so_lan_trung_local = 0;

            long tien_cao_nhat = 0;
            long luot_choi = 0;

            while (true)
            {
                luot_choi++;

                var maskChanLe = new List<KeyValuePair<ChanLe, long>>();
                maskChanLe.Add(new KeyValuePair<ChanLe, long>(ChanLe.Chan, 10000));
                maskChanLe.Add(new KeyValuePair<ChanLe, long>(ChanLe.Chan, 10000));
                maskChanLe.Add(new KeyValuePair<ChanLe, long>(ChanLe.Le, 10000));

                var maskLonNho = new List<KeyValuePair<LonNho, long>>();
                maskLonNho.Add(new KeyValuePair<LonNho, long>(LonNho.Lon, 10000));
                maskLonNho.Add(new KeyValuePair<LonNho, long>(LonNho.Lon, 10000));
                maskLonNho.Add(new KeyValuePair<LonNho, long>(LonNho.Nho, 10000));

                var tien_phi = maskChanLe.Sum(x => x.Value) + maskLonNho.Sum(x => x.Value);
                _log.LogDebug($"Tien phi: {tien_phi:C0}");

                tien_von -= tien_phi;

                if (tien_von <= 0)
                {
                    _log.LogWarning($"Het tien o luot choi {luot_choi}");
                    break;
                }

                var tien_trung_thuong = GameTurn(maskChanLe, maskLonNho);

                if (tien_trung_thuong > 0)
                {
                    so_lan_trung_local++;
                }

                if (tien_trung_thuong != tien_phi)
                {
                    if (tien_trung_thuong > tien_phi)
                    {
                        win_times++;
                    }
                    else
                    {
                        lose_times++;
                    }
                }                

                tien_von += tien_trung_thuong;
                _log.LogDebug("Tien hien tai " + tien_von.ToString("C0"));

                if (tien_von > tien_cao_nhat)
                {
                    tien_cao_nhat = tien_von;
                }

                _log.LogDebug("---------------");
            }

            //_log.LogWarning($"Tien le con lai: {tien_von:C0}");
            //_log.LogWarning($"So lan trung: {so_lan_trung_local}");
            //_log.LogWarning($"Tien thang nhieu nhat: {tien_cao_nhat:C0}");
            _log.LogWarning($"Win rate {Math.Round(win_times * 100 / lose_times, 2)}%");
        }

        private long GameTurn(List<KeyValuePair<ChanLe, long>> maskChanLe, List<KeyValuePair<LonNho, long>> maskLonNho)
        {
            long tienThuong = 0;

            var output = new List<int>();
            for (int i = 0; i < 20; i++)
            {
                var number = (int)Get64BitRandom(1, 80);
                output.Add(number);
            }

            _log.LogDebug(string.Join(", ", output));

            var tongSoChan = output.Count(x => x % 2 == 0);
            var tongSoLe = output.Count(x => x % 2 != 0);

            var is15Chan = tongSoChan >= 15;
            var is15Le = tongSoLe >= 15;

            var is1314Chan = tongSoChan == 13 || tongSoChan == 14;
            var is1314Le = tongSoLe == 13 || tongSoLe == 14;

            if (is15Chan || is15Le)
            {
                foreach (var item in maskChanLe)
                {
                    if (item.Key == ChanLe.Chan && is15Chan)
                    {
                        _log.LogDebug($"Trung 15 chan");
                        tienThuong += 210000;
                    }

                    if (item.Key == ChanLe.Le && is15Le)
                    {
                        _log.LogDebug($"Trung 15 le");
                        tienThuong += 210000;
                    }
                }
            }
            else
            {
                if (is1314Chan || is1314Le)
                {
                    foreach (var item in maskChanLe)
                    {
                        if (item.Key == ChanLe.Chan && is1314Chan)
                        {
                            _log.LogDebug($"Trung 13/14 chan");
                            tienThuong += 40000;
                        }

                        if (item.Key == ChanLe.Le && is1314Le)
                        {
                            _log.LogDebug($"Trung 13/14 le");
                            tienThuong += 40000;
                        }
                    }
                }
                else
                {
                    _log.LogDebug("Chan le khong trung");
                }
            }

            ///////////////
            var isSoLon = output.Count(x => x >= 41) >= 13;
            var isSoNho = output.Count(x => x < 41) >= 13;

            if (isSoLon || isSoNho)
            {
                foreach (var item in maskLonNho)
                {
                    if (item.Key == LonNho.Lon && isSoLon)
                    {
                        _log.LogDebug($"Trung so lon");
                        tienThuong += 56000;
                    }

                    if (item.Key == LonNho.Nho && isSoNho)
                    {
                        _log.LogDebug($"Trung so nho");
                        tienThuong += 56000;
                    }
                }
            }
            else
            {
                _log.LogDebug("lon nho khong trung");
            }


            _log.LogDebug("-----");

            _log.LogDebug($"Chan {(is15Chan || is1314Chan ? "YES" : "NO")}");
            _log.LogDebug($"Le {(is15Le || is1314Le ? "YES" : "NO")}");
            _log.LogDebug($"Lon {(isSoLon ? "YES" : "NO")}");
            _log.LogDebug($"Nho {(isSoNho ? "YES" : "NO")}");

            _log.LogDebug("So tien trung thuong: " + tienThuong.ToString("C0"));
            return tienThuong;
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

        enum ChanLe
        {
            Chan = 0,
            Le = 1
        }

        enum LonNho
        {
            Lon = 0,
            Nho = 1
        }
    }
}
