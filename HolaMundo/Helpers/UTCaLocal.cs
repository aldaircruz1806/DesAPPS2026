using System;
using System.Collections.Generic;
using System.Text;

namespace UltimaMilla.Repartidor.Helpers;

    public static class UTCaLocal
    {
        public static DateTime AHoraLocal(this DateTime fecha)
        {
            var utc = fecha.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(fecha, DateTimeKind.Utc)  // vino sin Kind (JSON/SQLite): es UTC
                : fecha.ToUniversalTime();
            return utc.ToLocalTime();
        }
    }

