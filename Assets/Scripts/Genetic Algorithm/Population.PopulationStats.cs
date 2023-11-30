using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// Population stats for a given generation, has all the lsystems data along with the generation number
/// </summary>
public partial class Population
{
    public struct PopulationStats
    {
        public readonly int genNumber;

        [JsonProperty("lsystemsData")]
        public readonly List<Lsystem.LsystemData> lsystemsData;
    

        public PopulationStats(int genNumber, List<Lsystem.LsystemData> lsystemsdata)
        {
            this.genNumber = genNumber;
            this.lsystemsData = lsystemsdata;
        }
    }
}
