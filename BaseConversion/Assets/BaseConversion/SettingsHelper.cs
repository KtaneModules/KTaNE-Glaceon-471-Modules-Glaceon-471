using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;

public static class SettingsHelper
{
    public static Data GetData(this KMModSettings settings)
    {
        settings.RefreshSettings();
        try { return JsonConvert.DeserializeObject<Data>(settings.Settings).CheckData(); }
        catch
        {
            Data data = new Data();
            File.WriteAllText(settings.SettingsPath, JsonConvert.SerializeObject(data));
            return data;
        }
    }

    public static List<uint> GetBoeforBaseNumbers(this KMModSettings settings)
    {
        return settings.GetData().BoeforBaseNumbers;
    }

    public static List<uint> GetAfterBaseNumbers(this KMModSettings settings)
    {
        return settings.GetData().AfterBaseNumbers;
    }

    public static uint GetMinimumValue(this KMModSettings settings)
    {
        return settings.GetData().MinimumValue;
    }

    public static uint GetMaximumValue(this KMModSettings settings)
    {
        return settings.GetData().MaximumValue;
    }

    public class Data
    {
        public List<uint> BoeforBaseNumbers;
        public List<uint> AfterBaseNumbers;
        public uint MinimumValue = 100;
        public uint MaximumValue = 1000;

        public Data CheckData()
        {
            BoeforBaseNumbers.RemoveAll(x => !CheckBaseNumber(x));
            if (BoeforBaseNumbers.Count <= 0) BoeforBaseNumbers = new List<uint>() { 2, 4, 8, 10, 12, 16 };
            AfterBaseNumbers.RemoveAll(x => !CheckBaseNumber(x));
            if (AfterBaseNumbers.Count <= 0) AfterBaseNumbers = new List<uint>() { 10 };
            if (MinimumValue > MaximumValue)
            {
                uint min = MinimumValue, max = MaximumValue;
                MaximumValue = min;
                MinimumValue = max;
            }
            return this;
        }
    }

    public static bool CheckBaseNumber(uint i)
    {
        if (i < 2) return false;
        if (i > 36) return false;
        return true;
    }

    public static T GetRandom<T>(this IEnumerable<T> data)
    {
        return data.ToArray()[Random.Range(0, data.Count())];
    }
}
