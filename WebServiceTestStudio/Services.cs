using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SettingsTracking;
using SettingsTracking.DataStoring;
using SettingsTracking.Serialization;

namespace WebServiceTestStudio
{
    public static class Services
    {
        public static SettingsTracker SettingsTracker = new SettingsTracker(
            new FileDataStore(Environment.SpecialFolder.ApplicationData),
            new JsonSerializer());
    }
}
